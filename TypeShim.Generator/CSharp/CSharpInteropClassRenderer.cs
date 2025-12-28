using Microsoft.CodeAnalysis;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo classInfo;
    private readonly RenderContext _ctx;

    private const string FromJSObjectMethodName = "FromJSObject";
    private const string FromObjectMethodName = "FromObject";

    public CSharpInteropClassRenderer(ClassInfo classInfo, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        ArgumentNullException.ThrowIfNull(context);
        if (!classInfo.Methods.Any() && !classInfo.Properties.Any())
        {
            throw new ArgumentException("Interop class must have at least one method or property to render.", nameof(classInfo));
        }
        this.classInfo = classInfo;
        this._ctx = context;
    }

    private string GetInteropClassName(string className) => $"{className}Interop";

    internal string Render()
    {
        _ctx.AppendLine("// Auto-generated TypeScript interop definitions")
            .AppendLine("using System;")
            .AppendLine("using System.Runtime.InteropServices.JavaScript;")
            .AppendLine("using System.Threading.Tasks;")
            .Append("namespace ").Append(classInfo.Namespace).AppendLine(";")
            .Append("public partial class ").AppendLine(GetInteropClassName(classInfo.Name))
            .AppendLine("{");

        using (_ctx.Indent())
        {
            if (classInfo.Constructor is not null)
            {
                RenderConstructor(classInfo.Constructor);
            }

            foreach (MethodInfo methodInfo in classInfo.Methods)
            {
                RenderMethod(methodInfo);
            }

            foreach (PropertyInfo propertyInfo in classInfo.Properties)
            {
                RenderProperty(propertyInfo);
            }
            
            if (!classInfo.IsStatic)
            {
                RenderFromObjectMapper();
            }

            if (classInfo.Constructor is { AcceptsInitializer: true, IsParameterless: true } constructorMethod)
            {
                RenderFromJSObjectMapper(constructorMethod);
            }
        }
        
        _ctx.AppendLine("}");
        return _ctx.Render();
    }

    private void RenderConstructor(ConstructorInfo constructorInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(constructorInfo.Type);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString())
            .AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        MethodParameterInfo[] allParameters = constructorInfo.GetParametersIncludingInitializerObject();
        RenderMethodSignature(constructorInfo.Name, constructorInfo.Type, allParameters);
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in allParameters)
            {
                RenderParameterTypeConversion(originalParamInfo);
            }
            RenderConstructorInvocation(constructorInfo);
        }

        _ctx.AppendLine("}");
    }

    private void RenderProperty(PropertyInfo propertyInfo)
    {
        RenderPropertyMethod(propertyInfo, propertyInfo.GetMethod);
        if (propertyInfo.SetMethod is null)
            return;
        RenderPropertyMethod(propertyInfo, propertyInfo.SetMethod);
    }

    private void RenderPropertyMethod(PropertyInfo propertyInfo, MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo);

        _ctx.AppendLine("{");

        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
            {
                RenderParameterTypeConversion(originalParamInfo);
            }

            string accessedObject = methodInfo.IsStatic ? classInfo.Name : GetTypedParameterName(methodInfo.MethodParameters.ElementAt(0));
            string accessorExpression = $"{accessedObject}.{propertyInfo.Name}";

            if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
            {
                // Handle Task<T>? property conversion to interop type Task<object>?
                string convertedTaskExpression = RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }
            else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
            {
                // Handle Task<T> property conversion to interop type Task<object>
                string convertedTaskExpression = RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }

            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
            {
                _ctx.AppendLine($"return {accessorExpression};");
            }
            else // setter
            {
                string valueVarName = GetTypedParameterName(methodInfo.MethodParameters.ElementAt(1));
                _ctx.AppendLine($"{accessorExpression} = {valueVarName};");
            }
        }
        
        _ctx.AppendLine("}");
    }

    private void RenderMethod(MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString())
            .AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo);        
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
            {
                RenderParameterTypeConversion(originalParamInfo);
            }
            RenderUserMethodInvocation(methodInfo);
        }
            
        _ctx.AppendLine("}");
    }

    private void RenderMethodSignature(MethodInfo methodInfo) 
        => RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.MethodParameters);

    private void RenderMethodSignature(string name, InteropTypeInfo returnType, IEnumerable<MethodParameterInfo> parameterInfos)
    {
        _ctx.Append("public static ")
            .Append(returnType.InteropTypeSyntax)
            .Append(' ')
            .Append(name)
            .Append('(');
        RenderMethodParameterList();
        _ctx.AppendLine(")");
        
        void RenderMethodParameterList()
        {
            if (!parameterInfos.Any())
                return;

            bool isFirst = true;
            foreach (MethodParameterInfo parameterInfo in parameterInfos)
            {
                if (!isFirst) _ctx.Append(", ");

                JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(parameterInfo.Type);
                _ctx.Append(marshalAsAttributeRenderer.RenderParameterAttribute().NormalizeWhitespace().ToFullString())
                    .Append(' ')
                    .Append(parameterInfo.Type.InteropTypeSyntax)
                    .Append(' ')
                    .Append(parameterInfo.Name);
                isFirst = false;
            }
        }
    }

    private void RenderUserMethodInvocation(MethodInfo methodInfo)
    {
        // Handle Task<T> return conversion for conversion requiring types
        if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            _ctx.Append("return ").Append(convertedTaskExpression).AppendLine(";");
        }
        else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            string convertedTaskExpression = RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            _ctx.Append("return ").Append(convertedTaskExpression).AppendLine(";");
        }
        else // direct return handling or void invocations
        {
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
            {
                _ctx.Append("return ");
            }
            _ctx.Append(GetInvocationExpression())
                .AppendLine(";");
        }

        string GetInvocationExpression()
        {
            if (!methodInfo.IsStatic)
            {
                MethodParameterInfo instanceParam = methodInfo.MethodParameters.ElementAt(0);
                List<MethodParameterInfo> memberParams = [.. methodInfo.MethodParameters.Skip(1)];
                return $"{GetTypedParameterName(instanceParam)}.{methodInfo.Name}({string.Join(", ", memberParams.Select(GetTypedParameterName))})";
            }
            else
            {
                return $"{classInfo.Name}.{methodInfo.Name}({string.Join(", ", methodInfo.MethodParameters.Select(GetTypedParameterName))})";
            }
        }
    }

    private string GetTypedParameterName(MethodParameterInfo paramInfo) => paramInfo.Type.RequiresCLRTypeConversion ? $"typed_{paramInfo.Name}" : paramInfo.Name;

    private void RenderParameterTypeConversion(MethodParameterInfo parameterInfo)
    {
        if (!parameterInfo.Type.RequiresCLRTypeConversion)
            return;

        // task pattern differs from other conversions, hence their fully separated rendering.
        if (parameterInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true }) // Task<T>?
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            _ctx.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = {convertedTaskExpression};");
            return;
        }
        if (parameterInfo.Type.IsTaskType) // Task<T>
        {
            string convertedTaskExpression = RenderTaskTypeConversion(parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            _ctx.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = {convertedTaskExpression};");
            return; 
        }

        _ctx.Append($"{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = ");
        RenderInlineTypeConversion(parameterInfo.Type, parameterInfo.Name, forceCovariantConversion: parameterInfo.IsInjectedInstanceParameter);
        _ctx.AppendLine(";");
    }

    private void RenderInlineTypeConversion(InteropTypeInfo typeInfo, string varName, bool forceCovariantConversion = false)
    {
        if (forceCovariantConversion)
        {
            RenderInlineCovariantTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.IsArrayType)
        {
            RenderInlineArrayTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.IsNullableType)
        {
            RenderInlineNullableTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.ManagedType is KnownManagedType.Object)
        {
            RenderInlineObjectTypeConversion(typeInfo, varName);
        }
        else // Tests guard against this case. Anyway, here is a state-of-the-art regression detector.
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CLRTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineCovariantTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType is KnownManagedType.Object or KnownManagedType.Array, "Unexpected non-object or non-array type with required type conversion");
        _ctx.Append($"({typeInfo.CLRTypeSyntax}){parameterName}");
    }

    private void RenderInlineObjectTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        InteropTypeInfo targetType = typeInfo.TypeArgument ?? typeInfo; // unwrap nullable or use simple type directly
        string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());

        if (typeInfo.IsTSExport)
        {
            _ctx.Append($"{targetInteropClass}.{FromObjectMethodName}({parameterName})");
        }
        else
        {
            RenderInlineCovariantTypeConversion(targetType, parameterName);
        }
    }

    private void RenderInlineNullableTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.IsNullableType, "Type must be nullable for nullable type conversion.");
        Debug.Assert(typeInfo.TypeArgument != null, "Nullable type must have a type argument.");

        _ctx.Append($"{parameterName} != null ? ");
        RenderInlineTypeConversion(typeInfo.TypeArgument, parameterName);
        _ctx.Append(" : null");
    }

    private void RenderInlineArrayTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.TypeArgument != null, "Array type must have a type argument.");
        InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Array type must have a type argument for conversion.");
        if (typeInfo.TypeArgument.IsTSExport == false)
        {
            RenderInlineCovariantTypeConversion(typeInfo, parameterName);
            // no special conversion possible for non-exported types
        } 
        else
        {
            _ctx.Append($"Array.ConvertAll({parameterName}, e => ");
            RenderInlineTypeConversion(typeInfo.TypeArgument, "e");
            _ctx.Append(')');
        }
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private string RenderTaskTypeConversion(InteropTypeInfo targetTaskType, string sourceVarName, string sourceTaskExpression)
    {
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CLRTypeSyntax}> {tcsVarName} = new();")
            .AppendLine($"{sourceTaskExpression}.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
            _ctx.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeConversion(taskTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");
        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
    }

    private string RenderNullableTaskTypeConversion(InteropTypeInfo targetNullableTaskType, string sourceVarName, string sourceTaskExpression)
    {
        InteropTypeInfo taskTypeParamInfo = targetNullableTaskType.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument for conversion.");
        InteropTypeInfo taskReturnTypeParamInfo = taskTypeParamInfo.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskReturnTypeParamInfo.CLRTypeSyntax}>? {tcsVarName} = {sourceTaskExpression} != null ? new() : null;");
        _ctx.AppendLine($"{sourceTaskExpression}?.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);")
                .AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");

            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeConversion(taskReturnTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");

        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }

    void RenderFromObjectMapper()
    {
        _ctx.AppendLine($"public static {classInfo.Type.CLRTypeSyntax} {FromObjectMethodName}(object obj)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"return obj switch");
            _ctx.AppendLine("{");
            using (_ctx.Indent())
            {
                _ctx.AppendLine($"{classInfo.Type.CLRTypeSyntax} instance => instance,");
                if (classInfo.IsSnapshotCompatible())
                {
                    _ctx.AppendLine($"JSObject jsObj => {FromJSObjectMethodName}(jsObj),");
                }
                _ctx.AppendLine($"_ => throw new ArgumentException($\"Invalid object type {{obj?.GetType().ToString() ?? \"null\"}}\", nameof(obj)),");
            }
            _ctx.AppendLine("};");
        }
        _ctx.AppendLine("}");
    }

    void RenderFromJSObjectMapper(ConstructorInfo constructorInfo)
    {
        _ctx.AppendLine($"public static {classInfo.Type.CLRTypeSyntax} {FromJSObjectMethodName}(JSObject jsObject)")
            .AppendLine("{");

        using (_ctx.Indent())
        {
            RenderConstructorInvocation(constructorInfo);
        }
        _ctx.AppendLine("}");
    }

    private void RenderConstructorInvocation(ConstructorInfo constructorInfo)
    {
        PropertyInfo[] propertiesInMapper = [.. constructorInfo.MemberInitializers];
        // Converting task types requires variable assignments, write those first, keep dict for assignments in initializer
        Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> propertyToConvertedVarDict = RenderNonInlinableTypeConversions(propertiesInMapper);
        _ctx.Append("return new ").Append(constructorInfo.Type.CLRTypeSyntax).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo param in constructorInfo.Parameters)
        {
            if (!isFirst) _ctx.Append(", ");
            _ctx.Append(param.Name);
            isFirst = false;
        }

        if (!constructorInfo.AcceptsInitializer)
        {
            _ctx.AppendLine(");");
            return;
        }

        _ctx.AppendLine(")");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            // TODO: support init properties
            foreach (PropertyInfo propertyInfo in propertiesInMapper)
            {
                if (propertyToConvertedVarDict.TryGetValue(propertyInfo, out TypeConversionExpressionRenderDelegate? expressionRenderer))
                {
                    _ctx.Append($"{propertyInfo.Name} = ");
                    expressionRenderer.Render();
                }
                else if (propertyInfo.Type.RequiresCLRTypeConversion)
                {
                    string propertyRetrievalExpression = $"jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")";
                    _ctx.Append($"{propertyInfo.Name} = ");
                    RenderInlineTypeConversion(propertyInfo.Type, propertyRetrievalExpression);
                }
                else
                {
                    _ctx.Append($"{propertyInfo.Name} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")"); // TODO: error handling? (null / missing props?)
                }
                _ctx.AppendLine(",");
            }
        }
        _ctx.AppendLine("};");

        Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> RenderNonInlinableTypeConversions(PropertyInfo[] properties)
        {
            Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> convertedTaskExpressionDict = [];
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = RenderNullableTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsTaskType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = RenderTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsNullableType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => RenderInlineTypeConversion(propertyInfo.Type, tmpVarName)));
                }
            }

            return convertedTaskExpressionDict;
        }
    }

    private class TypeConversionExpressionRenderDelegate(Action renderAction)
    {
        internal void Render() => renderAction();

        public static implicit operator TypeConversionExpressionRenderDelegate(Action renderAction)
        {
            return new TypeConversionExpressionRenderDelegate(renderAction);
        }
    }
}

