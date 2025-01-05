using Robust.Shared.Serialization;

namespace Content.Shared.Atmos.Piping.Unary.Components;

[Serializable, NetSerializable]
public sealed record GasBluespaceSenderData(float EnergyDelta);

[Serializable]
[NetSerializable]
public enum BluespaceGasSenderUiKey
{
    Key
}

[Serializable]
[NetSerializable]
public sealed class GasBluespaceSenderToggleMessage : BoundUserInterfaceMessage
{
}

[Serializable]
[NetSerializable]
public sealed class GasBluespaceSenderBoundUserInterfaceState : BoundUserInterfaceState
{
    public bool Enabled { get; }

    public GasBluespaceSenderBoundUserInterfaceState(bool enabled)
    {
        Enabled = enabled;
    }
}
