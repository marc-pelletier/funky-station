// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 osjarw <62134478+osjarw@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using Content.Server.NPC.HTN;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.NPC;

[TestFixture]
public sealed class NPCTest
{
    [Test]
    public async Task CompoundRecursion()
    {
        var pool = await PoolManager.GetServerClient();
        var server = pool.Server;

        await server.WaitIdleAsync();

        var htnSystem = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<HTNSystem>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();

        await server.WaitAssertion(() =>
        {
            var counts = new Dictionary<string, int>();

            foreach (var compound in protoManager.EnumeratePrototypes<HTNCompoundPrototype>())
            {
                Count(compound, counts, htnSystem, protoManager);
                counts.Clear();
            }
        });

        await pool.CleanReturnAsync();
    }

    private static void Count(HTNCompoundPrototype compound, Dictionary<string, int> counts, HTNSystem htnSystem, IPrototypeManager protoManager)
    {
        foreach (var branch in compound.Branches)
        {
            foreach (var task in branch.Tasks)
            {
                if (task is HTNCompoundTask compoundTask)
                {
                    var count = counts.GetOrNew(compound.ID);
                    count++;

                    // Compound tasks marked with AllowRecursion are only evaluated once
                    if (counts.ContainsKey(compound.ID) && compound.AllowRecursion)
                        continue;

                    Assert.That(count, Is.LessThan(50));
                    counts[compound.ID] = count;
                    Count(protoManager.Index<HTNCompoundPrototype>(compoundTask.Task), counts, htnSystem, protoManager);
                }
            }
        }
    }
}
