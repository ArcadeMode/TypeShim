import { People, Person, PeopleProvider } from '@client/wasm-exports';

export class PeopleRepository {

    constructor(private readonly peopleProvider: PeopleProvider) {
    }

    public async getAllPeople(): Promise<Person[]> {
        const people: People = await this.peopleProvider.FetchPeopleAsync();
        return people.All;
    }

    private async PrintAgeMethodUsage() {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person[] = (await this.peopleProvider.FetchPeopleAsync()).All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        const p = Person.materialize(person2);
        const jsPerson: Person.Initializer = { Id: 999, Name: "Snapshot Person", Age: 42, Pets: [] };
        const person3 = new Person({ 
            Id: 999, 
            Name: "Constructed Person", 
            Age: 420, 
            Pets: p.Pets
        });
        person3.AdoptPet();
        console.log(person1.Name, person1.Age, "isOlderThan", person2.Name, person2.Age, ":", person1.IsOlderThan(person2));
        console.log(person1.Name, person1.Age, "isOlderThan (initializer)", jsPerson.Name, jsPerson.Age, ":", person1.IsOlderThan(jsPerson));
        console.log(person1.Name, person1.Age, "isOlderThan (constructor)", person3.Name, person3.Age, ":", person1.IsOlderThan(person3));
    }
}
