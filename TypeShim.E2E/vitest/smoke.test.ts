import { describe, it, expect } from 'vitest';
import { CompilationTest, ExportedClass } from '../e2e-wasm-app/typeshim';
describe('wasm runtime bootstrap', () => {
  it('initializes TypeShim in setup', () => {
    // If setup failed, this file won't run.
    expect(true).toBe(true);
  });

  it('can create CompilationTest instance', async () => {
    const exportedClass = new ExportedClass({ Id: 1 });
    const testObject = new CompilationTest({
        NIntProperty: 1,
        ByteProperty: 2,
        ShortProperty: 3,
        IntProperty: 4,
        LongProperty: 5,
        BoolProperty: true,
        StringProperty: "Test",
        CharProperty: 'A',
        DoubleProperty: 6.7,
        FloatProperty: 8.9,
        DateTimeProperty: new Date(),
        DateTimeOffsetProperty: new Date(),
        ExportedClassProperty: exportedClass,
        ObjectProperty: exportedClass.instance,
        JSObjectProperty: { foo: "bar" },
        TaskProperty: Promise.resolve(),
        TaskOfByteProperty: Promise.resolve(22),
        TaskOfNIntProperty: Promise.resolve(42),
        TaskOfShortProperty: Promise.resolve(43),
        TaskOfIntProperty: Promise.resolve(44),
        TaskOfLongProperty: Promise.resolve(45),
        TaskOfBoolProperty: Promise.resolve(true),
        TaskOfCharProperty: Promise.resolve('B'),
        TaskOfStringProperty: Promise.resolve("Task String"),
        TaskOfDoubleProperty: Promise.resolve(67.8),
        TaskOfFloatProperty: Promise.resolve(89.0),
        TaskOfDateTimeProperty: Promise.resolve(new Date()),
        TaskOfDateTimeOffsetProperty: Promise.resolve(new Date()),
        TaskOfObjectProperty: Promise.resolve(exportedClass.instance),
        TaskOfExportedClassProperty: Promise.resolve(exportedClass),
        TaskOfJSObjectProperty: Promise.resolve({ baz: "qux" }),
        ByteArrayProperty: [1, 2, 3],
        IntArrayProperty: [7, 8, 9],
        StringArrayProperty: ["one", "two", "three"],
        DoubleArrayProperty: [1.1, 2.2, 3.3],
        JSObjectArrayProperty: [{ a: 1 }, { b: 2 }, { c: 3 }],
        ObjectArrayProperty: [exportedClass.instance],
        ExportedClassArrayProperty: [exportedClass],
    });

    expect(testObject).toBeDefined();
    expect(CompilationTest.materialize(testObject)).toBeDefined();

    //expect(testObject).toEqual(CompilationTest.materialize(testObject));
    await expect(testObject.TaskOfCharProperty).resolves.toBe('B');
    await expect(testObject.TaskOfStringProperty).resolves.toBe("Task String");
    await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    await expect(testObject.TaskOfDoubleProperty).resolves.toBe(67.8);
    await expect(testObject.TaskOfFloatProperty).resolves.toBe(89.0);
    await expect(testObject.TaskOfBoolProperty).resolves.toBe(true);
    await expect(testObject.TaskOfByteProperty).resolves.toBe(22);
    await expect(testObject.TaskOfNIntProperty).resolves.toBe(42);
    await expect(testObject.TaskOfShortProperty).resolves.toBe(43);
    //await expect(testObject.TaskOfLongProperty).resolves.toBe(45);
    //await expect(testObject.TaskOfLongProperty).resolves.toBe(45);
  });
});
