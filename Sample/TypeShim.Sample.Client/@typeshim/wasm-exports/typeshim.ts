class TypeShimConfig {
  private static _exports: AssemblyExports | null = null;

  static get exports() {
    if (!TypeShimConfig._exports) {
      throw new Error("TypeShim has not been initialized.");
    }
    return TypeShimConfig._exports;
  }

  static initialize(options: { assemblyExports: AssemblyExports, setModuleImports: (scriptName: string, imports: object) => void }) {
    if (TypeShimConfig._exports){
      throw new Error("TypeShim has already been initialized.");
    }
    options.setModuleImports("@typeshim", { unwrap: (obj: any) => obj });
    TypeShimConfig._exports = options.assemblyExports;
  }
}

export const TypeShimInitializer = { initialize: TypeShimConfig.initialize };

abstract class ProxyBase {
  instance: ManagedObject;
  constructor(instance: ManagedObject) {
    this.instance = instance;
  }

  static fromHandle<T extends ProxyBase>(ctor: { prototype: T }, handle: ManagedObject): T {
    const obj = Object.create(ctor.prototype) as T;
    obj.instance = handle;
    return obj;
  }
}

export interface IDisposable {
    dispose(): void;
    get isDisposed(): boolean;
}

export interface ManagedObject extends IDisposable {
    toString (): string;
}

export interface ManagedError extends IDisposable {
    get stack(): any;
    getSuperStack(): any; 
    getManageStack(): any;
}


