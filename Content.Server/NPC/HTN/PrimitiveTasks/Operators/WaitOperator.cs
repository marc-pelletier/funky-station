// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.HTN.PrimitiveTasks.Operators;

/// <summary>
/// Waits the specified amount of time. Removes the key when finished.
/// </summary>
public sealed partial class WaitOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    /// <summary>
    /// Blackboard key for the time we'll wait for.
    /// </summary>
    [DataField("key", required: true)] public string Key = string.Empty;

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        if (!blackboard.TryGetValue<float>(Key, out var timer, _entManager))
        {
            return HTNOperatorStatus.Finished;
        }

        timer -= frameTime;
        blackboard.SetValue(Key, timer);

        return timer <= 0f ? HTNOperatorStatus.Finished : HTNOperatorStatus.Continuing;
    }

    public override void TaskShutdown(NPCBlackboard blackboard, HTNOperatorStatus status)
    {
        base.TaskShutdown(blackboard, status);

        // The replacement plan may want this value so only dump it if we're successful.
        if (status != HTNOperatorStatus.BetterPlan)
        {
            blackboard.Remove<float>(Key);
        }
    }
}
