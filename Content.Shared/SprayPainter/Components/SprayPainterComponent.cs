using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.SprayPainter.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SprayPainterComponent : Component
{
    [DataField]
    public SoundSpecifier SpraySound = new SoundPathSpecifier("/Audio/Effects/spray2.ogg");

    [DataField]
    public TimeSpan AirlockSprayTime = TimeSpan.FromSeconds(3);

    [DataField]
    public TimeSpan PipeSprayTime = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Pipe color chosen to spray with.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PickedColor;

    /// <summary>
    /// Pipe colors that can be selected.
    /// </summary>
    [DataField]
    public Dictionary<string, Color> ColorPalette = new();

    /// <summary>
    /// Airlock style index selected.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Index;

    /// <summary>
    /// Primary color key selected for custom painting (always enabled).
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PrimaryColor;

    /// <summary>
    /// Secondary color key selected for custom painting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? SecondaryColor;

    /// <summary>
    /// Tertiary color key selected for custom painting.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? TertiaryColor;

    /// <summary>
    /// Whether the secondary color is enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SecondaryEnabled = true;

    /// <summary>
    /// Whether the tertiary color is enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TertiaryEnabled = true;
}