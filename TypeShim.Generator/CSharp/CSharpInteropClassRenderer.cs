using Microsoft.CodeAnalysis;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo classInfo;

    private readonly StringBuilder sb = new();

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

    internal string Render()
    {
        sb.AppendLine("// Auto-generated TypeScript interop definitions");
        sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine($"namespace {classInfo.Namespace};");
        sb.AppendLine($"public partial class {GetInteropClassName(classInfo.Name)}");
        sb.AppendLine("{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            RenderMethod(methodInfo);
            foreach (MethodOverloadInfo overloadInfo in methodInfo.Overloads)
            {
                RenderMethod(methodInfo, overloadInfo);
            }
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            RenderPropertyMethods(propertyInfo);
        }
        if (classInfo.IsSnapshotCompatible())
        {
            RenderFromJSObjectMapper(depth: 1);
        }
        
        sb.AppendLine("}");
        return sb.ToString();
    }

    private void RenderPropertyMethods(PropertyInfo propertyInfo)
    {
        RenderProperty(propertyInfo, getter: true);
        if (propertyInfo.SetMethod is not null)
        {
            RenderProperty(propertyInfo, getter: false);
        }

        if (propertyInfo.Type.IsSnapshotCompatible)
        {
            // adapt property info to make a set method that takes JSObject and maps to the existing set method
            // TODO BEEPBOOP
        }
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
        foreach (MethodParameterInfo paramInfo in methodInfo.MethodParameters)
        {
            RenderParameterTypeConversion(depth: 2, paramInfo, overloadParamInfo: null);
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

    private void RenderMethod(MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
    {
        string indent = new(' ', 4);
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(depth: 1, methodInfo, overloadInfo);

        sb.Append(indent);
        sb.AppendLine("{");

        IEnumerable<MethodParameterInfo?> overloadParams = overloadInfo?.MethodParameters ?? Enumerable.Repeat<MethodParameterInfo?>(null, methodInfo.MethodParameters.Count);
        foreach ((MethodParameterInfo originalParamInfo, MethodParameterInfo ? overloadParamInfo) in methodInfo.MethodParameters.Zip(overloadParams))
        {
            RenderParameterTypeConversion(depth: 2, originalParamInfo, overloadParamInfo);
        }
        RenderUserMethodInvocation(methodInfo, depth: 2);
        sb.Append(indent);
        sb.AppendLine("}");
    }

    private void RenderMethodSignature(int depth, MethodInfo methodInfo, MethodOverloadInfo? overloadInfo = null)
    {
        string indent = new(' ', depth * 4);
        sb.Append(indent);
        sb.Append("public static ");
        if (methodInfo.ReturnType.IsTaskType)
        {
            sb.Append("async ");
        }

        sb.Append(methodInfo.ReturnType.InteropTypeSyntax);
        sb.Append(' ');
        sb.Append(overloadInfo?.Name ?? methodInfo.Name);
        sb.Append('(');
        RenderMethodParameters(overloadInfo?.MethodParameters ?? methodInfo.MethodParameters);
        sb.Append(')');
        sb.AppendLine();
    }

    private void RenderUserMethodInvocation(MethodInfo methodInfo, int depth)
    {
        string indent = new(' ', depth * 4);

        sb.Append(indent);
        if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
        {
            sb.Append("return ");
        }
        if (methodInfo.ReturnType.IsTaskType)
        {
            sb.Append("await ");
        }
        if (!methodInfo.IsStatic)
        {
            MethodParameterInfo instanceParam = methodInfo.MethodParameters.ElementAt(0);
            List<MethodParameterInfo> memberParams = [.. methodInfo.MethodParameters.Skip(1)];
            sb.Append($"{GetTypedParameterName(instanceParam)}.{methodInfo.Name}({string.Join(", ", memberParams.Select(GetTypedParameterName))})");
        }
        else
        {
            sb.Append($"{classInfo.Name}.{methodInfo.Name}({string.Join(", ", methodInfo.MethodParameters.Select(GetTypedParameterName))})");
        }
        sb.AppendLine(";");
    }

    private string GetTypedParameterName(MethodParameterInfo paramInfo) => paramInfo.Type.RequiresCLRTypeConversion ? $"typed_{paramInfo.Name}" : paramInfo.Name;

    private void RenderParameterTypeConversion(int depth, MethodParameterInfo originalParamInfo, MethodParameterInfo? overloadParamInfo = null)
    {
        if (!originalParamInfo.Type.RequiresCLRTypeConversion)
            return;

        string indent = new(' ', depth * 4);

        if (originalParamInfo.Type.IsTaskType)
        {
            //task.ContinueWith(t => {
            //    if (t.IsFaulted) taskTcs.SetException(t.Exception.InnerExceptions);
            //    else if (t.IsCanceled) taskTcs.SetCanceled();
            //    else taskTcs.SetResult((MyClass)t.Result);
            //}, TaskContinuationOptions.ExecuteSynchronously);

            //TODO: fix invalid void ternary
            InteropTypeInfo userParamTypeInfo = originalParamInfo.Type.TypeArgument ?? throw new InvalidOperationException("Task type parameter must have a type argument for conversion.");
            string tcsVarName = $"{originalParamInfo.Name}Tcs";
            sb.Append(indent);
            sb.AppendLine($"TaskCompletionSource<{userParamTypeInfo.CLRTypeSyntax}> {tcsVarName} = new();");
            sb.Append(indent);
            sb.AppendLine("task.ContinueWith(t =>");
            string lambdaIndent = new(' ', (depth + 1) * 4);
            sb.Append(lambdaIndent);
            sb.AppendLine($"t.IsFaulted ? {tcsVarName}.SetException(t.Exception.InnerExceptions)");
            sb.Append(lambdaIndent);
            sb.AppendLine($": t.IsCanceled ? {tcsVarName}.SetCanceled()");
            sb.Append(lambdaIndent);

            string resultConversionExpression = overloadParamInfo?.Type.TypeArgument?.ManagedType == KnownManagedType.JSObject
                ? $"{GetInteropClassName(userParamTypeInfo.CLRTypeSyntax.ToString())}.FromJSObject(t.Result)" // only typeshim overloads get JSObject conversion, user can also use jsobject, they'll want to handle it themselves
                : $"({userParamTypeInfo.CLRTypeSyntax})t.Result";
            sb.AppendLine($": {tcsVarName}.SetResult({resultConversionExpression}), TaskContinuationOptions.ExecuteSynchronously);");
            sb.Append(indent);
            sb.AppendLine($"Task<{userParamTypeInfo.CLRTypeSyntax}> {GetTypedParameterName(originalParamInfo)} = {tcsVarName}.Task;");
        }
        else if (overloadParamInfo?.Type.ManagedType == KnownManagedType.JSObject)
        {
            InteropTypeInfo targetType = originalParamInfo.Type.TypeArgument ?? originalParamInfo.Type;
            string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());
            sb.AppendLine($"{indent}{targetType.CLRTypeSyntax} {GetTypedParameterName(originalParamInfo)} = {targetInteropClass}.FromJSObject({originalParamInfo.Name});");
        }
        else
        {
            Debug.Assert(originalParamInfo.Type.ManagedType is KnownManagedType.Object or KnownManagedType.Array, "Unexpected non-object or non-array type with required type conversion");
            sb.Append(indent);
            // covariance can be assumed here for objects (and arrays)
            sb.AppendLine($"{originalParamInfo.Type.CLRTypeSyntax} {GetTypedParameterName(originalParamInfo)} = ({originalParamInfo.Type.CLRTypeSyntax}){originalParamInfo.Name};");
        }
    }

    private void RenderMethodParameters(IEnumerable<MethodParameterInfo> parameterInfos)
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

    private void RenderFromJSObjectMapper(int depth)
    {
        // available methods:
        //obj.GetPropertyAsBoolean("a");
        //obj.GetPropertyAsDouble("b");
        //obj.GetPropertyAsString("c");
        //obj.GetPropertyAsInt32("d");
        //obj.GetPropertyAsJSObject("e");
        //obj.GetPropertyAsByteArray("f");

        // not an interop method, but used by interop methods to convert from JSObject to CLR type

        string indent = new(' ', depth * 4);
        string indent2 = new(' ', (depth + 1) * 4);
        string indent3 = new(' ', (depth + 2) * 4);

        sb.AppendLine($"{indent}public static {classInfo.Type.CLRTypeSyntax} FromJSObject(JSObject jsObject)");
        sb.AppendLine($"{indent}{{");
        sb.AppendLine($"{indent2}return new() {{");
        // initializer body
        // TODO: support init properties
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.Type.IsSnapshotCompatible && p.SetMethod != null))
        {
            if (propertyInfo.Type.IsArrayType)
            {
                sb.AppendLine($"{indent3}{propertyInfo.Name} = [],//MarshallAs{propertyInfo.Name}(jsObject.GetPropertyAsJSObject(\"{propertyInfo.Name}\")),");
            }
            else if (propertyInfo.Type.RequiresCLRTypeConversion)
            {
                InteropTypeInfo targetType = propertyInfo.Type.TypeArgument ?? propertyInfo.Type;
                string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());
                sb.AppendLine($"{indent3}{propertyInfo.Name} = {targetInteropClass}.FromJSObject(jsObject.GetPropertyAsJSObject(\"{propertyInfo.Name}\")),");
            }
            else if (propertyInfo.Type.IsTaskType)
            {
                throw new TypeNotSupportedException("Task types are not supported in FromJSObject conversion.");
                //sb.AppendLine($"{indent3}{propertyInfo.Name} = [],//MarshallAs{propertyInfo.Name}(jsObject.GetPropertyAsJSObject(\"{propertyInfo.Name}\")),");
            }
            else
            {
                sb.AppendLine($"{indent3}{propertyInfo.Name} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\"),"); // BEEPBOOP TODO: error handling? (null / missing props?)
            }
        }
        sb.AppendLine($"{indent2}}};");
        sb.AppendLine($"{indent}}}");

        static string ResolveJSObjectMethodName(InteropTypeInfo typeInfo)
        {
            return typeInfo.ManagedType switch
            {
                KnownManagedType.Boolean => "GetPropertyAsBoolean",
                KnownManagedType.Double => "GetPropertyAsDouble",
                KnownManagedType.String => "GetPropertyAsString",
                KnownManagedType.Int32 => "GetPropertyAsInt32",
                KnownManagedType.JSObject => "GetPropertyAsJSObject",
                //KnownManagedType.ByteArray => "GetPropertyAsByteArray",
                _ => throw new InvalidOperationException($"Unsupported type for JSObject conversion: {typeInfo.ManagedType}"),
            };
        }
    }
}