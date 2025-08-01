// SPDX-FileCopyrightText: 2023 HerCoyote23 <131214189+HerCoyote23@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Weapons.Melee.UI;

[GenerateTypedNameReferences]
public sealed partial class MeleeSpeechWindow : DefaultWindow
{

    public event Action<string>? OnBattlecryEntered;

	public MeleeSpeechWindow()
	{
        RobustXamlLoader.Load(this);

		BattlecryLineEdit.OnTextEntered += e => OnBattlecryEntered?.Invoke(e.Text);
	}


	public void SetCurrentBattlecry(string battlecry)
	{
		BattlecryLineEdit.Text = battlecry;
	}

}


