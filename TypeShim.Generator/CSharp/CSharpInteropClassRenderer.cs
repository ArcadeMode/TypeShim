using Microsoft.CodeAnalysis;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo classInfo;

    private readonly StringBuilder sb = new();
    private const string FromJSObjectMethodName = "FromJSObject";
    private const string FromObjectMethodName = "FromObject";

    public CSharpInteropClassRenderer(ClassInfo classInfo)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        if (!classInfo.Methods.Any() && !classInfo.Properties.Any())
        {
            throw new ArgumentException("Interop class must have at least one method or property to render.", nameof(classInfo));
        }
        this.classInfo = classInfo;
    }

    private string GetInteropClassName(string className) => $"{className}Interop";

    internal string Render(int depth = 0)
    {
        string indent = new(' ', depth * 4);
        sb.AppendLine($"{indent}// Auto-generated TypeScript interop definitions");
        sb.AppendLine($"{indent}using System;");
        sb.AppendLine($"{indent}using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine($"{indent}using System.Threading.Tasks;");
        sb.AppendLine($"{indent}namespace {classInfo.Namespace};");
        sb.AppendLine($"{indent}public partial class {GetInteropClassName(classInfo.Name)}");
        sb.AppendLine($"{indent}{{");

        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            RenderMethod(methodInfo);
        }

        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            RenderPropertyMethods(propertyInfo);
        }

        RenderObjectMappers(depth + 1);

        sb.AppendLine($"{indent}}}");
        return sb.ToString();
    }

    private void RenderPropertyMethods(PropertyInfo propertyInfo)
    {
        RenderProperty(propertyInfo, propertyInfo.GetMethod);
        if (propertyInfo.SetMethod is null)
            return;
        RenderProperty(propertyInfo, propertyInfo.SetMethod);
    }

    private void RenderProperty(PropertyInfo propertyInfo, MethodInfo methodInfo)
    {
        string indent = new(' ', 4);
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(depth: 1, methodInfo);

        sb.Append(indent);
        sb.AppendLine("{");

        foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
        {
            RenderParameterTypeConversion(depth: 2, originalParamInfo);
        }

        string accessedObject = methodInfo.IsStatic ? classInfo.Name : GetTypedParameterName(methodInfo.MethodParameters.ElementAt(0));
        string accessorExpression = $"{accessedObject}.{propertyInfo.Name}";

        if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
        {
            // Handle Task<T>? property conversion to interop type Task<object>?
            string convertedTaskExpression = RenderNullableTaskTypeConversion(2, methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
            accessorExpression = convertedTaskExpression; // continue with the converted expression
        }
        else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            // Handle Task<T> property conversion to interop type Task<object>
            string convertedTaskExpression = RenderTaskTypeConversion(2, methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
            accessorExpression = convertedTaskExpression; // continue with the converted expression
        }

        if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
        {
            sb.AppendLine($"{indent}{indent}return {accessorExpression};");
        }
        else // setter
        {
            string valueVarName = GetTypedParameterName(methodInfo.MethodParameters.ElementAt(1));
            sb.AppendLine($"{indent}{indent}{accessorExpression} = {valueVarName};");
        }

        sb.Append(indent);
        sb.AppendLine("}");
    }

    private void RenderMethod(MethodInfo methodInfo)
    {
        string indent = new(' ', 4);
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(depth: 1, methodInfo);

        sb.Append(indent);
        sb.AppendLine("{");

        foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
        {
            RenderParameterTypeConversion(depth: 2, originalParamInfo);
        }
        RenderUserMethodInvocation(depth: 2, methodInfo);
        sb.Append(indent);
        sb.AppendLine("}");
    }

    private void RenderMethodSignature(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 4);
        sb.Append(indent);
        sb.Append("public static ");

        // methodInfo.ReturnType.IsTaskType: Never render async! Gets in the way of Task properties, it isnt required anyway.
        // For returns just return the Task
        // For parameters Task can be passed or converted in a continuation (see RenderParameterTypeConversion)

        sb.Append(methodInfo.ReturnType.InteropTypeSyntax);
        sb.Append(' ');
        sb.Append(methodInfo.Name);
        sb.Append('(');
        RenderMethodParameterList(methodInfo.MethodParameters);
        sb.Append(')');
        sb.AppendLine();
        
        void RenderMethodParameterList(IEnumerable<MethodParameterInfo> parameterInfos)
        {
            if (!parameterInfos.Any())
                return;

            foreach (MethodParameterInfo parameterInfo in parameterInfos)
            {
                JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(parameterInfo.Type);
                sb.Append(marshalAsAttributeRenderer.RenderParameterAttribute().NormalizeWhitespace().ToFullString());
                sb.Append(' ');
                sb.Append(parameterInfo.Type.InteropTypeSyntax);
                sb.Append(' ');
                sb.Append(parameterInfo.Name);
                sb.Append(", ");
            }
            sb.Length -= 2; // Remove last ", "
        }
    }

    private void RenderUserMethodInvocation(int depth, MethodInfo methodInfo)
    {
        string indent = new(' ', depth * 4);

        // Handle Task<T> return conversion for conversion requiring types
        if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(depth, methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            sb.Append(indent);
            sb.Append("return ");
            sb.Append(convertedTaskExpression);
            sb.AppendLine(";");
        }
        else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            string convertedTaskExpression = RenderTaskTypeConversion(depth, methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            sb.Append(indent);
            sb.Append("return ");
            sb.Append(convertedTaskExpression);
            sb.AppendLine(";");
        }
        else // direct return handling or void invocations
        {
            sb.Append(indent);
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
            {
                sb.Append("return ");
            }
            sb.Append(GetInvocationExpression());
            sb.AppendLine(";");
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

    private void RenderParameterTypeConversion(int depth, MethodParameterInfo parameterInfo)
    {
        if (!parameterInfo.Type.RequiresCLRTypeConversion)
            return;

        string indent = new(' ', depth * 4);

        // task pattern differs from other conversions, hence their fully separated rendering.
        if (parameterInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true }) // Task<T>?
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(depth, parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            sb.Append(indent);
            sb.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = {convertedTaskExpression};");
            return;
        }
        if (parameterInfo.Type.IsTaskType) // Task<T>
        {
            string convertedTaskExpression = RenderTaskTypeConversion(depth, parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            sb.Append(indent);
            sb.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = {convertedTaskExpression};");
            return; 
        }

        sb.Append($"{indent}{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = ");
        RenderInlineTypeConversion(parameterInfo.Type, parameterInfo.Name, forceCovariantConversion: parameterInfo.IsInjectedInstanceParameter);
        sb.AppendLine(";");
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
            //TODO: make nullables actually ManagedType.Nullable with inner type Object or T, currently nullable reflects inner type, which is confusing sometimes
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
        sb.Append($"({typeInfo.CLRTypeSyntax}){parameterName}");
    }

    private void RenderInlineObjectTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        InteropTypeInfo targetType = typeInfo.TypeArgument ?? typeInfo; // unwrap nullable or use simple type directly
        string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());

        if (typeInfo.IsTSExport)
        {
            sb.Append($"{targetInteropClass}.{FromObjectMethodName}({parameterName})");
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

        sb.Append($"{parameterName} != null ? ");
        RenderInlineTypeConversion(typeInfo.TypeArgument, parameterName);
        sb.Append(" : null");
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
            sb.Append($"Array.ConvertAll({parameterName}, e => ");
            RenderInlineTypeConversion(typeInfo.TypeArgument, "e");
            sb.Append(')');
        }
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private string RenderTaskTypeConversion(int depth, InteropTypeInfo targetTaskType, string sourceVarName, string sourceTaskExpression)
    {
        string indent = new(' ', depth * 4);

        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        sb.Append(indent);
        sb.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CLRTypeSyntax}> {tcsVarName} = new();");
        sb.Append(indent);
        sb.AppendLine($"{sourceTaskExpression}.ContinueWith(t => {{");
        string lambdaIndent = new(' ', (depth + 1) * 4);
        sb.Append(lambdaIndent);
        sb.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
        sb.Append(lambdaIndent);
        sb.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
        sb.Append(lambdaIndent);
        sb.Append($"else {tcsVarName}.SetResult(");
        RenderInlineTypeConversion(taskTypeParamInfo, "t.Result");
        sb.AppendLine(");");
        sb.Append(indent);
        sb.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
    }

    private string RenderNullableTaskTypeConversion(int depth, InteropTypeInfo targetNullableTaskType, string sourceVarName, string sourceTaskExpression)
    {
        string indent = new(' ', depth * 4);

        InteropTypeInfo taskTypeParamInfo = targetNullableTaskType.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument for conversion.");
        InteropTypeInfo taskReturnTypeParamInfo = taskTypeParamInfo.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        sb.Append(indent);
        sb.AppendLine($"TaskCompletionSource<{taskReturnTypeParamInfo.CLRTypeSyntax}>? {tcsVarName} = {sourceTaskExpression} != null ? new() : null;");
        sb.Append(indent);
        sb.AppendLine($"{sourceTaskExpression}?.ContinueWith(t => {{");
        string lambdaIndent = new(' ', (depth + 1) * 4);
        sb.Append(lambdaIndent);
        sb.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
        sb.Append(lambdaIndent);
        sb.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
        sb.Append(lambdaIndent);
        sb.Append($"else {tcsVarName}.SetResult(");
        RenderInlineTypeConversion(taskReturnTypeParamInfo, "t.Result");
        sb.AppendLine(");");
        sb.Append(indent);
        sb.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }

    /// <summary>
    /// Renders helper methods for mapping from object / JSObject to the target type.
    /// </summary>
    /// <param name="depth"></param>
    /// <exception cref="TypeNotSupportedException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private void RenderObjectMappers(int depth)
    {
        if (!classInfo.Type.IsTSModule)
        {
            RenderFromObjectMapper(depth);
        }

        if (classInfo.IsSnapshotCompatible())
        {
            RenderFromJSObjectMapper(depth);
        }
        return;

        void RenderFromObjectMapper(int depth)
        {
            string indent = new(' ', depth * 4);
            string indent2 = new(' ', (depth + 1) * 4);

            sb.AppendLine($"{indent}public static {classInfo.Type.CLRTypeSyntax} {FromObjectMethodName}(object obj)");
            sb.AppendLine($"{indent}{{");
            sb.AppendLine($"{indent2}return obj switch");
            sb.AppendLine($"{indent2}{{");
            sb.AppendLine($"{indent2}    {classInfo.Type.CLRTypeSyntax} instance => instance,");
            if (classInfo.IsSnapshotCompatible())
            {
                sb.AppendLine($"{indent2}    JSObject jsObj => {FromJSObjectMethodName}(jsObj),");
            }
            sb.AppendLine($"{indent2}    _ => throw new ArgumentException($\"Invalid object type {{obj?.GetType().ToString() ?? \"null\"}}\", nameof(obj)),");
            sb.AppendLine($"{indent2}}};");
            sb.AppendLine($"{indent}}}");
        }

        void RenderFromJSObjectMapper(int depth)
        {
            string indent = new(' ', depth * 4);
            string indent2 = new(' ', (depth + 1) * 4);
            string indent3 = new(' ', (depth + 2) * 4);

            sb.AppendLine($"{indent}public static {classInfo.Type.CLRTypeSyntax} {FromJSObjectMethodName}(JSObject jsObject)");
            sb.AppendLine($"{indent}{{");

            PropertyInfo[] propertiesInMapper = [.. classInfo.Properties.Where(p => p.Type.IsSnapshotCompatible && p.SetMethod != null)];
            // Converting task types requires variable assignments, write those first, keep dict for assignments in initializer
            Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> propertyToConvertedVarDict = RenderNonInlinableTypeConversions(depth + 1, propertiesInMapper);

            sb.AppendLine($"{indent2}return new()"); // initializer body
            sb.AppendLine($"{indent2}{{");
            // TODO: support init properties
            foreach (PropertyInfo propertyInfo in propertiesInMapper)
            {
                if (propertyToConvertedVarDict.TryGetValue(propertyInfo, out TypeConversionExpressionRenderDelegate? expressionRenderer))
                {
                    sb.Append($"{indent3}{propertyInfo.Name} = ");
                    expressionRenderer.Render();
                }
                else if (propertyInfo.Type.RequiresCLRTypeConversion)
                {
                    string propertyRetrievalExpression = $"jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")";
                    sb.Append($"{indent3}{propertyInfo.Name} = ");
                    RenderInlineTypeConversion(propertyInfo.Type, propertyRetrievalExpression);
                }
                else
                {
                    sb.Append($"{indent3}{propertyInfo.Name} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")"); // TODO: error handling? (null / missing props?)
                }
                sb.AppendLine(",");
            }
            sb.AppendLine($"{indent2}}};");
            sb.AppendLine($"{indent}}}");
        }

        Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> RenderNonInlinableTypeConversions(int depth, PropertyInfo[] propertiesInMapper)
        {
            string indent = new(' ', depth * 4);
            Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> convertedTaskExpressionDict = new();
            foreach (PropertyInfo propertyInfo in propertiesInMapper)
            {
                if (propertyInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    sb.AppendLine($"{indent}var {tmpVarName} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = RenderNullableTaskTypeConversion(depth, propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => sb.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsTaskType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    sb.AppendLine($"{indent}var {tmpVarName} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = RenderTaskTypeConversion(depth, propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => sb.Append(convertedTaskExpression)));
                } 
                else if (propertyInfo.Type is { IsNullableType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    sb.AppendLine($"{indent}var {tmpVarName} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => RenderInlineTypeConversion(propertyInfo.Type, tmpVarName)));
                }
            }

            return convertedTaskExpressionDict;
        }

        static string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
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
    private class TypeConversionExpressionRenderDelegate(Action renderAction)
    {
        internal void Render() => renderAction();

        public static implicit operator TypeConversionExpressionRenderDelegate(Action renderAction)
        {
            return new TypeConversionExpressionRenderDelegate(renderAction);
        }
    }
}
