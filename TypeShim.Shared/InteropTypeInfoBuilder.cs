using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypeShim.Shared;

public sealed class InteropTypeInfoBuilder(ITypeSymbol typeSymbol, InteropTypeInfoCache cache)
{
    private readonly bool IsTSExport = typeSymbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TSExportAttribute" or "TSExport");

    public InteropTypeInfo Build()
    {
        return cache.GetOrAdd(typeSymbol, BuildInternal);
    }

    private InteropTypeInfo BuildInternal()
    {
        JSTypeInfo jsTypeInfo = JSTypeInfo.CreateJSTypeInfoForTypeSymbol(typeSymbol);
        TypeSyntax clrTypeSyntax = SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

        return jsTypeInfo switch
        {
            JSSimpleTypeInfo simpleType => BuildSimpleTypeInfo(simpleType, clrTypeSyntax),
            JSArrayTypeInfo arrayTypeInfo => BuildArrayTypeInfo(arrayTypeInfo, clrTypeSyntax),
            JSTaskTypeInfo taskTypeInfo => BuildTaskTypeInfo(taskTypeInfo, clrTypeSyntax),
            JSNullableTypeInfo nullableTypeInfo => BuildNullableTypeInfo(nullableTypeInfo, clrTypeSyntax),
            JSSpanTypeInfo => throw new NotImplementedException("Span<T> is not yet supported"),
            JSArraySegmentTypeInfo => throw new NotImplementedException("ArraySegment<T> is not yet supported"),
            JSFunctionTypeInfo => throw new NotImplementedException("Func & Action are not yet supported"),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
        };
    }

    private InteropTypeInfo BuildSimpleTypeInfo(JSSimpleTypeInfo simpleTypeInfo, TypeSyntax clrTypeSyntax)
    {
        return new InteropTypeInfo
        {
            IsTSExport = IsTSExport,
            ManagedType = simpleTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(simpleTypeInfo, clrTypeSyntax),
            InteropTypeSyntax = GetInteropTypeSyntax(simpleTypeInfo),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = null,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = clrTypeSyntax is NullableTypeSyntax,
            RequiresTypeConversion = RequiresTypeConversion(),
            SupportsTypeConversion = SupportsTypeConversion(),
        };

        bool RequiresTypeConversion()
        {
            // If the type inherits from 'object' it requires conversion to its original type after crossing the interop boundary
            if (simpleTypeInfo.KnownType != KnownManagedType.Object)
            {
                return false;
            }
            TypeSyntax unwrapped = clrTypeSyntax is NullableTypeSyntax n ? n.ElementType : clrTypeSyntax;
            // only 'object' itself requires no conversion, anything else does
            return unwrapped is not PredefinedTypeSyntax p || !p.Keyword.IsKind(SyntaxKind.ObjectKeyword);
        }

        bool SupportsTypeConversion()
        {
            if (simpleTypeInfo.KnownType != KnownManagedType.Object)
            {
                return true; // note that this is part of the simple type parsing, only 'object' needs further inspection
            }

            IPropertySymbol[] properties = [.. typeSymbol.GetMembers().OfType<IPropertySymbol>()];
            return IsTSExport && (properties.Length == 0 || !properties.Any(p => !new InteropTypeInfoBuilder(p.Type, cache).Build().SupportsTypeConversion));
        }
    }

