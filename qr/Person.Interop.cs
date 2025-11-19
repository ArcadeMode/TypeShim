// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
namespace QR.Wasm;
public partial class PersonInterop
{
    [JSExport]
    public static string GetName([JSMarshalAs<JSType.Any>] object instance)
    {
        Person typed_instance = (Person)instance;
        return typed_instance.GetName();
    }
    [JSExport]
    public static void SetName([JSMarshalAs<JSType.Any>] object instance, [JSMarshalAs<JSType.Any>] object name)
    {
        Person typed_instance = (Person)instance;
        PersonName typed_name = (PersonName)name;
        typed_instance.SetName(typed_name);
    }
}
