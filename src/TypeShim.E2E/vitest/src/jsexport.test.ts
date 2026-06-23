import { describe, expect, test } from 'vitest';
import { JSExportClass } from '@typeshim/e2e-wasm-lib';

describe('JSExportClass primitives and tasks', () => {
    test('Add returns number', () => {
        expect(JSExportClass.Add(2, 3)).toBe(5);
    });

    test('GetSum handles number arrays', () => {
        expect(JSExportClass.GetSum([1, 2, 3, 4])).toBe(10);
    });

    test('GetGreeting returns string', () => {
        expect(JSExportClass.GetGreeting()).toBe('Hello, from JSExport');
    });

    test('IsEven returns true for even value', () => {
        expect(JSExportClass.IsEven(4)).toBe(true);
    });

    test('IsEven returns false for odd value', () => {
        expect(JSExportClass.IsEven(5)).toBe(false);
    });

    test('MultiplyDouble returns expected product', () => {
        expect(JSExportClass.MultiplyDouble(2.5, 4)).toBeCloseTo(10);
    });

    test('Describe handles multiple primitive parameters', () => {
        expect(JSExportClass.Describe(7, true, 'x')).toBe('7:True:x');
    });

    test('IsPositiveAsync resolves true/false correctly', async () => {
        await expect(JSExportClass.IsPositiveAsync(3)).resolves.toBe(true);
        await expect(JSExportClass.IsPositiveAsync(-1)).resolves.toBe(false);
    });

    test('AddAsync resolves sum', async () => {
        await expect(JSExportClass.AddAsync(10, 20)).resolves.toBe(30);
    });

    test('CompleteAsync resolves void', async () => {
        await expect(JSExportClass.CompleteAsync()).resolves.toBeUndefined();
    });

    test('Object identity is preserved on C# side with sync method', () => {
        const identityObject = JSExportClass.CreateIdentityObject(321);
        JSExportClass.RememberObject(identityObject);

        expect(JSExportClass.ReadIdentityId(identityObject)).toBe(321);
        expect(JSExportClass.IsRememberedObject(identityObject)).toBe(true);
    });

    test('Object identity is preserved on C# side with async method', async () => {
        const identityObject = JSExportClass.CreateIdentityObject(654);
        JSExportClass.RememberObject(identityObject);

        await expect(JSExportClass.ReadIdentityIdAsync(identityObject)).resolves.toBe(654);
        expect(JSExportClass.IsRememberedObject(identityObject)).toBe(true);
    });

    test('ReadIdentityIds returns ids for all objects in array', async () => {
        const identityObject1 = JSExportClass.CreateIdentityObject(654);
        const identityObject2 = JSExportClass.CreateIdentityObject(321);
        const identityObjects = [identityObject1, identityObject2];
        expect(JSExportClass.ReadIdentityIds(identityObjects)).toEqual(new Int32Array([654, 321]));
    });

    test('Can pass string array around the boundary', () => {
        expect(JSExportClass.PrefixAll(['a', 'b', 'c'], 'pre-')).toEqual(['pre-a', 'pre-b', 'pre-c']);
    });
});