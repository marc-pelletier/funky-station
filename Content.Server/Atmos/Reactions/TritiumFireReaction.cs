using Content.Server.Atmos.EntitySystems;
using Content.Server.Radiation.Systems; // Assmos
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;
using Robust.Shared.Map; // Assmos
using Robust.Shared.Map.Components; // Assmos
using Robust.Shared.IoC; // Assmos
using Content.Shared.Radiation.Components; // Assmos
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Atmos.Reactions
{
    [UsedImplicitly]
    [DataDefinition]
    public sealed partial class TritiumFireReaction : IGasReactionEffect
    {
        private IEntityManager EntityManager => IoCManager.Resolve<IEntityManager>();
        private SharedMapSystem MapSystem => EntityManager.System<SharedMapSystem>();
        private IRobustRandom Random => IoCManager.Resolve<IRobustRandom>();
        private IGameTiming GameTiming => IoCManager.Resolve<IGameTiming>();
        
        private const float TritiumRadiationMinimumMoles = 0.1f;
        private const float TritiumRadiationReleaseThreshold = Atmospherics.FireHydrogenEnergyReleased;
        private const float TritiumRadiationRangeDivisor = 5f;
        private const float TritiumRadiationThreshold = 0.3f;
        private const float AtmosRadiationVolumeExp = 3f;
        private const float GasReactionMaximumRadiationPulseRange = 1f;
        private const float CellVolume = 2500f;
        public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
        {
            var energyReleased = 0f;
            var oldHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
            var temperature = mixture.Temperature;
            var location = holder as TileAtmosphere;
            mixture.ReactionResults[(byte)GasReaction.Fire] = 0f;
            var burnedFuel = 0f;
            var initialTrit = mixture.GetMoles(Gas.Tritium);

            if (mixture.GetMoles(Gas.Oxygen) < initialTrit ||
                Atmospherics.MinimumTritiumOxyburnEnergy > (temperature * oldHeatCapacity * heatScale))
            {
                burnedFuel = mixture.GetMoles(Gas.Oxygen) / Atmospherics.TritiumBurnOxyFactor;
                if (burnedFuel > initialTrit)
                    burnedFuel = initialTrit;

                mixture.AdjustMoles(Gas.Tritium, -burnedFuel);
            }
            else
            {
                burnedFuel = initialTrit;
                mixture.SetMoles(Gas.Tritium, mixture.GetMoles(Gas.Tritium ) * (1 - 1 / Atmospherics.TritiumBurnTritFactor));
                mixture.AdjustMoles(Gas.Oxygen, -mixture.GetMoles(Gas.Tritium));
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel * (Atmospherics.TritiumBurnTritFactor - 1));
            }

            if (burnedFuel > 0f)
            {
                energyReleased += (Atmospherics.FireHydrogenEnergyReleased * burnedFuel);

                // TODO ATMOS Radiation pulse here!
                if (holder != null)
                {
                    EntityUid targetEntity;

                    if (holder is TileAtmosphere tile)
                    {
                        // For tiles, use the grid entity
                        var tileRef = atmosphereSystem.GetTileRef(tile);
                        targetEntity = tileRef.GridUid;
                    }
                    else
                    {
                        // For other holders (pipes, canisters), try to get an associated entity
                        // This assumes the holder might have an EntityUid property or similar
                        // Adjust based on actual IGasMixtureHolder implementations
                        targetEntity = holder switch
                        {
                            IComponent component => component.Owner, // If holder is a component
                            _ => EntityUid.Invalid // Default to invalid if unknown
                        };

                        if (!targetEntity.IsValid())
                            goto skipRadiation; // Skip if we can’t find a valid entity
                    }

                    float volumeScaling = MathF.Pow(mixture.Volume / CellVolume, AtmosRadiationVolumeExp);
                    if (energyReleased > TritiumRadiationReleaseThreshold * volumeScaling / 10f) 
                    {
                        float maxRange = MathF.Min(MathF.Sqrt(burnedFuel) / TritiumRadiationRangeDivisor, GasReactionMaximumRadiationPulseRange);
                        float radsPerSecond = burnedFuel * 0.2f; // Adjust as needed
                        float slope = radsPerSecond / maxRange;
                        Console.WriteLine("in rad apply");

                        RadiationSourceComponent radComp;
                        if (EntityManager.TryGetComponent<RadiationSourceComponent>(targetEntity, out var existingRadComp))
                        {
                            Console.WriteLine("in existingrad change");
                            radComp = existingRadComp;
                            radComp.Enabled = true;
                            radComp.Intensity = Math.Max(radComp.Intensity, radsPerSecond); // Update intensity dynamically
                            radComp.Slope = Math.Min(radComp.Slope, slope);
                        }
                        else
                        {
                            Console.WriteLine("in apply new rad");
                            radComp = EntityManager.AddComponent<RadiationSourceComponent>(targetEntity);
                            radComp.Enabled = true;
                            radComp.Intensity = radsPerSecond;
                            radComp.Slope = slope;
                        }

                        // Ensure or update LifetimeComponent every tick
                        if (EntityManager.TryGetComponent<LifetimeComponent>(targetEntity, out var radLifeTimeComp))
                        {
                            Console.WriteLine("in existing lifetimecomp");
                            radLifeTimeComp.DeletionTime = GameTiming.CurTime + TimeSpan.FromSeconds(5f); // Refresh timer
                        }
                        else
                        {
                            Console.WriteLine("in apply new lifetimecomp");
                            var lifeTimeComp = EntityManager.EnsureComponent<LifetimeComponent>(targetEntity);
                            lifeTimeComp.DeletionTime = GameTiming.CurTime + TimeSpan.FromSeconds(5f);
                        }
                    }

                skipRadiation:;
                }

                // Conservation of mass is important.
                mixture.AdjustMoles(Gas.WaterVapor, burnedFuel);

                mixture.ReactionResults[(byte)GasReaction.Fire] += burnedFuel;
            }

            energyReleased /= heatScale; // adjust energy to make sure speedup doesn't cause mega temperature rise
            if (energyReleased > 0)
            {
                var newHeatCapacity = atmosphereSystem.GetHeatCapacity(mixture, true);
                if (newHeatCapacity > Atmospherics.MinimumHeatCapacity)
                    mixture.Temperature = ((temperature * oldHeatCapacity + energyReleased) / newHeatCapacity);
            }

            if (location != null)
            {
                temperature = mixture.Temperature;
                if (temperature > Atmospherics.FireMinimumTemperatureToExist)
                {
                    atmosphereSystem.HotspotExpose(location, temperature, mixture.Volume);
                }
            }

            return mixture.ReactionResults[(byte)GasReaction.Fire] != 0 ? ReactionResult.Reacting : ReactionResult.NoReaction;
        }
    }
}


namespace Content.Shared.Radiation.Components
{
    [RegisterComponent]
    public sealed partial class LifetimeComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        public float Lifetime { get; set; }

        [ViewVariables(VVAccess.ReadWrite)]
        public TimeSpan DeletionTime { get; set; }
    }
}

namespace Content.Server.Radiation.Systems
{
    public sealed class LifetimeSystem : EntitySystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            // Iterate over all LifetimeComponent instances
            foreach (var lifetime in _entityManager.EntityQuery<LifetimeComponent>())
            {
                var uid = lifetime.Owner; // Get the EntityUid from the component
                if (_timing.CurTime >= lifetime.DeletionTime)
                {
                    _entityManager.RemoveComponent<RadiationSourceComponent>(uid);
                    _entityManager.RemoveComponent<LifetimeComponent>(uid);
                }
            }
        }
    }
}