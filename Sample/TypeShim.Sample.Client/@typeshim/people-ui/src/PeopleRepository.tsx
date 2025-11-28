import { WasmModuleExports, WasmModule, PeopleProviderStatics, People, Person } from '@typeshim/people-exports';

export class PeopleRepository {

    private wasmModulePromise: Promise<WasmModule>;

    constructor() {
        this.wasmModulePromise = this.getWasmModule();
    }

    public async getAllPeople(): Promise<Person[]> {
        const wasmModule: WasmModule = await this.wasmModulePromise;
        const peopleProvider: PeopleProviderStatics = wasmModule.PeopleProvider();
        const people: People = await peopleProvider.FetchPeopleAsync();
        return people.GetAll();
    }

    public async getElderlyPeople(): Promise<Person[]> {
        const wasmModule: WasmModule = await this.wasmModulePromise;
        const peopleProvider: PeopleProviderStatics = wasmModule.PeopleProvider();
        const people: People = await peopleProvider.FetchElderlyPeopleAsync();
        return people.GetAll();
    }

    private async getWasmModule(): Promise<WasmModule> {
        const wasmModuleStarter = (window as any).wasmModuleStarter;
        if (!wasmModuleStarter) {
            throw new Error("wasmModuleStarter not found on window. Ensure dotnet-start.js is loaded.");
        }

        const exports: WasmModuleExports = await wasmModuleStarter.exports;
        return new WasmModule(exports)
    }
}