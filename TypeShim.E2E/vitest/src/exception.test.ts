import { describe, test, expect, beforeEach } from 'vitest';
import { ExportedClass, SimplePropertiesTest, TaskPropertiesClass } from "@typeshim/e2e-wasm-lib";
import { delay } from "./async";
import { dateOnly, dateOffsetHour } from './date';
import { isCI, skipInvalidPromiseResolveValueTests } from '../suite';

describe('Task Properties Test', () => {
    let exportedClass: ExportedClass;
    let taskTestObject: TaskPropertiesClass;
    let simpleTestObject: SimplePropertiesTest;
    let jsObject = { baz: "qux" };
    let dateNow = new Date();
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        taskTestObject = new TaskPropertiesClass({
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
        simpleTestObject = new SimplePropertiesTest({
            NIntProperty: 1,
            ByteProperty: 2,
            ShortProperty: 3,
            IntProperty: 4,
            LongProperty: 5,
            BoolProperty: true,
            StringProperty: "Test",
            CharProperty: 'A',
            CharNullableProperty: null,
            DoubleProperty: 6.7,
            FloatProperty: 8.9,
            DateTimeProperty: dateNow,
            DateTimeOffsetProperty: dateNow,
            ExportedClassProperty: exportedClass,
            ObjectProperty: exportedClass.instance,
            JSObjectProperty: jsObject,
        });
    });

    test('Setting property to throwing void Promise sets throwing Task', async () => {
        taskTestObject.TaskProperty = delay(10).then(() => { throw new Error("Thrown error"); });
        await expect(taskTestObject.TaskProperty).rejects.toThrow("Thrown error");
    });

    test('Setting property to rejecting void Promise sets throwing Task', async () => {
        taskTestObject.TaskProperty = Promise.reject(new Error("Rejected promise"));
        await expect(taskTestObject.TaskProperty).rejects.toThrow("Rejected promise");
    });

    test('Setting property to out-of-range NInt throws', () => {
        expect(() => simpleTestObject.NIntProperty = -2).toThrow();
        expect(() => simpleTestObject.NIntProperty = -3).toThrow();
    });

    test.skipIf(skipInvalidPromiseResolveValueTests)('Setting property to Promise of out-of-range NInt sets throwing Task', async () => {
        taskTestObject.TaskOfNIntProperty = Promise.resolve(-1);
        await expect(taskTestObject.TaskOfNIntProperty).rejects.toThrow(); // try to get the task back, it should throw the assertion
    });

    test('Cannot set out of range short', () => {
        expect(() => simpleTestObject.ShortProperty = 1<<15).toThrow();
        expect(() => simpleTestObject.ShortProperty = -(1<<15) - 1).toThrow();
    });

    test.skipIf(skipInvalidPromiseResolveValueTests)('Cannot set out of range Task of short', async () => {
        taskTestObject.TaskOfShortProperty = Promise.resolve(1<<15);     
        await expect(taskTestObject.TaskOfShortProperty).rejects.toThrow(); // try to get the task back, it should throw the assertion
    });
});