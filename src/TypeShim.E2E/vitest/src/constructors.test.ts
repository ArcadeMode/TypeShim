import { describe, test, expect, beforeEach } from 'vitest';
import { 
    IntConstructor,
    StringConstructor,
    MultipleConstructor,
    ExportedClass,
    ExportedClassConstructor,
    ExportedClassMultipleConstructor,
    ExportedClassArrayConstructor,
    ExportedClassActionConstructor,
    IntStringMixedConstructor
} from '@typeshim/e2e-wasm-lib';

describe('Constructors Test', () => {
    test('IntConstructor with int parameter', async () => {
        const instance = new IntConstructor(100);
        expect(instance.Value).toBe(100);
    });

    test('IntConstructor with different values', async () => {
        const instance1 = new IntConstructor(0);
        expect(instance1.Value).toBe(0);
        
        const instance2 = new IntConstructor(-50);
        expect(instance2.Value).toBe(-50);
        
        const instance3 = new IntConstructor(2147483647);
        expect(instance3.Value).toBe(2147483647);
    });

    test('StringConstructor with string parameter', async () => {
        const instance = new StringConstructor('hello');
        expect(instance.Value).toBe('hello');
    });

    test('StringConstructor with different strings', async () => {
        const instance1 = new StringConstructor('');
        expect(instance1.Value).toBe('');
        
        const instance2 = new StringConstructor('test string');
        expect(instance2.Value).toBe('test string');
        
        const instance3 = new StringConstructor('special chars: !@#$%');
        expect(instance3.Value).toBe('special chars: !@#$%');
    });

    test('MultipleConstructor with int and string parameters', async () => {
        const instance = new MultipleConstructor(42, 'test');
        expect(instance.IntValue).toBe(42);
        expect(instance.StringValue).toBe('test');
    });

    test('MultipleConstructor with various values', async () => {
        const instance1 = new MultipleConstructor(0, '');
        expect(instance1.IntValue).toBe(0);
        expect(instance1.StringValue).toBe('');
        
        const instance2 = new MultipleConstructor(-100, 'negative');
        expect(instance2.IntValue).toBe(-100);
        expect(instance2.StringValue).toBe('negative');
    });

    test('ExportedClassConstructor with ExportedClass parameter', async () => {
        const exported = new ExportedClass({ Id: 1 });
        const instance = new ExportedClassConstructor(exported);
        expect(instance.Value).toBe(exported);
    });

    test('ExportedClassMultipleConstructor with multiple ExportedClass parameters', async () => {
        const exported1 = new ExportedClass({ Id: 1 });
        const exported2 = new ExportedClass({ Id: 2 });
        const instance = new ExportedClassMultipleConstructor(exported1, exported2);
        expect(instance.Value).toBe(exported1);
        expect(instance.Value2).toBe(exported2);
    });

    test('ExportedClassArrayConstructor with ExportedClass array parameter', async () => {
        const exported1 = new ExportedClass({ Id: 1 });
        const exported2 = new ExportedClass({ Id: 2 });
        const exported3 = new ExportedClass({ Id: 3 });
        const array = [exported1, exported2, exported3];
        const instance = new ExportedClassArrayConstructor(array);
        expect(instance.Value).toStrictEqual(array);
        expect(instance.Value.length).toBe(3);
    });

    test('ExportedClassArrayConstructor with empty array', async () => {
        const array: ExportedClass[] = [];
        const instance = new ExportedClassArrayConstructor(array);
        expect(instance.Value).toStrictEqual(array);
        expect(instance.Value.length).toBe(0);
    });

    test('ExportedClassActionConstructor with Action<ExportedClass> parameter', async () => {
        let callCount = 0;
        const action = (obj: ExportedClass) => {
            callCount++;
        };
        const instance = new ExportedClassActionConstructor(action);
        instance.Value(new ExportedClass({ Id: 1 }))
        expect(callCount).toBe(1);
        instance.Value(new ExportedClass({ Id: 2 }))
        expect(callCount).toBe(2);
    });

    test('ExportedClassActionConstructor action is callable', async () => {
        let receivedValue: ExportedClass | null = null;
        const action = (obj: ExportedClass) => {
            receivedValue = obj;
        };
        const instance = new ExportedClassActionConstructor(action);
        const exported = new ExportedClass({ Id: 1 });
        instance.Value(exported);
        expect(receivedValue).toBe(exported);
    });

    test('IntStringMixedConstructor with int and string parameters', () =>{
        const instance = new IntStringMixedConstructor(42, { StringValue: 'test' });
        expect(instance.Value).toBe(42);
        expect(instance.StringValue).toBe('test');
    })
});