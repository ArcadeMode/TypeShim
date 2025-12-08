using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;

namespace TypeShim.Generator.Tests.TypeScript;

internal class TypeScriptInteropInterfaceRenderer
{
    [Test]
    public void TypeScriptInteropInterfaceRenderer_InstanceMethod_WithNullableUserClassParameterType_HasObjectOrNullType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class UserClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TsExport]
            public class C1
            {
                public void DoStuff(UserClass? u) {}
            }
        """);

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree, userClass]);
        List<INamedTypeSymbol> exportedClasses = [
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot()),
            ..TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(userClass), userClass.GetRoot()),
        ];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        INamedTypeSymbol userClassSymbol = exportedClasses[1];

        ClassInfo classInfo = new ClassInfoBuilder(classSymbol).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol).Build();

        TypeScriptTypeMapper typeMapper = new([classInfo, userClassInfo]);
        TypescriptClassNameBuilder classNameBuilder = new(typeMapper);
        TypeScriptMethodRenderer methodRenderer = new(typeMapper);

        string interopClass = new TypescriptInteropInterfaceRenderer(classInfo, methodRenderer, classNameBuilder).Render();

        Assert.That(interopClass, Is.EqualTo("""    
// Auto-generated TypeScript interop interface. Source class: N1.C1
export interface C1Interop {
    DoStuff(instance: object, u: object | null): void;
}

"""));
    }
}
