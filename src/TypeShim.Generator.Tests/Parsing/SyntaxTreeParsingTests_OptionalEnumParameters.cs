using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_OptionalEnumParameters
{
    private static MethodParameterInfo GetParameter(string methodBody, string parameterName)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
        using System;
        namespace N1;
        [TSExport]
        public enum Priority { Low, Medium, High }
        [TSExport]
        public class C1
        {
            {{methodBody}}
        }
    """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedSymbols = [.. symbolExtractor.ExtractAllExportedSymbols()];
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedSymbols.Single(s => s.TypeKind == TypeKind.Class), typeCache).Build();

        MethodInfo method = classInfo.Methods.Single();
        return method.Parameters.Single(p => !p.IsInjectedInstanceParameter && p.Name == parameterName);
    }

    [Test]
    public void OptionalEnum_NamedMember_ResolvesUnderlyingValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(Priority p = Priority.Medium) { }", "p");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Type.IsEnum, Is.True);
        Assert.That(parameter.Default!.Value, Is.EqualTo(1));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalEnum_DefaultLiteral_ResolvesZeroAndIsDefaultLiteral()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(Priority p = default) { }", "p");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.EqualTo(0));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.True);
    }

    [Test]
    public void OptionalEnum_Cast_ResolvesUnderlyingValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(Priority p = (Priority)99) { }", "p");
        Assert.That(parameter.Default!.Value, Is.EqualTo(99));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalNullableEnum_NullDefault_HasNullValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(Priority? p = null) { }", "p");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.Null);
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalNullableEnum_ValueDefault_ResolvesUnderlyingValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(Priority? p = Priority.High) { }", "p");
        Assert.That(parameter.Default!.Value, Is.EqualTo(2));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }
}
