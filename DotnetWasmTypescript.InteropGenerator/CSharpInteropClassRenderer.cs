using System.Text;

namespace DotnetWasmTypescript.InteropGenerator;

internal sealed class CSharpInteropClassRenderer(ClassInfo classInfo)
{
    internal string Render()
    {
        StringBuilder sb = new();
        sb.AppendLine("// Auto-generated TypeScript interop definitions");
        sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine($@"namespace {classInfo.Namespace};");
        sb.AppendLine($"public partial class {classInfo.Name}Interop");
        sb.AppendLine("{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            MethodInfoBuilder builder = new(null!, null!);
            sb.AppendLine(Render(methodInfo));
        }
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string Render(MethodInfo methodInfo)
    {
        StringBuilder sb = new();
        sb.AppendLine("    [JSExport]");
        if (methodInfo.ReturnKnownType == KnownManagedType.Object)
        {
            sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
        }

        List<MethodParameterInfo> methodParameters = [.. methodInfo.MethodParameters];

        sb.AppendLine($"    public static {methodInfo.ReturnInteropTypeSyntax} {methodInfo.Name}({Render(methodParameters)})");
        sb.AppendLine("    {");

        // Cast object-typed parameters to their original types
        foreach (MethodParameterInfo paramInfo in methodParameters)
        {
            if (paramInfo.KnownType is KnownManagedType.Object or KnownManagedType.Array)
            {
                sb.AppendLine($"        {paramInfo.CLRTypeSyntax} {paramInfo.GetTypedParameterName()} = ({paramInfo.CLRTypeSyntax}){paramInfo.ParameterName};");
            }
        }

        string methodInvocation;
        if (!methodInfo.IsStatic)
        {
            MethodParameterInfo instanceParam = methodParameters[0];
            List<MethodParameterInfo> memberParams = methodParameters[1..];
            methodInvocation = $"{instanceParam.GetTypedParameterName()}.{methodInfo.Name}({string.Join(", ", memberParams.Select(p => p.GetTypedParameterName()))})";
        }
        else
        {
            List<MethodParameterInfo> memberParams = methodParameters; // for static methods, all parameters are member parameters
            methodInvocation = $"{classInfo.Name}.{methodInfo.Name}({string.Join(", ", memberParams.Select(p => p.GetTypedParameterName()))})";
        }

        if (methodInfo.ReturnKnownType != KnownManagedType.Void)
        {
            sb.AppendLine($"        return {methodInvocation};");
        }
        else
        {
            sb.AppendLine($"        {methodInvocation};");
        }
        sb.AppendLine("    }");
        return sb.ToString();
    }

    private string Render(IEnumerable<MethodParameterInfo> parameterInfos)
    {
        StringBuilder codeBuilder = new();
        foreach (MethodParameterInfo parameterInfo in parameterInfos)
        {
            if (parameterInfo.KnownType is KnownManagedType.Object)
            {
                codeBuilder.Append("[JSMarshalAs<JSType.Any>] ");
            }
            codeBuilder.Append($"{parameterInfo.InteropTypeSyntax} {parameterInfo.ParameterName}, ");
        }
        return codeBuilder.ToString().TrimEnd().TrimEnd(',');
    }
}