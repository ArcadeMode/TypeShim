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
        foreach ((MethodInfo original, MethodInfo permutation) in classInfo.Methods.SelectMany(m => m.AllJSObjectParameterPermutations()))
        {
            RenderJSObjectMethod(permutation, original);
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            RenderPropertyMethods(propertyInfo);
        }
        if (classInfo.IsSnapshotCompatible())
        {
            RenderFromJSObjectHelper(depth: 1);
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

        RenderMethodSignature(methodInfo, depth: 1);

        sb.Append(indent);
        sb.AppendLine("{");
        foreach (MethodParameterInfo paramInfo in methodInfo.MethodParameters)
        {
            RenderParameterTypeConversion(paramInfo, depth: 2);
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

    //private void RenderMethod(MethodInfo methodInfo)
    //{
    //    string indent = new(' ', 4);
    //    JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
    //    sb.Append(indent);
    //    sb.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
    //    sb.Append(indent);
    //    sb.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

    //    RenderMethodSignature(methodInfo, depth: 1);

    //    sb.Append(indent);
    //    sb.AppendLine("{");
    //    foreach (MethodParameterInfo paramInfo in methodInfo.MethodParameters)
    //    {
    //        RenderParameterTypeConversion(paramInfo, depth: 2);
    //    }
    //    RenderUserMethodInvocation(methodInfo, depth: 2);
    //    sb.Append(indent);
    //    sb.AppendLine("}");
    //}

    private void RenderJSObjectMethod(MethodInfo methodInfo, MethodInfo originalMethodInfo)
    {
        string indent = new(' ', 4);
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        sb.Append(indent);
        sb.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo, depth: 1);

        sb.Append(indent);
        sb.AppendLine("{");
        foreach ((MethodParameterInfo paramInfo, MethodParameterInfo originalParamInfo) in methodInfo.MethodParameters.Zip(originalMethodInfo.MethodParameters))
        {
            RenderParameterTypeConversion(paramInfo, depth: 2, originalParamInfo);
        }
        RenderUserMethodInvocation(methodInfo, depth: 2);
        sb.Append(indent);
        sb.AppendLine("}");
    }

    private void RenderMethodSignature(MethodInfo methodInfo, int depth)
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
        sb.Append(methodInfo.Name);
        sb.Append('(');
        RenderMethodParameters(methodInfo.MethodParameters);
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

    private void RenderParameterTypeConversion(MethodParameterInfo paramInfo, int depth, MethodParameterInfo? originalParamInfo = null)
    {
        if (!paramInfo.Type.RequiresCLRTypeConversion)
            return;

        string indent = new(' ', depth * 4);

        if (paramInfo.Type.IsTaskType)
        {
            //task.ContinueWith(t => {
            //    if (t.IsFaulted) taskTcs.SetException(t.Exception.InnerExceptions);
            //    else if (t.IsCanceled) taskTcs.SetCanceled();
            //    else taskTcs.SetResult((MyClass)t.Result);
            //}, TaskContinuationOptions.ExecuteSynchronously);

            //BEEPBOOP
            //TODO: fix invalid void ternary
            InteropTypeInfo returnTypeInfo = paramInfo.Type.TypeArgument ?? throw new InvalidOperationException("Task type parameter must have a type argument for conversion.");
            string tcsVarName = $"{paramInfo.Name}Tcs";
            sb.Append(indent);
            sb.AppendLine($"TaskCompletionSource<{returnTypeInfo.CLRTypeSyntax}> {tcsVarName} = new();");
            sb.Append(indent);
            sb.AppendLine("task.ContinueWith(t =>");
            string lambdaIndent = new(' ', (depth + 1) * 4);
            sb.Append(lambdaIndent);
            sb.AppendLine($"t.IsFaulted ? {tcsVarName}.SetException(t.Exception.InnerExceptions)");
            sb.Append(lambdaIndent);
            sb.AppendLine($": t.IsCanceled ? {tcsVarName}.SetCanceled()");
            sb.Append(lambdaIndent);

            string resultConversion = returnTypeInfo.ManagedType == KnownManagedType.JSObject && originalParamInfo != null
                ? $"{GetInteropClassName(returnTypeInfo.CLRTypeSyntax.ToString())}.FromJSObject(t.Result)"
                : $"({returnTypeInfo.CLRTypeSyntax})t.Result";
            sb.AppendLine($": {tcsVarName}.SetResult({resultConversion}), TaskContinuationOptions.ExecuteSynchronously);");
            sb.Append(indent);
            sb.AppendLine($"Task<{returnTypeInfo.CLRTypeSyntax}> {GetTypedParameterName(paramInfo)} = {tcsVarName}.Task;");
        }
        else if (paramInfo.Type.ManagedType == KnownManagedType.JSObject)
        {
            if (originalParamInfo == null)
            {
                throw new InvalidOperationException("JSObject type parameter must have original parameter info for conversion.");
            }

            InteropTypeInfo targetType = originalParamInfo.Type.TypeArgument ?? originalParamInfo.Type;
            string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());
            sb.AppendLine($"{indent}{targetType.CLRTypeSyntax} {GetTypedParameterName(paramInfo)} = {targetInteropClass}.FromJSObject({paramInfo.Name});");
        }
        else
        {
            Debug.Assert(paramInfo.Type.ManagedType == KnownManagedType.Object, "Unexpected non-object type with required type conversion");
            sb.Append(indent);
            // covariance can be assumed here for objects (and arrays)
            sb.AppendLine($"{paramInfo.Type.CLRTypeSyntax} {GetTypedParameterName(paramInfo)} = ({paramInfo.Type.CLRTypeSyntax}){paramInfo.Name};");
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

    private void RenderFromJSObjectHelper(int depth)
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
        foreach (PropertyInfo propertyInfo in classInfo.Properties.Where(p => p.Type.IsSnapshotCompatible))
        {
            string propertyAssignment;
            if (propertyInfo.Type.RequiresCLRTypeConversion)
            {
                InteropTypeInfo targetType = propertyInfo.Type.TypeArgument ?? propertyInfo.Type;
                string targetInteropClass = GetInteropClassName(targetType.CLRTypeSyntax.ToString());
                propertyAssignment = $"{propertyInfo.Name} = {targetInteropClass}.FromJSObject(jsObject.GetPropertyAsJSObject(\"{propertyInfo.Name}\"))";
            }
            else
            {
                propertyAssignment = $"{propertyInfo.Name} = jsObject.{ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")"; // BEEPBOOP TODO: error handling? (null / missing props?)
            }
            sb.AppendLine($"{indent3}{propertyAssignment},");
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