import { describe, test, expect } from 'vitest';
import { ExportedClass } from "@typeshim/e2e-wasm-lib";

describe('Proxy Disposal Tests', () => {

    test('Cannot access property after proxy disposal', () => {
        const testInstance = new ExportedClass({ Id: 2 });
        expect(testInstance.instance.isDisposed).toBe(false);
        testInstance.instance.dispose();
        expect(testInstance.instance.isDisposed).toBe(true);
        expect(() => testInstance.Id).toThrow('ObjectDisposedException');
    });

    test('User exported Dispose method also disposes proxy instance', () => {
        const testInstance = new ExportedClass({ Id: 2 });
        expect(testInstance.instance.isDisposed).toBe(false);
        testInstance.Dispose();
        expect(testInstance.instance.isDisposed).toBe(true);
        expect(() => testInstance.Id).toThrow('ObjectDisposedException');
    });

});