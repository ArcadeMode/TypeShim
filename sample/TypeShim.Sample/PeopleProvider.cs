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

    internal PeopleProvider(PeopleApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<People> FetchPeopleAsync()
    {
        try
        {
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