using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.TypeScript;
using NJsonSchema.Generation;
using NJsonSchema.NewtonsoftJson.Generation;
using NSwag.CodeGeneration.TypeScript;
using Parlot.Fluent;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TypeScriptExportGenerator;

namespace TypeScriptExport;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: TypeScriptExport <dll-path> <output-ts-file>");
            return;
        }

        var dllPath = args[0];
        var outputPath = args[1];
        var asm = Assembly.LoadFrom(dllPath);

        // Find types with [TsExport] attribute
        var typesToExport = asm.GetTypes()
            .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == nameof(TsExportAttribute)))
            .ToList();

        // Generate JSON Schema for those types
        JsonSchemaGeneratorSettings schemaSettings = new NewtonsoftJsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            GenerateAbstractProperties = false
        };

        JsonSchemaGenerator schemaGenerator = new(new NewtonsoftJsonSchemaGeneratorSettings()
        {
            DefaultReferenceTypeNullHandling = ReferenceTypeNullHandling.NotNull,
            GenerateAbstractProperties = false
        });

        

        JsonSchemaResolver schemaResolver = new(new JsonSchema(), schemaSettings);
        //schemaGenerator.Generate(typeof(string), schemaResolver);
        //schemaGenerator.Generate(typeof(int), schemaResolver);

        TypeScriptGeneratorSettings generatorSettings = new()
        {
            TypeStyle = TypeScriptTypeStyle.Class,
            GenerateConstructorInterface = false,
        };

        TypeScriptTypeResolver typeResolver = new(generatorSettings);

        GeneratorTypeDictionary typeDict = new(typesToExport);

        TypeScriptGeneratorWithMethods typeScriptGenerator = new(schemaResolver.RootObject, typeResolver, schemaResolver, schemaGenerator, typeDict);

        foreach (Type? type in typesToExport)
        {
            JsonSchema schema = schemaGenerator.Generate(type, schemaResolver);
            var artifacts = typeScriptGenerator.GenerateTypes(schema, type.Name);
        }
        string tsCode = typeScriptGenerator.GenerateFile();

        File.WriteAllText(outputPath, tsCode);
        Console.WriteLine($"Exported {typesToExport.Count} classes to {outputPath}");
    }
}