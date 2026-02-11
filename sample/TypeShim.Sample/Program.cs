using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Sample;

public partial class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("WASM runtime is alive.");

        // You can put any startup logic here if needed, like set up DI even a whole host builder.
        // even make JSImport calls

        // this could then be combined with a static accessor to expose certain services to JS.

        // For this demo however, MyApp has been TSExport'ed and will be initialized from JS.
        Console.WriteLine($"{nameof(MyApp)} can be initialized even without calling into Main");
    }
}
