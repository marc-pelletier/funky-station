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
    public bool AllPartsConnected { get; set; }

    [DataField]
    public EntityUid? CompressorUid { get; set; }

    [DataField]
    public EntityUid? OutletUid { get; set; }

    [ViewVariables]
    public GasMixture RotorGasMix = new() { Volume = 3000f };

    [DataField]
    public float CompressorWork { get; set; }

    [DataField]
    public float CompressorPressure { get; set; }

    [DataField]
    public float CompressorIntakeRegulator { get; set; } = 0.5f;
}