// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client._EE.Supermatter.Components;
using Content.Shared._EE.Supermatter.Components;
using Robust.Client.GameObjects;

namespace Content.Client._EE.Supermatter.Systems;

public sealed class SupermatterVisualizerSystem : VisualizerSystem<SupermatterVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, SupermatterVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        var crystalLayer = args.Sprite.LayerMapGet(SupermatterVisuals.Crystal);
        var psyLayer = args.Sprite.LayerMapGet(SupermatterVisuals.Psy);

        if (AppearanceSystem.TryGetData(uid, SupermatterVisuals.Crystal, out SupermatterCrystalState crystalState, args.Component) &&
            component.CrystalVisuals.TryGetValue(crystalState, out var crystalData))
        {
            args.Sprite.LayerSetState(crystalLayer, crystalData.State);
        }

        if (AppearanceSystem.TryGetData(uid, SupermatterVisuals.Psy, out float psyState, args.Component))
        {
            var color = new Color(1f, 1f, 1f, psyState);
            args.Sprite.LayerSetColor(psyLayer, color);
        }
    }
}
