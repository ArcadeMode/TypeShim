using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Runtime.InteropServices.JavaScript;
using TypeShim.Generator;
using TypeShim.Generator.Parsing;

internal sealed class InteropTypeInfoBuilder(ITypeSymbol typeSymbol)
{
    internal InteropTypeInfo Build()
    {
        JSTypeInfo parameterMarshallingTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(typeSymbol);
        TypeSyntax clrTypeSyntax = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        return parameterMarshallingTypeInfo switch
        {
            JSSimpleTypeInfo simpleType => BuildSimpleTypeInfo(simpleType, clrTypeSyntax),
            JSArrayTypeInfo arrayTypeInfo => BuildArrayTypeInfo(arrayTypeInfo, clrTypeSyntax),
            JSTaskTypeInfo taskTypeInfo => BuildTaskTypeInfo(taskTypeInfo, clrTypeSyntax),
            JSNullableTypeInfo nullableTypeInfo => BuildNullableTypeInfo(nullableTypeInfo, clrTypeSyntax),
            JSSpanTypeInfo => throw new NotImplementedException("Span<T> is not yet supported"),
            JSArraySegmentTypeInfo => throw new NotImplementedException("ArraySegment<T> is not yet supported"),
            JSInvalidTypeInfo or _ => throw new TypeNotSupportedException(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
        };
    }

    private InteropTypeInfo BuildSimpleTypeInfo(JSSimpleTypeInfo simpleTypeInfo, TypeSyntax clrTypeSyntax)
    {
        // TODO: validate handling of generics
        return new InteropTypeInfo
        {
            ManagedType = simpleTypeInfo.KnownType,
            JSTypeSyntax = SyntaxFactory.ParseTypeName(GetSimpleJSMarshalAsTypeArgument(simpleTypeInfo.KnownType)),
            InteropTypeSyntax = simpleTypeInfo.Syntax,
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = null,
            RequiresCLRTypeConversion = simpleTypeInfo.KnownType == KnownManagedType.Object,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = clrTypeSyntax is NullableTypeSyntax
        };
    }

    private InteropTypeInfo BuildArrayTypeInfo(JSArrayTypeInfo arrayTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? elementTypeSymbol = GetTypeArgument(typeSymbol) ?? throw new TypeNotSupportedException("Only arrays with one element type are supported");
        InteropTypeInfo elementTypeInfo = new InteropTypeInfoBuilder(elementTypeSymbol).Build();

        return new InteropTypeInfo
        {
            ManagedType = arrayTypeInfo.KnownType,
            JSTypeSyntax = SyntaxFactory.ParseTypeName(GetArrayJSMarshalAsTypeArgument(arrayTypeInfo.ElementTypeInfo)),
            InteropTypeSyntax = SyntaxFactory.ArrayType(arrayTypeInfo.ElementTypeInfo.Syntax, [SyntaxFactory.ArrayRankSpecifier([])]),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = elementTypeInfo,
            RequiresCLRTypeConversion = elementTypeInfo.RequiresCLRTypeConversion,
            IsTaskType = false,
            IsArrayType = true,
            IsNullableType = false
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            return typeSymbol switch
            {
                IArrayTypeSymbol { IsSZArray: true, ElementType: ITypeSymbol elementType } => elementType,
                _ => null
            };
        }
    }

    private InteropTypeInfo BuildTaskTypeInfo(JSTaskTypeInfo taskTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? elementTypeSymbol = GetTypeArgument(typeSymbol);
        InteropTypeInfo? taskReturnTypeInfo = elementTypeSymbol != null ? new InteropTypeInfoBuilder(elementTypeSymbol).Build() : null;

        TypeSyntax interopTypeSyntax = SyntaxFactory.GenericName(nameof(Task))
            .WithTypeArgumentList(
                SyntaxFactory.TypeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(taskTypeInfo.ResultTypeInfo.Syntax)
                )
            );

        return new InteropTypeInfo
        {
            ManagedType = taskTypeInfo.KnownType,
            JSTypeSyntax = SyntaxFactory.ParseTypeName(GetPromiseJSMarshalAsTypeArgument(taskTypeInfo.ResultTypeInfo)),
            InteropTypeSyntax = interopTypeSyntax,
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = taskReturnTypeInfo,
            RequiresCLRTypeConversion = taskReturnTypeInfo?.RequiresCLRTypeConversion ?? false,
            IsTaskType = true,
            IsArrayType = false,
            IsNullableType = false
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return typeSymbol switch
            {
                ITypeSymbol when fullTypeName == Constants.TaskGlobal => null,
                INamedTypeSymbol { TypeArguments.Length: 1 } taskType when fullTypeName.StartsWith(Constants.TaskGlobal, StringComparison.Ordinal) => taskType.TypeArguments[0],
                _ => throw new TypeNotSupportedException("Tasks with more than one type arguments are not supported")
            };
        }
    }

