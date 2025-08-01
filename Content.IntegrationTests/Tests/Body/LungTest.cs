// SPDX-FileCopyrightText: 2020 DamianX <DamianX@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 a.rudenko <creadth@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 0x6273 <0x40@keemail.me>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Robust.Server.GameObjects;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using System.Linq;
using System.Numerics;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.Body
{
    [TestFixture]
    [TestOf(typeof(LungSystem))]
    public sealed class LungTest
    {
        [TestPrototypes]
        private const string Prototypes = @"
- type: entity
  name: HumanLungDummy
  id: HumanLungDummy
  components:
  - type: SolutionContainerManager
  - type: Body
    prototype: Human
  - type: MobState
    allowedStates:
      - Alive
  - type: Damageable
  - type: ThermalRegulator
    metabolismHeat: 5000
    radiatedHeat: 400
    implicitHeatRegulation: 5000
    sweatHeatRegulation: 5000
    shiveringHeatRegulation: 5000
    normalBodyTemperature: 310.15
    thermalRegulationTemperatureThreshold: 25
  - type: Respirator
    damage:
      types:
        Asphyxiation: 1.5
    damageRecovery:
      types:
        Asphyxiation: -1.5
";

        [Test]
        public async Task AirConsistencyTest()
        {
            // --- Setup
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            await server.WaitIdleAsync();

            var entityManager = server.ResolveDependency<IEntityManager>();
            var mapLoader = entityManager.System<MapLoaderSystem>();
            var mapSys = entityManager.System<SharedMapSystem>();

            EntityUid? grid = null;
            BodyComponent body = default;
            RespiratorComponent resp = default;
            EntityUid human = default;
            GridAtmosphereComponent relevantAtmos = default;
            var startingMoles = 0.0f;

            var testMapName = new ResPath("Maps/Test/Breathing/3by3-20oxy-80nit.yml");

            await server.WaitPost(() =>
            {
                mapSys.CreateMap(out var mapId);
                Assert.That(mapLoader.TryLoadGrid(mapId, testMapName, out var gridEnt));
                grid = gridEnt!.Value.Owner;
            });

            Assert.That(grid, Is.Not.Null, $"Test blueprint {testMapName} not found.");

            float GetMapMoles()
            {
                var totalMapMoles = 0.0f;
                foreach (var tile in relevantAtmos.Tiles.Values)
                {
                    totalMapMoles += tile.Air?.TotalMoles ?? 0.0f;
                }

                return totalMapMoles;
            }

            await server.WaitAssertion(() =>
            {
                var center = new Vector2(0.5f, 0.5f);
                var coordinates = new EntityCoordinates(grid.Value, center);
                human = entityManager.SpawnEntity("HumanLungDummy", coordinates);
                relevantAtmos = entityManager.GetComponent<GridAtmosphereComponent>(grid.Value);
                startingMoles = 100f; // Hardcoded because GetMapMoles returns 900 here for some reason.

#pragma warning disable NUnit2045
                Assert.That(entityManager.TryGetComponent(human, out body), Is.True);
                Assert.That(entityManager.TryGetComponent(human, out resp), Is.True);
#pragma warning restore NUnit2045
            });

            // --- End setup

            var inhaleCycles = 100;
            for (var i = 0; i < inhaleCycles; i++)
            {
                // Breathe in
                await PoolManager.WaitUntil(server, () => resp.Status == RespiratorStatus.Exhaling);
                Assert.That(
                    GetMapMoles(), Is.LessThan(startingMoles),
                    "Did not inhale in any gas"
                );

                // Breathe out
                await PoolManager.WaitUntil(server, () => resp.Status == RespiratorStatus.Inhaling);
                Assert.That(
                    GetMapMoles(), Is.EqualTo(startingMoles).Within(0.0002),
                    "Did not exhale as much gas as was inhaled"
                );
            }

            await pair.CleanReturnAsync();
        }

        [Test]
        public async Task NoSuffocationTest()
        {
            await using var pair = await PoolManager.GetServerClient();
            var server = pair.Server;

            var mapManager = server.ResolveDependency<IMapManager>();
            var entityManager = server.ResolveDependency<IEntityManager>();
            var cfg = server.ResolveDependency<IConfigurationManager>();
            var mapLoader = entityManager.System<MapLoaderSystem>();
            var mapSys = entityManager.System<SharedMapSystem>();

            EntityUid? grid = null;
            RespiratorComponent respirator = null;
            EntityUid human = default;

            var testMapName = new ResPath("Maps/Test/Breathing/3by3-20oxy-80nit.yml");

            await server.WaitPost(() =>
            {
                mapSys.CreateMap(out var mapId);
                Assert.That(mapLoader.TryLoadGrid(mapId, testMapName, out var gridEnt));
                grid = gridEnt!.Value.Owner;
            });

            Assert.That(grid, Is.Not.Null, $"Test blueprint {testMapName} not found.");

            await server.WaitAssertion(() =>
            {
                var center = new Vector2(0.5f, 0.5f);

                var coordinates = new EntityCoordinates(grid.Value, center);
                human = entityManager.SpawnEntity("HumanLungDummy", coordinates);

                var mixture = entityManager.System<AtmosphereSystem>().GetContainingMixture(human);
#pragma warning disable NUnit2045
                Assert.That(mixture.TotalMoles, Is.GreaterThan(0));
                Assert.That(entityManager.HasComponent<BodyComponent>(human), Is.True);
                Assert.That(entityManager.TryGetComponent(human, out respirator), Is.True);
                Assert.That(respirator.SuffocationCycles, Is.LessThanOrEqualTo(respirator.SuffocationCycleThreshold));
#pragma warning restore NUnit2045
            });

            var increment = 10;

            // 20 seconds
            var total = 20 * cfg.GetCVar(CVars.NetTickrate);

            for (var tick = 0; tick < total; tick += increment)
            {
                await server.WaitRunTicks(increment);
                await server.WaitAssertion(() =>
                {
                    Assert.That(respirator.SuffocationCycles, Is.LessThanOrEqualTo(respirator.SuffocationCycleThreshold),
                        $"Entity {entityManager.GetComponent<MetaDataComponent>(human).EntityName} is suffocating on tick {tick}");
                });
            }

            await pair.CleanReturnAsync();
        }
    }
}
