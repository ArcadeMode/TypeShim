using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_JSExport
{
    [Test]
    public void ClassInfoBuilder_TSExportClass_WithJSExportMethod_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            [TSExport]
            public partial class C1
            {
                [JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedMixedExportException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void SymbolExtractor_JSExportOnlyClass_IsDiscovered()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        Assert.That(exportedClasses[0].Name, Is.EqualTo("C1"));
    }

    [Test]
    public void SymbolExtractor_JSExportClass_NonJSExportMethodsDropped()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int Kept() => 1;
                public static int Dropped() => 2;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        IMethodSymbol[] publicOrdinaryMethods = [..
            classSymbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.MethodKind == MethodKind.Ordinary && m.DeclaredAccessibility == Accessibility.Public)];

        Assert.That(publicOrdinaryMethods, Has.Length.EqualTo(1));
        Assert.That(publicOrdinaryMethods[0].Name, Is.EqualTo("Kept"));
    }

    [Test]
    public void ClassInfoBuilder_JSExportClass_DropsProperties()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                public int Prop { get; set; }
                [JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        IPropertySymbol[] publicProperties = [..
            classSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.DeclaredAccessibility == Accessibility.Public)];

        Assert.That(publicProperties, Is.Empty);
    }

    [Test]
    public void ClassInfoBuilder_JSExportClass_DropsDeclaredConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                public C1(int x) {}
                [JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        // After the rewriter drops the declared C1(int) ctor, Roslyn synthesizes an implicit default ctor.
        IMethodSymbol[] publicInstanceCtors = [..
            classSymbol.Constructors.Where(m => m.DeclaredAccessibility == Accessibility.Public && !m.IsStatic)];

        Assert.That(publicInstanceCtors, Has.Length.EqualTo(1));
        Assert.That(publicInstanceCtors[0].Parameters, Is.Empty,
            "Expected only the implicit default ctor - the user-declared C1(int) should have been dropped by the rewriter.");
    }

    [Test]
    public void ClassInfoBuilder_JSExportMethod_WithOverload_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExport]
                public static int M1() => 1;
                [JSExport]
                public static int M1(int x) => x;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedMethodOverloadException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void SymbolExtractor_ClassWithoutAnyExportAttribute_IsNotDiscovered()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            public class C1
            {
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        Assert.That(symbolExtractor.ExtractAllExportedSymbols(), Is.Empty);
    }

    [Test]
    public void SymbolExtractor_JSExportMethodWithFullyQualifiedAttribute_IsRecognized()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            public partial class C1
            {
                [System.Runtime.InteropServices.JavaScript.JSExport]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        Assert.That(exportedClasses[0].Name, Is.EqualTo("C1"));
    }

    [Test]
    public void SymbolExtractor_JSExportMethodWithAttributeSuffix_IsRecognized()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;
            namespace N1;
            public partial class C1
            {
                [JSExportAttribute]
                public static int M1() => 1;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        Assert.That(exportedClasses[0].Name, Is.EqualTo("C1"));
    }

    [Test]
    public void ClassInfoBuilder_JSExportMethod_WithTSExportClassParameter_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;

            [TSExport]
            public class TSClass { }

            public partial class C1
            {
                [JSExport]
                public static void M1(TSClass p1) { }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        var classSymbol = symbolExtractor.ExtractAllExportedSymbols().First(s => s.Name == "C1");

        Assert.Throws<NotSupportedJSExportReferenceException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void ClassInfoBuilder_JSExportMethod_ReturningTSExportClass_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;

            [TSExport]
            public class TSClass { }

            public partial class C1
            {
                [JSExport]
                public static TSClass M1() => null;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        var classSymbol = symbolExtractor.ExtractAllExportedSymbols().First(s => s.Name == "C1");

        Assert.Throws<NotSupportedJSExportReferenceException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void ClassInfoBuilder_JSExportMethod_WithPlainClassParameter_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;

            public class PlainClass { }

            public partial class C1
            {
                [JSExport]
                public static void M1(PlainClass p1) { }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        var classSymbol = symbolExtractor.ExtractAllExportedSymbols().First(s => s.Name == "C1");
        // Throws NotSupportedTypeException because PlainClass is filtered out in TypeShim compilation (performance), the exception type is not ideal but its enough to prevent invalid codegen.
        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void ClassInfoBuilder_JSExportMethod_ReturningPlainClass_Throws()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Runtime.InteropServices.JavaScript;

            public class PlainClass { }

            public partial class C1
            {
                [JSExport]
                public static PlainClass M1() => null;
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        var classSymbol = symbolExtractor.ExtractAllExportedSymbols().First(s => s.Name == "C1");

        // Throws NotSupportedTypeException because PlainClass is filtered out in TypeShim compilation (performance), the exception type is not ideal but its enough to prevent invalid codegen.
        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }
}
