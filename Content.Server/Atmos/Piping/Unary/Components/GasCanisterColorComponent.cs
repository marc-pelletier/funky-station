using Content.Server.Atmos.Piping.Unary.EntitySystems;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Piping.Unary.Components;

[RegisterComponent]
public sealed partial class GasCanisterColorComponent : Component
{
    [DataField]
    public Color PrimaryColor { get; set; } = Color.FromHex("#B05EB4");

    [DataField]
    public Color SecondaryColor { get; set; } = Color.FromHex("#B05EB4");

    [DataField]
    public Color TertiaryColor { get; set; } = Color.FromHex("#B05EB4");

    [DataField]
    public bool SecondaryEnabled { get; set; } = true;

    [ViewVariables(VVAccess.ReadWrite), UsedImplicitly]
    public Color PrimaryColorVV
    {
        get => PrimaryColor;
        set => IoCManager.Resolve<IEntityManager>().System<GasCanisterColorSystem>().SetColors(Owner, this, value, SecondaryColor, TertiaryColor, SecondaryEnabled);
    }

    [ViewVariables(VVAccess.ReadWrite), UsedImplicitly]
    public Color SecondaryColorVV
    {
        get => SecondaryColor;
        set => IoCManager.Resolve<IEntityManager>().System<GasCanisterColorSystem>().SetColors(Owner, this, PrimaryColor, value, TertiaryColor, SecondaryEnabled);
    }

    [ViewVariables(VVAccess.ReadWrite), UsedImplicitly]
    public Color TertiaryColorVV
    {
        get => TertiaryColor;
        set => IoCManager.Resolve<IEntityManager>().System<GasCanisterColorSystem>().SetColors(Owner, this, PrimaryColor, SecondaryColor, value, SecondaryEnabled);
    }

    [ViewVariables(VVAccess.ReadWrite), UsedImplicitly]
    public bool SecondaryEnabledVV
    {
        get => SecondaryEnabled;
        set => IoCManager.Resolve<IEntityManager>().System<GasCanisterColorSystem>().SetColors(Owner, this, PrimaryColor, SecondaryColor, TertiaryColor, value);
    }
}

[ByRefEvent]
public record struct GasCanisterColorChangedEvent(Color color)
{
    public Color Color = color;
}
