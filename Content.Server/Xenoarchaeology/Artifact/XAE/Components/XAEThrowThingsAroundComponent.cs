// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Xenoarchaeology.Artifact.XAE.Components;

/// <summary>
/// Throws all nearby entities backwards.
/// Also pries nearby tiles.
/// </summary>
[RegisterComponent, Access(typeof(XAEThrowThingsAroundSystem))]
public sealed partial class XAEThrowThingsAroundComponent : Component
{
    /// <summary>
    /// How close do you have to be to get yeeted?
    /// </summary>
    [DataField("range")]
    public float Range = 2f;

    /// <summary>
    /// How likely is it that an individual tile will get pried?
    /// </summary>
    [DataField("tilePryChance")]
    public float TilePryChance = 0.5f;

    /// <summary>
    /// How strongly does stuff get thrown?
    /// </summary>
    [DataField("throwStrength"), ViewVariables(VVAccess.ReadWrite)]
    public float ThrowStrength = 5f;
}
