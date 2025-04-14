using Robust.Shared.Serialization;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Power.Generation.Turbine;

[RegisterComponent]
public sealed partial class TurbineRotorComponent : Component
{
    [DataField]
    public bool Active { get; set; }

    [DataField]
    public float Efficiency { get; set; } = 0.25f;

    [DataField]
    public float MaxRpm { get; set; } = 35000f;

    [DataField]
    public float MaxTemperature { get; set; } = 50000f;

    [DataField]
    public float Rpm { get; set; }

    [DataField]
    public float ProducedEnergy { get; set; }

    [DataField]
    public float Damage { get; set; }

    [DataField]
    public float DamageArchived { get; set; }

    [DataField]
    public float MaxAllowedRpm { get; set; }

    [DataField]
    public float MaxAllowedTemperature { get; set; }

    [DataField]
    public bool AllPartsConnected { get; set; }

    [ViewVariables]
    public GasMixture RotorGasMix = new() { Volume = 3000f };
}