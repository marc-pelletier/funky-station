// SPDX-FileCopyrightText: 2021 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stylesheets;
using Content.Shared.Administration.Notes;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Client.Administration.UI.AdminRemarks;

[GenerateTypedNameReferences]
public sealed partial class AdminMessagePopupWindow : Control
{
    private float _timer = float.MaxValue;

    public event Action? OnDismissPressed;

    public event Action? OnAcceptPressed;

    public AdminMessagePopupWindow()
    {
        RobustXamlLoader.Load(this);

        Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetSpace;

        AcceptButton.OnPressed += OnAcceptButtonPressed;
        DismissButton.OnPressed += OnDismissButtonPressed;
    }

    public float Timer
    {
        get => _timer;
        private set
        {
            WaitLabel.Text = Loc.GetString("admin-notes-message-wait", ("time", MathF.Floor(value)));
            _timer = value;
        }
    }

    public void SetState(AdminMessageEuiState state)
    {
        Timer = (float) state.Time.TotalSeconds;

        MessageContainer.RemoveAllChildren();

        foreach (var message in state.Messages)
        {
            MessageContainer.AddChild(new AdminMessagePopupMessage(message));
        }

        Description.SetMessage(FormattedMessage.FromMarkupOrThrow(Loc.GetString("admin-notes-message-desc", ("count", state.Messages.Length))));
    }

    private void OnDismissButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        OnDismissPressed?.Invoke();
    }

    private void OnAcceptButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        OnAcceptPressed?.Invoke();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        if (!AcceptButton.Disabled)
            return;

        if (Timer > 0.0)
        {
            if (Timer - args.DeltaSeconds < 0)
                Timer = 0;
            else
                Timer -= args.DeltaSeconds;
        }
        else
        {
            AcceptButton.Disabled = false;
            DismissButton.Disabled = false;
        }
    }
}
