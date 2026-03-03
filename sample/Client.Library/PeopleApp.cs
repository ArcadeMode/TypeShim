using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;

namespace Client.Library;

[TSExport]
public class PeopleAppOptions
{
    public required string BaseAddress { get; init; }
}

[TSExport]
public class PeopleApp
{
    private readonly IHost _host;

    public PeopleApp(PeopleAppOptions options)
    {
        // we dont -need- a servicecollection for this demo but its here to show you can use anything on the .net side
        _host = new HostBuilder().ConfigureServices(services =>
        {
            services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(options.BaseAddress) });
            services.AddSingleton<PeopleApiClient>();
            services.AddSingleton<PeopleProvider>(sp => new PeopleProvider(sp.GetRequiredService<PeopleApiClient>()));
        }).Build();
        Console.WriteLine($".NET {nameof(PeopleApp)} Constructor completed");
    }

    public PeopleProvider GetPeopleProvider()
    {
        return _host.Services.GetRequiredService<PeopleProvider>();
    }
}