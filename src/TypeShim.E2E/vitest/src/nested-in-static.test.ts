import { describe, test, expect } from 'vitest';
import { NestedInStaticContainer } from 'typeshim';

describe('Nested Class In Static Container', () => {
    test('Invokes a static method on the container that returns a nested instance', () => {
        const widget = NestedInStaticContainer.MakeWidget(4);
        expect(widget).toBeInstanceOf(NestedInStaticContainer.Widget);
        expect(widget.Size).toBe(4);
    });

    test('Invokes a static method returning an array of nested instances', () => {
        const widgets = NestedInStaticContainer.MakeWidgets([1, 2, 3]);
        expect(widgets).toHaveLength(3);
        expect(widgets[0]).toBeInstanceOf(NestedInStaticContainer.Widget);
        expect(widgets.map(w => w.Size)).toEqual([1, 2, 3]);
    });

    test('Instantiates the nested non-static class directly', () => {
        const widget = new NestedInStaticContainer.Widget({ Size: 8 });
        expect(widget).toBeInstanceOf(NestedInStaticContainer.Widget);
        expect(widget.Size).toBe(8);
    });

    test('Calls an instance method on the nested class', () => {
        const widget = new NestedInStaticContainer.Widget({ Size: 21 });
        expect(widget.Doubled()).toBe(42);
    });

    test('Mutates the nested class instance and reflects it', () => {
        const widget = NestedInStaticContainer.MakeWidget(5);
        widget.Size = 50;
        expect(widget.Doubled()).toBe(100);
    });
});
