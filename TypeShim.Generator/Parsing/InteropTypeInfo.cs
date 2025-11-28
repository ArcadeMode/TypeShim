using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace TypeShim.Generator.Parsing;

internal sealed class InteropTypeInfo
{
    internal required KnownManagedType ManagedType { get; init; } // TODO: keep track of inner types for generics like Task<T> and Array<T> ??
    internal required TypeSyntax InteropTypeSyntax { get; init; }
    internal required TypeSyntax CLRTypeSyntax { get; init; }
    internal required bool IsTaskType { get; init; }
    internal required bool IsArrayType { get; init; }
    internal required bool IsNullableType { get; init; } //TODO: factor out, terrible modelling. include Array<T?>, Array<T>, T? better.

    internal static InteropTypeInfo? FromTypeSymbol(ITypeSymbol typeSymbol)
    {
        JSTypeInfo parameterMarshallingTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(typeSymbol); // type info needed for jsexport to know how to marshal param type
        TypeSyntax clrTypeSyntax = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        TypeSyntax? returnTypeSyntax = parameterMarshallingTypeInfo switch
        {
            JSSimpleTypeInfo { Syntax: TypeSyntax typeSyntax } => typeSyntax,
            JSArrayTypeInfo arrayTypeInfo => SyntaxFactory.ArrayType(arrayTypeInfo.ElementTypeInfo.Syntax, [SyntaxFactory.ArrayRankSpecifier([])]),
            JSTaskTypeInfo taskTypeInfo => SyntaxFactory.GenericName("Task")
                                        .WithTypeArgumentList(
                                            SyntaxFactory.TypeArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(taskTypeInfo.ResultTypeInfo.Syntax)
                                            )
                                        ),
            JSNullableTypeInfo nullableTypeInfo => SyntaxFactory.NullableType(nullableTypeInfo.ResultTypeInfo.Syntax),
            _ => null
        };

        if (returnTypeSyntax == null)
        {
            return null;
        }


        //TODO: add support for struct nullable types (this ref only) for:
        // - T? / Nullable<T>
        // - Array<T?> / Array<Nullable<T>>

        //TODO: add support for ref+struct nullable types for:
        // - Task<T?> / Task<Nullable<T>>
        bool isNullable = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated 
            || typeSymbol is IArrayTypeSymbol { ElementType: ITypeSymbol elementTypeSymbol } && elementTypeSymbol.NullableAnnotation == NullableAnnotation.Annotated;

        return new InteropTypeInfo
        {
            ManagedType = parameterMarshallingTypeInfo.KnownType,
            InteropTypeSyntax = returnTypeSyntax,
            CLRTypeSyntax = clrTypeSyntax,
            IsTaskType = parameterMarshallingTypeInfo is JSTaskTypeInfo,
            IsArrayType = parameterMarshallingTypeInfo is JSArrayTypeInfo,
            IsNullableType = isNullable
        };
    }
}
