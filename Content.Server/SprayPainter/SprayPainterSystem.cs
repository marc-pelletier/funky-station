using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.EntitySystems;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Atmos.Piping.Unary.EntitySystems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.SprayPainter;
using Content.Shared.SprayPainter.Components;

namespace Content.Server.SprayPainter;

public sealed class SprayPainterSystem : SharedSprayPainterSystem
{
    [Dependency] private readonly AtmosPipeColorSystem _pipeColor = default!;
    [Dependency] private readonly GasCanisterColorSystem _canisterColor = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SprayPainterComponent, SprayPainterPipeDoAfterEvent>(OnPipeDoAfter);
        SubscribeLocalEvent<SprayPainterComponent, SprayPainterCanisterDoAfterEvent>(OnCanisterDoAfter);

        SubscribeLocalEvent<AtmosPipeColorComponent, InteractUsingEvent>(OnPipeInteract);
        SubscribeLocalEvent<GasCanisterColorComponent, InteractUsingEvent>(OnCanisterInteract);
    }

    private void OnCanisterDoAfter(Entity<SprayPainterComponent> ent, ref SprayPainterCanisterDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Args.Target is not {} target)
            return;

        if (!TryComp<GasCanisterColorComponent>(target, out var color))
            return;

        Audio.PlayPvs(ent.Comp.SpraySound, ent);

        _canisterColor.SetColors(target, color, args.PrimaryColor, args.SecondaryColor, args.TertiaryColor, args.SecondaryEnabled);

        args.Handled = true;
    }

    private void OnPipeDoAfter(Entity<SprayPainterComponent> ent, ref SprayPainterPipeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (args.Args.Target is not {} target)
            return;

        if (!TryComp<AtmosPipeColorComponent>(target, out var color))
            return;

        Audio.PlayPvs(ent.Comp.SpraySound, ent);

        _pipeColor.SetColor(target, color, args.Color);

        args.Handled = true;
    }

    private void OnCanisterInteract(Entity<GasCanisterColorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<SprayPainterComponent>(args.Used, out var painter) || painter.PrimaryColor is not {} primaryColorName)
            return;

        if (!painter.ColorPalette.TryGetValue(primaryColorName, out var primaryColor))
            return;

        // Secondary color falls back to PrimaryColor if not set
        var secondaryColorName = painter.SecondaryColor ?? primaryColorName;
        if (!painter.ColorPalette.TryGetValue(secondaryColorName, out var secondaryColor))
            secondaryColor = primaryColor;

        // Tertiary color falls back to PrimaryColor if not set
        var tertiaryColorName = painter.TertiaryColor ?? primaryColorName;
        if (!painter.ColorPalette.TryGetValue(tertiaryColorName, out var tertiaryColor))
            tertiaryColor = primaryColor;

        var secondaryEnabled = painter.SecondaryEnabled;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, painter.PipeSprayTime,
            new SprayPainterCanisterDoAfterEvent(primaryColor, secondaryColor, tertiaryColor, secondaryEnabled),
            args.Used, target: ent, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DuplicateCondition = DuplicateConditions.SameTarget,
            NeedHand = true,
        };

        args.Handled = DoAfter.TryStartDoAfter(doAfterEventArgs);
    }

    private void OnPipeInteract(Entity<AtmosPipeColorComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<SprayPainterComponent>(args.Used, out var painter) || painter.PickedColor is not {} colorName)
            return;

        if (!painter.ColorPalette.TryGetValue(colorName, out var color))
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, painter.PipeSprayTime,
            new SprayPainterPipeDoAfterEvent(color), args.Used, target: ent, used: args.Used)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            DuplicateCondition = DuplicateConditions.SameTarget,
            NeedHand = true,
        };

        args.Handled = DoAfter.TryStartDoAfter(doAfterEventArgs);
    }
}