// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 88tv <131759102+88tv@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Orsoniks <orsoniksstuff@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared.HealthExaminable;

[RegisterComponent, Access(typeof(HealthExaminableSystem))]
public sealed partial class HealthExaminableComponent : Component
{
    public List<FixedPoint2> Thresholds = new()
        { FixedPoint2.New(8), FixedPoint2.New(15), FixedPoint2.New(30), FixedPoint2.New(50), FixedPoint2.New(75), FixedPoint2.New(100), FixedPoint2.New(200) };

    [DataField(required: true)]
    public HashSet<ProtoId<DamageTypePrototype>> ExaminableTypes = default!;

    /// <summary>
    ///     Health examine text is automatically generated through creating loc string IDs, in the form:
    ///     `health-examine-[prefix]-[type]-[threshold]`
    ///     This part determines the prefix.
    /// </summary>
    [DataField]
    public string LocPrefix = "carbon";
}
