using Microsoft.AspNetCore.Mvc;
using Client.Library;

namespace Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PeopleController() : ControllerBase
{
    private readonly List<Person> _people = new RandomEntityGenerator().GeneratePersons(250);

    [HttpGet]
    [Route("all")]
    public PeopleDto GetAll()
    {
        return new PeopleDto
        {
            People = [.. _people.Select(PersonDto.FromPerson)]
        };
    }
}

