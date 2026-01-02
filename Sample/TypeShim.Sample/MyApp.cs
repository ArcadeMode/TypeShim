using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace TypeShim.Sample;

[TSExport]
public class MyApp
{
    private static IHost? _host;

    public static void Initialize(string baseAddress)
    {
        if (_host != null)
        {
            throw new InvalidOperationException("Module already initialized.");
        }

        Console.WriteLine($"Initializing {nameof(MyApp)} in .NET...");

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
        Console.WriteLine($"Initialized {nameof(MyApp)} in .NET.");
    }

    public static PeopleProvider GetPeopleProvider()
    {
        if (_host == null)
        {
            throw new InvalidOperationException("Module not initialized.");
        }
        return _host.Services.GetRequiredService<PeopleProvider>();
    }
}