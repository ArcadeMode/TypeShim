using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TypeShim.Sample.Capabilities;

[TSModule]
public static class CapabilitiesModule
{
    public static Capabilities Capabilities { get; } = new Capabilities();
}

[TSExport]
public class Capabilities
{
    public void VoidMethod()
    {
    }

    public PrimitivesCapability GetStringCapability(string baseString)
    {
        return new(baseString);
    }

    //public string StringMethod()
    //{
    //    return "Hello, TypeShim!";
    //}

    //public string[] ArrayMethod()
    //{
    //    return [ "One", "Two", "Three" ];
    //}

    //public async Task<int> IntMethodAsync()
    //{
    //    await Task.Delay(5);
    //    return 42;
    //}

    //public async Task<string> StringMethodAsync()
    //{
    //    await Task.Delay(5);
    //    return "Hello, TypeShim!";
    //}
}

[TSExport]
public class PrimitivesCapability(string baseString)
{
    public string BaseString => baseString;

    public int GetStringLength()
    {
        return baseString.Length;
    }
    
    public string ToUpperCase()
    {
        return baseString.ToUpper();
    }

    public string Concat(string str1, string str2)
    {
        return string.Concat(baseString, str1, str2);
    }

    public bool ContainsUpperCase()
    {
        return baseString.Equals(baseString.ToLowerInvariant(), StringComparison.CurrentCultureIgnoreCase);
    }
}

//[TSExport]
//public class 