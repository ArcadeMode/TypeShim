using System;
using System.Net.Http;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TsExport]
public class PeopleProvider(PeopleApiClient _apiClient)
{
    private static Person[]? AllPeople;
    private static Person[]? ElderlyPeople;

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
            return new People(AllPeople);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Exception occurred while fetching people data: {e}");
            throw; // hand over to js
        }
    }

    public async Task<People> FetchElderlyPeopleAsync()
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
