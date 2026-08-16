using TypeShim;
using Library.Pages;

namespace Library.Interop;

/// <summary>
/// Typed facade handed to JS for a live Counter instance.
/// Blazor owns UI state; this bridge exposes count and click notifications.
/// </summary>
[TSExport]
public class CounterBridge
{
    private readonly Counter _component;
    private Action<int, double, double>? _onCountChange;

    internal CounterBridge(Counter component)
    {
        _component = component;
    }

    public int Count => _component.Count;

    public void SetOnCountChange(Action<int, double, double>? callback) => _onCountChange = callback;

    internal void ClearCallbacks() => _onCountChange = null;

    internal void NotifyCountChanged(double x, double y) => _onCountChange?.Invoke(Count, x, y);
}
