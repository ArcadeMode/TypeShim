import { People, Person, PeopleProvider, TimeoutUnit, MyApp } from '@typeshim/wasm-exports';

export class PeopleRepository {

    public async getAllPeople(): Promise<Person[]> {
        const peopleProvider: PeopleProvider = MyApp.GetPeopleProvider();
        const people: People = await peopleProvider.FetchPeopleAsync();
        return people.All;
    }

    public SetDelays(jsTimeout: number, csDelay: number) {
        const peopleProvider: PeopleProvider = MyApp.GetPeopleProvider();
        const timeoutUnit: TimeoutUnit.Initializer | null = { Timeout: csDelay };
        peopleProvider.DelayTask = new Promise<TimeoutUnit.Initializer | null>((resolve) => setTimeout(() => resolve(timeoutUnit), jsTimeout));
    }

    private async PrintAgeMethodUsage() {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person[] = (await MyApp.GetPeopleProvider().FetchPeopleAsync()).All;
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
        console.log(person1.Name, person1.Age, "isOlderThan (snapshot)", jsPerson.Name, jsPerson.Age, ":", person1.IsOlderThan(jsPerson));
        console.log(person1.Name, person1.Age, "isOlderThan (constructor)", person3.Name, person3.Age, ":", person1.IsOlderThan(person3));
    }
}
