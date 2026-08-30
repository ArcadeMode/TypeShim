import { describe, test, expect, beforeEach } from 'vitest';
import { EnumMethodsClass, Priority, Season } from 'typeshim';

describe('Enum Test', () => {
    let testObject: EnumMethodsClass;
    beforeEach(() => {
        testObject = new EnumMethodsClass({
            PriorityProperty: Priority.Medium,
            NullablePriorityProperty: Priority.Low,
            PriorityArrayProperty: [Priority.Low, Priority.High],
            SeasonProperty: Season.Summer,
        });
    });

    test('Enum members have correct numeric values', () => {
        expect(Priority.Low).toBe(0);
        expect(Priority.Medium).toBe(1);
        expect(Priority.High).toBe(2);
    });

    test('Real TS enum has reverse name mapping', () => {
        expect(Priority[0]).toBe('Low');
        expect(Priority[2]).toBe('High');
    });

    test('uint-backed enum preserves explicit values', () => {
        expect(Season.Spring).toBe(1);
        expect(Season.Summer).toBe(2);
        expect(Season.Autumn).toBe(3);
        expect(Season.Winter).toBe(4);
    });

    test('Enum parameter and return round-trip through .NET', () => {
        expect(testObject.EchoPriority(Priority.High)).toBe(Priority.High);
        expect(testObject.EchoPriority(Priority.Low)).toBe(Priority.Low);
    });

    test('Enum property get and set', () => {
        expect(testObject.PriorityProperty).toBe(Priority.Medium);
        testObject.PriorityProperty = Priority.High;
        expect(testObject.PriorityProperty).toBe(Priority.High);
    });

    test('Nullable enum round-trips a value through property and method', () => {
        testObject.NullablePriorityProperty = Priority.High;
        expect(testObject.NullablePriorityProperty).toBe(Priority.High);
        expect(testObject.EchoNullablePriority(Priority.Medium)).toBe(Priority.Medium);
    });

    test('Enum array round-trips element values', () => {
        // Number arrays (including enum arrays) marshal as typed arrays, like int[].
        expect(Array.from(testObject.PriorityArrayProperty)).toStrictEqual([Priority.Low, Priority.High]);
        expect(Array.from(testObject.EchoPriorityArray([Priority.High, Priority.Medium, Priority.Low])))
            .toStrictEqual([Priority.High, Priority.Medium, Priority.Low]);
    });

    test('uint-backed enum round-trips through .NET', () => {
        expect(testObject.SeasonProperty).toBe(Season.Summer);
        expect(testObject.EchoSeason(Season.Winter)).toBe(Season.Winter);
    });

    test('Task returning an enum resolves to the enum value', async () => {
        await expect(testObject.HighestPriorityTask()).resolves.toBe(Priority.High);
    });

    test('Static method returns an enum value', () => {
        expect(EnumMethodsClass.HighestPriority()).toBe(Priority.High);
    });

    test('Enum arithmetic performed in .NET round-trips correctly', () => {
        expect(testObject.NextPriority(Priority.Low)).toBe(Priority.Medium);
        expect(testObject.NextPriority(Priority.Medium)).toBe(Priority.High);
        expect(testObject.NextPriority(Priority.High)).toBe(Priority.High);
    });
});
