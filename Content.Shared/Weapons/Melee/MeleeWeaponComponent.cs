// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Arimah Greene <30327355+arimah@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Errant <35878406+dmnct@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 HerCoyote23 <131214189+HerCoyote23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 I.K <45953835+notquitehadouken@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 jicksaw <jicksaw@pm.me>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 notquitehadouken <1isthisameme>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Scribbles0 <91828755+Scribbles0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Melee;

/// <summary>
/// When given to a mob lets them do unarmed attacks, or when given to an item lets someone wield it to do attacks.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true), AutoGenerateComponentPause]
public sealed partial class MeleeWeaponComponent : Component
{
    // TODO: This is becoming bloated as shit.
    // This should just be its own component for alt attacks.
    /// <summary>
    /// Does this entity do a disarm on alt attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AltDisarm = true;

    /// <summary>
    /// Should the melee weapon's damage stats be examinable.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Hidden;

    /// <summary>
    /// Next time this component is allowed to light attack. Heavy attacks are wound up and never have a cooldown.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextAttack;

    /// <summary>
    /// Starts attack cooldown when equipped if true.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ResetOnHandSelected = true;

    /*
     * Melee combat works based around 2 types of attacks:
     * 1. Click attacks with left-click. This attacks whatever is under your mnouse
     * 2. Wide attacks with right-click + left-click. This attacks whatever is in the direction of your mouse.
     */

    /// <summary>
    /// How many times we can attack per second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AttackRate = 1f;

    /// <summary>
    /// Are we currently holding down the mouse for an attack.
    /// Used so we can't just hold the mouse button and attack constantly.
    /// </summary>
    [AutoNetworkedField]
    public bool Attacking = false;

    /// <summary>
    /// If true, attacks will be repeated automatically without requiring the mouse button to be lifted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoAttack;

    /// <summary>
    /// If true, attacks will bypass armor resistances.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ResistanceBypass = false;

    /// <summary>
    /// Base damage for this weapon. Can be modified via heavy damage or other means.
    /// </summary>
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier Damage = default!;

    [DataField, AutoNetworkedField]
    public FixedPoint2 BluntStaminaDamageFactor = FixedPoint2.New(0.5f);

    /// <summary>
    /// Multiplies damage by this amount for single-target attacks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ClickDamageModifier = FixedPoint2.New(1);

    // TODO: Temporarily 1.5 until interactionoutline is adjusted to use melee, then probably drop to 1.2
    /// <summary>
    /// Nearest edge range to hit an entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    /// <summary>
    /// Total width of the angle for wide attacks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle Angle = Angle.FromDegrees(60);

    [DataField, AutoNetworkedField]
    public EntProtoId Animation = "WeaponArcPunch";

    [DataField, AutoNetworkedField]
    public EntProtoId WideAnimation = "WeaponArcSlash";

    /// <summary>
    /// Rotation of the animation.
    /// 0 degrees means the top faces the attacker.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle WideAnimationRotation = Angle.Zero;

    [DataField, AutoNetworkedField]
    public bool SwingLeft;


    // Sounds

    /// <summary>
    /// This gets played whenever a melee attack is done. This is predicted by the client.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("soundSwing"), AutoNetworkedField]
    public SoundSpecifier SwingSound { get; set; } = new SoundPathSpecifier("/Audio/Weapons/punchmiss.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f).WithVariation(0.025f),
    };

    // We do not predict the below sounds in case the client thinks but the server disagrees. If this were the case
    // then a player may doubt if the target actually took damage or not.
    // If overwatch and apex do this then we probably should too.

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("soundHit"), AutoNetworkedField]
    public SoundSpecifier? HitSound;

    /// <summary>
    /// Plays if no damage is done to the target entity.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("soundNoDamage"), AutoNetworkedField]
    public SoundSpecifier NoDamageSound { get; set; } = new SoundCollectionSpecifier("WeakHit");

    /// <summary>
    /// If true, the weapon must be equipped for it to be used.
    /// E.g boxing gloves must be equipped to your gloves,
    /// not just held in your hand to be used.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool MustBeEquippedToUse = false;

    // Shitmed Change Start

    /// <summary>
    ///     Shitmed Change: Part damage is multiplied by this amount for single-target attacks
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ClickPartDamageMultiplier = 1.00f;

    /// <summary>
    ///     Shitmed Change: Part damage is multiplied by this amount for heavy swings
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeavyPartDamageMultiplier = 0.5f;

    // Shitmed Change End

    // Goobstation
    [DataField, AutoNetworkedField]
    public bool CanWideSwing = true;
}

/// <summary>
/// Event raised on entity in GetWeapon function to allow systems to manually
/// specify what the weapon should be.
/// </summary>
public sealed class GetMeleeWeaponEvent : HandledEntityEventArgs
{
    public EntityUid? Weapon;
}
