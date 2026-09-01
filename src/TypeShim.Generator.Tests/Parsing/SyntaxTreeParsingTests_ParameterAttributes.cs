using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.Tests.Parsing;

internal class SyntaxTreeParsingTests_ParameterAttributes
{
    private static (ClassInfo Class, IParameterSymbol Symbol) GetParameter(string methodBody, string parameterName)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText($$"""
            using System;
            using System.Runtime.CompilerServices;
            using System.Runtime.InteropServices;
            namespace N1;
            [TSExport]
            public class C1
            {
                {{methodBody}}
            }
        """);

        SymbolExtractor symbolExtractor = new([CSharpFileInfo.Create(syntaxTree)], TestFixture.TargetingPackRefDir);
        List<INamedTypeSymbol> exportedClasses = [.. symbolExtractor.ExtractAllExportedSymbols()];
        Assert.That(exportedClasses, Has.Count.EqualTo(1));
        InteropTypeInfoCache typeCache = new();
        ClassInfo classInfo = new ClassInfoBuilder(exportedClasses[0], typeCache).Build();

        IParameterSymbol symbol = exportedClasses[0].GetMembers()
            .OfType<IMethodSymbol>()
            .First(m => m.MethodKind == MethodKind.Ordinary)
            .Parameters.Single(p => p.Name == parameterName);

        return (classInfo, symbol);
    }

    private static MethodParameterInfo Parsed(ClassInfo classInfo, string parameterName)
        => classInfo.Methods.Single().Parameters.Single(p => p.Name == parameterName);

    [Test]
    public void OptionalAndDefaultParameterValue_DoesNotBindAndHasNoDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1([Optional, DefaultParameterValue(19.99)] double price) { }",
            "price");

        Assert.That(symbol.HasExplicitDefaultValue, Is.False);
        Assert.That(symbol.IsOptional, Is.False);
        Assert.That(symbol.GetAttributes().Select(a => a.AttributeClass!.Kind), Is.All.EqualTo(SymbolKind.ErrorType));
        Assert.That(Parsed(classInfo, "price").Default, Is.Null);
    }

    [Test]
    public void OptionalAttributeAlone_DoesNotBindAndHasNoDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1([Optional] double price) { }",
            "price");

        Assert.That(symbol.HasExplicitDefaultValue, Is.False);
        Assert.That(symbol.IsOptional, Is.False);
        Assert.That(symbol.GetAttributes().Select(a => a.AttributeClass!.Kind), Is.All.EqualTo(SymbolKind.ErrorType));
        Assert.That(Parsed(classInfo, "price").Default, Is.Null);
    }

    [Test]
    public void DefaultParameterValueWithoutOptional_DoesNotBindAndHasNoDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1([DefaultParameterValue(19.99)] double price) { }",
            "price");

        Assert.That(symbol.HasExplicitDefaultValue, Is.False);
        Assert.That(symbol.IsOptional, Is.False);
        Assert.That(symbol.GetAttributes().Select(a => a.AttributeClass!.Kind), Is.All.EqualTo(SymbolKind.ErrorType));
        Assert.That(Parsed(classInfo, "price").Default, Is.Null);
    }

    [Test]
    public void CallerMemberName_BindsAndKeepsEmptyStringDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1(string message, [CallerMemberName] string caller = \"\") { }",
            "caller");

        AttributeData attr = symbol.GetAttributes().Single();
        Assert.That(attr.AttributeClass!.Name, Is.EqualTo("CallerMemberNameAttribute"));
        Assert.That(attr.AttributeClass.Kind, Is.Not.EqualTo(SymbolKind.ErrorType));
        Assert.That(symbol.HasExplicitDefaultValue, Is.True);

        MethodParameterInfo parameter = Parsed(classInfo, "caller");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.EqualTo(string.Empty));
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }

    [Test]
    public void CallerLineNumber_BindsAndKeepsZeroDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1(string message, [CallerLineNumber] int line = 0) { }",
            "line");

        AttributeData attr = symbol.GetAttributes().Single();
        Assert.That(attr.AttributeClass!.Name, Is.EqualTo("CallerLineNumberAttribute"));
        Assert.That(attr.AttributeClass.Kind, Is.Not.EqualTo(SymbolKind.ErrorType));
        Assert.That(symbol.HasExplicitDefaultValue, Is.True);

        MethodParameterInfo parameter = Parsed(classInfo, "line");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.EqualTo(0));
    }

    [Test]
    public void CallerArgumentExpression_BindsAndKeepsNullDefault()
    {
        (ClassInfo classInfo, IParameterSymbol symbol) = GetParameter(
            "public void M1(string value, [CallerArgumentExpression(nameof(value))] string? expr = null) { }",
            "expr");

        AttributeData attr = symbol.GetAttributes().Single();
        Assert.That(attr.AttributeClass!.Name, Is.EqualTo("CallerArgumentExpressionAttribute"));
        Assert.That(attr.AttributeClass.Kind, Is.Not.EqualTo(SymbolKind.ErrorType));
        Assert.That(symbol.HasExplicitDefaultValue, Is.True);

        MethodParameterInfo parameter = Parsed(classInfo, "expr");
        Assert.That(parameter.Default, Is.Not.Null);
        Assert.That(parameter.Default!.Value, Is.Null);
        Assert.That(parameter.Default!.IsDefaultLiteral, Is.False);
    }
}
