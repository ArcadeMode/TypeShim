using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeShim.Sample;

public class PersonRepository
{
    private readonly RandomEntityGenerator _generator = new();

    private readonly List<Person> _people;

    public PersonRepository()
    {
        _people = _generator.GeneratePersons(250);
    }

    public IEnumerable<Person> GetAll()
    {
        return _people;
    }
}
