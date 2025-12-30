using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Shared;

public sealed class InteropTypeInfo
{
    public required KnownManagedType ManagedType { get; init; }

    /// <summary>
    /// Represents the syntax for writing the associated <see cref="JSType"/>
    /// </summary>
    public required TypeSyntax JSTypeSyntax { get; init; }

    /// <summary>
    /// Syntax for writing the CLR type on the interop method. This is usually equal to <see cref="CLRTypeSyntax"/>,<br/>
    /// but e.g. YourClass, Task&lt;YourClass&gt; or YourClass[] have to be object, Task&lt;object&gt; or object[] for the interop method.
    /// </summary>
    public required TypeSyntax InteropTypeSyntax { get; init; }

    /// <summary>
    /// Syntax for writing the CLR type in C# (i.e. the user's original type)
    /// </summary>
    public required TypeSyntax CLRTypeSyntax { get; init; }

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

    [Obsolete("Use Requires/SupportsTypeConversion")]
    public bool ContainsExportedType()
    {
        return this.IsTSExport || (TypeArgument?.ContainsExportedType() ?? false);
    }

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
        if (TypeArgument == null && ManagedType is KnownManagedType.Object or KnownManagedType.JSObject)
        {
            return new InteropTypeInfo
            {
                IsTSExport = false,
                ManagedType = this.ManagedType,
                JSTypeSyntax = this.JSTypeSyntax,
                InteropTypeSyntax = this.InteropTypeSyntax,
                CLRTypeSyntax = this.InteropTypeSyntax,
                IsTaskType = false,
                IsArrayType = false,
                IsNullableType = this.IsNullableType,
                RequiresTypeConversion = false,
                TypeArgument = null,
                SupportsTypeConversion = this.SupportsTypeConversion,
            };

        }
        else if (TypeArgument != null)
        {
            return new InteropTypeInfo
            {
                IsTSExport = false,
                ManagedType = this.ManagedType,
                JSTypeSyntax = this.JSTypeSyntax,
                InteropTypeSyntax = this.InteropTypeSyntax,
                CLRTypeSyntax = this.InteropTypeSyntax,
                IsTaskType = this.IsTaskType,
                IsArrayType = this.IsArrayType,
                IsNullableType = this.IsNullableType,
                RequiresTypeConversion = false,
                TypeArgument = TypeArgument.AsInteropTypeInfo(),
                SupportsTypeConversion = this.SupportsTypeConversion,
            };
        }
        else
        {
            return this;
        }
    }

    public static readonly InteropTypeInfo JSObjectTypeInfo = new()
    {
        IsTSExport = false,
        ManagedType = KnownManagedType.JSObject,
        JSTypeSyntax = SyntaxFactory.ParseTypeName("JSType.Object"),
        InteropTypeSyntax = SyntaxFactory.ParseTypeName("JSObject"),
        CLRTypeSyntax = SyntaxFactory.ParseTypeName("JSObject"),
        IsTaskType = false,
        IsArrayType = false,
        IsNullableType = false,
        RequiresTypeConversion = false,
        TypeArgument = null,
        SupportsTypeConversion = false,
    };

    private static readonly InteropTypeInfo CLRObjectTypeInfo = new()
    {
        IsTSExport = false,
        ManagedType = KnownManagedType.Object,
        JSTypeSyntax = SyntaxFactory.ParseTypeName("JSType.Any"),
        InteropTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
        CLRTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
        IsTaskType = false,
        IsArrayType = false,
        IsNullableType = false,
        RequiresTypeConversion = false,
        TypeArgument = null,
        SupportsTypeConversion = false,
    };
}
