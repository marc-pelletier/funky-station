// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.Eui;

namespace Content.Client._DV.CosmicCult.UI;

public sealed class CosmicRoundStartEui : BaseEui
{
    private readonly CosmicRoundStartMenu _menu;

    public CosmicRoundStartEui()
    {
        _menu = new CosmicRoundStartMenu();
    }

    public override void Opened()
    {
        _menu.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