internal static class JSObjectMethodResolver
{
    internal static string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
    {
        return typeInfo.ManagedType switch
        {
            KnownManagedType.Nullable => ResolveJSObjectMethodName(typeInfo.TypeArgument!),
            KnownManagedType.Boolean => "GetPropertyAsBoolean",
            KnownManagedType.Double => "GetPropertyAsDouble",
            KnownManagedType.String => "GetPropertyAsString",
            KnownManagedType.Int32 => "GetPropertyAsInt32",
            KnownManagedType.JSObject => "GetPropertyAsJSObject",
            KnownManagedType.Object when typeInfo.IsTSExport => "GetPropertyAsJSObject", // exported object types have a FromJSObject mapper
            KnownManagedType.Array => typeInfo.TypeArgument switch
            {
                { ManagedType: KnownManagedType.Byte } => "GetPropertyAsByteArray",
                { ManagedType: KnownManagedType.Int32 } => "GetPropertyAsInt32Array",
                { ManagedType: KnownManagedType.Double } => "GetPropertyAsDoubleArray",
                { ManagedType: KnownManagedType.String } => "GetPropertyAsStringArray",
                { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectArray",
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectArray", // exported object types have a FromJSObject mapper
                { ManagedType: KnownManagedType.Nullable } elemTypeInfo => elemTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectArray",
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectArray", // exported object types have a FromJSObject mapper
                    _ => throw new InvalidOperationException($"Array of nullable type '{elemTypeInfo?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
                },
                _ => throw new InvalidOperationException($"Array of type '{typeInfo.TypeArgument?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
            },
            KnownManagedType.Task => typeInfo.TypeArgument switch
            {
                { ManagedType: KnownManagedType.Boolean } => "GetPropertyAsBooleanTask",
                { ManagedType: KnownManagedType.Byte } => "GetPropertyAsByteTask",
                { ManagedType: KnownManagedType.Char } => "GetPropertyAsCharTask",
                { ManagedType: KnownManagedType.Int16 } => "GetPropertyAsInt16Task",
                { ManagedType: KnownManagedType.Int32 } => "GetPropertyAsInt32Task",
                { ManagedType: KnownManagedType.Int64 } => "GetPropertyAsInt64Task",
                { ManagedType: KnownManagedType.Single } => "GetPropertyAsSingleTask",
                { ManagedType: KnownManagedType.Double } => "GetPropertyAsDoubleTask",
                { ManagedType: KnownManagedType.IntPtr } => "GetPropertyAsIntPtrTask",
                { ManagedType: KnownManagedType.DateTime } => "GetPropertyAsDateTimeTask",
                { ManagedType: KnownManagedType.DateTimeOffset } => "GetPropertyAsDateTimeOffsetTask",
                { ManagedType: KnownManagedType.Exception } => "GetPropertyAsExceptionTask",
                { ManagedType: KnownManagedType.String } => "GetPropertyAsStringTask",
                { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectTask",
                { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectTask", // TODO: include fromJSObject mapping?
                { ManagedType: KnownManagedType.Nullable } returnTypeInfo => returnTypeInfo.TypeArgument switch
                {
                    { ManagedType: KnownManagedType.JSObject } => "GetPropertyAsJSObjectTask",
                    { ManagedType: KnownManagedType.Object, IsTSExport: true } => "GetPropertyAsJSObjectTask", // exported object types have a FromJSObject mapper
                    _ => throw new InvalidOperationException($"Task of nullable type '{returnTypeInfo?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
                },
                _ => throw new InvalidOperationException($"Task of type '{typeInfo.TypeArgument?.ManagedType}' cannot be marshalled through TypeShim JSObject extensions"),
            },
            _ => throw new InvalidOperationException($"Type '{typeInfo.ManagedType}' cannot be marshalled through JSObject nor TypeShim JSObject extensions"),
        };
    }
}