// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Numerics;
using Content.Client.Computer;
using Content.Shared.Solar;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;

namespace Content.Client.Power
{
    [GenerateTypedNameReferences]
    public sealed partial class SolarControlWindow : DefaultWindow, IComputerWindow<SolarControlConsoleBoundInterfaceState>
    {
        [ViewVariables]
        private SolarControlConsoleBoundInterfaceState _lastState = new(0, 0, 0, 0);

        public SolarControlWindow()
        {
            RobustXamlLoader.Load(this);
        }

        public void SetupComputerWindow(ComputerBoundUserInterfaceBase cb)
        {
            PanelRotation.OnTextEntered += text =>
            {
                if (!double.TryParse((string?) text.Text, out var value))
                    return;

                SolarControlConsoleAdjustMessage msg = new()
                {
                    Rotation = Angle.FromDegrees(value), AngularVelocity = _lastState.AngularVelocity,
                };

                cb.SendMessage(msg);

                // Predict this...
                _lastState.Rotation = msg.Rotation;
                NotARadar.UpdateState(_lastState);
            };

            PanelVelocity.OnTextEntered += text =>
            {
                if (!double.TryParse((string?) text.Text, out var value))
                    return;

                SolarControlConsoleAdjustMessage msg = new()
                {
                    Rotation = NotARadar.PredictedPanelRotation, AngularVelocity = Angle.FromDegrees(value / 60),
                };

                cb.SendMessage(msg);

                // Predict this...
                _lastState.Rotation = NotARadar.PredictedPanelRotation;
                _lastState.AngularVelocity = msg.AngularVelocity;
                NotARadar.UpdateState(_lastState);
            };
        }

        private static string FormatAngle(Angle d)
        {
            return d.Degrees.ToString("F1");
        }

        // The idea behind this is to prevent every update from the server
        //  breaking the textfield.
        private static void UpdateField(LineEdit field, string newValue)
        {
            if (!field.HasKeyboardFocus())
            {
                field.Text = newValue;
            }
        }

        public void UpdateState(SolarControlConsoleBoundInterfaceState scc)
        {
            _lastState = scc;
            NotARadar.UpdateState(scc);
            OutputPower.Text = ((int) MathF.Floor(scc.OutputPower)).ToString();
            SunAngle.Text = FormatAngle(scc.TowardsSun);
            UpdateField(PanelRotation, FormatAngle(scc.Rotation));
            UpdateField(PanelVelocity, FormatAngle(scc.AngularVelocity * 60));
        }

    }

    public sealed class SolarControlNotARadar : Control
    {
        // This is used for client-side prediction of the panel rotation.
        // This makes the display feel a lot smoother.
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        private SolarControlConsoleBoundInterfaceState _lastState = new(0, 0, 0, 0);

        private TimeSpan _lastStateTime = TimeSpan.Zero;

        public const int StandardSizeFull = 290;
        public const int StandardRadiusCircle = 140;
        public int SizeFull => (int) (StandardSizeFull * UIScale);
        public int RadiusCircle => (int) (StandardRadiusCircle * UIScale);

        public SolarControlNotARadar()
        {
            IoCManager.InjectDependencies(this);
            MinSize = new Vector2(SizeFull, SizeFull);
        }

        public void UpdateState(SolarControlConsoleBoundInterfaceState ls)
        {
            _lastState = ls;
            _lastStateTime = _gameTiming.CurTime;
        }

        public Angle PredictedPanelRotation => _lastState.Rotation + _lastState.AngularVelocity * (_gameTiming.CurTime - _lastStateTime).TotalSeconds;

        protected override void Draw(DrawingHandleScreen handle)
        {
            var point = SizeFull / 2;
            var fakeAA = new Color(0.08f, 0.08f, 0.08f);
            var gridLines = new Color(0.08f, 0.08f, 0.08f);
            var panelExtentCutback = 4;
            var gridLinesRadial = 8;
            var gridLinesEquatorial = 8;

            // Draw base
            handle.DrawCircle(new Vector2(point, point), RadiusCircle + 1, fakeAA);
            handle.DrawCircle(new Vector2(point, point), RadiusCircle, Color.Black);

            // Draw grid lines
            for (var i = 0; i < gridLinesEquatorial; i++)
            {
                handle.DrawCircle(new Vector2(point, point), (RadiusCircle / gridLinesEquatorial) * i, gridLines, false);
            }

            for (var i = 0; i < gridLinesRadial; i++)
            {
                Angle angle = Math.PI / gridLinesRadial * i;
                var aExtent = angle.ToVec() * RadiusCircle;
                handle.DrawLine(new Vector2(point, point) - aExtent, new Vector2(point, point) + aExtent, gridLines);
            }

            // The rotations need to be adjusted because Y is inverted in Robust (like BYOND)
            var rotMul = new Vector2(1, -1);
            // Hotfix corrections I don't understand
            var rotOfs = new Angle(Math.PI * -0.5);

            var predictedPanelRotation = PredictedPanelRotation;

            var extent = (predictedPanelRotation + rotOfs).ToVec() * rotMul * RadiusCircle;
            var extentOrtho = new Vector2(extent.Y, -extent.X);
            handle.DrawLine(new Vector2(point, point) - extentOrtho, new Vector2(point, point) + extentOrtho, Color.White);
            handle.DrawLine(new Vector2(point, point) + (extent / panelExtentCutback), new Vector2(point, point) + extent - (extent / panelExtentCutback), Color.DarkGray);

            var sunExtent = (_lastState.TowardsSun + rotOfs).ToVec() * rotMul * RadiusCircle;
            handle.DrawLine(new Vector2(point, point) + sunExtent, new Vector2(point, point), Color.Yellow);
        }
    }

    [UsedImplicitly]
    public sealed class SolarControlConsoleBoundUserInterface : ComputerBoundUserInterface<SolarControlWindow, SolarControlConsoleBoundInterfaceState>
    {
        public SolarControlConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
        {
        }
    }
}
