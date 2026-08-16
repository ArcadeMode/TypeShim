using TypeShim;
using Library.Pages;

namespace Library.Interop;

/// <summary>
/// Wiring point: Blazor publishes bridge lifetime; JS subscribes via <see cref="Current"/>.
/// </summary>
[TSExport]
public class CounterBridgeHub
{
    public static CounterBridgeHub Current { get; } = new();

    private Action<CounterBridge>? _onCreate;
    private Action<CounterBridge>? _onDispose;

    private CounterBridgeHub()
    {
    }

    public void SetOnCreate(Action<CounterBridge>? callback) => _onCreate = callback;

    public void SetOnDispose(Action<CounterBridge>? callback) => _onDispose = callback;

    internal static CounterBridge Create(Counter component)
    {
        var bridge = new CounterBridge(component);
        Current._onCreate?.Invoke(bridge);
        return bridge;
    }

    internal static void Dispose(CounterBridge bridge)
    {
        bridge.ClearCallbacks();
        Current._onDispose?.Invoke(bridge);
    }
}
