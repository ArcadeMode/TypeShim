using System.Text.Json.Serialization;
using TypeShim.Sample;

namespace VisageNovel;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PeopleDto))]
[JsonSerializable(typeof(PersonDto))]
[JsonSerializable(typeof(PersonDto[]))]
[JsonSerializable(typeof(DogDto))]
[JsonSerializable(typeof(DogDto[]))]
internal partial class PersonDtoSerializerContext : JsonSerializerContext { }
