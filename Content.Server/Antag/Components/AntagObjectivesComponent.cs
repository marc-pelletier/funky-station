// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Antag;
using Content.Shared.Objectives.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Antag.Components;

/// <summary>
/// Gives antags selected by this rule a fixed list of objectives.
/// </summary>
[RegisterComponent, Access(typeof(AntagObjectivesSystem))]
public sealed partial class AntagObjectivesComponent : Component
{
    /// <summary>
    /// List of static objectives to give.
    /// </summary>
    [DataField(required: true)]
    public List<EntProtoId<ObjectiveComponent>> Objectives = new();
}
