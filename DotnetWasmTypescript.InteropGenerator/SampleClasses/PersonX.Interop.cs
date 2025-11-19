// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
namespace DotnetWasmTypescript.InteropGenerator.SampleClasses;
public partial class PersonXInterop
{
    [JSExport]
    public static string GetName([JSMarshalAs<JSType.Any>] object instance)
    {
        PersonX typed_instance = (PersonX)instance;
        return typed_instance.GetName();
    }

    [JSExport]
    public static void SetName([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object name)
    {
        PersonX typed_instance = (PersonX)instance;
        PersonNameX typed_name = (PersonNameX)name;
        typed_instance.SetName(typed_name);
    }

    [JSExport]
    public static void SetProperties([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object name, int age)
    {
        PersonX typed_instance = (PersonX)instance;
        PersonNameX typed_name = (PersonNameX)name;
        typed_instance.SetProperties(typed_name, age);
    }

    [JSExport]
    public static void SetProperties2([JSMarshalAs<JSType.Any>] object instance, int age, [JSMarshalAs<JSType.Any>] object name)
    {
        PersonX typed_instance = (PersonX)instance;
        PersonNameX typed_name = (PersonNameX)name;
        typed_instance.SetProperties2(age, typed_name);
    }

}
