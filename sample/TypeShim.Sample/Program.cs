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

        // You can put any startup logic here like any other .NET application
        // alternatively you could expose a class that embodies your app and treat the .NET code as a library.
        // For this demo we'll go with the latter, PeopleApp will be constructed from the JS side.
        Console.WriteLine($"{nameof(PeopleApp)} will be constructed from the JS side in this demo.");
    }
}
