using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Sample;

public partial class Program
{
    private static IHost? _host;
    public static void Main(string[] args)
    {
        Console.WriteLine("WASM runtime is alive.");

        string baseAddress = GetBaseAddress();
        Initialize(baseAddress);
    }

    [JSImport("globalThis.window.getBaseURI")]
    public static partial string GetBaseAddress();

    static void Initialize(string baseAddress)
    {
        if (_host != null)
        {
            throw new InvalidOperationException("Module already initialized.");
        }

        Console.WriteLine("Initializing SampleModule...");

        IConfigurationRoot config = new ConfigurationBuilder()
            // if desired, add configuration sources here, may also be passed through parameters
            .Build();
        _host = new HostBuilder()
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddConfiguration(config);
            })
            .ConfigureServices(services =>
            {
                services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
                services.AddSingleton<PeopleApiClient>();
                services.AddSingleton<PeopleProvider>();
            })
            .Build();

        TypeShimSampleModule.PeopleProvider = _host.Services.GetRequiredService<PeopleProvider>();

        Console.WriteLine("SampleModule initialized.");
    }
}
