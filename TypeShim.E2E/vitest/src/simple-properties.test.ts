import { describe, test, expect, beforeEach } from 'vitest';
import { ExportedClass, SimplePropertiesTest, TaskPropertiesClass } from '@typeshim/e2e-wasm-lib';

describe('Simple Properties Test', () => {
    let exportedClass: ExportedClass;
    let testObject: SimplePropertiesTest;
    let jsObject = { foo: "bar" };
    let dateNow = new Date();
    beforeEach(() => {
        exportedClass = new ExportedClass({ Id: 2 });
        testObject = new SimplePropertiesTest({
            NIntProperty: 1,
            ByteProperty: 2,
            ShortProperty: 3,
            IntProperty: 4,
            LongProperty: 5,
            BoolProperty: true,
            StringProperty: "Test",
            CharProperty: 'A',
            CharNullableProperty: null,
            DoubleProperty: 6.7,
            FloatProperty: 8.9,
            DateTimeProperty: dateNow,
            DateTimeOffsetProperty: dateNow,
            ExportedClassProperty: exportedClass,
            ObjectProperty: exportedClass.instance,
            JSObjectProperty: jsObject,
        });
    });

    test('Snapshot has property-value equality', async () => {
        expect(testObject).toBeDefined();
        const snapshot = SimplePropertiesTest.materialize(testObject);
        expect(testObject).toMatchObject(snapshot); // each property in snapshot should have a matching value in testObject
    });
    test('Returns ExportedClass property correctly', () => {
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(2);
    });
    test('Mutates ExportedClass property correctly', () => {
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(2);
        const newExportedClass = new ExportedClass({ Id: 99 });
        testObject.ExportedClassProperty = newExportedClass;
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(99);
        expect(testObject.ExportedClassProperty).toBe(newExportedClass);
    });
    test('Mutates ExportedClass property with Initializer', () => {
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(2);
        const exportedClassInitializer = { Id: 12345 };
        testObject.ExportedClassProperty = exportedClassInitializer;
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(12345);
    });
    test('Mutates ExportedClass property does not affect snapshot', () => {
        const snapshot = SimplePropertiesTest.materialize(testObject);
        expect(testObject.ExportedClassProperty).toBeInstanceOf(ExportedClass);
        expect(testObject.ExportedClassProperty.Id).toBe(2);
        const newExportedClass = new ExportedClass({ Id: 99 });
        testObject.ExportedClassProperty = newExportedClass;
        expect(snapshot.ExportedClassProperty.Id).toBe(2);
    });
    test('Returns JSObject property by reference', () => {
        expect(testObject.JSObjectProperty).toBe(jsObject);
        const obj = testObject.JSObjectProperty as any;
        obj.bar = 123;
        expect(testObject.JSObjectProperty).toHaveProperty("bar", 123);
        expect(testObject.JSObjectProperty).toHaveProperty("foo", "bar");
    });
    test('Returns DateTime property as new instance', () => {
        // dates are value object in dotnet, hence the new instance
        expect(testObject.DateTimeProperty).toBeInstanceOf(Date);
        expect(testObject.DateTimeProperty).not.toBe(dateNow);
        expect(testObject.DateTimeProperty).toEqual(dateNow);
    });
    test('Returns DateTimeOffset property as new instance', () => {
        // dates are value object in dotnet, hence the new instance
        expect(testObject.DateTimeOffsetProperty).toBeInstanceOf(Date);
        expect(testObject.DateTimeOffsetProperty).not.toBe(dateNow);
        expect(testObject.DateTimeOffsetProperty).toEqual(dateNow); 
    });
    test('Returns Long property by value', () => {
        expect(testObject.LongProperty).toBe(5);
    });
    test('Mutates Long property', () => {
        testObject.LongProperty = 50;
        expect(testObject.LongProperty).toBe(50);
    });
});
