using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TypeShim.Shared;

internal sealed class InteropTypeInfoBuilder(ITypeSymbol typeSymbol, InteropTypeInfoCache cache)
{
    private readonly bool IsTSExport = typeSymbol.GetAttributes().Any(attributeData => attributeData.AttributeClass?.Name is "TSExportAttribute" or "TSExport");

    public InteropTypeInfo Build()
    {
        ThrowIfGenericTSExport();       
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
            JSSpanTypeInfo or JSArraySegmentTypeInfo => BuildSpanOrArraySegmentTypeInfo(jsTypeInfo, clrTypeSyntax),
            JSFunctionTypeInfo functionTypeInfo => BuildFunctionTypeInfo(functionTypeInfo, clrTypeSyntax),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
        };
    }

    private InteropTypeInfo BuildSimpleTypeInfo(JSSimpleTypeInfo simpleTypeInfo, TypeSyntax clrTypeSyntax)
    {
        bool requiresTypeConversion = RequiresTypeConversion();
        bool supportsTypeConversion = SupportsTypeConversion();
        
        return new InteropTypeInfo
        {
            ManagedType = simpleTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(simpleTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = simpleTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = null,
            ArgumentInfo = null,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = clrTypeSyntax is NullableTypeSyntax,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = requiresTypeConversion,
            SupportsTypeConversion = supportsTypeConversion,
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
            return simpleTypeInfo.KnownType != KnownManagedType.Object || IsTSExport;
        }
    }

    private InteropTypeInfo BuildArrayTypeInfo(JSArrayTypeInfo arrayTypeInfo, TypeSyntax clrTypeSyntax)
    {
        ITypeSymbol? elementTypeSymbol = GetTypeArgument(typeSymbol) ?? throw new NotSupportedTypeException("Only arrays with one element type are supported");
        InteropTypeInfo elementTypeInfo = new InteropTypeInfoBuilder(elementTypeSymbol, cache).Build();

        return new InteropTypeInfo
        {
            ManagedType = arrayTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(arrayTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = arrayTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = elementTypeInfo,
            ArgumentInfo = null,
            IsTaskType = false,
            IsArrayType = true,
            IsNullableType = false,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = elementTypeInfo.RequiresTypeConversion,
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
            ManagedType = taskTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(taskTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = taskTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = taskReturnTypeInfo,
            ArgumentInfo = null,
            IsTaskType = true,
            IsArrayType = false,
            IsNullableType = false,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = taskReturnTypeInfo?.RequiresTypeConversion ?? false,
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
            ManagedType = nullableTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(nullableTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = nullableTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = innerTypeInfo,
            ArgumentInfo = null,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = true,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = innerTypeInfo.RequiresTypeConversion,
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

    private InteropTypeInfo BuildSpanOrArraySegmentTypeInfo(JSTypeInfo spanTypeInfo, TypeSyntax clrTypeSyntax)
    {
        if (spanTypeInfo is not JSSpanTypeInfo && spanTypeInfo is not JSArraySegmentTypeInfo || GetTypeArgument(typeSymbol) is not ITypeSymbol innerTypeSymbol)
        {
            throw new NotSupportedTypeException("Only Span<T> and ArraySegment<T> are supported");
        }
        InteropTypeInfo innerTypeInfo = new InteropTypeInfoBuilder(innerTypeSymbol, cache).Build();

        if (innerTypeInfo.ManagedType is not KnownManagedType.Byte and not KnownManagedType.Int32 and not KnownManagedType.Double)
        {
            throw new NotSupportedTypeException($"Type argument {innerTypeInfo.CSharpTypeSyntax} in {clrTypeSyntax} is not supported.");
        }

        return new InteropTypeInfo
        {
            ManagedType = spanTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(spanTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = spanTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = innerTypeInfo,
            ArgumentInfo = null,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = false,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = innerTypeInfo.RequiresTypeConversion,
            SupportsTypeConversion = innerTypeInfo.SupportsTypeConversion
        };

        static ITypeSymbol? GetTypeArgument(ITypeSymbol typeSymbol)
        {
            if (typeSymbol is INamedTypeSymbol { TypeArguments.Length: 1 } spanType)
            {
                return spanType.TypeArguments[0];
            }
            return null;
        }
    }

    private InteropTypeInfo BuildFunctionTypeInfo(JSFunctionTypeInfo functionTypeInfo, TypeSyntax clrTypeSyntax)
    {
        (InteropTypeInfo[] argTypeInfos, InteropTypeInfo returnTypeInfo) = GetArgumentTypeInfos(typeSymbol);
        InteropTypeInfo[] allArgTypeInfos = [.. argTypeInfos, returnTypeInfo];

        DelegateArgumentInfo argumentInfo = new()
        {
            ParameterTypes = argTypeInfos,
            ReturnType = returnTypeInfo
        };

        return new InteropTypeInfo
        {
            ManagedType = functionTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(functionTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = functionTypeInfo.GetTypeSyntax(),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeArgument = null,
            ArgumentInfo = argumentInfo,
            IsTaskType = false,
            IsArrayType = false,
            IsNullableType = false,
            IsTSExport = IsTSExport,
            RequiresTypeConversion = allArgTypeInfos.Any(info => info.RequiresTypeConversion),
            SupportsTypeConversion = allArgTypeInfos.Any(info => info.RequiresTypeConversion && info.SupportsTypeConversion)
        };
    }

    private (InteropTypeInfo[] Parameters, InteropTypeInfo ReturnType) GetArgumentTypeInfos(ITypeSymbol typeSymbol)
    {
        string fullTypeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        PredefinedTypeSyntax voidSyntax = SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));
        JSSimpleTypeInfo voidJSTypeInfo = new JSSimpleTypeInfo(KnownManagedType.Void)
        {
            Syntax = voidSyntax
        };
        switch (typeSymbol)
        {
            case ITypeSymbol when fullTypeName == Constants.ActionGlobal:
                return (Parameters: [], ReturnType: BuildSimpleTypeInfo(voidJSTypeInfo, voidSyntax));

            case INamedTypeSymbol actionType when fullTypeName.StartsWith(Constants.ActionGlobal, StringComparison.Ordinal):
                InteropTypeInfo[] argumentTypes = [.. actionType.TypeArguments.Select(arg => new InteropTypeInfoBuilder(arg, cache).Build())];
                return (Parameters: argumentTypes, ReturnType: BuildSimpleTypeInfo(voidJSTypeInfo, voidSyntax));

            // function
            case INamedTypeSymbol funcType when fullTypeName.StartsWith(Constants.FuncGlobal, StringComparison.Ordinal):
                InteropTypeInfo[] signatureTypes = [.. funcType.TypeArguments.Select(arg => new InteropTypeInfoBuilder(arg, cache).Build())];
                return (Parameters: [.. signatureTypes.Take(signatureTypes.Length - 1)], ReturnType: signatureTypes.Last());
        }
        throw new NotSupportedTypeException($"Delegate type '{typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' has an unsupported argument types");
    }

    private static TypeSyntax GetJSTypeSyntax(JSTypeInfo jSTypeInfo, TypeSyntax clrTypeSyntax)
    {
        return SyntaxFactory.ParseTypeName(jSTypeInfo switch
        {
            JSSimpleTypeInfo simpleType => GetSimpleJSMarshalAsTypeArgument(simpleType.KnownType),
            JSArrayTypeInfo arrayTypeInfo => GetArrayJSMarshalAsTypeArgument(arrayTypeInfo.ElementTypeInfo, clrTypeSyntax),
            JSTaskTypeInfo taskTypeInfo => GetPromiseJSMarshalAsTypeArgument(taskTypeInfo.ResultTypeInfo, clrTypeSyntax),
            JSNullableTypeInfo { IsValueType: true } nullableTypeInfo => GetNullableJSMarshalAsTypeArgument(nullableTypeInfo.ResultTypeInfo.KnownType, clrTypeSyntax),
            JSNullableTypeInfo { IsValueType: false } nullableTypeInfo => GetJSTypeSyntax(nullableTypeInfo.ResultTypeInfo, clrTypeSyntax).ToString(),
            JSSpanTypeInfo or JSArraySegmentTypeInfo => "JSType.MemoryView",
            JSFunctionTypeInfo functionTypeInfo => GetFunctionJSMarshalAsTypeArgument(functionTypeInfo),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(clrTypeSyntax.ToFullString()),
        });

        static string GetFunctionJSMarshalAsTypeArgument(JSFunctionTypeInfo functionTypeInfo)
        {
            if (functionTypeInfo.ArgsTypeInfo.Length == 0)
            {
                return $"JSType.Function";
            }
            string[] genericArguments = [.. functionTypeInfo.ArgsTypeInfo.Select(typeInfo => GetJSTypeSyntax(typeInfo, typeInfo.GetTypeSyntax()).ToString())];
            return $"JSType.Function<{string.Join(", ", genericArguments)}>";
        }

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

    private void ThrowIfGenericTSExport()
    {
        if (IsTSExport && typeSymbol is INamedTypeSymbol { Arity: not 0 })
        {
            throw new NotSupportedGenericClassException(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
        }
    }
}