import { describe, test, expect, beforeEach } from 'vitest';
import { DelegatesClass, ExportedClass } from '@typeshim/e2e-wasm-lib';

describe('Delegates Test', () => {
    let exportedClass: ExportedClass;
    let testObject: DelegatesClass;
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new DelegatesClass({ FuncBoolIntProperty: null, FuncCharProperty: () => 'A' });
    });

    test('Set and Get FuncBoolIntProperty', async () => {
        const func = (arg0: boolean) => {
            return arg0 ? 1 : 0;
        };
        testObject.FuncBoolIntProperty = func;
        const retrievedFunc = testObject.FuncBoolIntProperty;
        expect(retrievedFunc).not.toBeNull();
        expect(retrievedFunc!(true)).toBe(1);
        expect(retrievedFunc!(false)).toBe(0);
    });

    test('Initialize FuncBoolIntProperty', async () => {
        testObject = new DelegatesClass({ 
            FuncBoolIntProperty: (arg0: boolean) => {
                return arg0 ? 1 : 0;
            },
            FuncCharProperty: () => 'A'
        });
        const retrievedFunc = testObject.FuncBoolIntProperty;
        expect(retrievedFunc).not.toBeNull();
        expect(retrievedFunc!(true)).toBe(1);
        expect(retrievedFunc!(false)).toBe(0);
    });

    test('Invoke Void Action', async () => {
        let isInvoked = false;
        testObject.InvokeVoidAction(() => {
            isInvoked = true;
        });
        expect(isInvoked).toBe(true);
    });

    test('Invoke String Action', async () => {
        let receivedString = "";
        testObject.InvokeStringAction((arg0: string) => {
            receivedString = arg0;
        });
        expect(receivedString).toBe("Hello");
    });
    
    test('Invoke Int32 Action', async () => {
        let receivedInt = 0;
        testObject.InvokeInt32Action((arg0: number) => {
            receivedInt = arg0;
        });
        expect(receivedInt).toBe(42);
    });

    test('Invoke Bool Action', async () => {
        let receivedBool = false;
        testObject.InvokeBoolAction((arg0: boolean) => {
            receivedBool = arg0;
        });
        expect(receivedBool).toBe(true);
    });

    test('Invoke Char Action', async () => {
        let receivedChar = '';
        testObject.InvokeCharAction((arg0: string) => {
            receivedChar = arg0;
        });
        expect(receivedChar).toBeTypeOf('string');
        expect(receivedChar).toBe('Z');
    });

    test('Get GetCharCharFunc', async () => {
        const fn = testObject.GetCharCharFunc();
        const retVal = fn('D');
        expect(retVal).not.toBeNull();
        expect(retVal).toBeTypeOf('string');
        expect(retVal).toBe('E'); // char + 1 on CS side
    });

    test('Invoke Bool2 Action', async () => {
        let receivedBool1 = false;
        let receivedBool2 = false;
        testObject.InvokeBool2Action((arg0: boolean, arg1: boolean) => {
            receivedBool1 = arg0;
            receivedBool2 = arg1;
        });
        expect(receivedBool1).toBe(true);
        expect(receivedBool2).toBe(false);
    });

    test('Invoke Bool3 Action', async () => {
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

    test('Invoke String Func', async () => {
        const result = testObject.InvokeStringFunc(() => {
            return "Hello World";
        });
        expect(result).toBe("Hello World");
    }); 

    test('Invoke Int32 Func', async () => {
        const result = testObject.InvokeInt32Func(() => {
            return 12345;
        });
        expect(result).toBe(12345);
    });

    test('Invoke Bool Func', async () => {
        const result = testObject.InvokeBoolFunc(() => {
            return true;
        });
        expect(result).toBe(true);
    });

    test('Invoke Bool2 Func', async () => {
        const result = testObject.InvokeBool2Func((arg0: boolean) => {
            return arg0;
        });
        expect(result).toBe(true);
    });

    test('Invoke ExportedClass Action', async () => {
        let receivedInstance: ExportedClass = null as any;
        testObject.InvokeExportedClassAction((arg0: ExportedClass) => {
            receivedInstance = arg0;
        });
        expect(receivedInstance).not.toBeNull();
        expect(receivedInstance).toBeInstanceOf(ExportedClass);
        expect(receivedInstance.Id).toBe(100);
    });

    test('Get ExportedClass Func', async () => {
        const fn = testObject.GetExportedClassFunc();
        const exportedInstance = fn();
        expect(exportedInstance).not.toBeNull();
        expect(exportedInstance).toBeInstanceOf(ExportedClass);
        expect(exportedInstance.Id).toBe(200);
    });

    test('Get ExportedClassExportedClassFunc Proxy', async () => {
        const instance = new ExportedClass({ Id: 300 });
        const fn = testObject.GetExportedClassExportedClassFunc();
        const retVal = fn(instance);
        expect(retVal).not.toBeNull();
        expect(retVal).toBeInstanceOf(ExportedClass);
        expect(retVal.Id).toBe(300);
    });

    test('Get ExportedClassExportedClassFunc Initializer', async () => {
        const instance = { Id: 250 };
        const fn = testObject.GetExportedClassExportedClassFunc();
        const retVal = fn(instance);
        expect(retVal).not.toBeNull();
        expect(retVal).toBeInstanceOf(ExportedClass);
        expect(retVal.Id).toBe(250);
    });

    test('Invoke Function Param with return value from another Function Param (C#)', async () => {
        const retVal = testObject.InvokeExportedClassExportedClassFunc(testObject.GetExportedClassExportedClassFunc(), testObject.GetExportedClassFunc());
        expect(retVal).not.toBeNull();
        expect(retVal).toBeInstanceOf(ExportedClass);
        expect(retVal.Id).toBe(200);
    });

    test('Invoke Function Param with return value from another Function Param (C#-JS)', async () => {
        const retVal = testObject.InvokeExportedClassExportedClassFunc(testObject.GetExportedClassExportedClassFunc(), () => exportedClass);
        expect(retVal).not.toBeNull();
        expect(retVal).toBeInstanceOf(ExportedClass);
        expect(retVal.Id).toBe(exportedClass.Id);
        // TODO: fix identity (https://github.com/ArcadeMode/TypeShim/issues/20)
        //expect(retVal).toBe(exportedClass);
    });

    test('Initializer with FuncCharProperty', async () => {
        const testObject2 = new DelegatesClass({ 
            FuncCharProperty: () => {
                return 'X'
            },
            FuncBoolIntProperty: null
         });
        const fn = testObject2.FuncCharProperty;
        const retVal = fn();
        expect(retVal).not.toBeNull();
        expect(retVal).toBeTypeOf('string');
        expect(retVal).toBe('X');
    });
});