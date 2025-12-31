using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;



[TSExport]
public class PeopleProvider
{
    private readonly PeopleApiClient _apiClient;
    private Person[]? AllPeople;

    // internal constructor blocks access from JS
    internal PeopleProvider(PeopleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Person[]? PeopleCache => AllPeople;
    public Task<TimeoutUnit?>? DelayTask { get; set; } = null;

    public async Task<People> FetchPeopleAsync()
    {
        try
        {
            if (DelayTask != null)
            {
                await Task.Delay((await DelayTask)?.Timeout ?? 0);
            }

            if (AllPeople == null)
            {
                AllPeople = [.. await _apiClient.GetAllPeopleAsync()];
                Console.WriteLine("Fetched people data from webapi. Count: " + AllPeople.Length);
            } 
            else
            {
                Console.WriteLine("Returning cached people data from wasm.  Count: " + AllPeople.Length);
            }
            return new People() { All = AllPeople };
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred while fetching people data: {e}");
            throw; // hand over to js
        }
    }
}


[TSExport]
public class TimeoutUnit
{
    public int Timeout { get; set; } = 0;
}