// TypeShim generated TypeScript module exports interface
export interface AssemblyExports{
  TypeShim: {
    Sample: {
      Capabilities: {
        ArraysDemoInterop: {
          ctor(initialArray: Array<number>): ManagedObject;
          SumElements(instance: ManagedObject): number;
          Append(instance: ManagedObject, value: number): void;
          get_IntArrayProperty(instance: ManagedObject): Array<number>;
        };
        PrimitivesDemoInterop: {
          ctor(jsObject: object): ManagedObject;
          GetStringLength(instance: ManagedObject): number;
          ToUpperCase(instance: ManagedObject): string;
          Concat(instance: ManagedObject, str1: string, str2: string): string;
          ContainsUpperCase(instance: ManagedObject): boolean;
          ResetBaseString(instance: ManagedObject): void;
          MultiplyString(instance: ManagedObject, times: number): void;
          get_InitialStringProperty(instance: ManagedObject): string;
          get_StringProperty(instance: ManagedObject): string;
          set_StringProperty(instance: ManagedObject, value: string): void;
        };
      };
      CompilationTestInterop: {
        ctor(jsObject: object): ManagedObject;
        VoidMethod(instance: ManagedObject): void;
        IntMethod(instance: ManagedObject): number;
        BoolMethod(instance: ManagedObject): boolean;
        StringMethod(instance: ManagedObject): string;
        DoubleMethod(instance: ManagedObject): number;
        get_NIntProperty(instance: ManagedObject): number;
        set_NIntProperty(instance: ManagedObject, value: number): void;
        get_ByteProperty(instance: ManagedObject): number;
        set_ByteProperty(instance: ManagedObject, value: number): void;
        get_ShortProperty(instance: ManagedObject): number;
        set_ShortProperty(instance: ManagedObject, value: number): void;
        get_IntProperty(instance: ManagedObject): number;
        set_IntProperty(instance: ManagedObject, value: number): void;
        get_LongProperty(instance: ManagedObject): number;
        set_LongProperty(instance: ManagedObject, value: number): void;
        get_BoolProperty(instance: ManagedObject): boolean;
        set_BoolProperty(instance: ManagedObject, value: boolean): void;
        get_StringProperty(instance: ManagedObject): string;
        set_StringProperty(instance: ManagedObject, value: string): void;
        get_CharProperty(instance: ManagedObject): string;
        set_CharProperty(instance: ManagedObject, value: string): void;
        get_DoubleProperty(instance: ManagedObject): number;
        set_DoubleProperty(instance: ManagedObject, value: number): void;
        get_FloatProperty(instance: ManagedObject): number;
        set_FloatProperty(instance: ManagedObject, value: number): void;
        get_DateTimeProperty(instance: ManagedObject): Date;
        set_DateTimeProperty(instance: ManagedObject, value: Date): void;
        get_DateTimeOffsetProperty(instance: ManagedObject): Date;
        set_DateTimeOffsetProperty(instance: ManagedObject, value: Date): void;
        get_objectProperty(instance: ManagedObject): ManagedObject;
        set_objectProperty(instance: ManagedObject, value: ManagedObject): void;
        get_ExportedClassProperty(instance: ManagedObject): ManagedObject;
        set_ExportedClassProperty(instance: ManagedObject, value: ManagedObject | object): void;
        get_JSObjectProperty(instance: ManagedObject): object;
        set_JSObjectProperty(instance: ManagedObject, value: object): void;
        get_TaskProperty(instance: ManagedObject): Promise<void>;
        set_TaskProperty(instance: ManagedObject, value: Promise<void>): void;
        get_TaskOfNIntProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfNIntProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfShortProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfShortProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfIntProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfIntProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfLongProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfLongProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfBoolProperty(instance: ManagedObject): Promise<boolean>;
        set_TaskOfBoolProperty(instance: ManagedObject, value: Promise<boolean>): void;
        get_TaskOfByteProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfByteProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfCharProperty(instance: ManagedObject): Promise<string>;
        set_TaskOfCharProperty(instance: ManagedObject, value: Promise<string>): void;
        get_TaskOfStringProperty(instance: ManagedObject): Promise<string>;
        set_TaskOfStringProperty(instance: ManagedObject, value: Promise<string>): void;
        get_TaskOfDoubleProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfDoubleProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfFloatProperty(instance: ManagedObject): Promise<number>;
        set_TaskOfFloatProperty(instance: ManagedObject, value: Promise<number>): void;
        get_TaskOfDateTimeProperty(instance: ManagedObject): Promise<Date>;
        set_TaskOfDateTimeProperty(instance: ManagedObject, value: Promise<Date>): void;
        get_TaskOfDateTimeOffsetProperty(instance: ManagedObject): Promise<Date>;
        set_TaskOfDateTimeOffsetProperty(instance: ManagedObject, value: Promise<Date>): void;
        get_TaskOfObjectProperty(instance: ManagedObject): Promise<ManagedObject>;
        set_TaskOfObjectProperty(instance: ManagedObject, value: Promise<ManagedObject>): void;
        get_TaskOfExportedClassProperty(instance: ManagedObject): Promise<ManagedObject>;
        set_TaskOfExportedClassProperty(instance: ManagedObject, value: Promise<ManagedObject | object>): void;
        get_TaskOfJSObjectProperty(instance: ManagedObject): Promise<object>;
        set_TaskOfJSObjectProperty(instance: ManagedObject, value: Promise<object>): void;
        get_ByteArrayProperty(instance: ManagedObject): Array<number>;
        set_ByteArrayProperty(instance: ManagedObject, value: Array<number>): void;
        get_JSObjectArrayProperty(instance: ManagedObject): Array<object>;
        set_JSObjectArrayProperty(instance: ManagedObject, value: Array<object>): void;
        get_ObjectArrayProperty(instance: ManagedObject): Array<ManagedObject>;
        set_ObjectArrayProperty(instance: ManagedObject, value: Array<ManagedObject>): void;
        get_ExportedClassArrayProperty(instance: ManagedObject): Array<ManagedObject>;
        set_ExportedClassArrayProperty(instance: ManagedObject, value: Array<ManagedObject | object>): void;
        get_IntArrayProperty(instance: ManagedObject): Array<number>;
        set_IntArrayProperty(instance: ManagedObject, value: Array<number>): void;
        get_StringArrayProperty(instance: ManagedObject): Array<string>;
        set_StringArrayProperty(instance: ManagedObject, value: Array<string>): void;
        get_DoubleArrayProperty(instance: ManagedObject): Array<number>;
        set_DoubleArrayProperty(instance: ManagedObject, value: Array<number>): void;
      };
      ExportedClassInterop: {
        ctor(jsObject: object): ManagedObject;
        get_Id(instance: ManagedObject): number;
        set_Id(instance: ManagedObject, value: number): void;
      };
      PeopleInterop: {
        ctor(jsObject: object): ManagedObject;
        get_All(instance: ManagedObject): Array<ManagedObject>;
        set_All(instance: ManagedObject, value: Array<ManagedObject | object>): void;
      };
      PersonInterop: {
        ctor(jsObject: object): ManagedObject;
        IsOlderThan(instance: ManagedObject, other: ManagedObject | object): boolean;
        AdoptPet(instance: ManagedObject): void;
        Adopt(instance: ManagedObject, newPet: ManagedObject | object): void;
        get_Id(instance: ManagedObject): number;
        set_Id(instance: ManagedObject, value: number): void;
        get_Name(instance: ManagedObject): string;
        set_Name(instance: ManagedObject, value: string): void;
        get_Age(instance: ManagedObject): number;
        set_Age(instance: ManagedObject, value: number): void;
        get_Pets(instance: ManagedObject): Array<ManagedObject>;
        set_Pets(instance: ManagedObject, value: Array<ManagedObject | object>): void;
      };
      DogInterop: {
        ctor(jsObject: object): ManagedObject;
        Bark(instance: ManagedObject): string;
        GetAge(instance: ManagedObject, asHumanYears: boolean): number;
        get_Name(instance: ManagedObject): string;
        set_Name(instance: ManagedObject, value: string): void;
        get_Breed(instance: ManagedObject): string;
        set_Breed(instance: ManagedObject, value: string): void;
        get_Age(instance: ManagedObject): number;
        set_Age(instance: ManagedObject, value: number): void;
      };
      MyAppInterop: {
        ctor(): ManagedObject;
        Initialize(baseAddress: string): void;
        GetPeopleProvider(): ManagedObject;
      };
      PeopleProviderInterop: {
        ctor(apiClient: ManagedObject, jsObject: object): ManagedObject;
        FetchPeopleAsync(instance: ManagedObject): Promise<ManagedObject>;
        get_PeopleCache(instance: ManagedObject): Array<ManagedObject> | null;
        get_DelayTask(instance: ManagedObject): Promise<ManagedObject | null> | null;
        set_DelayTask(instance: ManagedObject, value: Promise<ManagedObject | object | null> | null): void;
      };
      TimeoutUnitInterop: {
        ctor(jsObject: object): ManagedObject;
        get_Timeout(instance: ManagedObject): number;
        set_Timeout(instance: ManagedObject, value: number): void;
      };
    };
  };
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.Capabilities.ArraysDemo
export class ArraysDemo extends ProxyBase {
  constructor(initialArray: Array<number>) {
    super(TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.ctor(initialArray));
  }

