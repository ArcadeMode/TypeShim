using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace TypeShim.Sample;

[TSExport]
public class PeopleApp
{
    private readonly IHost _host;

    public PeopleApp(string baseAddress)
    {
        Console.WriteLine($"Initializing {nameof(PeopleApp)} in .NET...");
        _host = new HostBuilder().ConfigureServices(services =>
        {
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });
            services.AddSingleton<PeopleApiClient>();
            services.AddSingleton<PeopleProvider>(sp => new PeopleProvider(sp.GetRequiredService<PeopleApiClient>()));
        }).Build();
        Console.WriteLine($"Initialized {nameof(PeopleApp)} in .NET.");
    }

    public PeopleProvider GetPeopleProvider()
    {
        if (_host == null)
        {
            throw new InvalidOperationException("Module not initialized.");
        }
        return _host.Services.GetRequiredService<PeopleProvider>();
    }
}