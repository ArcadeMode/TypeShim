using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Shared;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_UnsupportedType
{
    [TestCase("SByte")]
    [TestCase("sbyte")]
    [TestCase("UInt16")]
    [TestCase("ushort")]
    [TestCase("UInt32")]
    [TestCase("uint")]
    [TestCase("UInt64")]
    [TestCase("ulong")]
    [TestCase("UIntPtr")]
    [TestCase("nuint")]
    [TestCase("Decimal")]
    [TestCase("decimal")]
    public void ClassInfoBuilder_Throws_ForUnsupportedNumericReturnType(string typeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static {{typeExpression}} M1()
                {
                    return 1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [TestCase("MyUnannotatedClass")]
    [TestCase("ConcurrentDictionary<int, string>")]
    public void ClassInfoBuilder_Throws_ForUnknownReturnType(string typeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TSExport]
            public class C1
            {
                public {{typeExpression}} P1 { get; set; }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [TestCase("int[]")] // Task<int[]>
    [TestCase("int?")] // Task<int?>
    [TestCase("byte?")]
    [TestCase("double?")]
    [TestCase("long?")]
    [TestCase("Task")] // Task<Task>
    [TestCase("Task<int>")]
    [TestCase("Action")]
    [TestCase("Task<Action>")]
    [TestCase("Task<Span<Byte>")]
    public void ClassInfoBuilder_Throws_ForUnsupportedTaskTypeArguments(string typeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static Task<{{typeExpression}}> M1()
                {
                    return Task.FromResult(1);
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [TestCase("bool")] // bool[] not supported
    [TestCase("Char")]
    [TestCase("Int16")]
    [TestCase("Int64")]
    [TestCase("float")]
    [TestCase("IntPtr")]
    [TestCase("DateTime")]
    [TestCase("DateTimeOffset")]
    [TestCase("Exception")]
    [TestCase("Task")]
    [TestCase("Task<int>")]
    [TestCase("Action")]
    [TestCase("Function<int>")]
    [TestCase("Function<int, int>")]
    [TestCase("int?")] // int?[]
    public void ClassInfoBuilder_Throws_ForUnsupportedArrayTypeArguments(string typeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public static {{typeExpression}}[] M1()
                {
                    return Task.FromResult(1);
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedTypeException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }
    
    [Test]
    public void ClassInfoBuilder_Throws_ForGenericClass()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1<T>
            {
                public int M1()
                {
                    return 1;
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedGenericClassException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }
    [Test]
    public void ClassInfoBuilder_Throws_ForGenericMethod()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public int M1<T>()
                {
                    return 1;
                }
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<NotSupportedGenericMethodException>(() =>
        {
            InteropTypeInfoCache typeCache = new();
            _ = new ClassInfoBuilder(classSymbol, typeCache).Build();
        });
    }

    [Test]
    public void ClassInfoBuilder_Throws_ForUnexportedTypeInPrimaryConstructor()
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            public class UnmarshallableType
            {
            }
            [TSExport]
            public class C1(UnmarshallableType name)
            {
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses.Last();
        InteropTypeInfoCache typeCache = new();
        Assert.Throws<NotSupportedTypeException>(() => new ClassInfoBuilder(classSymbol, typeCache).Build());
    }

    [Test]
    public void ClassInfoBuilder_Throws_ForUnexportedTypeInDelegateProperty()
    {
        SyntaxTree userClass = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            //[TSExport] unexported
            public class MyClass
            {
                public int Id { get; set; }
            }
        """);

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            using System.Threading.Tasks;
            namespace N1;
            [TSExport]
            public class C1
            {
                public Func<MyClass, MyClass> P1 { get; set; }
            }
        """);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree), CSharpFileInfo.Create(userClass)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));

        InteropTypeInfoCache typeCache = new();

        Assert.Throws<NotSupportedTypeException>(() => new ClassInfoBuilder(exportedClasses.First(), typeCache).Build());
    }
}
