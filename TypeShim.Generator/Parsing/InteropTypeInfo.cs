using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Runtime.InteropServices.JavaScript;

namespace TypeShim.Generator.Parsing;

internal sealed class InteropTypeInfo
{
    internal required KnownManagedType ManagedType { get; init; }

    /// <summary>
    /// Represents the syntax for writing the associated <see cref="JSType"/>
    /// </summary>
    internal required TypeSyntax JSTypeSyntax { get; init; }

    /// <summary>
    /// Syntax for writing the CLR type on the interop method. This is usually equal to <see cref="CLRTypeSyntax"/>,<br/>
    /// but e.g. YourClass, Task&lt;YourClass&gt; or YourClass[] have to be object, Task&lt;object&gt; or object[] for the interop method.
    /// </summary>
    internal required TypeSyntax InteropTypeSyntax { get; init; }

    /// <summary>
    /// Syntax for writing the CLR type in C# (i.e. the user's original type)
    /// </summary>
    internal required TypeSyntax CLRTypeSyntax { get; init; }

    /// <summary>
    /// Tasks and Arrays _may_ have type arguments
    /// </summary>
    internal required InteropTypeInfo? TypeArgument { get; init; }

    internal required bool RequiresCLRTypeConversion { get; init; }

    internal required bool IsTaskType { get; init; }
    internal required bool IsArrayType { get; init; }
    internal required bool IsNullableType { get; init; }
    internal required bool IsSnapshotCompatible { get; init; }

    /// <summary>
    /// Transforms this <see cref="InteropTypeInfo"/> into one suitable for interop method signatures.
    /// </summary>
    /// <returns></returns>
    internal InteropTypeInfo AsInteropTypeInfo()
    {
        if (TypeArgument == null && ManagedType == KnownManagedType.Object)
        {
            return new InteropTypeInfo
            {
                ManagedType = this.ManagedType,
                JSTypeSyntax = CLRObjectTypeInfo.JSTypeSyntax,
                InteropTypeSyntax = CLRObjectTypeInfo.InteropTypeSyntax,
                CLRTypeSyntax = CLRObjectTypeInfo.CLRTypeSyntax,
                IsTaskType = false,
                IsArrayType = false,
                IsNullableType = this.IsNullableType,
                RequiresCLRTypeConversion = false,
                TypeArgument = null,
                IsSnapshotCompatible = this.IsSnapshotCompatible,
            };

        }
        else if (TypeArgument?.ManagedType == KnownManagedType.Object)
        {
            return new InteropTypeInfo
            {
                ManagedType = this.ManagedType,
                JSTypeSyntax = this.JSTypeSyntax,
                InteropTypeSyntax = this.InteropTypeSyntax,
                CLRTypeSyntax = this.CLRTypeSyntax,
                IsTaskType = this.IsTaskType,
                IsArrayType = this.IsArrayType,
                IsNullableType = this.IsNullableType,
                RequiresCLRTypeConversion = false,
                TypeArgument = CLRObjectTypeInfo,
                IsSnapshotCompatible = this.IsSnapshotCompatible,
            };
        }
        else
        {
            return this;
        }
    }

    internal InteropTypeInfo AsJSObjectTypeInfo()
    {
        if (this.TypeArgument == null)
        {
            return new()
            {
                ManagedType = KnownManagedType.JSObject,
                JSTypeSyntax = SyntaxFactory.ParseTypeName("JSType.Object"),
                InteropTypeSyntax = SyntaxFactory.ParseTypeName("JSObject"),
                CLRTypeSyntax = this.CLRTypeSyntax,
                IsTaskType = this.IsTaskType,
                IsArrayType = this.IsArrayType,
                IsNullableType = this.IsNullableType,
                RequiresCLRTypeConversion = this.RequiresCLRTypeConversion,
                TypeArgument = null,
                IsSnapshotCompatible = this.IsSnapshotCompatible,
            };
        }
        else
        {
            return new()
            {
                ManagedType = this.ManagedType,
                JSTypeSyntax = this.IsTaskType
                    ? SyntaxFactory.ParseTypeName("JSType.Promise<JSType.Object>")
                    : this.IsArrayType
                    ? SyntaxFactory.ParseTypeName("JSType.Array<JSType.Object>")
                    : SyntaxFactory.ParseTypeName("JSObject"),
                InteropTypeSyntax = this.IsTaskType 
                    ? SyntaxFactory.ParseTypeName("Task<JSObject>") 
                    : this.IsArrayType
                    ? SyntaxFactory.ParseTypeName("JSObject[]")
                    : SyntaxFactory.ParseTypeName("JSObject"),
                CLRTypeSyntax = this.CLRTypeSyntax,
                IsTaskType = this.IsTaskType,
                IsArrayType = this.IsArrayType,
                IsNullableType = this.IsNullableType,
                RequiresCLRTypeConversion = this.RequiresCLRTypeConversion,
                TypeArgument = this.TypeArgument.AsJSObjectTypeInfo(),
                IsSnapshotCompatible = this.IsSnapshotCompatible,
            };
        }
    }

    private static readonly InteropTypeInfo CLRObjectTypeInfo = new()
    {
        ManagedType = KnownManagedType.Object,
        JSTypeSyntax = SyntaxFactory.ParseTypeName("JSType.Any"),
        InteropTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
        CLRTypeSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ObjectKeyword)),
        IsTaskType = false,
        IsArrayType = false,
        IsNullableType = false,
        RequiresCLRTypeConversion = false,
        TypeArgument = null,
        IsSnapshotCompatible = true,
    };
}
