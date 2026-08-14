namespace Korp.Inventory.Api.Application;

public sealed class FailureSimulationState
{
    private int enabled;

    public bool Enabled => Volatile.Read(ref enabled) == 1;

    public void SetEnabled(bool value) =>
        Interlocked.Exchange(ref enabled, value ? 1 : 0);
}
