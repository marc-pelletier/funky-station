using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Components;
using Content.Server.Power.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Content.Shared._Funkystation.Power.Generation.Turbine;
using Content.Server.Atmos.Piping.Components;
using Robust.Shared.Map.Components;
using Content.Shared.Atmos.Components;

namespace Content.Server.Turbine;

public sealed class TurbineSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    // SS13 constants
    private const float RpmConversion = 15f;
    private const float EnergyRectificationMultiplier = 0.25f;
    private const float WorkConversionMultiplier = 0.01f;
    private const float HeatConversionMultiplier = 0.005f;
    private const float CompressorStatorInteractionMultiplier = 0.15f;
    private const float MinimumTurbinePressure = 0.01f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurbineCompressorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TurbineCompressorComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<TurbineRotorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TurbineRotorComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<TurbineOutletComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TurbineOutletComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<TurbineRotorComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
    }

    private void OnStartup(EntityUid uid, Component comp, ComponentStartup args)
    {
        UpdateConnections(uid, comp);
    }

    private void OnAnchorChanged(EntityUid uid, Component comp, AnchorStateChangedEvent args)
    {
        UpdateConnections(uid, comp);
    }

    private void OnAtmosUpdate(EntityUid uid, TurbineRotorComponent rotor, AtmosDeviceUpdateEvent args)
    {
        // Only process if all parts are connected
        if (!rotor.AllPartsConnected)
        {
            if (TryComp<PowerSupplierComponent>(uid, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
            return;
        }

        // Find Compressor and Outlet
        if (!TryComp<TransformComponent>(uid, out var rotorXform) || !rotorXform.Anchored || !rotorXform.GridUid.HasValue)
        {
            rotor.AllPartsConnected = false;
            if (TryComp<PowerSupplierComponent>(uid, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
            return;
        }

        var rotorMapCoords = _transform.GetMapCoordinates(uid);
        if (!_map.TryFindGridAt(rotorMapCoords, out var gridUid, out var gridComp))
        {
            rotor.AllPartsConnected = false;
            if (TryComp<PowerSupplierComponent>(uid, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
            return;
        }

        // Calculate positions
        var rotation = rotorXform.LocalRotation;
        var rotorPos = gridComp.WorldToTile(rotorMapCoords.Position);
        var compressorPos = rotorPos - rotation.GetCardinalDir().ToIntVec(); // Compressor at 180°
        var outletPos = rotorPos + rotation.GetCardinalDir().ToIntVec();     // Outlet forward

        // Find Compressor
        EntityUid? compressorEnt = null;
        TurbineCompressorComponent? compressorComp = null;
        foreach (var ent in gridComp.GetAnchoredEntities(compressorPos))
        {
            if (TryComp<TurbineCompressorComponent>(ent, out var comp) &&
                TryComp<TransformComponent>(ent, out var xform) &&
                xform.Anchored &&
                xform.LocalRotation.EqualsApprox(rotation))
            {
                compressorEnt = ent;
                compressorComp = comp;
                break;
            }
        }

        // Find Outlet
        EntityUid? outletEnt = null;
        TurbineOutletComponent? outletComp = null;
        foreach (var ent in gridComp.GetAnchoredEntities(outletPos))
        {
            if (TryComp<TurbineOutletComponent>(ent, out var comp) &&
                TryComp<TransformComponent>(ent, out var xform) &&
                xform.Anchored &&
                xform.LocalRotation.EqualsApprox(rotation))
            {
                outletEnt = ent;
                outletComp = comp;
                break;
            }
        }

        // Validate parts
        if (!compressorEnt.HasValue || !outletEnt.HasValue || compressorComp == null || outletComp == null)
        {
            rotor.AllPartsConnected = false;
            return;
        }

        // Step 1: Compressor pulls from input tile
        var inputCoords = GetInputTile(compressorEnt.Value);
        var inputTile = gridComp.WorldToTile(inputCoords.Position);
        GridAtmosphereComponent? gridAtmosComp = null;
        TryComp<GridAtmosphereComponent>(gridUid, out gridAtmosComp);
        var gridEnt = (gridUid, gridAtmosComp, (GasTileOverlayComponent?)null);
        MapAtmosphereComponent? mapAtmosComp = null;
        TryComp<MapAtmosphereComponent>(rotorXform.MapUid!.Value, out mapAtmosComp);
        var mapEnt = (rotorXform.MapUid!.Value, mapAtmosComp);
        var inputMix = _atmos.GetTileMixture(gridEnt, mapEnt, inputTile, excite: true);
        if (inputMix == null || inputMix.TotalMoles <= 0f)
            return;

        float intakeRegulator = 0.5f; 
        float compressorWork = TransferGases(inputMix, compressorComp.CompressorGasMix, 0f, intakeRegulator);
        float compressorPressure = MathF.Max(compressorComp.CompressorGasMix.Pressure, MinimumTurbinePressure);

        // Step 2: Rotor transfers from Compressor
        float rotorWork = TransferGases(compressorComp.CompressorGasMix, rotor.RotorGasMix, compressorWork);

        // Step 3: Outlet transfers from Rotor
        float turbineWork = TransferGases(rotor.RotorGasMix, outletComp.OutletGasMix, MathF.Abs(rotorWork));

        // Step 4: Outlet expels to output tile
        var outputCoords = GetOutputTile(outletEnt.Value);
        var outputTile = gridComp.WorldToTile(outputCoords.Position);
        var outputMix = _atmos.GetTileMixture(gridEnt, mapEnt, outputTile, excite: true);
        if (outputMix == null)
            return;

        // Expel all gas from Outlet's OutletGasMix to output tile
        var ejectedGas = outletComp.OutletGasMix.Remove(outletComp.OutletGasMix.Pressure * outletComp.OutletGasMix.Volume / Atmospherics.R);
        if (ejectedGas.TotalMoles > 0f)
            _atmos.Merge(outputMix, ejectedGas);

        // Step 5: Calculate and supply power
        if (TryComp<PowerSupplierComponent>(uid, out var powerSupplierComp))
        {
            // Calculate RPM from rotor work
            rotor.Rpm = MathF.Min(rotorWork * RpmConversion, rotor.MaxAllowedRpm);

            // Calculate power: rpm * compressor_stator_interaction * energy_rectification
            float power = rotor.Rpm * CompressorStatorInteractionMultiplier * EnergyRectificationMultiplier;

            // Update PowerSupplier
            powerSupplierComp.Enabled = rotor.Rpm > 0f;
            powerSupplierComp.MaxSupply = power;
            powerSupplierComp.SupplyRampRate = 10000f; // Ramp 10 kW/s for quick response
            powerSupplierComp.SupplyRampTolerance = 1000f; // Allow 1 kW overshoot

            // Debug log for power
            Logger.Debug($"Turbine {uid}: Rpm={rotor.Rpm:F2}, MaxSupply={power:F2}W, CurrentSupply={powerSupplierComp.CurrentSupply:F2}W");
        }
    }

    private void UpdateConnections(EntityUid entity, Component comp)
    {
        EntityUid? rotorEnt = null;
        TurbineRotorComponent? rotor = null;

        // Find Rotor
        if (comp is TurbineRotorComponent rotorComp)
        {
            rotorEnt = entity;
            rotor = rotorComp;
        }
        else if (comp is TurbineCompressorComponent || comp is TurbineOutletComponent)
        {
            // Don’t exit early if unanchored; we need to check for Rotor
            if (!TryComp<TransformComponent>(entity, out var entityXform) || !entityXform.GridUid.HasValue)
                return;

            var mapCoords = _transform.GetMapCoordinates(entity);
            if (!_map.TryFindGridAt(mapCoords, out var gridUid, out var grid))
                return;

            var entityPos = grid.WorldToTile(mapCoords.Position);
            var directions = new[] { entityXform.LocalRotation, new Angle(entityXform.LocalRotation.Theta + Math.PI) };

            foreach (var dir in directions)
            {
                var offset = dir.GetCardinalDir().ToIntVec();
                var neighborPos = entityPos + offset;
                foreach (var neighbor in grid.GetAnchoredEntities(neighborPos))
                {
                    if (TryComp<TurbineRotorComponent>(neighbor, out var foundRotor) &&
                        TryComp<TransformComponent>(neighbor, out var neighborXform) &&
                        neighborXform.Anchored)
                    {
                        rotorEnt = neighbor;
                        rotor = foundRotor;
                        break;
                    }
                }
                if (rotorEnt != null)
                    break;
            }
        }

        if (rotorEnt == null || rotor == null)
            return;

        // Validate Rotor anchoring
        var rotorXform = Transform(rotorEnt.Value);
        if (!rotorXform.Anchored || !rotorXform.GridUid.HasValue)
        {
            rotor.AllPartsConnected = false;
            rotor.MaxAllowedRpm = 0;
            rotor.MaxAllowedTemperature = 0;
            if (TryComp<PowerSupplierComponent>(rotorEnt.Value, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
            return;
        }

        var rotorMapCoords = _transform.GetMapCoordinates(rotorEnt.Value);
        if (!_map.TryFindGridAt(rotorMapCoords, out var foundGridUid, out var gridComp))
        {
            rotor.AllPartsConnected = false;
            rotor.MaxAllowedRpm = 0;
            rotor.MaxAllowedTemperature = 0;
            if (TryComp<PowerSupplierComponent>(rotorEnt.Value, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
            return;
        }

        // Calculate positions
        var rotation = rotorXform.LocalRotation;
        var rotorPos = gridComp.WorldToTile(rotorMapCoords.Position);
        var compressorPos = rotorPos - rotation.GetCardinalDir().ToIntVec(); // Compressor at 180°
        var outletPos = rotorPos + rotation.GetCardinalDir().ToIntVec();     // Outlet forward

        // Scan Compressor
        EntityUid? compressorEnt = null;
        foreach (var ent in gridComp.GetAnchoredEntities(compressorPos))
        {
            if (TryComp<TurbineCompressorComponent>(ent, out _) &&
                TryComp<TransformComponent>(ent, out var xform) &&
                xform.Anchored && // Explicit anchoring check
                xform.LocalRotation.EqualsApprox(rotation))
            {
                compressorEnt = ent;
                break;
            }
        }

        // Scan Outlet
        EntityUid? outletEnt = null;
        foreach (var ent in gridComp.GetAnchoredEntities(outletPos))
        {
            if (TryComp<TurbineOutletComponent>(ent, out _) &&
                TryComp<TransformComponent>(ent, out var xform) &&
                xform.Anchored && // Explicit anchoring check
                xform.LocalRotation.EqualsApprox(rotation))
            {
                outletEnt = ent;
                break;
            }
        }

        // Update Rotor
        rotor.AllPartsConnected = compressorEnt.HasValue && outletEnt.HasValue;
        if (rotor.AllPartsConnected)
        {
            rotor.MaxAllowedRpm = rotor.MaxRpm;
            rotor.MaxAllowedTemperature = rotor.MaxTemperature;
        }
        else
        {
            rotor.MaxAllowedRpm = 0;
            rotor.MaxAllowedTemperature = 0;
            if (TryComp<PowerSupplierComponent>(rotorEnt.Value, out var powerSupplier))
            {
                powerSupplier.Enabled = false;
                powerSupplier.MaxSupply = 0f;
            }
        }
    }

    private float TransferGases(GasMixture inputMix, GasMixture outputMix, float workToRemove, float intakeSize = 1f)
    {
        // Ensure non-negative pressures
        float PressureMax(float pressure) => MathF.Max(pressure, MinimumTurbinePressure);

        // Get initial output pressure
        float outputPressure = PressureMax(outputMix.Pressure);

        // Pump gas from input to output
        float inputPressure = inputMix.Pressure * intakeSize;
        var transferredGas = inputMix.Remove(inputPressure * inputMix.Volume / Atmospherics.R);
        if (transferredGas.TotalMoles <= 0f)
            return 0f;

        _atmos.Merge(outputMix, transferredGas);

        float workDone = (float)(Math.Floor(transferredGas.TotalMoles) * Atmospherics.R * 
                         transferredGas.Temperature * Math.Log((outputMix.Volume * PressureMax(outputMix.Pressure)) /
                         (transferredGas.Volume * PressureMax(transferredGas.Pressure))) *
                         WorkConversionMultiplier);

        // Subtract work to remove (e.g., Compressor work for Rotor)
        if (workToRemove > 0f)
            workDone = MathF.Max(workDone - workToRemove, 0f);

        // Compute temperature-based work cap
        
        float outputHeatCapacity = _atmos.GetHeatCapacity(outputMix, true);
        if (outputHeatCapacity <= 0f)
            return 0f;

        workDone = MathF.Min(workDone, (outputHeatCapacity * outputMix.Temperature - outputHeatCapacity * Atmospherics.TCMB) / HeatConversionMultiplier);

        // Update output temperature: T₂ = (T₂ * C + work * HeatConversionMultiplier) / C
        outputMix.Temperature = MathF.Max((outputMix.Temperature * outputHeatCapacity + workDone * HeatConversionMultiplier) / outputHeatCapacity, Atmospherics.TCMB);

        return workDone;
    }

    private MapCoordinates GetInputTile(EntityUid compressor)
    {
        var xform = Transform(compressor);
        var dir = xform.LocalRotation;
        var oppositeDir = new Angle(dir.Theta + Math.PI);
        return _transform.GetMapCoordinates(compressor).Offset(oppositeDir.ToVec());
    }

    private MapCoordinates GetOutputTile(EntityUid outlet)
    {
        var xform = Transform(outlet);
        var dir = xform.LocalRotation;
        return _transform.GetMapCoordinates(outlet).Offset(dir.ToVec());
    }
}