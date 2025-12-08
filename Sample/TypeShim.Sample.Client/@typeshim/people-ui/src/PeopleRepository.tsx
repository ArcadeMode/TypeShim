import { WasmModuleExports, WasmModule, People, Person, TypeShimSampleModuleStatics, PeopleProvider } from '@typeshim/people-exports';

class PeopleRepository {

    private wasmModulePromise: Promise<TypeShimSampleModuleStatics>;

    constructor() {
        this.wasmModulePromise = this.getInitializedSampleModule();
    }

    public async getAllPeople(): Promise<Person[]> {
        const sampleModule: TypeShimSampleModuleStatics = await this.wasmModulePromise;
        const peopleProvider: PeopleProvider | null = sampleModule.PeopleProvider;
        if (!peopleProvider) {
            throw new Error("PeopleProvider is null");
        }
        const people: People = await peopleProvider.FetchPeopleAsync();
        this.PrintAgeMethodUsage(people);
        return people.All;
    }

    public async getElderlyPeople(): Promise<Person[]> {
        const sampleModule: TypeShimSampleModuleStatics = await this.wasmModulePromise;
        const peopleProvider: PeopleProvider | null = sampleModule.PeopleProvider;
        if (!peopleProvider) {
            throw new Error("PeopleProvider is null");
        }
        const people: People = await peopleProvider.FetchElderlyPeopleAsync();
        return people.All;
    }

    private PrintAgeMethodUsage(people: People){
        const persons: Person[] = people.All;
        const person1 = persons[(Math.random() * persons.length) | 0];
        const person2 = persons[(Math.random() * persons.length) | 0];
        console.log(person1.Name, person1.Age, "isOlderThan", person2.Name, person2.Age, ":", person1.IsOlderThan(person2));
    }

    private async getInitializedSampleModule(): Promise<TypeShimSampleModuleStatics> {
        const wasmModuleStarter = (window as any).wasmModuleStarter;
        if (!wasmModuleStarter) {
            throw new Error("wasmModuleStarter not found on window. Ensure dotnet-start.js is loaded.");
        }

        const exports: WasmModuleExports = await wasmModuleStarter.exports;
        const wasmModule = new WasmModule(exports);

        const sampleModule: TypeShimSampleModuleStatics = wasmModule.TypeShimSampleModule;
        return sampleModule;
    }
}

export const PeopleRepositoryInstance = new PeopleRepository();
