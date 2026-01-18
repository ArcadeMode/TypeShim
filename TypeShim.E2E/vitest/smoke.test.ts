import { describe, it, expect, beforeEach } from 'vitest';
import { ArrayPropertiesClass, ExportedClass, SimplePropertiesTest, TaskPropertiesClass } from '../e2e-wasm-app/typeshim';

describe('wasm runtime bootstrap', () => {
    let exportedClass: ExportedClass;
    let testObject: SimplePropertiesTest;
    let jsObject = { foo: "bar" };
    let dateNow = new Date();
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new SimplePropertiesTest({
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
            DateTimeProperty: dateNow,
            DateTimeOffsetProperty: dateNow,
            ExportedClassProperty: exportedClass,
            ObjectProperty: exportedClass.instance,
            JSObjectProperty: jsObject,
        });
    });

    it('Snapshot has property-value equality', async () => {
        expect(testObject).toBeDefined();
        const snapshot = SimplePropertiesTest.materialize(testObject);
        expect(testObject).toMatchObject(snapshot); // each property in snapshot should have a matching value in testObject
    });
    it('Returns ExportedClass property correctly', () => {
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(2);
    });
    it('Returns JSObject property by reference', () => {
        expect(testObject.JSObjectProperty).toBe(jsObject);
        const obj = testObject.JSObjectProperty as any;
        obj.bar = 123;
        expect(testObject.JSObjectProperty).toHaveProperty("bar", 123);
        expect(testObject.JSObjectProperty).toHaveProperty("foo", "bar");
    });
    it('Returns DateTime property as new instance', () => {
        // dates are value object in dotnet, hence the new instance
        expect(testObject.DateTimeProperty).toBeInstanceOf(Date);
        expect(testObject.DateTimeProperty).not.toBe(dateNow);
        expect(testObject.DateTimeProperty).toEqual(dateNow);
    });
    it('Returns DateTimeOffset property as new instance', () => {
        // dates are value object in dotnet, hence the new instance
        expect(testObject.DateTimeOffsetProperty).toBeInstanceOf(Date);
        expect(testObject.DateTimeOffsetProperty).not.toBe(dateNow);
        expect(testObject.DateTimeOffsetProperty).toEqual(dateNow); 
    });
    it('Returns Long property by value', () => {
        expect(testObject.LongProperty).toBe(5);
    });
    it('Mutates Long property', () => {
        testObject.LongProperty = 50;
        expect(testObject.LongProperty).toBe(50);
    });
});

describe('Task tests', () => {
    let exportedClass: ExportedClass;
    let testObject: TaskPropertiesClass;
    let jsObject = { baz: "qux" };
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
            testObject = new TaskPropertiesClass({
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
            TaskOfJSObjectProperty: Promise.resolve(jsObject),
        });
    });

    it('Resolves void Task', async () => {
        await expect(testObject.TaskProperty).resolves.toBeUndefined();
    });
    it('Resolves Byte Task', async () => {
        await expect(testObject.TaskOfByteProperty).resolves.toBe(22);
    });
    it('Resolves NInt Task', async () => {
        await expect(testObject.TaskOfNIntProperty).resolves.toBe(42);
    });
    it('Resolves Short Task', async () => {
        await expect(testObject.TaskOfShortProperty).resolves.toBe(43);
    });
    it('Resolves Int Task', async () => {
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    });
    it('Resolves Long Task', async () => {
        testObject.TaskOfLongProperty.then(value => {
           console.log("Long task resolved to:", value);
        });
        await expect(testObject.TaskOfLongProperty).resolves.toBe(45);
    });
    it('Resolves Bool Task', async () => {
        await expect(testObject.TaskOfBoolProperty).resolves.toBe(true);
    });
    it('Resolves Char Task', async () => {
        await expect(testObject.TaskOfCharProperty).resolves.toBe('B');
    });
    it('Resolves String Task', async () => {
        await expect(testObject.TaskOfStringProperty).resolves.toBe("Task String");
    });
    it('Resolves Double Task', async () => {
        await expect(testObject.TaskOfDoubleProperty).resolves.toBe(67.8);
    });
    it('Resolves Float Task', async () => {
        await expect(testObject.TaskOfFloatProperty).resolves.toBe(89.0);
    });
    it('Resolves DateTime Task', async () => {
        const result = await testObject.TaskOfDateTimeProperty;
        expect(result).toBeInstanceOf(Date);
    });
    it('Resolves DateTimeOffset Task', async () => {
        const result = await testObject.TaskOfDateTimeOffsetProperty;
        expect(result).toBeInstanceOf(Date);
    }); 
    it('Resolves Object Task', async () => {
        const result = await testObject.TaskOfObjectProperty;
        expect(result).toEqual(exportedClass.instance);
    });
    it('Resolves ExportedClass Task', async () => {
        const result = await testObject.TaskOfExportedClassProperty;
        expect(result).toBeInstanceOf(ExportedClass);
        expect(result.Id).toBe(2);
    });
    it('Resolves JSObject Task', async () => {
        const result = await testObject.TaskOfJSObjectProperty;
        expect(result).toBe(jsObject);
    });
});

describe('Array tests', () => {
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