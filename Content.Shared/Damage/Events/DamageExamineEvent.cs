// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Utility;

namespace Content.Shared.Damage.Events;

[ByRefEvent]
public readonly record struct DamageExamineEvent(FormattedMessage Message, EntityUid User);
