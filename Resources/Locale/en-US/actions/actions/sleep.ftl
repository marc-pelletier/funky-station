# SPDX-FileCopyrightText: 2022 Rane <60792108+Elijahrane@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
# SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
# SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
#
# SPDX-License-Identifier: MIT

action-name-wake = Wake up

sleep-onomatopoeia = Zzz...
sleep-examined = [color=lightblue]{CAPITALIZE(SUBJECT($target))} {CONJUGATE-BE($target)} asleep.[/color]

wake-other-success = You shake {THE($target)} awake.
wake-other-failure = You shake {THE($target)}, but {SUBJECT($target)} {CONJUGATE-BE($target)} not waking up.
