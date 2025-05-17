using Content.Client.Atmos.Components;
using Content.Shared.Atmos.Unary.Visuals;
using Robust.Client.GameObjects;

namespace Content.Client.Atmos.EntitySystems;

public sealed class GasCanisterColorVisualizerSystem : VisualizerSystem<GasCanisterColorVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, GasCanisterColorVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (!TryComp<SpriteComponent>(uid, out var sprite))
            return;

        if (sprite.LayerMapTryGet(GasCanisterVisualLayers.Canister, out var canisterLayer) &&
            AppearanceSystem.TryGetData<Color>(uid, GasCanisterColorVisuals.PrimaryColor, out var primaryColor, args.Component))
        {
            sprite[canisterLayer].Color = primaryColor.WithAlpha(sprite[canisterLayer].Color.A);
        }

        if (sprite.LayerMapTryGet(GasCanisterVisualLayers.DoubleStripe, out var stripeLayer))
        {
            if (AppearanceSystem.TryGetData<Color>(uid, GasCanisterColorVisuals.SecondaryColor, out var secondaryColor, args.Component))
            {
                sprite[stripeLayer].Color = secondaryColor.WithAlpha(sprite[stripeLayer].Color.A);
            }

            if (AppearanceSystem.TryGetData<bool>(uid, GasCanisterColorVisuals.SecondaryEnabled, out var secondaryEnabled, args.Component))
            {
                sprite[stripeLayer].Visible = secondaryEnabled;
            }
        }
    }
}

public enum GasCanisterVisualLayers : byte
{
    Canister,
    DoubleStripe
}