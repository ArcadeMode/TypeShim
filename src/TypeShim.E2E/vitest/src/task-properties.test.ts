import { describe, test, expect, beforeEach } from 'vitest';
import { ExportedClass, TaskPropertiesClass } from "@typeshim/e2e-wasm-lib";
import { delay } from "./async";
import { dateOnly, dateOffsetHour } from './date';
import { isCI } from '../suite';

describe('Task Properties Test', () => {
    let exportedClass: ExportedClass;
    let testObject: TaskPropertiesClass;
    let jsObject = { baz: "qux" };
    let dateNow = new Date();
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new TaskPropertiesClass({
            TaskProperty: Promise.resolve(),
            TaskOfByteProperty: Promise.resolve(32),
            TaskOfNIntProperty: Promise.resolve(42),
            TaskOfShortProperty: Promise.resolve(43),
            TaskOfIntProperty: Promise.resolve(44),
            TaskOfLongProperty: Promise.resolve(45),
            TaskOfBoolProperty: Promise.resolve(true),
            TaskOfCharProperty: Promise.resolve('B'),
            TaskOfStringProperty: Promise.resolve("Task String"),
            TaskOfDoubleProperty: Promise.resolve(67.8),
            TaskOfFloatProperty: Promise.resolve(89.0),
            TaskOfDateTimeProperty: Promise.resolve(dateNow),
            TaskOfDateTimeOffsetProperty: Promise.resolve(dateNow),
            TaskOfObjectProperty: Promise.resolve(exportedClass.instance),
            TaskOfExportedClassProperty: Promise.resolve(exportedClass),
            TaskOfJSObjectProperty: Promise.resolve(jsObject),
        });
    });

    test('Resolves void Task', async () => {
        await expect(testObject.TaskProperty).resolves.toBeUndefined();
    });
    
    test('Can set and resolve void Task', async () => {
        testObject.TaskProperty = delay(10);
        await expect(testObject.TaskProperty).resolves.toBeUndefined();
    });
    
    test('Can set and resolve completed void Task', async () => {
        testObject.TaskProperty = Promise.resolve();
        await expect(testObject.TaskProperty).resolves.toBeUndefined();
    });

    test('Resolves Byte Task', async () => {
        await expect(testObject.TaskOfByteProperty).resolves.toBe(32);
    });
    
    test('Can set and resolve Byte Task', async () => {
        testObject.TaskOfByteProperty = delay(10).then(() => 255);
        await expect(testObject.TaskOfByteProperty).resolves.toBe(255);
    });
    
    test('Can set and resolve completed Byte Task', async () => {
        testObject.TaskOfByteProperty = Promise.resolve(128);
        await expect(testObject.TaskOfByteProperty).resolves.toBe(128);
    });

    test('Resolves NInt Task', async () => {
        await expect(testObject.TaskOfNIntProperty).resolves.toBe(42);
    });
    
    test('Can set and resolve NInt Task', async () => {
        testObject.TaskOfNIntProperty = delay(10).then(() => 420);
        await expect(testObject.TaskOfNIntProperty).resolves.toBe(420);
    });
    
    test('Can set and resolve completed NInt Task', async () => {
        testObject.TaskOfNIntProperty = Promise.resolve(420);
        await expect(testObject.TaskOfNIntProperty).resolves.toBe(420);
    });

    test('Resolves Short Task', async () => {
        await expect(testObject.TaskOfShortProperty).resolves.toBe(43);
    });
    
    test('Resolves Int Task immediately', async () => {
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    });
    
    test('Resolves Int Task', async () => {
        testObject.TaskOfIntProperty = delay(10).then(() => 440);
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });
    
    test('Resolves completed Int Task', async () => {
        testObject.TaskOfIntProperty = delay(10).then(() => 440);
        await delay(20); // wait longer than the task delay so its in completed state
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });

    test('Resolves Long Task', async () => {
        testObject.TaskOfLongProperty = delay(10).then(() => 450);
        await expect(testObject.TaskOfLongProperty).resolves.toBe(450);
    });

    // Completed task of long fails to marshall, skip in CI, keep active locally for visibility
    // https://github.com/dotnet/runtime/pull/123366
    test.skipIf(isCI)('Resolves completed Long Task', async () => {
        testObject.TaskOfLongProperty = delay(10).then(() => 450);
        await delay(20); // wait longer than the task delay so its in completed state
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

    test('Doesnt preserve String Task object identity', () => {
        // each access returns a new promise
        expect(testObject.TaskOfStringProperty).not.toBe(testObject.TaskOfStringProperty);
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
        expect(result).toEqual(dateNow);
    });
    
    test('Can set and resolve DateTime Task', async () => {
        const newDate = dateOnly(dateNow);
        testObject.TaskOfDateTimeProperty = Promise.resolve(newDate);
        const result = await testObject.TaskOfDateTimeProperty;
        expect(result).toBeInstanceOf(Date);
        expect(result).toEqual(newDate);
    });

    test('Resolves DateTimeOffset Task', async () => {
        const result = await testObject.TaskOfDateTimeOffsetProperty;
        expect(result).toBeInstanceOf(Date);
        expect(result).toEqual(dateNow);
    });
    
    test('Can set and resolve DateTimeOffset Task', async () => {
        const newDate = dateOffsetHour(dateNow, 2);
        testObject.TaskOfDateTimeOffsetProperty = Promise.resolve(newDate);
        const result = await testObject.TaskOfDateTimeOffsetProperty;
        expect(result).toBeInstanceOf(Date);
        expect(result).toEqual(newDate);
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

    test('Doesnt preserve ExportedClass Task identity', () => {
        const promise = testObject.TaskOfExportedClassProperty;
        const promiseAgain = testObject.TaskOfExportedClassProperty;
        expect(promise).not.toBe(promiseAgain); // promise is a new object each time
    });

    test('Preserves ExportedClass Task result identity', async () => {
        const result = await testObject.TaskOfExportedClassProperty;
        const resultAgain = await testObject.TaskOfExportedClassProperty;
        expect(result).toBe(resultAgain); // reference equality check
    });

    test('Preserves ExportedClass Task result identity with initializer', async () => {
        testObject.TaskOfExportedClassProperty = Promise.resolve(new ExportedClass({ Id: 12345 }));
        const result = await testObject.TaskOfExportedClassProperty;
        const resultAgain = await testObject.TaskOfExportedClassProperty;
        expect(result).toBe(resultAgain); // reference equality check
    });
    
    test('Resolves JSObject Task', async () => {
        const result = await testObject.TaskOfJSObjectProperty;
        expect(result).toBe(jsObject);
    });
});