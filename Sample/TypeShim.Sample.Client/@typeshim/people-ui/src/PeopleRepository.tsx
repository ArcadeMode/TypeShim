import { People, Person, PeopleProvider, TypeShimSampleModule, AssemblyExports, Dog, TimeoutUnit } from '@typeshim/wasm-exports';

export class PeopleRepository {


    constructor(exportsPromise: Promise<AssemblyExports>) {
    }

    public async getAllPeople(): Promise<Person.Proxy[]> {
        const peopleProvider: PeopleProvider.Proxy | null = TypeShimSampleModule.Proxy.PeopleProvider;
        if (!peopleProvider) {
            throw new Error("PeopleProvider is null");
        }
        const timeoutUnit: TimeoutUnit.Snapshot | null = { Timeout: 1000 };
        peopleProvider.Unit = new Promise<TimeoutUnit.Snapshot | null>((resolve) => setTimeout(() => resolve(timeoutUnit), 500));
        const people: People.Proxy = await peopleProvider.FetchPeopleAsync();
        
        this.PrintAgeMethodUsage(people);
        return people.All;
    }

    private PrintAgeMethodUsage(people: People.Proxy) {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person.Proxy[] = people.All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        const jsPerson: Person.Snapshot = { Id: 999, Name: "Snapshot Person", Age: 42, Pets: [] };
        console.log(person1.Name, person1.Age, "isOlderThan", person2.Name, person2.Age, ":", person1.IsOlderThan(person2));
        console.log(person1.Name, person1.Age, "isOlderThan (snapshot)", jsPerson.Name, jsPerson.Age, ":", person1.IsOlderThan(jsPerson));
    }
}
