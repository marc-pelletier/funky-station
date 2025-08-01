// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Silicons.Borgs;

[GenerateTypedNameReferences]
public sealed partial class BorgModuleControl : PanelContainer
{
    public Action? RemoveButtonPressed;

    public BorgModuleControl(EntityUid entity, IEntityManager entityManager, bool canRemove)
    {
        RobustXamlLoader.Load(this);

        ModuleView.SetEntity(entity);
        ModuleName.Text = entityManager.GetComponent<MetaDataComponent>(entity).EntityName;
        RemoveButton.TexturePath = "/Textures/Interface/Nano/cross.svg.png";
        RemoveButton.OnPressed += _ =>
        {
            RemoveButtonPressed?.Invoke();
        };
        RemoveButton.Visible = canRemove;
    }
}

