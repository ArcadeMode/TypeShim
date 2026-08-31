import { describe, test, expect, beforeEach } from 'vitest';
import { OptionalParametersClass, OptionalCtorParamClass } from 'typeshim';

describe('Optional Parameters Test', () => {
    let instance: OptionalParametersClass;
    beforeEach(() => {
        instance = new OptionalParametersClass();
    });

    test('primitive int defaults applied when omitted', () => {
        expect(instance.SumWithDefaults(1)).toBe(1 + 10 + 7);
    });

    test('primitive int defaults overridden when provided', () => {
        expect(instance.SumWithDefaults(1, 2, 3)).toBe(6);
    });

    test('partial override respects remaining default', () => {
        expect(instance.SumWithDefaults(1, 2)).toBe(1 + 2 + 7);
    });

    test('string default applied and overridden', () => {
        expect(instance.Greet('World')).toBe('Hello, World');
        expect(instance.Greet('World', 'Hi')).toBe('Hi, World');
    });

    test('bool default applied and overridden', () => {
        expect(instance.Flag()).toBe(true);
        expect(instance.Flag(false)).toBe(false);
    });

    test('double default applied and overridden', () => {
        expect(instance.Scale(2)).toBeCloseTo(3);
        expect(instance.Scale(2, 4)).toBeCloseTo(8);
    });

    test('nullable value-type null default', () => {
        expect(instance.NullableOrFallback()).toBe(-1);
        expect(instance.NullableOrFallback(5)).toBe(5);
    });

    test('nullable value-type non-null default', () => {
        expect(instance.NullableWithDefault()).toBe(42);
        expect(instance.NullableWithDefault(9)).toBe(9);
    });

    test('char default applied and overridden', () => {
        expect(instance.CharOrDefault()).toBe('Z');
        expect(instance.CharOrDefault('q')).toBe('q');
    });

    test('nullable char default applied and overridden', () => {
        expect(instance.NullableCharOrFallback()).toBe('none');
        expect(instance.NullableCharOrFallback('x')).toBe('x');
    });

    test('nullable int array default null applied and overridden', () => {
        expect(instance.ArrayLengthOrFallback()).toBe(-1);
        expect(instance.ArrayLengthOrFallback([1, 2, 3])).toBe(3);
    });

    test('nullable string array default (default literal) applied and overridden', () => {
        expect(instance.JoinOrFallback()).toBe('none');
        expect(instance.JoinOrFallback(['a', 'b'])).toBe('a,b');
    });

    test('nullable task default null applied and overridden', async () => {
        await expect(instance.AwaitOrFallback()).resolves.toBe(-1);
        await expect(instance.AwaitOrFallback(Promise.resolve(5))).resolves.toBe(5);
    });

    test('static method optional parameter applied and overridden', () => {
        expect(OptionalParametersClass.StaticSum(1)).toBe(101);
        expect(OptionalParametersClass.StaticSum(1, 2)).toBe(3);
    });

    test('DateTime default omitted marshals to DateTime.MinValue', () => {
        expect(instance.DateIsMinValue()).toBe(true);
    });

    test('DateTimeOffset default omitted marshals to DateTimeOffset.MinValue', () => {
        expect(instance.DateOffsetIsMinValue()).toBe(true);
    });

    test('explicitly passed DateTime is not treated as default', () => {
        expect(instance.DateIsMinValue(new Date(Date.UTC(2020, 0, 1)))).toBe(false);
        expect(instance.YearOf(new Date(Date.UTC(2020, 0, 1)))).toBe(2020);
    });
});

describe('Optional Constructor Parameter Test', () => {
    test('constructor default applied and initializer omitted', () => {
        const obj = new OptionalCtorParamClass();
        expect(obj.Seed).toBe(3);
        expect(obj.Label).toBeNull();
    });

    test('constructor parameter overridden, initializer omitted', () => {
        const obj = new OptionalCtorParamClass(9);
        expect(obj.Seed).toBe(9);
        expect(obj.Label).toBeNull();
    });

    test('constructor parameter overridden and initializer provided', () => {
        const obj = new OptionalCtorParamClass(9, { Label: 'hello' });
        expect(obj.Seed).toBe(9);
        expect(obj.Label).toBe('hello');
    });
});
