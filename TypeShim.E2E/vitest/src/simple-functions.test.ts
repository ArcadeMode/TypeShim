import { describe, test, expect, beforeEach } from 'vitest';
import { ExportedClass, SimpleReturnMethodsClass, TaskPropertiesClass } from '@typeshim/e2e-wasm-lib';
import { dateOffsetHour, dateOnly } from './date';

describe('Simple Functions Test', () => {
    let exportedClass: ExportedClass;
    let testObject: SimpleReturnMethodsClass;
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new SimpleReturnMethodsClass();
    });

    test('Void returning parameterless method', () => {
        expect(testObject.VoidMethod()).toBe(undefined);
    });

    test('Int returning parameterless method', () => {
        expect(testObject.IntMethod()).toBe(42);
    });

    test('String returning parameterless method', () => {
        expect(testObject.StringMethod()).toBe("Hello, from .NET");
    });

    test('Bool returning parameterless method', () => {
        expect(testObject.BoolMethod()).toBe(true);
    });

    test('Double returning parameterless method', () => {
        expect(testObject.DoubleMethod()).toBeCloseTo(3.14159);
    });

    test('DateTime returning parameterless method', () => {
        const result = testObject.DateTimeMethod();
        expect(result).toBeInstanceOf(Date);
        expect(result).toEqual(new Date(1995, 3, 1));
    });

    test('DateTime.Now.Date returning parameterless method', () => {
        const result = testObject.DateTimeNowDateMethod();
        expect(result).toBeInstanceOf(Date);
        const now = new Date();
        expect(result).toEqual(dateOnly(now));
    });

    test('DateTimeOffset returning parameterless method', () => {
        const result = testObject.DateTimeOffsetMethod();
        expect(result).toBeInstanceOf(Date);

        var expectedUtcMillis = Date.UTC(1998, 3, 20);
        const dateWithTimezoneOffset = dateOffsetHour(new Date(expectedUtcMillis), -3);
        expect(result).toEqual(dateWithTimezoneOffset);
    });

    test('ExportedClass returning parameterless method', () => {
        const result = testObject.ExportedClassMethod();
        expect(result).toBeInstanceOf(ExportedClass);
        expect(result.Id).toBe(420);
    });
});
