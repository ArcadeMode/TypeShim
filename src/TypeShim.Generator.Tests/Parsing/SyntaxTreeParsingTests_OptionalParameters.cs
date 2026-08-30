using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_OptionalParameters
{
    private static MethodParameterInfo GetParameter(string methodBody, string parameterName)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
        using System;
        namespace N1;
        [TSExport]
        public class C1
        {
            public const int RetriesConst = 3;

            {{methodBody}}
        }
    """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        MethodInfo method = classInfo.Methods.Single();
        return method.Parameters.Single(p => !p.IsInjectedInstanceParameter && p.Name == parameterName);
    }

    [Test]
    public void RequiredParameter_HasNoDefault()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(int value) { }", "value");
        Assert.That(parameter.Default, Is.Null);
    }

    [Test]
    public void OptionalInt_ResolvesLiteralValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(int value = 5) { }", "value");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.EqualTo(5));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalString_ResolvesLiteralValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(string value = \"abc\") { }", "value");
        Assert.That(parameter.Default!.Value, Is.EqualTo("abc"));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalBool_ResolvesLiteralValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(bool flag = true) { }", "flag");
        Assert.That(parameter.Default!.Value, Is.EqualTo(true));
    }

    [Test]
    public void OptionalDouble_ResolvesLiteralValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(double value = 1.5) { }", "value");
        Assert.That(parameter.Default!.Value, Is.EqualTo(1.5));
    }

    [Test]
    public void OptionalSameClassConst_ResolvesToConstantValue()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(int value = RetriesConst) { }", "value");
        Assert.That(parameter.Default!.Value, Is.EqualTo(3));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalReferenceType_NullDefault_IsNotDefaultLiteral()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(string value = null) { }", "value");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.Null);
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void OptionalDateTime_BareDefaultLiteral_IsDefaultLiteral()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(DateTime value = default) { }", "value");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.Null);
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.True);
    }

    [Test]
    public void OptionalDateTime_DefaultOfTypeLiteral_IsDefaultLiteral()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(DateTime value = default(DateTime)) { }", "value");
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.True);
    }

    [Test]
    public void OptionalNullableValueType_DefaultLiteral_IsDefaultLiteral()
    {
        MethodParameterInfo parameter = GetParameter("public void M1(int? value = default) { }", "value");
        Assert.That(parameter.Default!.Value, Is.Null);
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.True);
    }
}
