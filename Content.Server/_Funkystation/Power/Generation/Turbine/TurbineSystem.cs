using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Power.Components;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Content.Shared._Funkystation.Power.Generation.Turbine;
using Content.Server.Atmos.Piping.Components;

namespace Content.Server.Turbine;

public sealed class TurbineSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;

    // SS13 constants
    private const float RpmConversion = 15f;
    private const float EnergyRectificationMultiplier = 10.25f;
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

    private void OnAtmosUpdate(EntityUid uid, TurbineRotorComponent rotor, ref AtmosDeviceUpdateEvent args)
    {
        if (!TryComp<PowerSupplierComponent>(uid, out var powerSupplier))
            return;

        if (!rotor.AllPartsConnected || !EntityManager.EntityExists(rotor.CompressorUid) || !EntityManager.EntityExists(rotor.OutletUid))
        {
            rotor.AllPartsConnected = false;
            rotor.CompressorUid = null;
            rotor.OutletUid = null;
            powerSupplier.Enabled = false;
            powerSupplier.MaxSupply = 0f;
            return;
        }

        // Get input tile mixture
        var inputTile = GetTile(rotor.CompressorUid!.Value);
        if (!_map.TryFindGridAt(inputTile, out var gridUid, out var gridComp))
            return;
        var inputTilePos = gridComp.WorldToTile(inputTile.Position);
        var inputMix = _atmos.GetTileMixture(gridUid, null, inputTilePos, true);
        if (inputMix is not { TotalMoles: > 0 })
            return;

        if (!TryComp<TurbineCompressorComponent>(rotor.CompressorUid!.Value, out var compressorComp))
            return;

        if (!TryComp<TurbineOutletComponent>(rotor.OutletUid!.Value, out var outletComp))
            return;

        var compressorMix = compressorComp.CompressorGasMix;
        var rotorMix = rotor.RotorGasMix;
        var outletMix = outletComp.OutletGasMix;

        var temperature = CompressGases(inputMix, compressorMix, rotor);
        var compressorPressure = compressorMix.Pressure;
        var rotorWork = TransferGases(compressorMix, rotorMix, rotor.CompressorWork);
        var turbineWork = TransferGases(rotorMix, outletMix, Math.Abs(rotorWork));

        // Get output tile mixture
        var outputTile = GetTile(rotor.OutletUid!.Value);
        if (!_map.TryFindGridAt(outputTile, out gridUid, out gridComp))
            return;
        var outputTilePos = gridComp.WorldToTile(outputTile.Position);
        var outputTileMix = _atmos.GetTileMixture(gridUid, null, outputTilePos, true);
        if (outputTileMix is null)
            return;

        // Expel gas from outlet
        var gasExpelled = ExpelGases(outletMix, outputTileMix, outletMix.Pressure);
        if (!gasExpelled)
        {
            rotor.Rpm = 0f;
            rotor.ProducedEnergy = 0f;
            return;
        }

        // Calculate final work and RPM
        var ejectedPressure = Math.Max(outletMix.Pressure, MinimumTurbinePressure);
        compressorPressure = Math.Max(compressorPressure, MinimumTurbinePressure);
        var workDone = outletMix.TotalMoles * Atmospherics.R * outletMix.Temperature * MathF.Log(Math.Max(compressorPressure / ejectedPressure, 1f)) * WorkConversionMultiplier;
        workDone = Math.Max(workDone - rotor.CompressorWork * CompressorStatorInteractionMultiplier - turbineWork, 0f);

        // Calculate RPM
        var rpm = workDone / RpmConversion;
        rotor.Rpm = MathF.Floor(Math.Min(rpm, rotor.MaxRpm));

        Console.WriteLine("compressorPressure: " + compressorPressure);
        Console.WriteLine("ejectedPressure: " + ejectedPressure);
        Console.WriteLine("outletMix.TotalMoles: " + outletMix.TotalMoles);
        Console.WriteLine("outletMix.Temperature: " + outletMix.Temperature);
        Console.WriteLine("rotor.CompressorWork: " + rotor.CompressorWork);
        Console.WriteLine("rotorWork: " + rotorWork);
        Console.WriteLine("turbineWork: " + turbineWork);
        Console.WriteLine("workDone: " + workDone);

        // Update power supply
        Console.WriteLine("rpm is " + rotor.Rpm);
        powerSupplier.Enabled = rotor.Rpm > 0;
        powerSupplier.MaxSupply = rotor.Rpm * EnergyRectificationMultiplier; // Convert RPM to power
        Console.WriteLine("max supply is " + powerSupplier.MaxSupply);

        // Merge to output
        _gasOverlaySystem.UpdateSessions();
    }

    private void UpdateConnections(EntityUid entity, Component comp)
    {
        // Find rotor
        EntityUid? rotorEnt = null;
        TurbineRotorComponent? rotor = null;
        Vector2i? rotorTilePos = null; // Store rotor's tile position

        if (comp is TurbineRotorComponent rotorComp)
        {
            rotorEnt = entity;
            rotor = rotorComp;
        }
        else if ((comp is TurbineCompressorComponent || comp is TurbineOutletComponent) &&
                TryComp<TransformComponent>(entity, out var entityXform) && entityXform.Anchored)
        {
            var mapCoords = _transform.GetMapCoordinates(entity);
            if (!_map.TryFindGridAt(mapCoords, out _, out var grid))
                return;

            var entityPos = grid.WorldToTile(mapCoords.Position);
            var offset = (comp is TurbineCompressorComponent ? entityXform.LocalRotation : new Angle(entityXform.LocalRotation.Theta + Math.PI)).GetCardinalDir().ToIntVec();
            var expectedRotorPos = entityPos + offset;

            foreach (var neighbor in grid.GetAnchoredEntities(expectedRotorPos))
            {
                if (TryComp<TurbineRotorComponent>(neighbor, out var foundRotor) &&
                    Transform(neighbor).Anchored)
                {
                    rotorEnt = neighbor;
                    rotor = foundRotor;
                    rotorTilePos = expectedRotorPos; // Store the position
                    break;
                }
            }
        }

        if (rotorEnt == null || rotor == null)
            return;

        // Validate rotor and grid
        var rotorXform = Transform(rotorEnt.Value);
        if (!rotorXform.Anchored || !_map.TryFindGridAt(_transform.GetMapCoordinates(rotorEnt.Value), out _, out var gridComp))
        {
            ResetRotor(rotor);
            return;
        }

        // Get rotor's tile position if not already set
        rotorTilePos ??= gridComp.WorldToTile(rotorXform.WorldPosition);

        // Calculate compressor and outlet positions
        var rotation = rotorXform.LocalRotation;
        var compressorPos = rotorTilePos.Value - rotation.GetCardinalDir().ToIntVec();
        var outletPos = rotorTilePos.Value + rotation.GetCardinalDir().ToIntVec();

        // Scan compressor
        EntityUid? compressorEnt = null;
        foreach (var ent in gridComp.GetAnchoredEntities(compressorPos))
        {
            if (TryComp<TurbineCompressorComponent>(ent, out _) &&
                Transform(ent).Anchored &&
                Transform(ent).LocalRotation.EqualsApprox(rotation))
            {
                compressorEnt = ent;
                break;
            }
        }

        // Scan outlet
        EntityUid? outletEnt = null;
        foreach (var ent in gridComp.GetAnchoredEntities(outletPos))
        {
            if (TryComp<TurbineOutletComponent>(ent, out _) &&
                Transform(ent).Anchored &&
                Transform(ent).LocalRotation.EqualsApprox(rotation))
            {
                outletEnt = ent;
                break;
            }
        }

        // Update rotor
        rotor.AllPartsConnected = compressorEnt.HasValue && outletEnt.HasValue;
        if (rotor.AllPartsConnected)
        {
            rotor.CompressorUid = compressorEnt;
            rotor.OutletUid = outletEnt;
        }
        else
        {
            ResetRotor(rotor);
        }
    }

    private void ResetRotor(TurbineRotorComponent rotor)
    {
        rotor.AllPartsConnected = false;
        rotor.CompressorUid = null;
        rotor.OutletUid = null;
        if (TryComp<PowerSupplierComponent>(rotor.Owner, out var powerSupplier))
        {
            powerSupplier.Enabled = false;
            powerSupplier.MaxSupply = 0f;
        }
    }

    private float TransferGases(GasMixture inputMix, GasMixture outputMix, float workToRemove, float intakeSize = 1f)
    {
        // Ensure output pressure is at least minimum to avoid division issues
        var outputPressure = Math.Max(outputMix.Pressure, MinimumTurbinePressure);

        // Transfer gas: Remove intakeSize fraction from input and pump to output
        var initialMoles = outputMix.TotalMoles;
        var targetPressure = inputMix.Pressure * intakeSize;
        var transferred = _atmos.PumpGasTo(inputMix, outputMix, targetPressure);
        if (!transferred || outputMix.TotalMoles <= 0)
            return 0f;

        // Compute transferred moles
        var transferredMoles = Math.Max(outputMix.TotalMoles - initialMoles, 0f);
        if (transferredMoles <= 0f)
            return 0f;

        var transferredTemperature = inputMix.Temperature;

        // Calculate pressure ratio using outputMix volume and post-transfer pressure
        var postTransferPressure = Math.Max(outputMix.Pressure, MinimumTurbinePressure);
        var pressureRatio = postTransferPressure / outputPressure;

        // Compute work done: moles * R * T * log(P_transferred / P_output) * multiplier
        var workDone = transferredMoles * Atmospherics.R * transferredTemperature * MathF.Log(Math.Max(pressureRatio, 1f)) * WorkConversionMultiplier;

        // Subtract work to remove
        workDone = Math.Max(0f, workDone - workToRemove);

        // Compute temperature-limited work
        var outputHeatCapacity = _atmos.GetHeatCapacity(outputMix, true);
        if (outputHeatCapacity <= 0)
            return 0f;

        // Limit work by available heat
        workDone = Math.Min(workDone,
            (outputHeatCapacity * outputMix.Temperature - outputHeatCapacity * Atmospherics.TCMB) / HeatConversionMultiplier);

        // Update output temperature
        outputMix.Temperature = Math.Max((outputMix.Temperature * outputHeatCapacity + workDone * HeatConversionMultiplier) / outputHeatCapacity, 0f);
        return workDone;
    }

    private MapCoordinates GetTile(EntityUid uid)
    {
        var xform = Transform(uid);
        return _transform.GetMapCoordinates(uid);
    }

    private float CompressGases(GasMixture inputMix, GasMixture compressorMix, TurbineRotorComponent rotor)
    {
        rotor.CompressorWork = TransferGases(inputMix, compressorMix, 0f, rotor.CompressorIntakeRegulator);
        rotor.CompressorPressure = Math.Max(compressorMix.Pressure, 0.01f);

        return inputMix.Temperature;
    }

    private bool ExpelGases(GasMixture inputMix, GasMixture outputTileMix, float pressure)
    {
        return _atmos.PumpGasTo(inputMix, outputTileMix, pressure);
    }
}