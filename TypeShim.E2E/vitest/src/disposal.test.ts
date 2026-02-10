import { describe, test, expect } from 'vitest';
import { ExportedClass } from "@typeshim/e2e-wasm-lib";

describe('Proxy Disposal Tests', () => {

    test('Cannot access property after disposal', () => {
        const testInstance = new ExportedClass({ Id: 2 });
        testInstance.instance.dispose();
        expect(() => testInstance.Id).toThrow('ObjectDisposedException');
    });

});