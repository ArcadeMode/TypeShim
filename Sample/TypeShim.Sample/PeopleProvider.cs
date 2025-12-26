using System;
using System.Net.Http;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using TypeShim;

namespace TypeShim.Sample;



[TSExport]
public class PeopleProvider(PeopleApiClient? _apiClient = null)
{
    private static Person[]? AllPeople;
    public Person?[]? PeopleCache => AllPeople ?? [];
    public Task<TimeoutUnit?>? Unit { get; set; } = Task.FromResult<TimeoutUnit?>(null);

    public void DoStuff(Task<TimeoutUnit?> task)
    {
        Unit = task; 
    }

    public async Task<People> FetchPeopleAsync()
    {
        try
        {
            await Task.Delay((await Unit)?.Timeout ?? 0);

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


[TSExport]
public class TimeoutUnit
{
    public int Timeout { get; set; } = 0;
}