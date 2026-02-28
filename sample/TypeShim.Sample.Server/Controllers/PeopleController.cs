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
        IEnumerable<Person> people = repository.GetAll();
        return new PeopleDto
        {
            People = [.. people.Select(PersonDto.FromPerson)]
        };
    }
}

