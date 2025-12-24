using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TSExport]
public class TimeoutUnit
{
    public Task<int> Timeout { get; set; } = Task.FromResult(1000);
}

[TSExport]
public class PeopleProvider(PeopleApiClient? _apiClient = null)
{
    private static Person[]? AllPeople;
    
    public TimeoutUnit? Unit { get; set; }

    public async Task<People> FetchPeopleAsync()
    {
        try
        {
            if (Unit != null)
            {
                await Task.Delay(await Unit.Timeout);
            }

            if (AllPeople == null)
            {
                AllPeople = [.. await _apiClient?.GetAllPeopleAsync()];
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


