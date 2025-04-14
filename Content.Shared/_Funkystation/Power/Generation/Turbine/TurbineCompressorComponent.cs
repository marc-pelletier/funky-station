using Robust.Shared.Serialization;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Power.Generation.Turbine;

[RegisterComponent]
public sealed partial class TurbineCompressorComponent : Component
{
    [DataField]
    public bool Active { get; set; }

    [ViewVariables]
    public GasMixture CompressorGasMix = new() { Volume = 1000f };
}