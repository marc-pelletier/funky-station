// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Javier Guardia Fernández <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Shared.Damage;

namespace Content.Server.Destructible.Thresholds
{
    [DataDefinition]
    public sealed partial class DamageThreshold
    {
        [DataField("behaviors")]
        private List<IThresholdBehavior> _behaviors = new();

        /// <summary>
        ///     Whether or not this threshold was triggered in the previous call to
        ///     <see cref="Reached"/>.
        /// </summary>
        [ViewVariables] public bool OldTriggered { get; private set; }

        /// <summary>
        ///     Whether or not this threshold has already been triggered.
        /// </summary>
        [DataField("triggered")]
        public bool Triggered { get; private set; }

        /// <summary>
        ///     Whether or not this threshold only triggers once.
        ///     If false, it will trigger again once the entity is healed
        ///     and then damaged to reach this threshold once again.
        ///     It will not repeatedly trigger as damage rises beyond that.
        /// </summary>
        [DataField("triggersOnce")]
        public bool TriggersOnce { get; set; }

        /// <summary>
        ///     The trigger that decides if this threshold has been reached.
        /// </summary>
        [DataField("trigger")]
        public IThresholdTrigger? Trigger { get; set; }

        /// <summary>
        ///     Behaviors to activate once this threshold is triggered.
        /// </summary>
        [ViewVariables] public IReadOnlyList<IThresholdBehavior> Behaviors => _behaviors;

        public bool Reached(DamageableComponent damageable, DestructibleSystem system)
        {
            if (Trigger == null)
            {
                return false;
            }

            if (Triggered && TriggersOnce)
            {
                return false;
            }

            if (OldTriggered)
            {
                OldTriggered = Trigger.Reached(damageable, system);
                return false;
            }

            if (!Trigger.Reached(damageable, system))
            {
                return false;
            }

            OldTriggered = true;
            return true;
        }

        /// <summary>
        ///     Triggers this threshold.
        /// </summary>
        /// <param name="owner">The entity that owns this threshold.</param>
        /// <param name="system">
        ///     An instance of <see cref="DestructibleSystem"/> to get dependency and
        ///     system references from, if relevant.
        /// </param>
        /// <param name="entityManager"></param>
        /// <param name="cause"></param>
        public void Execute(EntityUid owner, DestructibleSystem system, IEntityManager entityManager, EntityUid? cause)
        {
            Triggered = true;

            foreach (var behavior in Behaviors)
            {
                // The owner has been deleted. We stop execution of behaviors here.
                if (!entityManager.EntityExists(owner))
                    return;

                behavior.Execute(owner, system, cause);
            }
        }
    }
}
