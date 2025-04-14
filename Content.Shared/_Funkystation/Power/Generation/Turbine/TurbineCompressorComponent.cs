using Robust.Shared.Serialization;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Power.Generation.Turbine;

[RegisterComponent]
public sealed partial class TurbineCompressorComponent : Component
{
    [DataField]
    public bool Active { get; set; }

    [DataField]
    public float IntakeRegulator { get; set; } = 0.5f;

    [DataField]
    public float CompressorWork { get; set; }

    [DataField]
    public float CompressorPressure { get; set; } = 0.01f;

    [ViewVariables]
    public GasMixture CompressorGasMix = new() { Volume = 1000f };
}