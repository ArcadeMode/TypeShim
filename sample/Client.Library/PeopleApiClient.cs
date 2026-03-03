using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Library;

public class PeopleApiClient(HttpClient httpClient)
{
    public async Task<IEnumerable<Person>> GetAllPeopleAsync()
    {
        PeopleDto? dto = await httpClient.GetFromJsonAsync("/people/all", typeof(PeopleDto), PersonDtoSerializerContext.Default) as PeopleDto;
        return dto?.People?.Select(dto => dto.ToPerson()) ?? [];
    }
}

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PeopleDto))]
[JsonSerializable(typeof(PersonDto))]
[JsonSerializable(typeof(PersonDto[]))]
[JsonSerializable(typeof(DogDto))]
[JsonSerializable(typeof(DogDto[]))]
internal partial class PersonDtoSerializerContext : JsonSerializerContext { }
