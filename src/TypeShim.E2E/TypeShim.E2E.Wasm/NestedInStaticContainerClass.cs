using System;

namespace TypeShim.E2E.Wasm;

[TSExport]
public static class NestedInStaticContainer
{
    public static Widget MakeWidget(int size) => new() { Size = size };

    public static Widget[] MakeWidgets(int[] sizes)
    {
        Widget[] widgets = new Widget[sizes.Length];
        for (int i = 0; i < sizes.Length; i++)
        {
            widgets[i] = new Widget { Size = sizes[i] };
        }
        return widgets;
    }

    public class Widget
    {
        public int Size { get; set; }

        public int Doubled() => Size * 2;
    }
}
