using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;

[TSExport]
public class PeopleProvider(PeopleApiClient _apiClient)
{
    private static Person[]? AllPeople;

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

    //public void Ok(JSObject obj)
    //{
    //    obj.
    //}
}
