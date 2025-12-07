using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.CSharp;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Tests;

internal class SyntaxTreeParsingTests_UnsupportedType
{
    [TestCase("SByte", "sbyte")]
    [TestCase("sbyte", "sbyte")]
    [TestCase("UInt16", "ushort")]
    [TestCase("ushort", "ushort")]
    [TestCase("UInt32", "uint")]
    [TestCase("uint", "uint")]
    [TestCase("UInt64", "ulong")]
    [TestCase("ulong", "ulong")]
    [TestCase("UIntPtr", "nuint")]
    [TestCase("nuint", "nuint")]
    [TestCase("Decimal", "decimal")]
    [TestCase("decimal", "decimal")]
    public void ClassInfoBuilder_Throws_ForUnsupportedNumericReturnType(string typeExpression, string interopTypeExpression)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText("""
            using System;
            namespace N1;
            [TsExport]
            public class C1
            {
                public static {{typeExpression}} M1()
                {
                    return 1;
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];
        Assert.Throws<TypeNotSupportedException>(() =>
        {
            _ = new ClassInfoBuilder(classSymbol).Build();
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
            [TsExport]
            public class C1
            {
                public static Task<{{typeExpression}}> M1()
                {
                    return Task.FromResult(1);
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<TypeNotSupportedException>(() =>
        {
            _ = new ClassInfoBuilder(classSymbol).Build();
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
            [TsExport]
            public class C1
            {
                public static {{typeExpression}}[] M1()
                {
                    return Task.FromResult(1);
                }
            }
        """.Replace("{{typeExpression}}", typeExpression));

        CSharpCompilation compilation = CSharpPartialCompilation.CreatePartialCompilation([syntaxTree]);
        List<INamedTypeSymbol> exportedClasses = [.. TsExportAnnotatedClassFinder.FindLabelledClassSymbols(compilation.GetSemanticModel(syntaxTree), syntaxTree.GetRoot())];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        INamedTypeSymbol classSymbol = exportedClasses[0];

        Assert.Throws<TypeNotSupportedException>(() =>
        {
            _ = new ClassInfoBuilder(classSymbol).Build();
        });
    }
}
