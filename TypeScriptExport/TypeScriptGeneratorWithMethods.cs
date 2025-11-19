using NJsonSchema;
using NJsonSchema.CodeGeneration;
using NJsonSchema.CodeGeneration.TypeScript;
using NJsonSchema.CodeGeneration.TypeScript.Models;
using NJsonSchema.Generation;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using TypeScriptExportGenerator;

namespace TypeScriptExport;

public sealed class GeneratorTypeDictionary(IEnumerable<Type> types) : Dictionary<string, Type>(types.ToDictionary(type => type.Name)) { }

/// <summary>Generates TypeScript code from a JSON schema model, including C# method signatures injected via reflection.</summary>
public class TypeScriptGeneratorWithMethods : GeneratorBase
{
    private readonly TypeScriptTypeResolver _typeResolver;
    private readonly JsonSchemaGenerator _schemaGenerator;
    private readonly JsonSchemaResolver _schemaResolver;
    private readonly GeneratorTypeDictionary _typeDict;

    private TypeScriptExtensionCode? _extensionCode;

    /// <summary>Gets the generator settings.</summary>
    public TypeScriptGeneratorSettings Settings { get; }

    /// <summary>Initializes a new instance of the <see cref="TypeScriptGenerator"/> class.</summary>
    /// <param name="rootObject">The root object to search for all JSON Schemas.</param>
    /// <param name="settings">The generator settings.</param>
    public TypeScriptGeneratorWithMethods(object rootObject, 
        TypeScriptTypeResolver typeScriptTypeResolver, 
        JsonSchemaResolver jsonSchemaResolver,
        JsonSchemaGenerator jsonSchemaGenerator,
        GeneratorTypeDictionary typeDict)
        : base(rootObject, typeScriptTypeResolver, typeScriptTypeResolver.Settings)
    {
        _typeResolver = typeScriptTypeResolver;
        _schemaResolver = jsonSchemaResolver;
        _schemaGenerator = jsonSchemaGenerator;
        _typeDict = typeDict;
        Settings = typeScriptTypeResolver.Settings;
    }

    

    /// <summary>Generates all types from the resolver with extension code from the settings.</summary>
    /// <returns>The code.</returns>
    public override IEnumerable<CodeArtifact> GenerateTypes()
    {
        _extensionCode ??= new TypeScriptExtensionCode(Settings.ExtensionCode, Settings.ExtendedClasses);

        return GenerateTypes(_extensionCode);
    }

    /// <summary>Generates all types from the resolver with the given extension code.</summary>
    /// <returns>The code.</returns>
    public IEnumerable<CodeArtifact> GenerateTypes(TypeScriptExtensionCode extensionCode)
    {
        var artifacts = base.GenerateTypes();
        foreach (var artifact in artifacts)
        {
            if (extensionCode?.ExtensionClasses.ContainsKey(artifact.TypeName) == true)
            {
                var classCode = artifact.Code;

                var index = classCode.IndexOf("constructor(", StringComparison.Ordinal);
                if (index != -1)
                {
                    var code = classCode.Insert(index, extensionCode.GetExtensionClassBody(artifact.TypeName).Trim() + "\n\n    ");
                    yield return new CodeArtifact(artifact.TypeName, artifact.BaseTypeName, artifact.Type, artifact.Language, artifact.Category, code);
                }
                else
                {
                    index = classCode.IndexOf("class", StringComparison.Ordinal);
                    index = classCode.IndexOf('{', index) + 1;

                    var code = classCode.Insert(index, "\n    " + extensionCode.GetExtensionClassBody(artifact.TypeName).Trim() + "\n");
                    yield return new CodeArtifact(artifact.TypeName, artifact.BaseTypeName, artifact.Type, artifact.Language, artifact.Category, code);
                }
            }
            else
            {
                yield return artifact;
            }
        }

        if (artifacts.Any(r => r.Code.Contains("formatDate(")))
        {
            var template = Settings.TemplateFactory.CreateTemplate("TypeScript", "File.FormatDate", new object());
            yield return new CodeArtifact("formatDate", CodeArtifactType.Function, CodeArtifactLanguage.CSharp, CodeArtifactCategory.Utility, template);
        }
        if (artifacts.Any(r => r.Code.Contains("parseDateOnly(")))
        {
            var template = Settings.TemplateFactory.CreateTemplate("TypeScript", "File.ParseDateOnly", new object());
            yield return new CodeArtifact("parseDateOnly", CodeArtifactType.Function, CodeArtifactLanguage.CSharp, CodeArtifactCategory.Utility, template);
        }

        if (Settings.HandleReferences)
        {
            var template = Settings.TemplateFactory.CreateTemplate("TypeScript", "File.ReferenceHandling", new object());
            yield return new CodeArtifact("jsonParse", CodeArtifactType.Function, CodeArtifactLanguage.CSharp, CodeArtifactCategory.Utility, template);
        }
    }

