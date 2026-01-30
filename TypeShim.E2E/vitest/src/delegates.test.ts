import { describe, test, expect, beforeEach } from 'vitest';
import { DelegatesTest, ExportedClass } from '@typeshim/e2e-wasm-lib';

describe('Delegates Test', () => {
    let exportedClass: ExportedClass;
    let testObject: DelegatesTest;
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new DelegatesTest();
    });

    test('Void Action', async () => {
        let isInvoked = false;
        testObject.InvokeVoidAction(() => {
            isInvoked = true;
        });
        expect(isInvoked).toBe(true);
    });

    test('String Action', async () => {
        let receivedString = "";
        testObject.InvokeStringAction((arg0: string) => {
            receivedString = arg0;
        });
        expect(receivedString).toBe("Hello");
    });
    
    test('Int32 Action', async () => {
        let receivedInt = 0;
        testObject.InvokeInt32Action((arg0: number) => {
            receivedInt = arg0;
        });
        expect(receivedInt).toBe(42);
    });

    test('Bool Action', async () => {
        let receivedBool = false;
        testObject.InvokeBoolAction((arg0: boolean) => {
            receivedBool = arg0;
        });
        expect(receivedBool).toBe(true);
    });

    test('Bool2 Action', async () => {
        let receivedBool1 = false;
        let receivedBool2 = false;
        testObject.InvokeBool2Action((arg0: boolean, arg1: boolean) => {
            receivedBool1 = arg0;
            receivedBool2 = arg1;
        });
        expect(receivedBool1).toBe(true);
        expect(receivedBool2).toBe(false);
    });

    test('Bool3 Action', async () => {
        let receivedBool1 = false;
        let receivedBool2 = false;
        let receivedBool3 = false;
        testObject.InvokeBool3Action((arg0: boolean, arg1: boolean, arg2: boolean) => {
            receivedBool1 = arg0;
            receivedBool2 = arg1;
            receivedBool3 = arg2;
        });
        expect(receivedBool1).toBe(true);
        expect(receivedBool2).toBe(false);
        expect(receivedBool3).toBe(true);
    });

    test('String Func', async () => {
        const result = testObject.InvokeStringFunc(() => {
            return "Hello World";
        });
        expect(result).toBe("Hello World");
    }); 

    test('Int32 Func', async () => {
        const result = testObject.InvokeInt32Func(() => {
            return 12345;
        });
        expect(result).toBe(12345);
    });

    test('Bool Func', async () => {
        const result = testObject.InvokeBoolFunc(() => {
            return true;
        });
        expect(result).toBe(true);
    });

    test('Bool2 Func', async () => {
        const result = testObject.InvokeBool2Func((arg0: boolean) => {
            return arg0;
        });
        expect(result).toBe(true);
    });
});