using System;
using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class NestedTypesContainer
{
    public class Item
    {
        public int Value { get; set; }
        public string Label { get; set; } = "";

        public string Describe() => $"{Label}:{Value}";
    }

    public static class Factory
    {
        public static Item Create(int value, string label) => new() { Value = value, Label = label };
    }

    public enum Kind
    {
        Alpha,
        Beta,
        Gamma
    }

    public Item ItemProperty { get; set; } = new();
    public Item? NullableItem { get; set; }
    public Item[] ItemArray { get; set; } = [];
    public required Func<Item, Item> ItemFunc { get; set; }

    public Kind KindProperty { get; set; }
    public Kind? NullableKind { get; set; }
    public Kind[] KindArray { get; set; } = [];

    public Item EchoItem(Item item) => item;

    public Item? EchoNullableItem(Item? item) => item;

    public Item[] EchoItemArray(Item[] items) => items;

    public Task<Item> EchoItemAsync(Item item) => Task.FromResult(item);

    public Item InvokeItemFunc(Func<Item, Item> func, Item item) => func(item);

    public Kind EchoKind(Kind kind) => kind;

    public Kind? EchoNullableKind(Kind? kind) => kind;

    public Kind[] EchoKindArray(Kind[] kinds) => kinds;

    public Task<Kind> EchoKindAsync(Kind kind) => Task.FromResult(kind);

    public Kind InvokeKindFunc(Func<Kind, Kind> func, Kind kind) => func(kind);
}
