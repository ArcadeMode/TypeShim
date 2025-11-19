// Auto-generated TypeScript interop definitions
using System.Runtime.InteropServices.JavaScript;
namespace QR.Wasm;
public partial class PersonRepositoryInterop
{
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object GetPerson1([JSMarshalAs<JSType.Any>] object instance)
    {
        PersonRepository typed_instance = (PersonRepository)instance;
        return typed_instance.GetPerson1();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object GetPerson2([JSMarshalAs<JSType.Any>] object instance)
    {
        PersonRepository typed_instance = (PersonRepository)instance;
        return typed_instance.GetPerson2();
    }
    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object GetPerson3()
    {
        return PersonRepository.GetPerson3();
    }
}
