import { People, Person, PeopleProvider, TypeShimSampleModule, AssemblyExports, Dog } from '@typeshim/wasm-exports';

export class PeopleRepository {

    private wasmModulePromise: Promise<TypeShimSampleModule>;

    constructor(exportsPromise: Promise<AssemblyExports>) {
        this.wasmModulePromise = this.getInitializedSampleModule(exportsPromise);
    }

    public async getAllPeople(): Promise<Person.Proxy[]> {
        const sampleModule: TypeShimSampleModule = await this.wasmModulePromise;
        const peopleProvider: PeopleProvider.Proxy | null = sampleModule.PeopleProvider;
        if (!peopleProvider) {
            throw new Error("PeopleProvider is null");
        }
        const people: People.Proxy = await peopleProvider.FetchPeopleAsync();

        
        const person: Person.Proxy = people.All.find(person => person.Pet)!;
        const petObj = Dog.snapshot(person.Pet!);
        petObj.Ints = [1, 2, 3, 4, 5];
        
        person.Pet = petObj;
        person.Pet!.Ints.forEach(element => {
            console.log("Ints element:", element);
        });
        this.PrintAgeMethodUsage(people);
        return people.All;
    }

    private PrintAgeMethodUsage(people: People.Proxy) {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person.Proxy[] = people.All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        console.log(person1.Name, person1.Age, "isOlderThan", person2.Name, person2.Age, ":", person1.IsOlderThan(person2));
    }

    private async getInitializedSampleModule(exportsPromise: Promise<AssemblyExports>): Promise<TypeShimSampleModule> {
        const exports: AssemblyExports = await exportsPromise;
        return new TypeShimSampleModule(exports);
    }
}
