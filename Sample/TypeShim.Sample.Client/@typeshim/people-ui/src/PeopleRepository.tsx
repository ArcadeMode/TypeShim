import { People, Person, PeopleProvider, TypeShimSampleModule, AssemblyExports } from '@typeshim/wasm-exports';

export class PeopleRepository {

    private wasmModulePromise: Promise<TypeShimSampleModule>;

    constructor(exportsPromise: Promise<AssemblyExports>) {
        this.wasmModulePromise = this.getInitializedSampleModule(exportsPromise);
    }

    public async getAllPeople(): Promise<Person[]> {
        const sampleModule: TypeShimSampleModule = await this.wasmModulePromise;
        const peopleProvider: PeopleProvider | null = sampleModule.PeopleProvider;
        if (!peopleProvider) {
            throw new Error("PeopleProvider is null");
        }
        const people: People = await peopleProvider.FetchPeopleAsync();
        this.PrintAgeMethodUsage(people);
        return people.All;
    }

    private PrintAgeMethodUsage(people: People) {
        console.log("Demonstrating Person.IsOlderThan method:");
        const persons: Person[] = people.All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        console.log(person1.Name, person1.Age, "isOlderThan", person2.Name, person2.Age, ":", person1.IsOlderThan(person2));
    }

    private async getInitializedSampleModule(exportsPromise: Promise<AssemblyExports>): Promise<TypeShimSampleModule> {
        const exports: AssemblyExports = await exportsPromise;
        return new TypeShimSampleModule(exports);
    }
}
