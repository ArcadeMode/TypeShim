// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

//// @ts-ignore
//import { dotnet } from './_framework/dotnet.js'
//import type { PersonName, Person, QRCode } from './exported-types.js';

//let exportsPromise: Promise<{ QRCode: QRCode }> = null;

//async function createRuntimeAndGetExports() {
//    const { getAssemblyExports, getConfig } = await dotnet.create();
//    const config = getConfig();
//    return await getAssemblyExports(config.mainAssemblyName);
//}

//export async function generate(text: string, pixelsPerBlock: number) {
//    if (!exportsPromise) {
//        exportsPromise = createRuntimeAndGetExports();
//    }

//    const exports = await exportsPromise;
//    return exports.QRCode.Generate(text, pixelsPerBlock);
//}

//export async function GetPerson(name: PersonName): Promise<Person> {
//    const exports = await exportsPromise;
//    return exports.QRCode.GetPerson(name);
//}

export * from './_framework/dotnet.js'
export * from './exported-types'