using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_NestedTypes
{
    private static ClassInfo BuildClass(string source, string className)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);
        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exported = [.. symbolExtractor.ExtractAllExportedSymbols()];
        INamedTypeSymbol classSymbol = exported.First(s => s.Name == className);
        return new ClassInfoBuilder(classSymbol, new InteropTypeInfoCache()).Build();
    }

    [Test]
    public void NestedClassAndEnum_AreDiscovered_WithoutTSExportAnnotation()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }

                public enum Kind { A, B }

                public class Inner
                {
                    public int Value { get; set; }
                }
            }
        """, "Outer");

        Assert.That(outer.NestedTypes.Select(n => n.Name), Is.EquivalentTo(new[] { "Kind", "Inner" }));
        Assert.That(outer.NestedTypes.OfType<EnumInfo>().Single().Name, Is.EqualTo("Kind"));
        Assert.That(outer.NestedTypes.OfType<ClassInfo>().Single().Name, Is.EqualTo("Inner"));
    }

    [Test]
    public void NestedTypes_InheritExportedness_FromContainer()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }
                public class Inner
                {
                    public int Value { get; set; }
                }
            }
        """, "Outer");

        ClassInfo inner = outer.NestedTypes.OfType<ClassInfo>().Single();
        Assert.That(inner.IsTSExport, Is.True);
        Assert.That(inner.Type.IsTSExport, Is.True);
    }

    [Test]
    public void MultiLevelNesting_IsDiscoveredRecursively()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }
                public class Middle
                {
                    public int MiddleValue { get; set; }
                    public class Inner
                    {
                        public int InnerValue { get; set; }
                    }
                }
            }
        """, "Outer");

        ClassInfo middle = outer.NestedTypes.OfType<ClassInfo>().Single(c => c.Name == "Middle");
        ClassInfo inner = middle.NestedTypes.OfType<ClassInfo>().Single(c => c.Name == "Inner");
        Assert.That(inner.Name, Is.EqualTo("Inner"));
        Assert.That(inner.NestedTypes, Is.Empty);
    }

    [Test]
    public void ContainerClass_WithOnlyNestedTypes_IsRetained()
    {
        ClassInfo container = BuildClass("""
            namespace N1;
            [TSExport]
            public class Container
            {
                public class Thing
                {
                    public int Value { get; set; }
                }
            }
        """, "Container");

        Assert.That(container.Methods, Is.Empty);
        Assert.That(container.Properties, Is.Empty);
        Assert.That(container.NestedTypes.Select(n => n.Name), Is.EquivalentTo(new[] { "Thing" }));
    }

    [Test]
    public void EmptyNestedClass_IsDropped()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }
                public class Empty
                {
                }
            }
        """, "Outer");

        Assert.That(outer.NestedTypes, Is.Empty);
    }

    [Test]
    public void NonPublicNestedTypes_AreNotDiscovered()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }
                private class Hidden
                {
                    public int Value { get; set; }
                }
            }
        """, "Outer");

        Assert.That(outer.NestedTypes, Is.Empty);
    }

    [Test]
    public void InternalNestedTypes_AreNotDiscovered()
    {
        ClassInfo outer = BuildClass("""
            namespace N1;
            [TSExport]
            public class Outer
            {
                public int Id { get; set; }
                internal class Hidden
                {
                    public int Value { get; set; }
                }
                internal enum HiddenKind
                {
                    A,
                    B
                }
            }
        """, "Outer");

        Assert.That(outer.NestedTypes, Is.Empty);
    }
}
