// SPDX-FileCopyrightText: 2022 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.FixedPoint;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Client.Fluids;

public sealed class PuddleOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;
    private readonly PuddleDebugOverlaySystem _debugOverlaySystem;
    private readonly SharedTransformSystem _transformSystem;

    private readonly Color _heavyPuddle = new(0, 255, 255, 50);
    private readonly Color _mediumPuddle = new(0, 150, 255, 50);
    private readonly Color _lightPuddle = new(0, 50, 255, 50);

    private readonly Font _font;

    public override OverlaySpace Space => OverlaySpace.ScreenSpace | OverlaySpace.WorldSpace;

    public PuddleOverlay()
    {
        IoCManager.InjectDependencies(this);
        _debugOverlaySystem = _entitySystemManager.GetEntitySystem<PuddleDebugOverlaySystem>();
        var cache = IoCManager.Resolve<IResourceCache>();
        _font = new VectorFont(cache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 8);
        _transformSystem = _entityManager.System<SharedTransformSystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        switch (args.Space)
        {
            case OverlaySpace.ScreenSpace:
                DrawScreen(args);
                break;
            case OverlaySpace.WorldSpace:
                DrawWorld(args);
                break;
        }
    }

    private void DrawWorld(in OverlayDrawArgs args)
    {
        var drawHandle = args.WorldHandle;
        Box2 gridBounds;
        var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();

        foreach (var gridId in _debugOverlaySystem.TileData.Keys)
        {
            if (!_entityManager.TryGetComponent(gridId, out MapGridComponent? mapGrid))
                continue;

            var gridXform = xformQuery.GetComponent(gridId);
            var (_, _, worldMatrix, invWorldMatrix) = _transformSystem.GetWorldPositionRotationMatrixWithInv(gridXform, xformQuery);
            gridBounds = invWorldMatrix.TransformBox(args.WorldBounds).Enlarged(mapGrid.TileSize * 2);
            drawHandle.SetTransform(worldMatrix);

            foreach (var debugOverlayData in _debugOverlaySystem.GetData(gridId))
            {
                var centre = (debugOverlayData.Pos + Vector2Helpers.Half) * mapGrid.TileSize;

                // is the center of this tile visible
                if (!gridBounds.Contains(centre))
                    continue;

                var box = Box2.UnitCentered.Translated(centre);
                drawHandle.DrawRect(box, Color.Blue, false);
                drawHandle.DrawRect(box, ColorMap(debugOverlayData.CurrentVolume));
            }
        }

        drawHandle.SetTransform(Matrix3x2.Identity);
    }

    private void DrawScreen(in OverlayDrawArgs args)
    {
        var drawHandle = args.ScreenHandle;
        var xformQuery = _entityManager.GetEntityQuery<TransformComponent>();


        foreach (var gridId in _debugOverlaySystem.TileData.Keys)
        {
            if (!_entityManager.TryGetComponent(gridId, out MapGridComponent? mapGrid))
                continue;

            var gridXform = xformQuery.GetComponent(gridId);
            var (_, _, matrix, invMatrix) = _transformSystem.GetWorldPositionRotationMatrixWithInv(gridXform, xformQuery);
            var gridBounds = invMatrix.TransformBox(args.WorldBounds).Enlarged(mapGrid.TileSize * 2);

            foreach (var debugOverlayData in _debugOverlaySystem.GetData(gridId))
            {
                var centre = (debugOverlayData.Pos + Vector2Helpers.Half) * mapGrid.TileSize;

                // // is the center of this tile visible
                if (!gridBounds.Contains(centre))
                    continue;

                var screenCenter = _eyeManager.WorldToScreen(Vector2.Transform(centre, matrix));

                drawHandle.DrawString(_font, screenCenter, debugOverlayData.CurrentVolume.ToString(), Color.White);
            }
        }
    }

    private Color ColorMap(FixedPoint2 intensity)
    {
        var fraction = 1 - intensity / FixedPoint2.New(20f);
        var result  = fraction < 0.5f
            ? Color.InterpolateBetween(_mediumPuddle, _heavyPuddle, fraction.Float() * 2)
            : Color.InterpolateBetween(_lightPuddle, _mediumPuddle, (fraction.Float() - 0.5f) * 2);
        return result;
    }
}
