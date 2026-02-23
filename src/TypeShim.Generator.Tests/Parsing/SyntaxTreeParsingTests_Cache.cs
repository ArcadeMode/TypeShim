using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_Cache
{
    [Test]
    public void CSharpInteropClass_InstanceProperty_WithNullableUserClassType()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class MyClass
            {
                public void M1()
                {
                }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public MyClass? P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(2));
        INamedTypeSymbol classSymbol = exportedClasses.First();
        INamedTypeSymbol userClassSymbol = exportedClasses.Last();

        InteropTypeInfoCache cache = new ();

        // Regression being guarded:
        // 1. first time encountering the user class type is as a nullable property type. this is cached
        // 2. cache does not distinguish typesymbol from non-nullable while building class info for the user class
        // 3. user class info (served from cache) incorrectly shows the type as nullable, resulting in invalid generated code
        ClassInfo c1ClassInfo = new ClassInfoBuilder(classSymbol, cache).Build();
        ClassInfo userClassInfo = new ClassInfoBuilder(userClassSymbol, cache).Build();

        Assert.That(userClassInfo.Type.IsNullableType, Is.False);
        Assert.That(userClassInfo.Type.CSharpTypeSyntax.ToString(), Is.EqualTo("MyClass"));
        Assert.That(userClassInfo.Type.IsTSExport, Is.True);

        PropertyInfo p1PropertyInfo = c1ClassInfo.Properties.Single();
        Assert.That(p1PropertyInfo.Type.IsNullableType, Is.True);
        Assert.That(p1PropertyInfo.Type.CSharpTypeSyntax.ToString(), Is.EqualTo("MyClass?"));
        Assert.That(p1PropertyInfo.Type.IsTSExport, Is.True);
    }
}
