using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

[TSExport]
public class DelegatePropertyClass
{
    public required Func<ExportedClass, ExportedClass> ExportedClassFuncProperty { get; set; }
}

[TSExport]
public class DelegatesClass
{
    public Func<bool, int>? FuncBoolIntProperty { get; set; }
    public required Func<char> FuncCharProperty { get; set; }

    public void InvokeVoidAction(Action action)
    {
        action();
    }

    public void InvokeStringAction(Action<string> action)
    {
        action("Hello");
    }

    public void InvokeInt32Action(Action<int> action)
    {
        action(42);
    }

    public void InvokeBoolAction(Action<bool> action)
    {
        action(true);
    }
    public void InvokeCharAction(Action<char> action)
    {
        action('Z');
    }

    public Func<char, char> GetCharCharFunc()
    {
        return (char c) => (char)(c + 1);
    }

    public void InvokeBool2Action(Action<bool, bool> action)
    {
        action(true, false);
    }

    public void InvokeBool3Action(Action<bool, bool, bool> action)
    {
        action(true, false, true);
    }

    public string InvokeStringFunc(Func<string> func)
    {
        return func();
    }

    public int InvokeInt32Func(Func<int> func)
    {
        return func();
    }

    public bool InvokeBoolFunc(Func<bool> func)
    {
        return func();
    }

    public bool InvokeBool2Func(Func<bool, bool> func)
    {
        return func(true);
    }

    public void InvokeExportedClassAction(Action<ExportedClass> action)
    {
        action(new ExportedClass { Id = 100 });
    }

    public Func<ExportedClass> GetExportedClassFunc() 
    {
        return () => new ExportedClass { Id = 200 };
    }

    public Func<bool, int, string, ExportedClass> GetBoolIntStringExportFunc()
    {
        return (bool b, int a, string c) => new ExportedClass { Id = b ? a : c.Length };
    }
    
    public Func<bool, int, char, ExportedClass> GetBoolIntCharExportFunc()
    {
        return (bool b, int a, char c) => new ExportedClass { Id = b ? a : c };
    }

    public Func<bool, int, ExportedClass, char> GetBoolIntExportCharFunc()
    {
        return (bool b, int a, ExportedClass c) => b ? (char)a : (char)c.Id;
    }

    public Func<ExportedClass, ExportedClass> GetExportedClassExportedClassFunc() 
    {
        return (ExportedClass classIn) => classIn;
    }

    public ExportedClass InvokeExportedClassExportedClassFunc(Func<ExportedClass, ExportedClass> func, Func<ExportedClass> paramFunc) 
    {
        return func(paramFunc());
    }
}
