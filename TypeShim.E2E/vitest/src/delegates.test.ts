import { describe, test, expect, beforeEach } from 'vitest';
import { DelegatesTest, ExportedClass, SimplePropertiesTest, TaskPropertiesClass } from '@typeshim/e2e-wasm-lib';

describe('Delegates Test', () => {
    let exportedClass: ExportedClass;
    let testObject: DelegatesTest;
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new DelegatesTest();
    });

    test('Snapshot has property-value equality', async () => {
        let isInvoked = false;
        testObject.InvokeAction(() => {
            isInvoked = true;
        });
        expect(isInvoked).toBe(true);
    });
    
});
