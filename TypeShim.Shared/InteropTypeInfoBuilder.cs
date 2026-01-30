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
            JSFunctionTypeInfo functionTypeInfo => BuildFunctionTypeInfo(functionTypeInfo, clrTypeSyntax),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)),
        };
    }

    private InteropTypeInfo BuildFunctionTypeInfo(JSFunctionTypeInfo functionTypeInfo, TypeSyntax clrTypeSyntax)
    {
        (InteropTypeInfo[] argTypeInfos, InteropTypeInfo returnTypeInfo) = GetArgumentTypeInfos(typeSymbol);
        InteropTypeInfo[] allArgTypeInfos = [.. argTypeInfos, returnTypeInfo];
        TypeScriptFunctionParameterTemplate[] tsParameterTemplates = [.. argTypeInfos.Select((InteropTypeInfo typeInfo, int i) =>
            new TypeScriptFunctionParameterTemplate($"arg{i}", GetSimpleTypeScriptSymbolTemplate(typeInfo.ManagedType, typeInfo.CSharpTypeSyntax, typeInfo.RequiresTypeConversion, typeInfo.SupportsTypeConversion))
        )];
        TypeScriptSymbolNameTemplate tsSyntax = TypeScriptSymbolNameTemplate.ForDelegateType(tsParameterTemplates, GetSimpleTypeScriptSymbolTemplate(returnTypeInfo.ManagedType, returnTypeInfo.CSharpTypeSyntax, returnTypeInfo.RequiresTypeConversion, returnTypeInfo.SupportsTypeConversion));
        
        TypeScriptFunctionParameterTemplate[] tsInteropParameterTemplates = [.. argTypeInfos.Select((InteropTypeInfo typeInfo, int i) =>
            new TypeScriptFunctionParameterTemplate($"arg{i}", GetSimpleTypeScriptSymbolTemplate(typeInfo.ManagedType, typeInfo.CSharpTypeSyntax, typeInfo.RequiresTypeConversion, typeInfo.SupportsTypeConversion))
        )];
        TypeScriptSymbolNameTemplate tsInteropSyntax = TypeScriptSymbolNameTemplate.ForDelegateType(tsInteropParameterTemplates, GetInteropSimpleTypeScriptSymbolTemplate(returnTypeInfo.ManagedType, returnTypeInfo.CSharpTypeSyntax));
        return new InteropTypeInfo
        {
            ManagedType = functionTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(functionTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = GetCSInteropTypeSyntax(functionTypeInfo),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeScriptTypeSyntax = tsSyntax,
            TypeScriptInteropTypeSyntax = tsInteropSyntax,
            TypeArgument = null,
            ArgumentInfo = new DelegateArgumentInfo()
            {
                ParameterTypes = argTypeInfos,
                ReturnType = returnTypeInfo
            },
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


    

    private InteropTypeInfo BuildSimpleTypeInfo(JSSimpleTypeInfo simpleTypeInfo, TypeSyntax clrTypeSyntax)
    {
        bool requiresTypeConversion = RequiresTypeConversion();
        bool supportsTypeConversion = SupportsTypeConversion();
        
        return new InteropTypeInfo
        {
            ManagedType = simpleTypeInfo.KnownType,
            JSTypeSyntax = GetJSTypeSyntax(simpleTypeInfo, clrTypeSyntax),
            CSharpInteropTypeSyntax = GetCSInteropTypeSyntax(simpleTypeInfo),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeScriptTypeSyntax = GetSimpleTypeScriptSymbolTemplate(simpleTypeInfo.KnownType, clrTypeSyntax, requiresTypeConversion, supportsTypeConversion),
            TypeScriptInteropTypeSyntax = GetInteropSimpleTypeScriptSymbolTemplate(simpleTypeInfo.KnownType, clrTypeSyntax),
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
            CSharpInteropTypeSyntax = GetCSInteropTypeSyntax(arrayTypeInfo),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeScriptTypeSyntax = TypeScriptSymbolNameTemplate.ForArrayType(elementTypeInfo.TypeScriptTypeSyntax),
            TypeScriptInteropTypeSyntax = TypeScriptSymbolNameTemplate.ForArrayType(elementTypeInfo.TypeScriptInteropTypeSyntax),
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
            CSharpInteropTypeSyntax = GetCSInteropTypeSyntax(taskTypeInfo),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeScriptTypeSyntax = TypeScriptSymbolNameTemplate.ForPromiseType(taskReturnTypeInfo?.TypeScriptTypeSyntax),
            TypeScriptInteropTypeSyntax = TypeScriptSymbolNameTemplate.ForPromiseType(taskReturnTypeInfo?.TypeScriptInteropTypeSyntax),
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
            CSharpInteropTypeSyntax = GetCSInteropTypeSyntax(nullableTypeInfo),
            CSharpTypeSyntax = clrTypeSyntax,
            TypeScriptTypeSyntax = TypeScriptSymbolNameTemplate.ForNullableType(innerTypeInfo.TypeScriptTypeSyntax),
            TypeScriptInteropTypeSyntax = TypeScriptSymbolNameTemplate.ForNullableType(innerTypeInfo.TypeScriptInteropTypeSyntax),
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

    private static TypeSyntax GetCSInteropTypeSyntax(JSTypeInfo jsTypeInfo)
    {
        return jsTypeInfo switch
        {
            JSSimpleTypeInfo simpleTypeInfo => simpleTypeInfo.Syntax,
            JSArrayTypeInfo arrayTypeInfo => arrayTypeInfo.GetTypeSyntax(),
            JSTaskTypeInfo taskTypeInfo => taskTypeInfo.GetTypeSyntax(),
            JSNullableTypeInfo nullableTypeInfo => SyntaxFactory.NullableType(GetCSInteropTypeSyntax(nullableTypeInfo.ResultTypeInfo)),
            JSFunctionTypeInfo functionTypeInfo => functionTypeInfo.GetTypeSyntax().NormalizeWhitespace(),
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
            JSNullableTypeInfo { IsValueType: false } nullableTypeInfo => GetJSTypeSyntax(nullableTypeInfo.ResultTypeInfo, clrTypeSyntax).ToString(),
            JSSpanTypeInfo => throw new NotImplementedException("Span<T> is not yet supported"),
            JSArraySegmentTypeInfo => throw new NotImplementedException("ArraySegment<T> is not yet supported"),
            JSFunctionTypeInfo functionTypeInfo => GetFunctionJSMarshalAsTypeArgument(functionTypeInfo),
            JSInvalidTypeInfo or _ => throw new NotSupportedTypeException(clrTypeSyntax.ToFullString()),
        });

        static string GetFunctionJSMarshalAsTypeArgument(JSFunctionTypeInfo functionTypeInfo)
        {
            if (functionTypeInfo.ArgsTypeInfo.Length == 0)
            {
                return $"JSType.Function";
            }
            string[] genericArguments = [.. functionTypeInfo.ArgsTypeInfo.Select(typeInfo => GetJSTypeSyntax(typeInfo, typeInfo.Syntax).ToString())];
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

    private TypeScriptSymbolNameTemplate GetInteropSimpleTypeScriptSymbolTemplate(KnownManagedType managedType, TypeSyntax originalSyntax)
    {
        return managedType switch
        {
            KnownManagedType.Object // objects are represented differently on the interop boundary
                => TypeScriptSymbolNameTemplate.ForUserType("ManagedObject"),
            KnownManagedType.Char // chars are represented as numbers on the interop boundary (is intended: https://github.com/dotnet/runtime/issues/123187)
                => TypeScriptSymbolNameTemplate.ForSimpleType("number"),
            _ => GetSimpleTypeScriptSymbolTemplate(managedType, originalSyntax, true, false)
        };
    }

    private TypeScriptSymbolNameTemplate GetSimpleTypeScriptSymbolTemplate(KnownManagedType managedType, TypeSyntax originalSyntax, bool requiresTypeConversion, bool supportsTypeConversion)
    {
        return managedType switch
        {
            KnownManagedType.Object when requiresTypeConversion && supportsTypeConversion
                => TypeScriptSymbolNameTemplate.ForUserType(originalSyntax.ToString()),
            KnownManagedType.Object when requiresTypeConversion && !supportsTypeConversion
                => TypeScriptSymbolNameTemplate.ForUserType("ManagedObject"),
            KnownManagedType.Object when !requiresTypeConversion
                => TypeScriptSymbolNameTemplate.ForSimpleType("ManagedObject"),

            KnownManagedType.None => TypeScriptSymbolNameTemplate.ForSimpleType("undefined"),
            KnownManagedType.Void => TypeScriptSymbolNameTemplate.ForSimpleType("void"),
            KnownManagedType.JSObject
                => TypeScriptSymbolNameTemplate.ForSimpleType("object"),

            KnownManagedType.Boolean => TypeScriptSymbolNameTemplate.ForSimpleType("boolean"),
            KnownManagedType.Char
            or KnownManagedType.String => TypeScriptSymbolNameTemplate.ForSimpleType("string"),
            KnownManagedType.Byte
            or KnownManagedType.Int16
            or KnownManagedType.Int32
            or KnownManagedType.Int64
            or KnownManagedType.Double
            or KnownManagedType.Single
            or KnownManagedType.IntPtr
                => TypeScriptSymbolNameTemplate.ForSimpleType("number"),
            KnownManagedType.DateTime
            or KnownManagedType.DateTimeOffset => TypeScriptSymbolNameTemplate.ForSimpleType("Date"),
            KnownManagedType.Exception => TypeScriptSymbolNameTemplate.ForSimpleType("Error"),

            // TODO: add support for ArraySegment<T> and Span<T> i.e. MemoryView
            KnownManagedType.ArraySegment
            or KnownManagedType.Span
                => throw new NotImplementedException("ArraySegment and Span are not yet supported"),
            // TODO: add support for Action and Function types
            KnownManagedType.Action => throw new NotImplementedException("Action is not yet supported"), // "(() => void)"
            KnownManagedType.Function => throw new NotImplementedException("Function is not yet supported"), // "Function"

            KnownManagedType.Unknown
            or _ => TypeScriptSymbolNameTemplate.ForSimpleType("any"),
        };
    }
}