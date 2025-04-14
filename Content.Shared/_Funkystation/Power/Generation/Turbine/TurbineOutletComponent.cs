using Robust.Shared.Serialization;
using Content.Shared.Atmos;

namespace Content.Shared._Funkystation.Power.Generation.Turbine;

[RegisterComponent]
public sealed partial class TurbineOutletComponent : Component
{
    [DataField]
    public bool Active { get; set; }

    [ViewVariables]
    public GasMixture OutletGasMix = new() { Volume = 6000f };
}