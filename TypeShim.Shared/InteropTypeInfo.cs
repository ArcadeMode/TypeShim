using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Shared;

internal sealed class InteropTypeInfo
{
    public required KnownManagedType ManagedType { get; init; }

    /// <summary>
    /// Represents the syntax for writing the associated <see cref="JSType"/>
    /// </summary>
    public required TypeSyntax JSTypeSyntax { get; init; }

    /// <summary>
    /// Syntax for writing the CLR type in C# (i.e. the user's original type)
    /// </summary>
    public required TypeSyntax CSharpTypeSyntax { get; init; }
    
    /// <summary>
    /// Syntax for writing the CLR type on the interop method. This is usually equal to <see cref="CSharpTypeSyntax"/>,<br/>
    /// but e.g. YourClass, Task&lt;YourClass&gt; or YourClass[] have to be object, Task&lt;object&gt; or object[] for the interop method.
    /// </summary>
    public required TypeSyntax CSharpInteropTypeSyntax { get; init; }
    
    public required TypeScriptSymbolNameTemplate TypeScriptTypeSyntax { get; init; }
    public required TypeScriptSymbolNameTemplate TypeScriptInteropTypeSyntax { get; init; }

    /// <summary>
    /// Tasks and Arrays _may_ have type arguments
    /// </summary>
    public required InteropTypeInfo? TypeArgument { get; init; }

    public required bool IsTaskType { get; init; }
    public required bool IsArrayType { get; init; }
    public required bool IsNullableType { get; init; }

    public required bool IsTSExport { get; init; }
    public required bool RequiresTypeConversion { get; init; }
    public required bool SupportsTypeConversion { get; init; }

    public InteropTypeInfo GetInnermostType()
    {
        if (TypeArgument != null)
        {
            return TypeArgument.GetInnermostType();
        }
        else
        {
            return this;
        }
    }

    /// <summary>
    /// Transforms this <see cref="InteropTypeInfo"/> into one suitable for interop method signatures.
    /// </summary>
    /// <returns></returns>
    public InteropTypeInfo AsInteropTypeInfo()
    {
        return new InteropTypeInfo
        {
            IsTSExport = false,
            ManagedType = this.ManagedType,
            JSTypeSyntax = this.JSTypeSyntax,
            CSharpTypeSyntax = this.CSharpInteropTypeSyntax, // essentially overwrite clr type with interop type
            CSharpInteropTypeSyntax = this.CSharpInteropTypeSyntax,
            TypeScriptTypeSyntax = this.TypeScriptInteropTypeSyntax,
            TypeScriptInteropTypeSyntax = this.TypeScriptInteropTypeSyntax,
            IsTaskType = this.IsTaskType,
            IsArrayType = this.IsArrayType,
            IsNullableType = this.IsNullableType,
            TypeArgument = TypeArgument?.AsInteropTypeInfo(),
            RequiresTypeConversion = false,
            SupportsTypeConversion = false,
        };
    }

    public static readonly InteropTypeInfo JSObjectTypeInfo = new()
    {
        IsTSExport = false,
        ManagedType = KnownManagedType.JSObject,
        TypeScriptTypeSyntax = TypeScriptSymbolNameTemplate.ForSimpleType("object"),
        TypeScriptInteropTypeSyntax = TypeScriptSymbolNameTemplate.ForSimpleType("object"),
        JSTypeSyntax = SyntaxFactory.ParseTypeName("JSType.Object"),
        CSharpInteropTypeSyntax = SyntaxFactory.ParseTypeName("JSObject"),
        CSharpTypeSyntax = SyntaxFactory.ParseTypeName("JSObject"),
        IsTaskType = false,
        IsArrayType = false,
        IsNullableType = false,
        RequiresTypeConversion = false,
        TypeArgument = null,
        SupportsTypeConversion = false,
    };
}
