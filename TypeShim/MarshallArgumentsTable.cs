using System.Runtime.CompilerServices;

namespace TypeShim;

public static class MarshallArgumentsTable
{
    private static readonly ConditionalWeakTable<Type, ConditionalWeakTable<object, object>> _typeTables = new();

    public static T GetOrCreate<T>(object target, Func<T> factory) where T : class
    {
        ConditionalWeakTable<object, object> table = _typeTables.GetValue(typeof(T), _ => new ConditionalWeakTable<object, object>());
        return (T)table.GetValue(target, _ => factory()!);
    }
}