using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TypeShim.Shared;

public sealed class InteropTypeInfo : IEquatable<InteropTypeInfo>
{
    public required bool IsTSExport { get; init; }

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

    public required bool RequiresCLRTypeConversion { get; init; }

    public required bool IsTaskType { get; init; }
    public required bool IsArrayType { get; init; }
    public required bool IsNullableType { get; init; }
    public required bool IsSnapshotCompatible { get; init; }

    public bool ContainsExportedType()
    {
        return this.IsTSExport || (TypeArgument?.ContainsExportedType() ?? false);
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
                RequiresCLRTypeConversion = false,
                TypeArgument = TypeArgument.AsInteropTypeInfo(),
                IsSnapshotCompatible = this.IsSnapshotCompatible,
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
        RequiresCLRTypeConversion = false,
        TypeArgument = null,
        IsSnapshotCompatible = false,
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
        RequiresCLRTypeConversion = false,
        TypeArgument = null,
        IsSnapshotCompatible = false,
    };

    public override bool Equals(object? obj)
    {
        return obj is InteropTypeInfo other && Equals(other);
    }

    public bool Equals(InteropTypeInfo? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return IsTSExport == other.IsTSExport
            && ManagedType == other.ManagedType
            && string.Equals(JSTypeSyntax.ToString(), other.JSTypeSyntax.ToString(), StringComparison.Ordinal)
            && string.Equals(InteropTypeSyntax.ToString(), other.InteropTypeSyntax.ToString(), StringComparison.Ordinal)
            && string.Equals(CLRTypeSyntax.ToString(), other.CLRTypeSyntax.ToString(), StringComparison.Ordinal)
            && Equals(TypeArgument, other.TypeArgument)
            && RequiresCLRTypeConversion == other.RequiresCLRTypeConversion
            && IsTaskType == other.IsTaskType
            && IsArrayType == other.IsArrayType
            && IsNullableType == other.IsNullableType
            && IsSnapshotCompatible == other.IsSnapshotCompatible;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = (hash * 31) + IsTSExport.GetHashCode();
            hash = (hash * 31) + ManagedType.GetHashCode();
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(JSTypeSyntax.ToString());
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(InteropTypeSyntax.ToString());
            hash = (hash * 31) + StringComparer.Ordinal.GetHashCode(CLRTypeSyntax.ToString());
            hash = (hash * 31) + (TypeArgument?.GetHashCode() ?? 0);
            hash = (hash * 31) + RequiresCLRTypeConversion.GetHashCode();
            hash = (hash * 31) + IsTaskType.GetHashCode();
            hash = (hash * 31) + IsArrayType.GetHashCode();
            hash = (hash * 31) + IsNullableType.GetHashCode();
            hash = (hash * 31) + IsSnapshotCompatible.GetHashCode();
            return hash;
        }
    }

    public static bool operator ==(InteropTypeInfo? left, InteropTypeInfo? right)
    {
        return EqualityComparer<InteropTypeInfo?>.Default.Equals(left, right);
    }

    public static bool operator !=(InteropTypeInfo? left, InteropTypeInfo? right)
    {
        return !(left == right);
    }
}
