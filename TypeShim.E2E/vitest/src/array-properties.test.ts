import { describe, test, expect, beforeEach } from 'vitest';
import { ArrayPropertiesClass, ExportedClass } from "@typeshim/e2e-wasm-lib";

describe('Array Properties Test', () => {
    let exportedClass: ExportedClass;
    let testObject: ArrayPropertiesClass;
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 3 });
        testObject = new ArrayPropertiesClass({
            ByteArrayProperty: [1, 2, 3],
            IntArrayProperty: [7, 8, 9],
            StringArrayProperty: ["one", "two", "three"],
            DoubleArrayProperty: [1.1, 2.2, 3.3],
            JSObjectArrayProperty: [{ a: 1 }, { b: 2 }, { c: 3 }],
            ObjectArrayProperty: [exportedClass.instance],
            ExportedClassArrayProperty: [exportedClass],
        });
    });

    test('Byte array property', () => {
        const original = testObject.ByteArrayProperty.slice();
        testObject.ByteArrayProperty[0] = 42;
        expect(testObject.ByteArrayProperty).toStrictEqual(original);
    });

    test('Does not mutate Int array property', () => {
        const original = testObject.IntArrayProperty.slice();
        testObject.IntArrayProperty[0] = 42;
        expect(testObject.IntArrayProperty).toStrictEqual(original);
    });

    test('Initialized ExportedClass array property', () => {
        expect(testObject.ExportedClassArrayProperty).toBeInstanceOf(Array);
        expect(testObject.ExportedClassArrayProperty.length).toBe(1);
        const item = testObject.ExportedClassArrayProperty[0];
        expect(item).toBeInstanceOf(ExportedClass);
        expect(item.Id).toBe(exportedClass.Id);
        // TODO: fix identity (https://github.com/ArcadeMode/TypeShim/issues/20)
        // expect(item).toBe(exportedClass);
    });

    test('Initializer JSObject array property', () => {
        expect(testObject.JSObjectArrayProperty).toBeInstanceOf(Array);
        expect(testObject.JSObjectArrayProperty.length).toBe(3);
        expect(testObject.JSObjectArrayProperty[0]).toMatchObject({ a: 1 });
        expect(testObject.JSObjectArrayProperty[1]).toMatchObject({ b: 2 });
        expect(testObject.JSObjectArrayProperty[2]).toMatchObject({ c: 3 });
    });

    test('ExportedClass array property set with ExportedClass.Initializer', () => {
        const exportedClassInitializer = { Id: 12345 };
        testObject.ExportedClassArrayProperty = [exportedClassInitializer];
        expect(testObject.ExportedClassArrayProperty).toBeInstanceOf(Array);
        expect(testObject.ExportedClassArrayProperty.length).toBe(1);
        const item = testObject.ExportedClassArrayProperty[0];
        expect(item).toBeInstanceOf(ExportedClass);
        expect(item.Id).toBe(12345);
    });

    test('ExportedClass array property set with ExportedClass.Initializer', () => {
        const exportedClassInitializer = { Id: 12345 };
        const testObject = new ArrayPropertiesClass({
            ByteArrayProperty: [],
            IntArrayProperty: [],
            StringArrayProperty: [],
            DoubleArrayProperty: [],
            JSObjectArrayProperty: [],
            ObjectArrayProperty: [],
            ExportedClassArrayProperty: [exportedClassInitializer],
        });

        expect(testObject.ExportedClassArrayProperty).toBeInstanceOf(Array);
        expect(testObject.ExportedClassArrayProperty.length).toBe(1);
        const item = testObject.ExportedClassArrayProperty[0];
        expect(item).toBeInstanceOf(ExportedClass);
        expect(item.Id).toBe(12345);
    });

    test('Object from ObjectArrayProperty has reference equality', () => {
        const item = testObject.ObjectArrayProperty[0];
        expect(item).toBe(exportedClass.instance);
    });
});