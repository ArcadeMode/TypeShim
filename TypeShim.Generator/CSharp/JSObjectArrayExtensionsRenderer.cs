using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class JSObjectArrayExtensionsRenderer()
{
    private readonly StringBuilder sb = new();
    public string Render()
    {
        sb.AppendLine("// JSImports for the type marshalling process");

        // TODO: try provide module to user for importing with JSHost.Import ?? needs referencing in JSImport(xx)

        // raison d'etre: type mapping limitations: https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mapping-limitations
        // 1. JSObject has no means to retrieve arrays beside ByteArray (automapping user classes with an array property type is therefore not possible by default)
        // 2. Nested types cannot be represented on the interop boundary (i.e. Task<int[]>

        // Workaround: 're-marshalling'. i.e. going back and forth over the interop boundary with altering type annotations so dotnet unwraps array types.
        // General flow snapshot mapping
        // 1. C#. Receive JSObject for mapping, from generation context its assumed to match the structure of some class, for example consider 'UserClass' with properties P1: int and P2: int[].
        // 2. C#. P1 is resolved using JSObject.GetPropertyAsInt32("P1")
        // 3. C#. P2 cannot be directly resolved. Instead: p2Obj = JSObject.GetPropertyAsJSObject()
        // 4. C#. call JSImport method GetAsIntArray (see below)
        // 5. JS. invoked method simply returns object, its still an array, we only need to type it.
        // 6. C#. GetAsIntArray marshals the JSObject back to int[], then returns
        // 7. C#. P2 is resolved

        //[JSImport("globalThis.window.unwrap")]
        //[return: JSMarshalAs<JSType.Array<JSType.Number>>]
        //public static partial int[] GetAsIntArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

        sb.AppendLine(JSObjectArrayExtensionsClass);
        //TODO: reconsider targetting different moniker and provide this class through TypeShim nuget so the user can utilize these directly if they so wish.
        return sb.ToString();
    }

    private const string JSObjectArrayExtensionsClass = """
using System.Runtime.InteropServices.JavaScript;

public static partial class JSObjectArrayExtensions
{
    public static int[] GetPropertyAsInt32Array(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsIntArray(value) : [];
    }

    public static double[] GetPropertyAsDoubleArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDoubleArray(value) : [];
    }

    public static string[] GetPropertyAsStringArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsStringArray(value) : [];
    }

    public static JSObject[] GetPropertyAsJSObjectArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsJSObjectArray(value) : [];
    }

    public static object[] GetPropertyAsObjectArray(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsObjectArray(value) : [];
    }

    [JSImport("globalThis.window.unwrap")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static partial int[] MarshallAsIntArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("globalThis.window.unwrap")]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static partial double[] MarshallAsDoubleArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("globalThis.window.unwrap")]
    [return: JSMarshalAs<JSType.Array<JSType.String>>]
    public static partial string[] MarshallAsStringArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("globalThis.window.unwrap")]
    [return: JSMarshalAs<JSType.Array<JSType.Object>>]
    public static partial JSObject[] MarshallAsJSObjectArray([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("globalThis.window.unwrap")]
    [return: JSMarshalAs<JSType.Array<JSType.Any>>]
    public static partial object[] MarshallAsObjectArray([JSMarshalAs<JSType.Object>] JSObject jsObject);
}
""";
}