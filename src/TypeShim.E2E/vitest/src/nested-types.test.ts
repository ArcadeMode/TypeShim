import { describe, test, expect, beforeEach } from 'vitest';
import { NestedTypesContainer } from 'typeshim';

describe('Nested Types', () => {
    let testObject: NestedTypesContainer;
    beforeEach(() => {
        testObject = new NestedTypesContainer({
            ItemFunc: (item: NestedTypesContainer.Item) => item,
            ItemProperty: new NestedTypesContainer.Item({ Value: 1, Label: 'init' }),
            NullableItem: null,
            ItemArray: [],
            KindProperty: NestedTypesContainer.Kind.Alpha,
            NullableKind: null,
            KindArray: [],
        });
    });

    test('Instantiates a nested instance class', () => {
        const item = new NestedTypesContainer.Item({ Value: 5, Label: 'five' });
        expect(item).toBeInstanceOf(NestedTypesContainer.Item);
        expect(item.Value).toBe(5);
        expect(item.Label).toBe('five');
        expect(item.Describe()).toBe('five:5');
    });

    test('Mutates a nested instance class property', () => {
        const item = new NestedTypesContainer.Item({ Value: 1, Label: 'one' });
        item.Value = 42;
        item.Label = 'answer';
        expect(item.Describe()).toBe('answer:42');
    });

    test('Invokes a nested static class method', () => {
        const item = NestedTypesContainer.Factory.Create(7, 'seven');
        expect(item).toBeInstanceOf(NestedTypesContainer.Item);
        expect(item.Describe()).toBe('seven:7');
    });

    test('Nested enum members have correct numeric values', () => {
        expect(NestedTypesContainer.Kind.Alpha).toBe(0);
        expect(NestedTypesContainer.Kind.Beta).toBe(1);
        expect(NestedTypesContainer.Kind.Gamma).toBe(2);
    });

    test('Reads and mutates a nested class reference property', () => {
        expect(testObject.ItemProperty).toBeInstanceOf(NestedTypesContainer.Item);
        expect(testObject.ItemProperty.Describe()).toBe('init:1');
        testObject.ItemProperty = new NestedTypesContainer.Item({ Value: 2, Label: 'next' });
        expect(testObject.ItemProperty.Label).toBe('next');
    });

    test('Handles a nullable nested class property', () => {
        expect(testObject.NullableItem).toBeNull();
        testObject.NullableItem = new NestedTypesContainer.Item({ Value: 3, Label: 'set' });
        expect(testObject.NullableItem).toBeInstanceOf(NestedTypesContainer.Item);
        expect(testObject.NullableItem!.Label).toBe('set');
    });

    test('Passes and returns a nested class through a method', () => {
        const result = testObject.EchoItem(new NestedTypesContainer.Item({ Value: 6, Label: 'echo' }));
        expect(result).toBeInstanceOf(NestedTypesContainer.Item);
        expect(result.Describe()).toBe('echo:6');
    });

    test('Passes and returns a nullable nested class through a method', () => {
        expect(testObject.EchoNullableItem(null)).toBeNull();
        const result = testObject.EchoNullableItem(new NestedTypesContainer.Item({ Value: 7, Label: 'maybe' }));
        expect(result).toBeInstanceOf(NestedTypesContainer.Item);
        expect(result!.Label).toBe('maybe');
    });

    test('Handles an array of nested class references', () => {
        const roundTripped = testObject.EchoItemArray([
            new NestedTypesContainer.Item({ Value: 1, Label: 'a' }),
            new NestedTypesContainer.Item({ Value: 2, Label: 'b' }),
        ]);
        expect(roundTripped).toHaveLength(2);
        expect(roundTripped[0]).toBeInstanceOf(NestedTypesContainer.Item);
        expect(roundTripped.map(x => x.Label)).toEqual(['a', 'b']);
    });

    test('Returns a nested class nested in a Task', async () => {
        const result = await testObject.EchoItemAsync(new NestedTypesContainer.Item({ Value: 8, Label: 'async' }));
        expect(result).toBeInstanceOf(NestedTypesContainer.Item);
        expect(result.Describe()).toBe('async:8');
    });

    test('Invokes a delegate over nested class references', () => {
        const result = testObject.InvokeItemFunc(value => value, new NestedTypesContainer.Item({ Value: 9, Label: 'fn' }));
        expect(result).toBeInstanceOf(NestedTypesContainer.Item);
        expect(result.Label).toBe('fn');
    });

    test('Reads a nested class reference through a delegate property', () => {
        const echoed = testObject.ItemFunc(new NestedTypesContainer.Item({ Value: 10, Label: 'prop' }));
        expect(echoed).toBeInstanceOf(NestedTypesContainer.Item);
        expect(echoed.Label).toBe('prop');
    });

    test('Reads and mutates a nested enum property', () => {
        expect(testObject.KindProperty).toBe(NestedTypesContainer.Kind.Alpha);
        testObject.KindProperty = NestedTypesContainer.Kind.Gamma;
        expect(testObject.KindProperty).toBe(NestedTypesContainer.Kind.Gamma);
    });

    test('Handles a nullable nested enum property', () => {
        expect(testObject.NullableKind).toBeNull();
        testObject.NullableKind = NestedTypesContainer.Kind.Beta;
        expect(testObject.NullableKind).toBe(NestedTypesContainer.Kind.Beta);
    });

    test('Passes and returns a nested enum through a method', () => {
        expect(testObject.EchoKind(NestedTypesContainer.Kind.Beta)).toBe(NestedTypesContainer.Kind.Beta);
    });

    test('Passes and returns a nullable nested enum through a method', () => {
        expect(testObject.EchoNullableKind(null)).toBeNull();
        expect(testObject.EchoNullableKind(NestedTypesContainer.Kind.Gamma)).toBe(NestedTypesContainer.Kind.Gamma);
    });

    test('Handles an array of nested enums', () => {
        const roundTripped = testObject.EchoKindArray([NestedTypesContainer.Kind.Alpha, NestedTypesContainer.Kind.Gamma]);
        expect(Array.from(roundTripped)).toEqual([NestedTypesContainer.Kind.Alpha, NestedTypesContainer.Kind.Gamma]);
    });

    test('Returns a nested enum nested in a Task', async () => {
        expect(await testObject.EchoKindAsync(NestedTypesContainer.Kind.Beta)).toBe(NestedTypesContainer.Kind.Beta);
    });

    test('Invokes a delegate over nested enums', () => {
        const result = testObject.InvokeKindFunc(value => value, NestedTypesContainer.Kind.Gamma);
        expect(result).toBe(NestedTypesContainer.Kind.Gamma);
    });
});
