// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Botany.Components;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.EntityEffects.Effects.PlantMetabolism;

[UsedImplicitly]
[DataDefinition]
public sealed partial class PlantCryoxadone : EntityEffect
{
    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out PlantHolderComponent? plantHolderComp)
        || plantHolderComp.Seed == null || plantHolderComp.Dead)
            return;

        var deviation = 0;
        var seed = plantHolderComp.Seed;
        var random = IoCManager.Resolve<IRobustRandom>();
        if (plantHolderComp.Age > seed.Maturation)
            deviation = (int) Math.Max(seed.Maturation - 1, plantHolderComp.Age - random.Next(7, 10));
        else
            deviation = (int) (seed.Maturation / seed.GrowthStages);
        plantHolderComp.Age -= deviation;
        plantHolderComp.LastProduce = plantHolderComp.Age;
        plantHolderComp.SkipAging++;
        plantHolderComp.ForceUpdate = true;
    }

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys) => Loc.GetString("reagent-effect-guidebook-plant-cryoxadone", ("chance", Probability));
}
