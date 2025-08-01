// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using Robust.Shared.Prototypes; // Goobstation - anythingburgers
using Content.Shared._Goobstation.Nutrition.EntitySystems; // Goobstation - anythingburgers

namespace Content.Shared.Nutrition.Components;

/// <summary>
/// Tndicates that this entity can be inserted into FoodSequence, which will transfer all reagents to the target.
/// </summary>
[RegisterComponent, Access(typeof(SharedFoodSequenceSystem))]
public sealed partial class FoodSequenceElementComponent : Component
{
    /// <summary>
    /// the same object can be used in different sequences, and it will have a different sprite in different sequences.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, FoodSequenceElementEntry> Entries = new();

    /// <summary>
    /// which solution we will add to the main dish
    /// </summary>
    [DataField]
    public string Solution = "food";

    /// <summary>
    /// state used to generate the appearance of the added layer
    /// </summary>
    [DataField]
    public SpriteSpecifier? Sprite;
}

[DataRecord, Serializable, NetSerializable]
public sealed class FoodSequenceElementEntry
{
    /// <summary>
    /// A localized name piece to build into the item name generator.
    /// </summary>
    public LocId? Name { get; set; } = null;

    /// <summary>
    /// overriding default sprite
    /// </summary>
    public SpriteSpecifier? Sprite { get; set; } = null;

    /// <summary>
    /// If the layer is the final one, it can be added over the limit, but no other layers can be added after it.
    /// </summary>
    public bool Final { get; set; } = false;

    /// <summary>
    /// the shear of a particular layer. Allows a little "randomization" of each layer.
    /// </summary>
    public Vector2 LocalOffset { get; set; } = Vector2.Zero;

    /// <summary>
    /// the original prototype of the layer
    /// </summary>
    public string? Proto { get; set; } = null; // Goobstation - anythingburgers
}