    private InteropTypeInfo BuildArrayTypeInfo(JSArrayTypeInfo arrayTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? elementTypeSymbol = GetTypeArgument(typeSymbol) ?? throw new NotSupportedTypeException("Only arrays with one element type are supported");
        InteropTypeInfo elementTypeInfo = new InteropTypeInfoBuilder(elementTypeSymbol, cache).Build();

        return new InteropTypeInfo
        {
            IsTSExport = IsTSExport,
            ManagedType = arrayTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(arrayTypeInfo, clrTypeSyntax),
            InteropTypeSyntax = GetInteropTypeSyntax(arrayTypeInfo),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = elementTypeInfo,
            RequiresTypeConversion = elementTypeInfo.RequiresTypeConversion,
            IsTaskType = false,
            IsArrayType = true,
            IsNullableType = false,
            SupportsTypeConversion = elementTypeInfo.SupportsTypeConversion
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
        InteropTypeInfo? taskReturnTypeInfo = elementTypeSymbol != null ? new InteropTypeInfoBuilder(elementTypeSymbol, cache).Build() : null;

        return new InteropTypeInfo
        {
            IsTSExport = IsTSExport,
            ManagedType = taskTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(taskTypeInfo, clrTypeSyntax),
            InteropTypeSyntax = GetInteropTypeSyntax(taskTypeInfo),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = taskReturnTypeInfo,
            RequiresTypeConversion = taskReturnTypeInfo?.RequiresTypeConversion ?? false,
            IsTaskType = true,
            IsArrayType = false,
            IsNullableType = false,
            SupportsTypeConversion = taskReturnTypeInfo?.SupportsTypeConversion ?? false
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            return typeSymbol switch
            {
                ITypeSymbol when fullTypeName == Constants.TaskGlobal => null,
                INamedTypeSymbol { TypeArguments.Length: 1 } taskType when fullTypeName.StartsWith(Constants.TaskGlobal, StringComparison.Ordinal) => taskType.TypeArguments[0],
                _ => throw new NotSupportedTypeException("Tasks with more than one type arguments are not supported")
            };
        }
    }

    private InteropTypeInfo BuildNullableTypeInfo(JSNullableTypeInfo nullableTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? innertypeSymbol = GetTypeArgument(typeSymbol) ?? throw new NotSupportedTypeException("Only nullables with one element type are supported");
        InteropTypeInfo innerTypeInfo = new InteropTypeInfoBuilder(innertypeSymbol, cache).Build();

        return new InteropTypeInfo
        {
            IsTSExport = IsTSExport,
            ManagedType = nullableTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(nullableTypeInfo, clrTypeSyntax),
            InteropTypeSyntax = GetInteropTypeSyntax(nullableTypeInfo),
            CLRTypeSyntax = clrTypeSyntax,
            TypeArgument = innerTypeInfo,
            RequiresTypeConversion = innerTypeInfo.RequiresTypeConversion,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = true,
            SupportsTypeConversion = innerTypeInfo.SupportsTypeConversion
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T, TypeArguments.Length: 1 } named)
            {
                return named.TypeArguments[0];
            }
            else if (typeSymbol.NullableAnnotation == NullableAnnotation.Annotated)
            {
                return typeSymbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            } 
            else
            {
                return null;
            }
        }
    }

    private static TypeSyntax GetInteropTypeSyntax(JSTypeInfo jsTypeInfo)
    {
        return jsTypeInfo switch
        {
            JSSimpleTypeInfo simpleTypeInfo => simpleTypeInfo.Syntax,
            JSArrayTypeInfo arrayTypeInfo => arrayTypeInfo.GetTypeSyntax(),
            JSTaskTypeInfo taskTypeInfo => taskTypeInfo.GetTypeSyntax(),
            JSNullableTypeInfo nullableTypeInfo => SyntaxFactory.NullableType(GetInteropTypeSyntax(nullableTypeInfo.ResultTypeInfo)),
            _ => throw new NotSupportedTypeException("Unsupported JSTypeInfo for interop type syntax generation"),
        } ?? throw new ArgumentException($"Invalid JSTypeInfo of known type '{jsTypeInfo.KnownType}' yielded no syntax");
    }

    private static TypeSyntax GetJSTypeSyntax(JSTypeInfo jSTypeInfo, TypeSyntax clrTypeSyntax)
    {
        return SyntaxFactory.ParseTypeName(jSTypeInfo switch
        {
            JSSimpleTypeInfo simpleType => GetSimpleJSMarshalAsTypeArgument(simpleType.KnownType),
            JSArrayTypeInfo arrayTypeInfo => GetArrayJSMarshalAsTypeArgument(arrayTypeInfo.ElementTypeInfo, clrTypeSyntax),
            JSTaskTypeInfo taskTypeInfo => GetPromiseJSMarshalAsTypeArgument(taskTypeInfo.ResultTypeInfo, clrTypeSyntax),
            JSNullableTypeInfo { IsValueType: true } nullableTypeInfo => GetNullableJSMarshalAsTypeArgument(nullableTypeInfo.ResultTypeInfo.KnownType, clrTypeSyntax),
            JSNullableTypeInfo { IsValueType: false } nullableTypeInfo => GetJSTypeSyntax(nullableTypeInfo.ResultTypeInfo, clrTypeSyntax).ToString(), // todo: needed?
            JSSpanTypeInfo => throw new NotImplementedException("Span<T> is not yet supported"),
            JSArraySegmentTypeInfo => throw new NotImplementedException("ArraySegment<T> is not yet supported"),
            JSFunctionTypeInfo => throw new NotImplementedException("Func & Action are not yet supported"),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(clrTypeSyntax.ToFullString()),
        });

        static string GetSimpleJSMarshalAsTypeArgument(KnownManagedType knownManagedType)
        {
            // Only certain simple types are supported, this method maps them
            return knownManagedType switch
            {
                KnownManagedType.Boolean => "JSType.Boolean",
                KnownManagedType.Byte
                or KnownManagedType.Int16
                or KnownManagedType.Int32
                or KnownManagedType.Int64
                or KnownManagedType.IntPtr
                or KnownManagedType.Single //i.e. float
                or KnownManagedType.Double => "JSType.Number",
                KnownManagedType.Char
                or KnownManagedType.String => "JSType.String",
                KnownManagedType.Object => "JSType.Any",
                KnownManagedType.Void => "JSType.Void",
                KnownManagedType.JSObject => "JSType.Object",
                KnownManagedType.DateTime => "JSType.Date",
                KnownManagedType.DateTimeOffset => "JSType.Date",
                _ => throw new NotSupportedTypeException($"Unsupported simple type {knownManagedType}")
            };
        }

        static string GetPromiseJSMarshalAsTypeArgument(JSTypeInfo typeInfo, TypeSyntax syntax)
        {
            if (typeInfo is JSNullableTypeInfo { IsValueType: false, ResultTypeInfo: JSTypeInfo resultTypeInfo })
            {
                return GetPromiseJSMarshalAsTypeArgument(resultTypeInfo, syntax);
            }

            // Only certain types are supported in Task<T>, this method maps them
            string innerJsType = typeInfo.KnownType switch
            {
                KnownManagedType.Boolean => "JSType.Boolean",
                KnownManagedType.Byte
                or KnownManagedType.Int16
                or KnownManagedType.Int32
                or KnownManagedType.Int64
                or KnownManagedType.IntPtr
                or KnownManagedType.Single
                or KnownManagedType.Double => "JSType.Number",
                KnownManagedType.DateTime
                or KnownManagedType.DateTimeOffset => "JSType.Date",
                KnownManagedType.Exception => "JSType.Error",
                KnownManagedType.JSObject => "JSType.Object",
                KnownManagedType.Char
                or KnownManagedType.String => "JSType.String",
                KnownManagedType.Object => "JSType.Any",
                KnownManagedType.Void => "JSType.Void",
                _ => throw new NotSupportedTypeException($"Unsupported Task<T> type argument {typeInfo.KnownType} ({syntax})")
            };
            return $"JSType.Promise<{innerJsType}>";

        }

        static string GetArrayJSMarshalAsTypeArgument(JSTypeInfo typeInfo, TypeSyntax syntax)
        {
            if (typeInfo is JSNullableTypeInfo { IsValueType: false, ResultTypeInfo: JSTypeInfo resultTypeInfo })
            {
                return GetArrayJSMarshalAsTypeArgument(resultTypeInfo, syntax);
            }

            // Only certain types are supported in arrays, this method maps them
            string innerJsType = typeInfo.KnownType switch
            {
                KnownManagedType.Byte
                or KnownManagedType.Int32
                or KnownManagedType.Double => "JSType.Number",
                KnownManagedType.JSObject => "JSType.Object",
                KnownManagedType.String => "JSType.String",
                KnownManagedType.Object => "JSType.Any",

                _ => throw new NotSupportedTypeException($"Unsupported Array<T> type argument {typeInfo.KnownType} ({syntax})")
            };

            return $"JSType.Array<{innerJsType}>";
        }

        static string GetNullableJSMarshalAsTypeArgument(KnownManagedType managedType, TypeSyntax syntax)
        {
            // Only certain types are supported as nullables, this method maps them
            return managedType switch
            {
                KnownManagedType.Boolean => "JSType.Boolean",
                KnownManagedType.Byte
                or KnownManagedType.Int16
                or KnownManagedType.Int32
                or KnownManagedType.Int64
                or KnownManagedType.IntPtr
                or KnownManagedType.Single
                or KnownManagedType.Double => "JSType.Number",
                KnownManagedType.Char => "JSType.String",
                KnownManagedType.DateTime => "JSType.Date",
                KnownManagedType.DateTimeOffset => "JSType.Date",

                _ => throw new NotSupportedTypeException($"Unsupported Nullable<T> type argument {managedType} ({syntax})")
            };
        }
    }
}