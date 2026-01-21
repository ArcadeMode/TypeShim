import { describe, test, expect, beforeEach } from 'vitest';
import { ExportedClass, TaskPropertiesClass } from "@typeshim/e2e-wasm-lib";
import { delay } from "./async";
import { dateOnly, dateOffsetHour } from './date';
import { isCI } from '../suite';

describe('Task Properties Test', () => {
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
                TaskOfLongProperty: Promise.resolve(45),// new Promise(resolve => setTimeout(() => resolve(45), 1000)),
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

    test('Resolves void Task', async () => {
        await expect(testObject.TaskProperty).resolves.toBeUndefined();
    });
    test('Resolves Byte Task', async () => {
        await expect(testObject.TaskOfByteProperty).resolves.toBe(22);
    });
    test('Resolves NInt Task', async () => {
        await expect(testObject.TaskOfNIntProperty).resolves.toBe(42);
    });
    test('Resolves Short Task', async () => {
        await expect(testObject.TaskOfShortProperty).resolves.toBe(43);
    });
    test('Resolves Int Task immediately', async () => {
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    });
    test('Resolves Int Task', async () => {
        testObject.TaskOfIntProperty = delay(100).then(() => 440);
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });
    test('Resolves completed Int Task', async () => {
        testObject.TaskOfIntProperty = delay(100).then(() => 440);
        await delay(200); // wait longer than the task delay so its in completed state
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });

    test('Resolves Long Task', async () => {
        testObject.TaskOfLongProperty = delay(100).then(() => 450);
        await expect(testObject.TaskOfLongProperty).resolves.toBe(450);
    });

    // Completed task of long fails to marshall, skip in CI, keep active locally for visibility
    // https://github.com/dotnet/runtime/pull/123366
    test.skipIf(isCI)('Resolves completed Long Task', async () => {
        testObject.TaskOfLongProperty = delay(100).then(() => 450);
        await delay(200); // wait longer than the task delay so its in completed state
        await expect(testObject.TaskOfLongProperty).resolves.toBe(450);
    });

    test('Resolves Bool Task', async () => {
        await expect(testObject.TaskOfBoolProperty).resolves.toBe(true);
    });
    test('Resolves Char Task', async () => {
        await expect(testObject.TaskOfCharProperty).resolves.toBe('B');
    });
    test('Resolves String Task', async () => {
        await expect(testObject.TaskOfStringProperty).resolves.toBe("Task String");
    });
    test('Resolves Double Task', async () => {
        await expect(testObject.TaskOfDoubleProperty).resolves.toBe(67.8);
    });
    test('Resolves Float Task', async () => {
        await expect(testObject.TaskOfFloatProperty).resolves.toBe(89.0);
    });
    test('Resolves DateTime Task', async () => {
        const result = await testObject.TaskOfDateTimeProperty;
        expect(result).toBeInstanceOf(Date);
        console.log("DateTime result:", await testObject.TaskOfDateTimeProperty);
        await delay(1000);
        console.log("DateTime result:", await testObject.TaskOfDateTimeProperty);
        await delay(1000);
        console.log("DateTime result:", await testObject.TaskOfDateTimeProperty);
        expect(result).toEqual(dateOnly(new Date(Date.now())));

    });
    test('Resolves DateTimeOffset Task', async () => {
        const result = await testObject.TaskOfDateTimeOffsetProperty;
        expect(result).toBeInstanceOf(Date);
        console.log("DateTimeOffset result:", result);
        expect(result).toEqual(dateOffsetHour(dateOnly(new Date(Date.now())), 1));
    }); 
    test('Resolves Object Task', async () => {
        const result = await testObject.TaskOfObjectProperty;
        expect(result).toEqual(exportedClass.instance);
    });
    test('Resolves ExportedClass Task', async () => {
        const result = await testObject.TaskOfExportedClassProperty;
        expect(result).toBeInstanceOf(ExportedClass);
        expect(result.Id).toBe(2);
    });
    test('Resolves JSObject Task', async () => {
        const result = await testObject.TaskOfJSObjectProperty;
        expect(result).toBe(jsObject);
    });
});