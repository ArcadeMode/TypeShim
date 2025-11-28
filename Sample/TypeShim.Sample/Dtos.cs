namespace TypeShim.Sample;

public class PeopleDto
{
    public required PersonDto[] People { get; set; }
}

public class PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Age { get; set; }
    public DogDto? Pet { get; set; }

    public static PersonDto FromPerson(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Pet = person.Pet != null ? new DogDto { Name = person.Pet.Name, Breed = person.Pet.Breed } : null
        };
    }

    public Person ToPerson()
    {
        Dog? dog = Pet != null ? new Dog { Name = Pet.Name, Breed = Pet.Breed } : null;
        return new Person(Id, Name, Age, dog);
    }
}

public class DogDto
{
    public string Name { get; set; }
    public string Breed { get; set; }
}