import { describe, test, expect, beforeEach } from 'vitest';
import { CrossNamespaceClass, ExternalClass, ExternalEnum } from 'typeshim';

describe('Cross Namespace References', () => {
    let testObject: CrossNamespaceClass;
    beforeEach(() => {
        testObject = new CrossNamespaceClass({
            Reference: new ExternalClass({ Id: 1, Name: 'ref' }),
            NullableReference: null,
            ReferenceArray: [],
            ReferenceFunc: (value: ExternalClass) => value,
            EnumReference: ExternalEnum.First,
            NullableEnumReference: null,
            EnumReferenceArray: [],
        });
    });

    test('Reads a cross-namespace reference property', () => {
        expect(testObject.Reference).toBeInstanceOf(ExternalClass);
        expect(testObject.Reference.Describe()).toBe('1:ref');
    });

    test('Mutates a cross-namespace reference property', () => {
        testObject.Reference = new ExternalClass({ Id: 2, Name: 'next' });
        expect(testObject.Reference.Name).toBe('next');
    });

    test('Handles a nullable cross-namespace reference property', () => {
        expect(testObject.NullableReference).toBeNull();
        testObject.NullableReference = new ExternalClass({ Id: 3, Name: 'set' });
        expect(testObject.NullableReference).toBeInstanceOf(ExternalClass);
        expect(testObject.NullableReference!.Name).toBe('set');
    });

    test('Handles an array of cross-namespace references', () => {
        const roundTripped = testObject.EchoArray([
            new ExternalClass({ Id: 4, Name: 'a' }),
            new ExternalClass({ Id: 5, Name: 'b' }),
        ]);
        expect(roundTripped).toHaveLength(2);
        expect(roundTripped[0]).toBeInstanceOf(ExternalClass);
        expect(roundTripped.map(x => x.Name)).toEqual(['a', 'b']);
    });

    test('Passes and returns a cross-namespace reference through a method', () => {
        const result = testObject.Echo(new ExternalClass({ Id: 6, Name: 'echo' }));
        expect(result).toBeInstanceOf(ExternalClass);
        expect(result.Describe()).toBe('6:echo');
    });

    test('Passes and returns a nullable cross-namespace reference through a method', () => {
        expect(testObject.EchoNullable(null)).toBeNull();
        const result = testObject.EchoNullable(new ExternalClass({ Id: 7, Name: 'maybe' }));
        expect(result).toBeInstanceOf(ExternalClass);
        expect(result!.Name).toBe('maybe');
    });

    test('Returns a cross-namespace reference nested in a Task', async () => {
        const result = await testObject.EchoAsync(new ExternalClass({ Id: 8, Name: 'async' }));
        expect(result).toBeInstanceOf(ExternalClass);
        expect(result.Describe()).toBe('8:async');
    });

    test('Invokes a delegate over cross-namespace references', () => {
        const result = testObject.InvokeFunc(value => value, new ExternalClass({ Id: 9, Name: 'fn' }));
        expect(result).toBeInstanceOf(ExternalClass);
        expect(result.Name).toBe('fn');
    });

    test('Reads a cross-namespace reference through a delegate property', () => {
        const echoed = testObject.ReferenceFunc(new ExternalClass({ Id: 10, Name: 'prop' }));
        expect(echoed).toBeInstanceOf(ExternalClass);
        expect(echoed.Name).toBe('prop');
    });

    test('Cross-namespace enum members have correct numeric values', () => {
        expect(ExternalEnum.First).toBe(0);
        expect(ExternalEnum.Second).toBe(1);
        expect(ExternalEnum.Third).toBe(2);
    });

    test('Reads and mutates a cross-namespace enum property', () => {
        expect(testObject.EnumReference).toBe(ExternalEnum.First);
        testObject.EnumReference = ExternalEnum.Third;
        expect(testObject.EnumReference).toBe(ExternalEnum.Third);
    });

    test('Handles a nullable cross-namespace enum property', () => {
        expect(testObject.NullableEnumReference).toBeNull();
        testObject.NullableEnumReference = ExternalEnum.Second;
        expect(testObject.NullableEnumReference).toBe(ExternalEnum.Second);
    });

    test('Handles an array of cross-namespace enums', () => {
        const roundTripped = testObject.EchoEnumArray([ExternalEnum.First, ExternalEnum.Third]);
        expect(Array.from(roundTripped)).toEqual([ExternalEnum.First, ExternalEnum.Third]);
    });

    test('Passes and returns a cross-namespace enum through a method', () => {
        expect(testObject.EchoEnum(ExternalEnum.Second)).toBe(ExternalEnum.Second);
    });

    test('Passes and returns a nullable cross-namespace enum through a method', () => {
        expect(testObject.EchoNullableEnum(null)).toBeNull();
        expect(testObject.EchoNullableEnum(ExternalEnum.Third)).toBe(ExternalEnum.Third);
    });

    test('Returns a cross-namespace enum nested in a Task', async () => {
        expect(await testObject.EchoEnumAsync(ExternalEnum.Second)).toBe(ExternalEnum.Second);
    });

    test('Invokes a delegate over cross-namespace enums', () => {
        const result = testObject.InvokeEnumFunc(value => value, ExternalEnum.Third);
        expect(result).toBe(ExternalEnum.Third);
    });
});