    private InteropTypeInfo BuildNullableTypeInfo(JSNullableTypeInfo nullableTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? innertypeSymbol = GetTypeArgument(typeSymbol) ?? throw new TypeNotSupportedException("Only nullables with one element type are supported");
        InteropTypeInfo innerTypeInfo = new InteropTypeInfoBuilder(innertypeSymbol).Build();

        return new InteropTypeInfo
        {
            ManagedType = nullableTypeInfo.KnownType,
            JSTypeSyntax = SyntaxFactory.ParseTypeName(GetNullableJSMarshalAsTypeArgument(nullableTypeInfo.ResultTypeInfo)),
            InteropTypeSyntax = SyntaxFactory.NullableType(nullableTypeInfo.ResultTypeInfo.Syntax),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = innerTypeInfo,
            RequiresCLRTypeConversion = innerTypeInfo.RequiresCLRTypeConversion,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = true
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            return typeSymbol is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments.Length: 1 } named
                ? named.TypeArguments[0]
                : null;
        }
    }

    private static string GetSimpleJSMarshalAsTypeArgument(KnownManagedType knownManagedType)
    {
        // Only certain simple types are supported, this method maps them
        return knownManagedType switch
        {
            KnownManagedType.Boolean => "JSType.Boolean",
            KnownManagedType.Byte => "JSType.Number",
            KnownManagedType.Char => "JSType.Number",
            KnownManagedType.Int16 => "JSType.Number",
            KnownManagedType.Int32 => "JSType.Number",
            KnownManagedType.Int64 => "JSType.Number",
            KnownManagedType.IntPtr => "JSType.Number",
            KnownManagedType.Single => "JSType.Number", //i.e. float
            KnownManagedType.Double => "JSType.Number",
            KnownManagedType.String => "JSType.String",
            KnownManagedType.Object => "JSType.Any",
            KnownManagedType.Void => "JSType.Void",
            KnownManagedType.JSObject => "JSType.Object", // TODO: warning, not fully supported
            KnownManagedType.DateTime => "JSType.Date",
            KnownManagedType.DateTimeOffset => "JSType.Date",
            _ => throw new TypeNotSupportedException($"Unsupported simple type {knownManagedType}")
        };
    }

    private static string GetPromiseJSMarshalAsTypeArgument(JSSimpleTypeInfo innerTypeInfo)
    {
        // Only certain types are supported in Task<T>, this method maps them
        string innerJsType = innerTypeInfo.KnownType switch
        {
            KnownManagedType.Boolean => "JSType.Boolean",
            KnownManagedType.Byte => "JSType.Number",
            KnownManagedType.Char => "JSType.Number",
            KnownManagedType.Int16 => "JSType.Number",
            KnownManagedType.Int32 => "JSType.Number",
            KnownManagedType.Int64 => "JSType.Number",
            KnownManagedType.Single => "JSType.Number", //i.e. float
            KnownManagedType.Double => "JSType.Number",
            KnownManagedType.IntPtr => "JSType.Number",
            KnownManagedType.DateTime => "JSType.Date",
            KnownManagedType.DateTimeOffset => "JSType.Date",
            KnownManagedType.Exception => "JSType.Error",
            KnownManagedType.JSObject => "JSType.Object", // TODO: warning, not fully supported
            KnownManagedType.String => "JSType.String",
            KnownManagedType.Object => "JSType.Any",
            _ => throw new TypeNotSupportedException($"Unsupported Task<T> type argument {innerTypeInfo.KnownType} ({innerTypeInfo.Syntax})")
        };
        return $"JSType.Promise<{innerJsType}>";

    }

    private static string GetArrayJSMarshalAsTypeArgument(JSSimpleTypeInfo innerTypeInfo)
    {
        // Only certain types are supported in arrays, this method maps them
        string innerJsType = innerTypeInfo.KnownType switch
        {
            KnownManagedType.Byte => "JSType.Number",
            KnownManagedType.Int32 => "JSType.Number",
            KnownManagedType.Double => "JSType.Number",
            KnownManagedType.JSObject => "JSType.Object",
            KnownManagedType.String => "JSType.String",
            KnownManagedType.Object => "JSType.Any",

            _ => throw new TypeNotSupportedException($"Unsupported Array<T> type argument {innerTypeInfo.KnownType} ({innerTypeInfo.Syntax})")
        };

        return $"JSType.Array<{innerJsType}>";
    }

    private static string GetNullableJSMarshalAsTypeArgument(JSSimpleTypeInfo innerTypeInfo)
    {
        // Only certain types are supported as nullables, this method maps them
        return innerTypeInfo.KnownType switch
        {
            KnownManagedType.Boolean => "JSType.Boolean",
            KnownManagedType.Byte => "JSType.Number",
            KnownManagedType.Char => "JSType.Number",
            KnownManagedType.Int16 => "JSType.Number",
            KnownManagedType.Int32 => "JSType.Number",
            KnownManagedType.Int64 => "JSType.Number", // TODO: BigInt marshalling is supported, add option?
            KnownManagedType.Single => "JSType.Number", //i.e. float
            KnownManagedType.Double => "JSType.Number",
            KnownManagedType.IntPtr => "JSType.Number",
            KnownManagedType.DateTime => "JSType.Date",
            KnownManagedType.DateTimeOffset => "JSType.Date",

            _ => throw new TypeNotSupportedException($"Unsupported Nullable<T> type argument {innerTypeInfo.KnownType} ({innerTypeInfo.Syntax})")
        };
    }
}