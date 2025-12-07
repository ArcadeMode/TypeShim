using Microsoft.CodeAnalysis;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
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

    internal string Render()
    {
        sb.AppendLine("// Auto-generated TypeScript interop definitions");
        sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine($"namespace {classInfo.Namespace};");
        sb.AppendLine($"public partial class {classInfo.Name}Interop");
        sb.AppendLine("{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            RenderMethod(methodInfo);
        }
        foreach (PropertyInfo propertyInfo in classInfo.Properties)
        {
            RenderPropertyMethods(propertyInfo);
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

    private void RenderMethod(MethodInfo methodInfo)
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
        foreach (MethodParameterInfo paramInfo in methodInfo.MethodParameters)
        {
            RenderParameterTypeConversion(paramInfo, depth: 2);
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

    private string GetTypedParameterName(MethodParameterInfo paramInfo) => paramInfo.Type.RequiresCLRTypeConversion ? $"typed_{paramInfo.ParameterName}" : paramInfo.ParameterName;

    private void RenderParameterTypeConversion(MethodParameterInfo paramInfo, int depth)
    {
        if (!paramInfo.Type.RequiresCLRTypeConversion)
            return;

        string indent = new(' ', depth * 4);

        if (paramInfo.Type.IsTaskType)
        {
            InteropTypeInfo returnTypeInfo = paramInfo.Type.TypeArgument ?? throw new InvalidOperationException("Task type parameter must have a type argument for conversion.");
            string tcsVarName = $"{paramInfo.ParameterName}Tcs";
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
            sb.AppendLine($": {tcsVarName}.SetResult(({returnTypeInfo.CLRTypeSyntax})t.Result), TaskContinuationOptions.ExecuteSynchronously);");
            sb.Append(indent);
            sb.AppendLine($"Task<{returnTypeInfo.CLRTypeSyntax}> {GetTypedParameterName(paramInfo)} = {tcsVarName}.Task;");
        }
        //else if (paramInfo.Type.IsArrayType) {} // TODO: write some tests to see array type conversion + run in sample / integrations
        else
        {
            Debug.Assert(paramInfo.Type.ManagedType == KnownManagedType.Object, "Unexpected non-object type with required type conversion");
            sb.Append(indent);
            sb.AppendLine($"{paramInfo.Type.CLRTypeSyntax} {GetTypedParameterName(paramInfo)} = ({paramInfo.Type.CLRTypeSyntax}){paramInfo.ParameterName};");
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
            sb.Append(parameterInfo.ParameterName);
            sb.Append(", ");
        }
        sb.Length -= 2; // Remove last ", "
    }
}