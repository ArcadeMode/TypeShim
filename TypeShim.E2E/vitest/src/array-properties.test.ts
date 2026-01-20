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

    it('Does not mutate Byte array property', () => {
        const original = testObject.ByteArrayProperty.slice();
        testObject.ByteArrayProperty[0] = 42;
        expect(testObject.ByteArrayProperty).toStrictEqual(original);
    });

    it('Does not mutate Int array property', () => {
        const original = testObject.IntArrayProperty.slice();
        testObject.IntArrayProperty[0] = 42;
        expect(testObject.IntArrayProperty).toStrictEqual(original);
    });
});