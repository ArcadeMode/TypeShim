using System;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Sample;

[TSExport]
public class DelegatesTest
{
    public void InvokeAction(Action action)
    {
        action();
    }

}
