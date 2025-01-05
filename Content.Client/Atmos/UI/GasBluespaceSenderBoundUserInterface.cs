using Content.Shared.Atmos;
using Content.Shared.Atmos.Monitor;
using Content.Shared.Atmos.Monitor.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Atmos.UI;

public sealed class GasBluespaceSenderBoundUserInterface : BoundUserInterface
{
    private GasBluespaceSenderWindow? _window;

    public GasBluespaceSenderBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GasBluespaceSenderWindow>();
        _window.SetEntity(Owner);

        _window.PowerButton.OnPressed += _ => OnPowerButtonPressed();
        _window.AtmosAlarmThresholdChanged += OnThresholdChanged;
    }

    private void OnPowerButtonPressed()
    {
        if (_window is null) return;

        _window.SetActive(!_window.Active);
    }

    private void OnThresholdChanged(string address, AtmosMonitorThresholdType type, AtmosAlarmThreshold threshold, Gas? gas = null)
    {
        SendMessage(new AirAlarmUpdateAlarmThresholdMessage(address, type, threshold, gas));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AirAlarmUIState cast || _window == null)
        {
            return;
        }

        _window.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }
}