  public SumElements(): number {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.SumElements(this.instance);
  }

  public Append(value: number): void {
    TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.Append(this.instance, value);
  }
  public get IntArrayProperty(): Array<number> {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.ArraysDemoInterop.get_IntArrayProperty(this.instance);
  }
}
export namespace ArraysDemo {
  export interface Snapshot {
    IntArrayProperty: Array<number>;
  }
  export function materialize(proxy: ArraysDemo): ArraysDemo.Snapshot {
    return {
      IntArrayProperty: proxy.IntArrayProperty,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.Capabilities.PrimitivesDemo
export class PrimitivesDemo extends ProxyBase {
  constructor(jsObject: PrimitivesDemo.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ctor(jsObject));
  }

  public GetStringLength(): number {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.GetStringLength(this.instance);
  }

  public ToUpperCase(): string {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ToUpperCase(this.instance);
  }

  public Concat(str1: string, str2: string): string {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.Concat(this.instance, str1, str2);
  }

  public ContainsUpperCase(): boolean {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ContainsUpperCase(this.instance);
  }

  public ResetBaseString(): void {
    TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.ResetBaseString(this.instance);
  }

  public MultiplyString(times: number): void {
    TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.MultiplyString(this.instance, times);
  }
  public get InitialStringProperty(): string {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_InitialStringProperty(this.instance);
  }

  public get StringProperty(): string {
    return TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.get_StringProperty(this.instance);
  }

  public set StringProperty(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.Capabilities.PrimitivesDemoInterop.set_StringProperty(this.instance, value);
  }
}
export namespace PrimitivesDemo {
  export interface Initializer {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export interface Snapshot {
    InitialStringProperty: string;
    StringProperty: string;
  }
  export function materialize(proxy: PrimitivesDemo): PrimitivesDemo.Snapshot {
    return {
      InitialStringProperty: proxy.InitialStringProperty,
      StringProperty: proxy.StringProperty,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.CompilationTest
export class CompilationTest extends ProxyBase {
  constructor(jsObject: CompilationTest.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.ctor(jsObject));
  }

  public VoidMethod(): void {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.VoidMethod(this.instance);
  }

  public IntMethod(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.IntMethod(this.instance);
  }

  public BoolMethod(): boolean {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.BoolMethod(this.instance);
  }

  public StringMethod(): string {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.StringMethod(this.instance);
  }

  public DoubleMethod(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.DoubleMethod(this.instance);
  }
  public get NIntProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_NIntProperty(this.instance);
  }

  public set NIntProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_NIntProperty(this.instance, value);
  }

  public get ByteProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ByteProperty(this.instance);
  }

  public set ByteProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ByteProperty(this.instance, value);
  }

  public get ShortProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ShortProperty(this.instance);
  }

