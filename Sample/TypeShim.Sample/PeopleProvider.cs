using System;
using System.Net.Http;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class PeopleProvider
{
    private static readonly HttpClient _httpClient = new() { BaseAddress = new System.Uri("https://localhost:7266/") };
    private static readonly PeopleApiClient _apiClient = new(_httpClient);

    private static Person[]? AllPeople;
    private static Person[]? ElderlyPeople;

    public static async Task<People> FetchPeopleAsync()
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
            return new People(AllPeople);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred while fetching people data: {e}");
            throw; // hand over to js
        }
    }

    public static async Task<People> FetchElderlyPeopleAsync()
    {
        try
        {
            if (ElderlyPeople == null)
            {
                ElderlyPeople = [.. await _apiClient.GetElderlyPeopleAsync()];
                Console.WriteLine("Fetched elderly people data from webapi. Count: " + ElderlyPeople.Length);
            } 
            else
            {
                Console.WriteLine("Returning cached elderly people data from wasm.  Count: " + ElderlyPeople.Length);
            }
            return new People(ElderlyPeople);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred while fetching elderly people data: {e}");
            throw; // hand over to js
        }
    }
}
