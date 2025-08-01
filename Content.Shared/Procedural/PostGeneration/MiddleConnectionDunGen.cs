// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Places the specified entities on the middle connections between rooms
/// </summary>
public sealed partial class MiddleConnectionDunGen : IDunGenLayer
{
    /// <summary>
    /// How much overlap there needs to be between 2 rooms exactly.
    /// </summary>
    [DataField]
    public int OverlapCount = -1;

    /// <summary>
    /// How many connections to spawn between rooms.
    /// </summary>
    [DataField]
    public int Count = 1;
}
