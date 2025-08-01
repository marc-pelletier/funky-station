// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
﻿using Content.Shared.Atmos;

namespace Content.Server.Atmos.Components;

/// <summary>
/// This is basically a siphon vent for <see cref="GetFilterAirEvent"/>.
/// </summary>
[RegisterComponent, Access(typeof(AirFilterSystem))]
public sealed partial class AirIntakeComponent : Component
{
    /// <summary>
    /// Target pressure change for a single atmos tick
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float TargetPressureChange = 5f;

    /// <summary>
    /// How strong the intake pump is, it will be able to replenish air from lower pressure areas.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float PumpPower = 2f;

    /// <summary>
    /// Pressure to intake gases up to, maintains pressure of the air volume.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Pressure = Atmospherics.OneAtmosphere;
}
