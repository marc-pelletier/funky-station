using Content.Shared.SprayPainter;
using Content.Shared.SprayPainter.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.SprayPainter.UI;

public sealed class SprayPainterBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SprayPainterWindow? _window;

    public SprayPainterBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<SprayPainterWindow>();

        _window.OnSpritePicked = OnSpritePicked;
        _window.OnColorPicked = OnColorPicked;
        _window.OnPrimaryColorPicked = OnPrimaryColorPicked;
        _window.OnSecondaryColorPicked = OnSecondaryColorPicked;
        _window.OnTertiaryColorPicked = OnTertiaryColorPicked;
        _window.OnSecondaryEnabledChanged = OnSecondaryEnabledChanged;
        _window.OnTertiaryEnabledChanged = OnTertiaryEnabledChanged;

        if (EntMan.TryGetComponent(Owner, out SprayPainterComponent? comp))
        {
            _window.Populate(
                EntMan.System<SprayPainterSystem>().Entries,
                comp.Index,
                comp.PickedColor,
                comp.ColorPalette,
                comp.PrimaryColor,
                comp.SecondaryColor,
                comp.TertiaryColor,
                comp.SecondaryEnabled,
                comp.TertiaryEnabled
            );
        }
    }

    private void OnSpritePicked(ItemList.ItemListSelectedEventArgs args)
    {
        SendMessage(new SprayPainterSpritePickedMessage(args.ItemIndex));
    }

    private void OnColorPicked(ItemList.ItemListSelectedEventArgs args)
    {
        var key = _window?.IndexToColorKey(args.ItemIndex);
        SendMessage(new SprayPainterColorPickedMessage(key));
    }

    private void OnPrimaryColorPicked(ItemList.ItemListSelectedEventArgs args)
    {
        var key = _window?.IndexToColorKey(args.ItemIndex);
        SendMessage(new SprayPainterPrimaryColorPickedMessage(key));
    }

    private void OnSecondaryColorPicked(ItemList.ItemListSelectedEventArgs args)
    {
        var key = _window?.IndexToColorKey(args.ItemIndex);
        SendMessage(new SprayPainterSecondaryColorPickedMessage(key));
    }

    private void OnTertiaryColorPicked(ItemList.ItemListSelectedEventArgs args)
    {
        var key = _window?.IndexToColorKey(args.ItemIndex);
        SendMessage(new SprayPainterTertiaryColorPickedMessage(key));
    }

    private void OnSecondaryEnabledChanged(bool enabled)
    {
        SendMessage(new SprayPainterSecondaryEnabledChangedMessage(enabled));
    }

    private void OnTertiaryEnabledChanged(bool enabled)
    {
        SendMessage(new SprayPainterTertiaryEnabledChangedMessage(enabled));
    }
}