  public set ShortProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ShortProperty(this.instance, value);
  }

  public get IntProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_IntProperty(this.instance);
  }

  public set IntProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_IntProperty(this.instance, value);
  }

  public get LongProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_LongProperty(this.instance);
  }

  public set LongProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_LongProperty(this.instance, value);
  }

  public get BoolProperty(): boolean {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_BoolProperty(this.instance);
  }

  public set BoolProperty(value: boolean) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_BoolProperty(this.instance, value);
  }

  public get StringProperty(): string {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_StringProperty(this.instance);
  }

  public set StringProperty(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_StringProperty(this.instance, value);
  }

  public get CharProperty(): string {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_CharProperty(this.instance);
  }

  public set CharProperty(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_CharProperty(this.instance, value);
  }

  public get DoubleProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_DoubleProperty(this.instance);
  }

  public set DoubleProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_DoubleProperty(this.instance, value);
  }

  public get FloatProperty(): number {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_FloatProperty(this.instance);
  }

  public set FloatProperty(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_FloatProperty(this.instance, value);
  }

  public get DateTimeProperty(): Date {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_DateTimeProperty(this.instance);
  }

  public set DateTimeProperty(value: Date) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_DateTimeProperty(this.instance, value);
  }

  public get DateTimeOffsetProperty(): Date {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_DateTimeOffsetProperty(this.instance);
  }

  public set DateTimeOffsetProperty(value: Date) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_DateTimeOffsetProperty(this.instance, value);
  }

  public get objectProperty(): object {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_objectProperty(this.instance);
  }

  public set objectProperty(value: object) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_objectProperty(this.instance, value);
  }

  public get ExportedClassProperty(): ExportedClass {
    const res = TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ExportedClassProperty(this.instance);
    return ProxyBase.fromHandle(ExportedClass, res);
  }

  public set ExportedClassProperty(value: ExportedClass | ExportedClass.Initializer) {
    const valueInstance = value instanceof ExportedClass ? value.instance : value;
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ExportedClassProperty(this.instance, valueInstance);
  }

  public get JSObjectProperty(): object {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_JSObjectProperty(this.instance);
  }

  public set JSObjectProperty(value: object) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_JSObjectProperty(this.instance, value);
  }

  public get TaskProperty(): Promise<void> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskProperty(this.instance);
  }

  public set TaskProperty(value: Promise<void>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskProperty(this.instance, value);
  }

  public get TaskOfNIntProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfNIntProperty(this.instance);
  }

  public set TaskOfNIntProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfNIntProperty(this.instance, value);
  }

  public get TaskOfShortProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfShortProperty(this.instance);
  }

  public set TaskOfShortProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfShortProperty(this.instance, value);
  }

  public get TaskOfIntProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfIntProperty(this.instance);
  }

  public set TaskOfIntProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfIntProperty(this.instance, value);
  }

  public get TaskOfLongProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfLongProperty(this.instance);
  }

  public set TaskOfLongProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfLongProperty(this.instance, value);
  }

  public get TaskOfBoolProperty(): Promise<boolean> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfBoolProperty(this.instance);
  }

  public set TaskOfBoolProperty(value: Promise<boolean>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfBoolProperty(this.instance, value);
  }

  public get TaskOfByteProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfByteProperty(this.instance);
  }

  public set TaskOfByteProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfByteProperty(this.instance, value);
  }

  public get TaskOfCharProperty(): Promise<string> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfCharProperty(this.instance);
  }

  public set TaskOfCharProperty(value: Promise<string>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfCharProperty(this.instance, value);
  }

  public get TaskOfStringProperty(): Promise<string> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfStringProperty(this.instance);
  }

  public set TaskOfStringProperty(value: Promise<string>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfStringProperty(this.instance, value);
  }

  public get TaskOfDoubleProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfDoubleProperty(this.instance);
  }

  public set TaskOfDoubleProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfDoubleProperty(this.instance, value);
  }

  public get TaskOfFloatProperty(): Promise<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfFloatProperty(this.instance);
  }

  public set TaskOfFloatProperty(value: Promise<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfFloatProperty(this.instance, value);
  }

  public get TaskOfDateTimeProperty(): Promise<Date> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfDateTimeProperty(this.instance);
  }

  public set TaskOfDateTimeProperty(value: Promise<Date>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfDateTimeProperty(this.instance, value);
  }

  public get TaskOfDateTimeOffsetProperty(): Promise<Date> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfDateTimeOffsetProperty(this.instance);
  }

  public set TaskOfDateTimeOffsetProperty(value: Promise<Date>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfDateTimeOffsetProperty(this.instance, value);
  }

  public get TaskOfObjectProperty(): Promise<object> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfObjectProperty(this.instance);
  }

  public set TaskOfObjectProperty(value: Promise<object>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfObjectProperty(this.instance, value);
  }

  public get TaskOfExportedClassProperty(): Promise<ExportedClass> {
    const res = TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfExportedClassProperty(this.instance);
    return res.then(e => ProxyBase.fromHandle(ExportedClass, e));
  }

  public set TaskOfExportedClassProperty(value: Promise<ExportedClass | ExportedClass.Initializer>) {
    const valueInstance = value.then(e => e instanceof ExportedClass ? e.instance : e);
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfExportedClassProperty(this.instance, valueInstance);
  }

  public get TaskOfJSObjectProperty(): Promise<object> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_TaskOfJSObjectProperty(this.instance);
  }

  public set TaskOfJSObjectProperty(value: Promise<object>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_TaskOfJSObjectProperty(this.instance, value);
  }

  public get ByteArrayProperty(): Array<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ByteArrayProperty(this.instance);
  }

  public set ByteArrayProperty(value: Array<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ByteArrayProperty(this.instance, value);
  }

  public get JSObjectArrayProperty(): Array<object> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_JSObjectArrayProperty(this.instance);
  }

  public set JSObjectArrayProperty(value: Array<object>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_JSObjectArrayProperty(this.instance, value);
  }

  public get ObjectArrayProperty(): Array<object> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ObjectArrayProperty(this.instance);
  }

  public set ObjectArrayProperty(value: Array<object>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ObjectArrayProperty(this.instance, value);
  }

  public get ExportedClassArrayProperty(): Array<ExportedClass> {
    const res = TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_ExportedClassArrayProperty(this.instance);
    return res.map(e => ProxyBase.fromHandle(ExportedClass, e));
  }

  public set ExportedClassArrayProperty(value: Array<ExportedClass | ExportedClass.Initializer>) {
    const valueInstance = value.map(e => e instanceof ExportedClass ? e.instance : e);
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_ExportedClassArrayProperty(this.instance, valueInstance);
  }

  public get IntArrayProperty(): Array<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_IntArrayProperty(this.instance);
  }

  public set IntArrayProperty(value: Array<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_IntArrayProperty(this.instance, value);
  }

  public get StringArrayProperty(): Array<string> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_StringArrayProperty(this.instance);
  }

  public set StringArrayProperty(value: Array<string>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_StringArrayProperty(this.instance, value);
  }

  public get DoubleArrayProperty(): Array<number> {
    return TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.get_DoubleArrayProperty(this.instance);
  }

  public set DoubleArrayProperty(value: Array<number>) {
    TypeShimConfig.exports.TypeShim.Sample.CompilationTestInterop.set_DoubleArrayProperty(this.instance, value);
  }
}
export namespace CompilationTest {
  export interface Initializer {
    NIntProperty: number;
    ByteProperty: number;
    ShortProperty: number;
    IntProperty: number;
    LongProperty: number;
    BoolProperty: boolean;
    StringProperty: string;
    CharProperty: string;
    DoubleProperty: number;
    FloatProperty: number;
    DateTimeProperty: Date;
    DateTimeOffsetProperty: Date;
    objectProperty: object;
    ExportedClassProperty: ExportedClass | ExportedClass.Initializer;
    JSObjectProperty: object;
    TaskProperty: Promise<void>;
    TaskOfNIntProperty: Promise<number>;
    TaskOfShortProperty: Promise<number>;
    TaskOfIntProperty: Promise<number>;
    TaskOfLongProperty: Promise<number>;
    TaskOfBoolProperty: Promise<boolean>;
    TaskOfByteProperty: Promise<number>;
    TaskOfCharProperty: Promise<string>;
    TaskOfStringProperty: Promise<string>;
    TaskOfDoubleProperty: Promise<number>;
    TaskOfFloatProperty: Promise<number>;
    TaskOfDateTimeProperty: Promise<Date>;
    TaskOfDateTimeOffsetProperty: Promise<Date>;
    TaskOfObjectProperty: Promise<object>;
    TaskOfExportedClassProperty: Promise<ExportedClass | ExportedClass.Initializer>;
    TaskOfJSObjectProperty: Promise<object>;
    ByteArrayProperty: Array<number>;
    JSObjectArrayProperty: Array<object>;
    ObjectArrayProperty: Array<object>;
    ExportedClassArrayProperty: Array<ExportedClass | ExportedClass.Initializer>;
    IntArrayProperty: Array<number>;
    StringArrayProperty: Array<string>;
    DoubleArrayProperty: Array<number>;
  }
  export interface Snapshot {
    NIntProperty: number;
    ByteProperty: number;
    ShortProperty: number;
    IntProperty: number;
    LongProperty: number;
    BoolProperty: boolean;
    StringProperty: string;
    CharProperty: string;
    DoubleProperty: number;
    FloatProperty: number;
    DateTimeProperty: Date;
    DateTimeOffsetProperty: Date;
    objectProperty: object;
    ExportedClassProperty: ExportedClass.Snapshot;
    JSObjectProperty: object;
    TaskProperty: Promise<void>;
    TaskOfNIntProperty: Promise<number>;
    TaskOfShortProperty: Promise<number>;
    TaskOfIntProperty: Promise<number>;
    TaskOfLongProperty: Promise<number>;
    TaskOfBoolProperty: Promise<boolean>;
    TaskOfByteProperty: Promise<number>;
    TaskOfCharProperty: Promise<string>;
    TaskOfStringProperty: Promise<string>;
    TaskOfDoubleProperty: Promise<number>;
    TaskOfFloatProperty: Promise<number>;
    TaskOfDateTimeProperty: Promise<Date>;
    TaskOfDateTimeOffsetProperty: Promise<Date>;
    TaskOfObjectProperty: Promise<object>;
    TaskOfExportedClassProperty: Promise<ExportedClass.Snapshot>;
    TaskOfJSObjectProperty: Promise<object>;
    ByteArrayProperty: Array<number>;
    JSObjectArrayProperty: Array<object>;
    ObjectArrayProperty: Array<object>;
    ExportedClassArrayProperty: Array<ExportedClass.Snapshot>;
    IntArrayProperty: Array<number>;
    StringArrayProperty: Array<string>;
    DoubleArrayProperty: Array<number>;
  }
  export function materialize(proxy: CompilationTest): CompilationTest.Snapshot {
    return {
      NIntProperty: proxy.NIntProperty,
      ByteProperty: proxy.ByteProperty,
      ShortProperty: proxy.ShortProperty,
      IntProperty: proxy.IntProperty,
      LongProperty: proxy.LongProperty,
      BoolProperty: proxy.BoolProperty,
      StringProperty: proxy.StringProperty,
      CharProperty: proxy.CharProperty,
      DoubleProperty: proxy.DoubleProperty,
      FloatProperty: proxy.FloatProperty,
      DateTimeProperty: proxy.DateTimeProperty,
      DateTimeOffsetProperty: proxy.DateTimeOffsetProperty,
      objectProperty: proxy.objectProperty,
      ExportedClassProperty: ExportedClass.materialize(proxy.ExportedClassProperty),
      JSObjectProperty: proxy.JSObjectProperty,
      TaskProperty: proxy.TaskProperty,
      TaskOfNIntProperty: proxy.TaskOfNIntProperty,
      TaskOfShortProperty: proxy.TaskOfShortProperty,
      TaskOfIntProperty: proxy.TaskOfIntProperty,
      TaskOfLongProperty: proxy.TaskOfLongProperty,
      TaskOfBoolProperty: proxy.TaskOfBoolProperty,
      TaskOfByteProperty: proxy.TaskOfByteProperty,
      TaskOfCharProperty: proxy.TaskOfCharProperty,
      TaskOfStringProperty: proxy.TaskOfStringProperty,
      TaskOfDoubleProperty: proxy.TaskOfDoubleProperty,
      TaskOfFloatProperty: proxy.TaskOfFloatProperty,
      TaskOfDateTimeProperty: proxy.TaskOfDateTimeProperty,
      TaskOfDateTimeOffsetProperty: proxy.TaskOfDateTimeOffsetProperty,
      TaskOfObjectProperty: proxy.TaskOfObjectProperty,
      TaskOfExportedClassProperty: proxy.TaskOfExportedClassProperty.then(e => ExportedClass.materialize(e)),
      TaskOfJSObjectProperty: proxy.TaskOfJSObjectProperty,
      ByteArrayProperty: proxy.ByteArrayProperty,
      JSObjectArrayProperty: proxy.JSObjectArrayProperty,
      ObjectArrayProperty: proxy.ObjectArrayProperty,
      ExportedClassArrayProperty: proxy.ExportedClassArrayProperty.map(e => ExportedClass.materialize(e)),
      IntArrayProperty: proxy.IntArrayProperty,
      StringArrayProperty: proxy.StringArrayProperty,
      DoubleArrayProperty: proxy.DoubleArrayProperty,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.ExportedClass
export class ExportedClass extends ProxyBase {
  constructor(jsObject: ExportedClass.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.ExportedClassInterop.ctor(jsObject));
  }

  public get Id(): number {
    return TypeShimConfig.exports.TypeShim.Sample.ExportedClassInterop.get_Id(this.instance);
  }

  public set Id(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.ExportedClassInterop.set_Id(this.instance, value);
  }
}
export namespace ExportedClass {
  export interface Initializer {
    Id: number;
  }
  export interface Snapshot {
    Id: number;
  }
  export function materialize(proxy: ExportedClass): ExportedClass.Snapshot {
    return {
      Id: proxy.Id,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.People
export class People extends ProxyBase {
  constructor(jsObject: People.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.ctor(jsObject));
  }

  public get All(): Array<Person> {
    const res = TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.get_All(this.instance);
    return res.map(e => ProxyBase.fromHandle(Person, e));
  }

  public set All(value: Array<Person | Person.Initializer>) {
    const valueInstance = value.map(e => e instanceof Person ? e.instance : e);
    TypeShimConfig.exports.TypeShim.Sample.PeopleInterop.set_All(this.instance, valueInstance);
  }
}
export namespace People {
  export interface Initializer {
    All: Array<Person | Person.Initializer>;
  }
  export interface Snapshot {
    All: Array<Person.Snapshot>;
  }
  export function materialize(proxy: People): People.Snapshot {
    return {
      All: proxy.All.map(e => Person.materialize(e)),
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.Person
export class Person extends ProxyBase {
  constructor(jsObject: Person.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.PersonInterop.ctor(jsObject));
  }

  public IsOlderThan(other: Person | Person.Initializer): boolean {
    const otherInstance = other instanceof Person ? other.instance : other;
    return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.IsOlderThan(this.instance, otherInstance);
  }

  public AdoptPet(): void {
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.AdoptPet(this.instance);
  }

  public Adopt(newPet: Dog | Dog.Initializer): void {
    const newPetInstance = newPet instanceof Dog ? newPet.instance : newPet;
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.Adopt(this.instance, newPetInstance);
  }
  public get Id(): number {
    return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Id(this.instance);
  }

  public set Id(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Id(this.instance, value);
  }

  public get Name(): string {
    return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Name(this.instance);
  }

  public set Name(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Name(this.instance, value);
  }

  public get Age(): number {
    return TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Age(this.instance);
  }

  public set Age(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Age(this.instance, value);
  }

  public get Pets(): Array<Dog> {
    const res = TypeShimConfig.exports.TypeShim.Sample.PersonInterop.get_Pets(this.instance);
    return res.map(e => ProxyBase.fromHandle(Dog, e));
  }

  public set Pets(value: Array<Dog | Dog.Initializer>) {
    const valueInstance = value.map(e => e instanceof Dog ? e.instance : e);
    TypeShimConfig.exports.TypeShim.Sample.PersonInterop.set_Pets(this.instance, valueInstance);
  }
}
export namespace Person {
  export interface Initializer {
    Id: number;
    Name: string;
    Age: number;
    Pets: Array<Dog | Dog.Initializer>;
  }
  export interface Snapshot {
    Id: number;
    Name: string;
    Age: number;
    Pets: Array<Dog.Snapshot>;
  }
  export function materialize(proxy: Person): Person.Snapshot {
    return {
      Id: proxy.Id,
      Name: proxy.Name,
      Age: proxy.Age,
      Pets: proxy.Pets.map(e => Dog.materialize(e)),
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.Dog
export class Dog extends ProxyBase {
  constructor(jsObject: Dog.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.DogInterop.ctor(jsObject));
  }

  public Bark(): string {
    return TypeShimConfig.exports.TypeShim.Sample.DogInterop.Bark(this.instance);
  }

  public GetAge(asHumanYears: boolean): number {
    return TypeShimConfig.exports.TypeShim.Sample.DogInterop.GetAge(this.instance, asHumanYears);
  }
  public get Name(): string {
    return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Name(this.instance);
  }

  public set Name(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Name(this.instance, value);
  }

  public get Breed(): string {
    return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Breed(this.instance);
  }

  public set Breed(value: string) {
    TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Breed(this.instance, value);
  }

  public get Age(): number {
    return TypeShimConfig.exports.TypeShim.Sample.DogInterop.get_Age(this.instance);
  }

  public set Age(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.DogInterop.set_Age(this.instance, value);
  }
}
export namespace Dog {
  export interface Initializer {
    Name: string;
    Breed: string;
    Age: number;
  }
  export interface Snapshot {
    Name: string;
    Breed: string;
    Age: number;
  }
  export function materialize(proxy: Dog): Dog.Snapshot {
    return {
      Name: proxy.Name,
      Breed: proxy.Breed,
      Age: proxy.Age,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.MyApp
export class MyApp extends ProxyBase {
  constructor() {
    super(TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.ctor());
  }

  public static Initialize(baseAddress: string): void {
    TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.Initialize(baseAddress);
  }

  public static GetPeopleProvider(): PeopleProvider {
    const res = TypeShimConfig.exports.TypeShim.Sample.MyAppInterop.GetPeopleProvider();
    return ProxyBase.fromHandle(PeopleProvider, res);
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.PeopleProvider
export class PeopleProvider extends ProxyBase {
  constructor(apiClient: ManagedObject, jsObject: PeopleProvider.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.ctor(apiClient, jsObject));
  }

  public async FetchPeopleAsync(): Promise<People> {
    const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.FetchPeopleAsync(this.instance);
    return res.then(e => ProxyBase.fromHandle(People, e));
  }
  public get PeopleCache(): Array<Person> | null {
    const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_PeopleCache(this.instance);
    return res ? res.map(e => ProxyBase.fromHandle(Person, e)) : null;
  }

  public get DelayTask(): Promise<TimeoutUnit | null> | null {
    const res = TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.get_DelayTask(this.instance);
    return res ? res.then(e => e ? ProxyBase.fromHandle(TimeoutUnit, e) : null) : null;
  }

  public set DelayTask(value: Promise<TimeoutUnit | TimeoutUnit.Initializer | null> | null) {
    const valueInstance = value ? value.then(e => e ? e instanceof TimeoutUnit ? e.instance : e : null) : null;
    TypeShimConfig.exports.TypeShim.Sample.PeopleProviderInterop.set_DelayTask(this.instance, valueInstance);
  }
}
export namespace PeopleProvider {
  export interface Initializer {
    DelayTask: Promise<TimeoutUnit | TimeoutUnit.Initializer | null> | null;
  }
  export interface Snapshot {
    PeopleCache: Array<Person.Snapshot> | null;
    DelayTask: Promise<TimeoutUnit.Snapshot | null> | null;
  }
  export function materialize(proxy: PeopleProvider): PeopleProvider.Snapshot {
    return {
      PeopleCache: proxy.PeopleCache ? proxy.PeopleCache.map(e => Person.materialize(e)) : null,
      DelayTask: proxy.DelayTask ? proxy.DelayTask.then(e => e ? TimeoutUnit.materialize(e) : null) : null,
    };
  }
}

// TypeShim generated TypeScript definitions for class: TypeShim.Sample.TimeoutUnit
export class TimeoutUnit extends ProxyBase {
  constructor(jsObject: TimeoutUnit.Initializer) {
    super(TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.ctor(jsObject));
  }

  public get Timeout(): number {
    return TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.get_Timeout(this.instance);
  }

  public set Timeout(value: number) {
    TypeShimConfig.exports.TypeShim.Sample.TimeoutUnitInterop.set_Timeout(this.instance, value);
  }
}
export namespace TimeoutUnit {
  export interface Initializer {
    Timeout: number;
  }
  export interface Snapshot {
    Timeout: number;
  }
  export function materialize(proxy: TimeoutUnit): TimeoutUnit.Snapshot {
    return {
      Timeout: proxy.Timeout,
    };
  }
}

