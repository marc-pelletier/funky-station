// SPDX-FileCopyrightText: 2022 eclips_e <67359748+Just-a-Unity-Dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 lzk228 <124214523+lzk228@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Dae <60460608+ZeroDayDaemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 Voomra <dimon550@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Administration.Managers;
using Content.Server.Bible.Components;
using Content.Server.Chat.Managers;
using Content.Server.Popups;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Chat;
using Content.Shared.Prayer;
using Content.Shared._Goobstation.Religion;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Prayer;
/// <summary>
/// System to handle subtle messages and praying
/// </summary>
/// <remarks>
/// Rain is a professional developer and this did not take 2 PRs to fix subtle messages
/// </remarks>
public sealed class PrayerSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly IAdminManager _adminManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrayableComponent, GetVerbsEvent<ActivationVerb>>(AddPrayVerb);
    }

    private void AddPrayVerb(EntityUid uid, PrayableComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        // if it doesn't have an actor and we can't reach it then don't add the verb
        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        // this is to prevent ghosts from using it
        if (!args.CanInteract)
            return;

        var prayerVerb = new ActivationVerb
        {
            Text = Loc.GetString(comp.Verb),
            Icon = comp.VerbImage,
            Act = () =>
            {
                //Public Domain Code start
                if (EntityManager.TryGetComponent(args.User, out ReligionComponent? religionComponent) && religionComponent.Type == Religion.Atheist)
                {
                    _popupSystem.PopupEntity(Loc.GetString("prayer-popup-notify-pray-atheist"), uid, actor.PlayerSession, PopupType.Large);
                    return;
                }
                //Public Domain Code end

                if (comp.BibleUserOnly && !EntityManager.TryGetComponent<BibleUserComponent>(args.User, out var bibleUser))
                {
                    _popupSystem.PopupEntity(Loc.GetString("prayer-popup-notify-pray-locked"), uid, actor.PlayerSession, PopupType.Large);
                    return;
                }

                _quickDialog.OpenDialog(actor.PlayerSession, Loc.GetString(comp.Verb), Loc.GetString("prayer-popup-notify-pray-ui-message"), (string message) =>
                {
                    // Make sure the player's entity and the Prayable entity+component still exist
                    if (actor?.PlayerSession != null && HasComp<PrayableComponent>(uid))
                        Pray(actor.PlayerSession, comp, message);
                });
            },
            Impact = LogImpact.Low,

        };
        prayerVerb.Impact = LogImpact.Low;
        args.Verbs.Add(prayerVerb);
    }

    /// <summary>
    /// Subtly messages a player by giving them a popup and a chat message.
    /// </summary>
    /// <param name="target">The IPlayerSession that you want to send the message to</param>
    /// <param name="source">The IPlayerSession that sent the message</param>
    /// <param name="messageString">The main message sent to the player via the chatbox</param>
    /// <param name="popupMessage">The popup to notify the player, also prepended to the messageString</param>
    public void SendSubtleMessage(ICommonSession target, ICommonSession source, string messageString, string popupMessage)
    {
        if (target.AttachedEntity == null)
            return;

        var message = popupMessage == "" ? "" : popupMessage + (messageString == "" ? "" : $" \"{messageString}\"");

        _popupSystem.PopupEntity(popupMessage, target.AttachedEntity.Value, target, PopupType.Large);
        _chatManager.ChatMessageToOne(ChatChannel.Local, messageString, message, EntityUid.Invalid, false, target.Channel);
        _adminLogger.Add(LogType.AdminMessage, LogImpact.Low, $"{ToPrettyString(target.AttachedEntity.Value):player} received subtle message from {source.Name}: {message}");
    }

    /// <summary>
    /// Sends a message to the admin channel with a message and username
    /// </summary>
    /// <param name="sender">The IPlayerSession who sent the original message</param>
    /// <param name="comp">Prayable component used to make the prayer</param>
    /// <param name="message">Message to be sent to the admin chat</param>
    /// <remarks>
    /// You may be wondering, "Why the admin chat, specifically? Nobody even reads it!"
    /// Exactly.
    ///  </remarks>
    public void Pray(ICommonSession sender, PrayableComponent comp, string message)
    {
        if (sender.AttachedEntity == null)
            return;

        _popupSystem.PopupEntity(Loc.GetString(comp.SentMessage), sender.AttachedEntity.Value, sender, PopupType.Medium);

        _chatManager.SendAdminAnnouncement($"{Loc.GetString(comp.NotificationPrefix)} <{sender.Name}>: {message}");
        _adminLogger.Add(LogType.AdminMessage, LogImpact.Low, $"{ToPrettyString(sender.AttachedEntity.Value):player} sent prayer ({Loc.GetString(comp.NotificationPrefix)}): {message}");
        _audioSystem.PlayGlobal("/Audio/Items/ring.ogg",
            Filter.Empty().AddPlayers(_adminManager.ActiveAdmins), false, AudioParams.Default.WithVolume(-8f));
    }
}
