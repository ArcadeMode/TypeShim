using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace TypeShim.E2E.Wasm;

public static partial class JSExportClass
{
    private static object? _identityObject;

    [JSExport]
    public static int Add(int a, int b) => a + b;

    [JSExport]
    public static int GetSum(int[] ints) => ints.Sum();

    [JSExport]
    public static string GetGreeting() => "Hello, from JSExport";

    [JSExport]
    public static bool IsEven(int value) => value % 2 == 0;

    [JSExport]
    public static double MultiplyDouble(double left, double right) => left * right;

    [JSExport]
    public static string Describe(int x, bool y, string z) => $"{x}:{y}:{z}";

    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Boolean>>]
    public static Task<bool> IsPositiveAsync([JSMarshalAs<JSType.Number>] int value) => Task.FromResult(value > 0);

    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static Task<int> AddAsync([JSMarshalAs<JSType.Number>] int a, [JSMarshalAs<JSType.Number>] int b) => Task.FromResult(a + b);

    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Void>>]
    public static Task CompleteAsync() => Task.CompletedTask;

    [JSExport]
    [return: JSMarshalAs<JSType.Any>]
    public static object CreateIdentityObject([JSMarshalAs<JSType.Number>] int id) => new IdentityPayload(id);

    [JSExport]
    [return: JSMarshalAs<JSType.Void>]
    public static void RememberObject([JSMarshalAs<JSType.Any>] object obj) => _identityObject = obj;

    [JSExport]
    [return: JSMarshalAs<JSType.Number>]
    public static int ReadIdentityId([JSMarshalAs<JSType.Any>] object obj) => ((IdentityPayload)obj).Id;

    [JSExport]
    [return: JSMarshalAs<JSType.Promise<JSType.Number>>]
    public static Task<int> ReadIdentityIdAsync([JSMarshalAs<JSType.Any>] object obj) => Task.FromResult(((IdentityPayload)obj).Id);

    [JSExport]
    [return: JSMarshalAs<JSType.Array<JSType.Number>>]
    public static int[] ReadIdentityIds([JSMarshalAs<JSType.Array<JSType.Any>>] object[] objs) => objs.Select(obj => ((IdentityPayload)obj).Id).ToArray();

    [JSExport]
    [return: JSMarshalAs<JSType.Boolean>]
    public static bool IsRememberedObject([JSMarshalAs<JSType.Any>] object obj) => ReferenceEquals(_identityObject, obj);

    [JSExport]
    public static string[] PrefixAll(string[] values, string prefix) => values.Select(value => $"{prefix}{value}").ToArray();

    private sealed record IdentityPayload(int Id);
}