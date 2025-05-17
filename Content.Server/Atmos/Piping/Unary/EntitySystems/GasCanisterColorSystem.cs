using Content.Server.Atmos.Piping.Unary.Components;
using Content.Shared.Atmos.Unary.Visuals;
using Robust.Server.GameObjects;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems;

public sealed class GasCanisterColorSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasCanisterColorComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GasCanisterColorComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, GasCanisterColorComponent component, ComponentStartup args)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        _appearance.SetData(uid, GasCanisterColorVisuals.PrimaryColor, component.PrimaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryColor, component.SecondaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.TertiaryColor, component.TertiaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryEnabled, component.SecondaryEnabled, appearance);
    }

    private void OnShutdown(EntityUid uid, GasCanisterColorComponent component, ComponentShutdown args)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        _appearance.SetData(uid, GasCanisterColorVisuals.PrimaryColor, Color.White, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryColor, Color.White, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.TertiaryColor, Color.White, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryEnabled, true, appearance);
    }

    public void SetColors(EntityUid uid, GasCanisterColorComponent component, Color primaryColor, Color secondaryColor, Color tertiaryColor, bool secondaryEnabled)
    {
        if (component.PrimaryColor == primaryColor &&
            component.SecondaryColor == secondaryColor &&
            component.TertiaryColor == tertiaryColor &&
            component.SecondaryEnabled == secondaryEnabled)
            return;

        component.PrimaryColor = primaryColor;
        component.SecondaryColor = secondaryColor;
        component.TertiaryColor = tertiaryColor;
        component.SecondaryEnabled = secondaryEnabled;

        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        _appearance.SetData(uid, GasCanisterColorVisuals.PrimaryColor, primaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryColor, secondaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.TertiaryColor, tertiaryColor, appearance);
        _appearance.SetData(uid, GasCanisterColorVisuals.SecondaryEnabled, secondaryEnabled, appearance);
    }
}