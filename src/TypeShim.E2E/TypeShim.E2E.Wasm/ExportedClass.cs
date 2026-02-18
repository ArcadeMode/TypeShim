using System;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.E2E.Wasm;

/// <summary>
/// This comment has <a href="https://learn.microsoft.com/">an <i>anchor</i></a>.
/// <c>const i = 1;</c>
/// but also
/// <code>string s = "commented"</code>
/// and <b>don't</b> forget:
/// <example>
///     // some nice sample code
///     ExportedClass e = new();
///     e.Dispose(); // disposes
/// </example>
/// reference <see cref="ManualExport"/> or <see cref="Unknown.Place.AnotherClass"/>
/// <br/>
/// Now lets take some steps
/// <list type="number">
/// <item><description>Step one</description></item>
/// <item><description>Step two</description></item>
/// </list>
/// Awesome. Now here is an example <example>Process(data)</example>
/// <br/>
/// jup.
/// </summary>
/// <param name="something">Look at this param</param>
/// <param name="value">theres no parameters but hey, this works anyway</param>
/// <exception cref="SuperCoolException">This exception was not imported</exception>
/// <exception cref="InvalidOperationException">This exception is imported</exception>
/// <returns>The quotient</returns>
/// <remarks>that was a long comment</remarks>
[TSExport]
public class ExportedClass : IDisposable
{
    public int Id { get; set; }

    /// <summary>
    /// Typeshim codegen ensures after an instance's Dispose is invoked, that the interop proxy is also disposed automatically.<br/>
    /// So this method is mostly here for testing that.
    /// </summary>
    public void Dispose()
    {
        // no-op for testing purposes
    }
}


public partial class ManualExport
{
    [JSExport]
    [return: JSMarshalAs<JSType.MemoryView>]
    public static Span<int> GetSpan()
    {
        return new Span<int>([1, 2, 3]);
    }
}