using System.Text;

namespace TypeShim.Generator.CSharp;

internal sealed class JSObjectTaskExtensionsRenderer()
{
    private readonly StringBuilder sb = new();
    public string Render()
    {
        // raison d'etre: type mapping limitations: https://learn.microsoft.com/en-us/aspnet/core/client-side/dotnet-interop/?view=aspnetcore-10.0#type-mapping-limitations
        // 1. JSObject has no means to retrieve arrays beside ByteArray (automapping user classes with an array property type is therefore not possible by default)
        // 2. Nested types cannot be represented on the interop boundary (i.e. Task<int[]>

        sb.AppendLine(JSObjectTaskExtensionsClass);
        //TODO: reconsider targetting different moniker and provide this class through TypeShim nuget so the user can utilize these directly if they so wish.
        return sb.ToString();
    }


    private const string JSObjectTaskExtensionsClass = """
#nullable enable
using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
public static partial class JSObjectTaskExtensions
{
    public static Task<bool>? GetPropertyAsBooleanTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsBooleanTask(value) : null;
    }

    public static Task<byte>? GetPropertyAsByteTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsByteTask(value) : null;
    }

    public static Task<char>? GetPropertyAsCharTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsCharTask(value) : null;
    }

    public static Task<short>? GetPropertyAsInt16Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt16Task(value) : null;
    }

    public static Task<int>? GetPropertyAsInt32Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt32Task(value) : null;
    }

    public static Task<long>? GetPropertyAsInt64Task(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsInt64Task(value) : null;
    }

    public static Task<float>? GetPropertyAsSingleTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsSingleTask(value) : null;
    }

    public static Task<double>? GetPropertyAsDoubleTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDoubleTask(value) : null;
    }

    public static Task<nint>? GetPropertyAsIntPtrTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsIntPtrTask(value) : null;
    }

    public static Task<DateTime>? GetPropertyAsDateTimeTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTimeTask(value) : null;
    }

    public static Task<DateTimeOffset>? GetPropertyAsDateTimeOffsetTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsDateTimeOffsetTask(value) : null;
    }

    public static Task<Exception>? GetPropertyAsExceptionTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsExceptionTask(value) : null;
    }

    public static Task<JSObject>? GetPropertyAsJSObjectTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsJSObjectTask(value) : null;
    }

    public static Task<string>? GetPropertyAsStringTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsStringTask(value) : null;
    }

    public static Task<object>? GetPropertyAsObjectTask(this JSObject jsObject, string propertyName)
    {
        return jsObject.GetPropertyAsJSObject(propertyName) is JSObject value ? MarshallAsObjectTask(value) : null;
    }

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
    public static partial Task<bool> MarshallAsBooleanTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<byte> MarshallAsByteTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    public static partial Task<char> MarshallAsCharTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<short> MarshallAsInt16Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<int> MarshallAsInt32Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<long> MarshallAsInt64Task([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<float> MarshallAsSingleTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<double> MarshallAsDoubleTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static partial Task<nint> MarshallAsIntPtrTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>]
    public static partial Task<DateTime> MarshallAsDateTimeTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Date>>]
    public static partial Task<DateTimeOffset> MarshallAsDateTimeOffsetTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Error>>]
    public static partial Task<Exception> MarshallAsExceptionTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Object>>]
    public static partial Task<JSObject> MarshallAsJSObjectTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.String>>]
    public static partial Task<string> MarshallAsStringTask([JSMarshalAs<JSType.Object>] JSObject jsObject);

    [JSImport("unwrap", "@typeshim")]
    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]
    public static partial Task<object> MarshallAsObjectTask([JSMarshalAs<JSType.Object>] JSObject jsObject);
}
""";
}