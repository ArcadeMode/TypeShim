using Microsoft.CodeAnalysis;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeShim.Shared;
using TypeShim.Generator.Parsing;

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
        RenderProperty(propertyInfo, getter: true);
        if (propertyInfo.SetMethod is null)
            return;
        RenderProperty(propertyInfo, getter: false);
    }

    private void RenderProperty(PropertyInfo propertyInfo, bool getter)
    {
        MethodInfo methodInfo = getter ? propertyInfo.GetMethod : propertyInfo.SetMethod ?? throw new InvalidOperationException("RenderProperty called for setter with null SetMethod");

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

        string accessorName = methodInfo.IsStatic ? classInfo.Name : GetTypedParameterName(methodInfo.MethodParameters.ElementAt(0));
        if (getter)
        {
            sb.AppendLine($"{indent}{indent}return {accessorName}.{propertyInfo.Name};");
        }
        else
        {
            string valueVarName = GetTypedParameterName(methodInfo.MethodParameters.ElementAt(1));
            sb.AppendLine($"{indent}{indent}{accessorName}.{propertyInfo.Name} = {valueVarName};");
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
        if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            sb.Append(indent);
            sb.AppendLine($"{methodInfo.ReturnType.CLRTypeSyntax} result = {GetInvocationExpression()};");
            string convertedTaskExpression = RenderTaskTypeConversion(depth, methodInfo.ReturnType.AsInteropTypeInfo(), "result");
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
        if (parameterInfo.Type.IsTaskType)
        {
            string convertedTaskExpression = RenderTaskTypeConversion(depth, parameterInfo.Type, parameterInfo.Name);
            sb.Append(indent);
            sb.AppendLine($"Task<{parameterInfo.Type.TypeArgument!.CLRTypeSyntax}> {GetTypedParameterName(parameterInfo)} = {convertedTaskExpression};");
            return; // task pattern differs from other conversions, hence its fully separated rendering.
        }

        sb.Append($"{indent}{parameterInfo.Type.CLRTypeSyntax} {GetTypedParameterName(parameterInfo)} = ");
        if (parameterInfo.IsInjectedInstanceParameter)
        {
            RenderCovariantTypeConversion(parameterInfo.Type, parameterInfo.Name);
        }
        else if (parameterInfo.Type.IsArrayType)
        {
            RenderArrayTypeConversion(parameterInfo.Type, parameterInfo.Name);
        }
        else if (parameterInfo.Type.ManagedType is KnownManagedType.Object)
        {
            //TODO: make nullables actually ManagedType.Nullable with inner type Object or T, currently nullable reflects inner type, which is confusing sometimes
            RenderFromObjectTypeConversion(parameterInfo.Type, parameterInfo.Name);
        }
        else // Tests guard against this case. Anyway, here is a state-of-the-art regression detector.
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {parameterInfo.Type.CLRTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
        sb.AppendLine(";");
    }

    private void RenderCovariantTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType is KnownManagedType.Object or KnownManagedType.Array, "Unexpected non-object or non-array type with required type conversion");
        sb.Append($"({typeInfo.CLRTypeSyntax}){parameterName}");
    }

    private void RenderFromObjectTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        InteropTypeInfo targetType = typeInfo.TypeArgument ?? typeInfo; // unwrap nullable or use simple type directly
        string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());

        if (typeInfo.IsNullableType)
        {
            sb.Append($"{parameterName} != null ? ");
        }
        
        if (typeInfo.IsTSExport)
        {
            sb.Append($"{targetInteropClass}.{FromObjectMethodName}({parameterName})");
        }
        else
        {
            RenderCovariantTypeConversion(typeInfo, parameterName);
        }

        if (typeInfo.IsNullableType)
        {
            sb.Append(" : null");
        }
    }

    private void RenderArrayTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.TypeArgument != null, "Array type must have a type argument.");

        if (typeInfo.TypeArgument.IsTSExport == false)
        {
            RenderCovariantTypeConversion(typeInfo, parameterName);
            return; // early return for non-exported types, no special conversion possible
        }

        sb.Append($"Array.ConvertAll({parameterName}, e => ");
        RenderFromObjectTypeConversion(typeInfo.TypeArgument, "e");
        sb.Append(')');
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <param name="depth"></param>
    /// <param name="targetTaskType"></param>
    /// <param name="sourceVarName"></param>
    /// <param name="convertedVarName"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string RenderTaskTypeConversion(int depth, InteropTypeInfo targetTaskType, string sourceVarName)
    {
        string indent = new(' ', depth * 4);
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type parameter must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        sb.Append(indent);
        sb.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CLRTypeSyntax}> {tcsVarName} = new();");
        sb.Append(indent);
        sb.AppendLine($"{sourceVarName}.ContinueWith(t => {{");
        string lambdaIndent = new(' ', (depth + 1) * 4);
        sb.Append(lambdaIndent);
        sb.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
        sb.Append(lambdaIndent);
        sb.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");

        string resultConversionExpression = taskTypeParamInfo.IsTSExport || taskTypeParamInfo.IsTSModule
            ? $"{GetInteropClassName(taskTypeParamInfo.CLRTypeSyntax.ToString())}.{FromObjectMethodName}(t.Result)"
            : $"({taskTypeParamInfo.CLRTypeSyntax})t.Result";

        sb.Append(lambdaIndent);
        sb.AppendLine($"else {tcsVarName}.SetResult({resultConversionExpression});");
        sb.Append(indent);
        sb.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
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
            sb.AppendLine($"{indent2}return new() {{"); // initializer body

            // TODO: support init properties
            foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.Type.IsSnapshotCompatible && p.SetMethod != null))
            {
                if (propertyInfo.Type.RequiresCLRTypeConversion)
                {
                    InteropTypeInfo targetType = propertyInfo.Type.TypeArgument ?? propertyInfo.Type;
                    string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());
                    sb.Append($"{indent3}{propertyInfo.Name} = ");
                    string propertyRetrievalExpression = $"jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")";
                    if (propertyInfo.Type.IsArrayType)
                    {
                        sb.AppendLine($"Array.ConvertAll({propertyRetrievalExpression}, {targetInteropClass}.{FromJSObjectMethodName}),");
                    }
                    else if (propertyInfo.Type.IsTaskType)
                    {
                        // TODO: retrieve task object through new JSObjectExtensions

                        // TODO: IF JSObject inner type, inject the taskcompletion trick. else preserve readily marshalled task?
                        //RenderTaskTypeConversion(depth, propertyInfo.SetMethod!.MethodParameters.Single());
                        throw new TypeNotSupportedException("Task types are not (yet) supported in FromJSObject conversion.");
                    }
                    else
                    {
                        sb.AppendLine($"{targetInteropClass}.{FromJSObjectMethodName}({propertyRetrievalExpression}),");
                    }
                }
                else
                {
                    sb.AppendLine($"{indent3}{propertyInfo.Name} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\"),"); // TODO: error handling? (null / missing props?)
                }
            }
            sb.AppendLine($"{indent2}}};");
            sb.AppendLine($"{indent}}}");
        }

        static string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
        {
            return typeInfo.ManagedType switch
            {
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
                    _ => throw new InvalidOperationException($"Array of type {typeInfo.TypeArgument?.ManagedType} cannot be marshalled by TypeShim JSObject extensions"),
                },
                _ => throw new InvalidOperationException($"Type {typeInfo.ManagedType} cannot be marshalled by JSObject"),
            };
        }
    }
}