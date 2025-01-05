using Content.Shared.Atmos;

namespace Content.Server.Atmos.Piping.Unary.Components
{
    [RegisterComponent]
    public sealed partial class GasBluespaceSenderComponent : Component
    {
        [DataField("inlet")]
        public string InletName = "pipe";

        /// <summary>
        /// If true, heat is exclusively exchanged with the local atmosphere instead of the inlet pipe air
        /// </summary>
        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public bool Atmospheric = false;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("gasMixture")]
        public GasMixture GasStorage { get; set; } = new();
    }
}