    /// <summary>Generates the file.</summary>
    /// <returns>The file contents.</returns>
    protected override string GenerateFile(IEnumerable<CodeArtifact> artifacts)
    {
        var model = new FileTemplateModel(Settings)
        {
            Types = artifacts.OrderByBaseDependency().Concatenate(),
            ExtensionCode = _extensionCode
        };

        var template = Settings.TemplateFactory.CreateTemplate("TypeScript", "File", model);
        return ConversionUtilities.TrimWhiteSpaces(template.Render());
    }

    /// <summary>Generates the type.</summary>
    /// <param name="schema">The schema.</param>
    /// <param name="typeNameHint">The fallback type name.</param>
    /// <returns>The code.</returns>
    protected override CodeArtifact GenerateType(JsonSchema schema, string typeNameHint)
    {
        Console.WriteLine($"Starting type generation with hint: {typeNameHint}");

        var typeName = _typeResolver.GetOrGenerateTypeName(schema, typeNameHint);

        if (schema.IsEnumeration)
        {
            EnumTemplateModel model = new(typeName, schema, Settings);

            string templateName;
            if (Settings.EnumStyle == TypeScriptEnumStyle.Enum)
            {
                templateName = nameof(TypeScriptEnumStyle.Enum);
            }
            else if (Settings.EnumStyle == TypeScriptEnumStyle.StringLiteral)
            {
                templateName = $"{nameof(TypeScriptEnumStyle.Enum)}.{nameof(TypeScriptEnumStyle.StringLiteral)}";
            }
            else
            {
                throw new NotImplementedException($"{nameof(Settings.EnumStyle)}:{Settings.EnumStyle} is not implemented");
            }

            ITemplate template = Settings.TemplateFactory.CreateTemplate("TypeScript", templateName, model);
            return new CodeArtifact(typeName, CodeArtifactType.Enum, CodeArtifactLanguage.TypeScript, CodeArtifactCategory.Contract, template);
        }
        else
        {
            ClassTemplateModel model = new ClassTemplateModel(typeName, typeNameHint, Settings, _typeResolver, schema, RootObject);
            var template = Settings.TemplateFactory.CreateTemplate("TypeScript", "Interface", model);

            CodeArtifactType type = Settings.TypeStyle == TypeScriptTypeStyle.Interface
                ? CodeArtifactType.Interface
                : CodeArtifactType.Class;

            string code = GenerateTypeScriptWithMethods(typeName, template);
            return new CodeArtifact(typeName, model.BaseClass, type, CodeArtifactLanguage.TypeScript, CodeArtifactCategory.Contract, code);
        }

        
    }

    private string GenerateTypeScriptWithMethods(string typeName, ITemplate template)
    {
        string code = template.Render();

        if (string.Equals(typeName, "Void", StringComparison.OrdinalIgnoreCase))
        { 
            // TODO: handle void type properly
            return string.Empty;
        }

        if (!_typeDict.TryGetValue(typeName, out var dotNetType))
        {
            throw new InvalidOperationException($"Type '{typeName}' not found in type dictionary, all types to generate must be provided.");
        }

        Console.WriteLine($"Injecting methods for TypeScript type '{typeName}' from .NET type '{dotNetType.FullName}'");
        IEnumerable<MethodInfo> methodsToGenerateTsFor = dotNetType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName);

        IEnumerable<string> typescriptMethodDefinitions = methodsToGenerateTsFor.Select(m =>
        {
            var parameterExpressions = m.GetParameters()
                .Select(p =>
                {
                    
                    return $"{p.Name}: {ResolveTypeReference(p)}";
                });
            string parameters = string.Join(", ", parameterExpressions);
            
            return $"    {m.Name}({parameters}): {ResolveTypeReference(m.ReturnParameter)};"; // each method signature is generated here
            //TODO: bodies etc.
        });

        Console.WriteLine($"Generating {typescriptMethodDefinitions.Count()} method signatures for type '{typeName}'.");

        // Insert before last '}' in the code artifact
        var braceClose = code.LastIndexOf('}');
        if (braceClose > 0 && typescriptMethodDefinitions.Any())
        {
            code = code.Insert(braceClose, "\n" + string.Join("\n", typescriptMethodDefinitions) + "\n");
        }

        return code;
    }

    private string ResolveTypeReference(ParameterInfo parameter)
    {
        Type referencedType = parameter.ParameterType;

        if (referencedType == typeof(void))
        {
            return "void"; // void is not handled right by the lib (i.e. generates a new type 'Void') this hack fixes that for now.
        }

        CustomAttributeData? attributeData = parameter.GetCustomAttributesData().FirstOrDefault(attributeData => attributeData.AttributeType.Name == typeof(TsExportAsAttribute<>).Name);
        Type? userDefinedType = attributeData?.AttributeType.GetGenericArguments().FirstOrDefault();

        //attributeData.ConstructorArguments.First().

        Type typeToGenerate = userDefinedType ?? referencedType;
        return _typeResolver.Resolve(_schemaGenerator.Generate(typeToGenerate, _schemaResolver), IsNullableType(typeToGenerate), typeToGenerate.Name);

        static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}