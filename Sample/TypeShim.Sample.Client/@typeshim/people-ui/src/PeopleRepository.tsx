import { People, Person, PeopleProvider, TimeoutUnit, MyApp } from '@typeshim/wasm-exports';

export class PeopleRepository {

    public async getAllPeople(): Promise<Person.Proxy[]> {
        const peopleProvider: PeopleProvider.Proxy = MyApp.Proxy.GetPeopleProvider();
        const people: People.Proxy = await peopleProvider.FetchPeopleAsync();
        this.PrintAgeMethodUsage(people);
        return people.All;
    }

    public SetDelays(jsTimeout: number, csDelay: number) {
        const peopleProvider: PeopleProvider.Proxy = MyApp.Proxy.GetPeopleProvider();
        const timeoutUnit: TimeoutUnit.Properties | null = { Timeout: csDelay };
        peopleProvider.DelayTask = new Promise<TimeoutUnit.Properties | null>((resolve) => setTimeout(() => resolve(timeoutUnit), jsTimeout));
    }

    private PrintAgeMethodUsage(people: People.Proxy) {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person.Proxy[] = people.All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        const p = Person.properties(person2);
        const jsPerson: Person.Props = { Id: 999, Name: "Snapshot Person", Age: 42, Pets: [] };
        const person3 = new Person.Proxy({ 
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
