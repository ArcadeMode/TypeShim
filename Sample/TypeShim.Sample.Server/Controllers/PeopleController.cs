using Microsoft.AspNetCore.Mvc;
using TypeShim.Sample;

namespace TypeShim.Sample.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PeopleController(PersonRepository repository) : ControllerBase
{
    [HttpGet]
    [Route("all")]
    public PeopleDto GetAll()
    {
        IEnumerable<Person> elderlyPeople = repository.GetAll();
        return new PeopleDto
        {
            People = [.. elderlyPeople.Select(PersonDto.FromPerson)]
        };
    }
}

