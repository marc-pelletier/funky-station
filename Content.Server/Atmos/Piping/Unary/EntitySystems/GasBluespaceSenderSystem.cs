using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Shared.Atmos;
using Robust.Server.GameObjects;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.UserInterface;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems;

public sealed class GasBluespaceSenderSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasBluespaceSenderComponent, AtmosDeviceUpdateEvent>(OnUpdated);
        SubscribeLocalEvent<GasBluespaceSenderComponent, ExaminedEvent>(OnExamined);

        SubscribeLocalEvent<GasBluespaceSenderComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
    }

    private void OnUpdated(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, ref AtmosDeviceUpdateEvent args)
    {
        if (!(_power.IsPowered(uid) && TryComp<ApcPowerReceiverComponent>(uid, out var receiver)))
            return;

        GetBluespaceSenderMixture(uid, bluespaceGasSender, out var bluespaceSenderGasMixture);
        if (bluespaceSenderGasMixture == null) return;

        var removedGas = bluespaceSenderGasMixture.RemoveRatio(1f);
        var storedGases = bluespaceGasSender.GasStorage;

        _atmos.Merge(storedGases, removedGas);
        storedGases.Temperature = Atmospherics.T20C;
    }

    private void GetBluespaceSenderMixture(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, out GasMixture? bluespaceSenderGasMixture)
    {
        bluespaceSenderGasMixture = null;
        if (!_nodeContainer.TryGetNode(uid, bluespaceGasSender.InletName, out PipeNode? inlet)) return;
        bluespaceSenderGasMixture = inlet.Air;
    }

    private void DirtyUI(EntityUid uid, GasBluespaceSenderComponent? bluespaceGasSender, UserInterfaceComponent? ui=null)
    {
        if (!Resolve(uid, ref bluespaceGasSender, ref ui, false))
            return;

        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(uid, ref powerReceiver))
            return;

        _userInterfaceSystem.SetUiState(uid, BluespaceGasSenderUiKey.Key,
            new GasBluespaceSenderBoundUserInterfaceState(!powerReceiver.PowerDisabled));
    }

    private void OnToggleMessage(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, GasBluespaceSenderToggleMessage args)
    {
        var powerState = _power.TogglePower(uid);
        _adminLogger.Add(LogType.AtmosPowerChanged, $"{ToPrettyString(args.Actor)} turned {(powerState ? "On" : "Off")} {ToPrettyString(uid)}");
        DirtyUI(uid, bluespaceGasSender);
    }

    private void OnExamined(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        if (Loc.TryGetString("gas-bluespace-sender-system-examined", out var str,
                    ("machineName", "bluespace gas sender"),
                    ("gasName", "oxygen"),
                    ("gasMoles", bluespaceGasSender.GasStorage.GetMoles(Gas.Oxygen))
            ))

            args.PushMarkup(str);
    }

    private void OnBeforeOpened(Entity<GasBluespaceSenderComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent, ent.Comp);
    }

}