using Content.Server.Atmos.EntitySystems;
using Content.Server.EntityEffects.Effects;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Assmos - /tg/ gases
///     Consumes a tiny amount of tritium to convert CO2 and oxygen to pluoxium.
/// </summary>
[UsedImplicitly]
public sealed partial class PluoxiumTritiumProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        float initO2 = mixture.GetMoles(Gas.Oxygen);
        float initCO2 = mixture.GetMoles(Gas.CarbonDioxide);
        float initTrit = mixture.GetMoles(Gas.Tritium);

        float[] efficiencies = {5f, initCO2, initO2 * 2f, initTrit * 100f};
        Array.Sort(efficiencies);
        float producedAmount = efficiencies[0];

        float co2Removed = producedAmount;
        float oxyRemoved = producedAmount * 0.5f;
        float tritRemoved = producedAmount * 0.01f;

        if (producedAmount <= 0 ||
            co2Removed > initCO2 ||
            oxyRemoved > initO2 ||
            tritRemoved > initTrit)
            return ReactionResult.NoReaction;

        float pluoxProduced = producedAmount;
        float hydroProduced = producedAmount * 0.01f;

        mixture.AdjustMoles(Gas.CarbonDioxide, -co2Removed);
        mixture.AdjustMoles(Gas.Oxygen, -oxyRemoved);
        mixture.AdjustMoles(Gas.Tritium, -tritRemoved);
        mixture.AdjustMoles(Gas.Pluoxium, pluoxProduced);
        mixture.AdjustMoles(Gas.Hydrogen, hydroProduced);

        float energyReleased = producedAmount * Atmospherics.PluoxiumProductionEnergy;
        float heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}