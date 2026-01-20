import { describe, it, expect, beforeEach } from 'vitest';
import { ExportedClass, TaskPropertiesClass } from "@typeshim/e2e-wasm-lib";
import { delay } from "./utils";

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
    it('Resolves Int Task immediately', async () => {
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    });
    it('Resolves Int Task immediately from completed state', async () => {
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
        await delay(100); // Ensure task is completed
        await expect(testObject.TaskOfIntProperty).resolves.toBe(44);
    });
    it('Resolves Int Task after 100ms', async () => {
        testObject.TaskOfIntProperty = delay(100).then(() => 440);
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });
    it('Resolves Int Task immediately after 100ms', async () => {
        testObject.TaskOfIntProperty = delay(100).then(() => 440);
        await delay(200); // wait longer than the task delay so its in completed state
        await expect(testObject.TaskOfIntProperty).resolves.toBe(440);
    });
    it('Resolves Long Task', async () => {
        testObject.TaskOfLongProperty.then(value => {
           console.log("Long task resolved to:", value);
        });
        await expect(testObject.TaskOfLongProperty).resolves.toBe(45);
        await new Promise(resolve => setTimeout(resolve, 1100)); // ensure the timeout in the Task has completed
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