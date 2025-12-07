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
        const persons: Person[] = people.All;
        return persons;
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
