namespace TypeShim.Sample;

public class PeopleDto
{
    public required PersonDto[] People { get; set; }
}

public class PersonDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required int Age { get; set; }
    public required DogDto? Pet { get; set; }

    public static PersonDto FromPerson(Person person)
    {
        return new PersonDto
        {
            Id = person.Id,
            Name = person.Name,
            Age = person.Age,
            Pet = person.Pet != null ? DogDto.FromDog(person.Pet) : null
        };
    }

    public Person ToPerson()
    {
        Dog? dog = Pet != null ? new Dog { Name = Pet.Name, Breed = Pet.Breed, Age = Pet.Age } : null;
        return new Person()
        {
            Id = Id,
            Name = Name,
            Age = Age,
            Pet = dog
        };
    }
}

public class DogDto
{
    public required string Name { get; set; }
    public required string Breed { get; set; }
    public required int Age { get; set; }

    public static DogDto FromDog(Dog dog)
    {
        return new DogDto
        {
            Name = dog.Name,
            Breed = dog.Breed,
            Age = dog.Age
        };
    }

    public Dog ToDog()
    {
        return new Dog
        {
            Name = Name,
            Breed = Breed,
            Age = Age
        };
    }
}