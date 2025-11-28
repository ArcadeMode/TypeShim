using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TypeShim;
using VisageNovel;

namespace TypeShim.Sample;

public class PeopleApiClient(HttpClient httpClient)
{
    public async Task<IEnumerable<Person>> GetElderlyPeopleAsync()
    {
        PeopleDto? dto = await httpClient.GetFromJsonAsync("/people/elderly", typeof(PeopleDto), PersonDtoSerializerContext.Default) as PeopleDto;
        return dto?.People?.Select(dto => dto.ToPerson()) ?? [];
    }

    public async Task<IEnumerable<Person>> GetAllPeopleAsync()
    {
        PeopleDto? dto = await httpClient.GetFromJsonAsync("/people/all", typeof(PeopleDto), PersonDtoSerializerContext.Default) as PeopleDto;
        return dto?.People?.Select(dto => dto.ToPerson()) ?? [];
    }
}
