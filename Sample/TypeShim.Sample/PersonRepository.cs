using System;
using System.Collections.Generic;
using System.Linq;

namespace TypeShim.Sample;

public class PersonRepository
{
    private readonly RandomEntityGenerator _generator = new();

    private List<Person> _people;
    private Dictionary<Person, Person> _partnerDict;
    private Dictionary<Person, Person[]> _friendsDict;

    public PersonRepository()
    {
        _people = _generator.GeneratePersons(250);
        _partnerDict = _generator.GeneratePartnerDictionary(_people);
        _friendsDict = _generator.GenerateFriendsDictionary(_people);
    }

    public IEnumerable<Person> GetElderlyPeople()
    {
        return _people.Where(p => p.Age > 65);
    }

    public IEnumerable<Person> GetPetOwners()
    {
        return _people.Where(p => p.Pet != null);
    }

    public IEnumerable<Person> GetFriends(Person person)
    {
        if (_friendsDict.TryGetValue(person, out var friends))
        {
            return friends;
        }
        return [];
    }

    public Person? GetPartner(Person person)
    {
        if (_partnerDict.TryGetValue(person, out var partner))
        {
            return partner;
        }
        return null;
    }

    public IEnumerable<Person> GetAll()
    {
        return _people;
    }
}
