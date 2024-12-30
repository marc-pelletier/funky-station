using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Cargo.Systems;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Lock;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;

namespace Content.Server.Atmos.Piping.Unary.EntitySystems;

public sealed class GasBluespaceSenderSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasBluespaceSenderComponent, AtmosDeviceUpdateEvent>(OnUpdated);
    }

    private void OnUpdated(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, ref AtmosDeviceUpdateEvent args)
    {
        if (!(_power.IsPowered(uid) && TryComp<ApcPowerReceiverComponent>(uid, out var receiver)))
            return;

        GetBluespaceSenderMixture(uid, bluespaceGasSender, out var bluespaceSenderGasMixture);
        if (bluespaceSenderGasMixture == null) return;

        bluespaceSenderGasMixture.RemoveRatio(1f);

        
    }

    private void GetBluespaceSenderMixture(EntityUid uid, GasBluespaceSenderComponent bluespaceGasSender, out GasMixture? bluespaceSenderGasMixture)
    {
        bluespaceSenderGasMixture = null;
        if (bluespaceGasSender.Atmospheric)
        {
            bluespaceSenderGasMixture = _atmos.GetContainingMixture(uid, excite: true);
        }
        else
        {
            if (!_nodeContainer.TryGetNode(uid, bluespaceGasSender.InletName, out PipeNode? inlet)) return;
            bluespaceSenderGasMixture = inlet.Air;
            var gasRemoved = bluespaceSenderGasMixture.RemoveRatio(1f);
        }
    }

}