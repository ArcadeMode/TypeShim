using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo classInfo;

    public CSharpInteropClassRenderer(ClassInfo classInfo)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        if (!classInfo.Methods.Any())
        {
            throw new ArgumentException("Interop class must have at least one method to render.", nameof(classInfo));
        }
        this.classInfo = classInfo;
    }

    internal string Render()
    {
        StringBuilder sb = new();
        sb.AppendLine("// Auto-generated TypeScript interop definitions");
        sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine($@"namespace {classInfo.Namespace};");
        sb.AppendLine($"public partial class {classInfo.Name}Interop");
        sb.AppendLine("{");
        foreach (MethodInfo methodInfo in classInfo.Methods)
        {
            sb.AppendLine(Render(methodInfo));
        }
        sb.Length -= 2; // Remove last newline
        sb.AppendLine("}");
        return sb.ToString();
    }

    private string Render(MethodInfo methodInfo)
    {
        StringBuilder sb = new();
        sb.AppendLine("    [JSExport]");
        bool async = false;
        if (methodInfo.ReturnType.ManagedType is KnownManagedType.Object)
        {
            sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
        }
        if (methodInfo.ReturnType.ManagedType == KnownManagedType.Task) {
            sb.AppendLine("    [return: JSMarshalAs<JSType.Promise<JSType.Any>>]");
            async = true;
        }
        if (methodInfo.ReturnType.ManagedType == KnownManagedType.Array) {
            sb.AppendLine("    [return: JSMarshalAs<JSType.Array<JSType.Any>>]");
        }

        List<MethodParameterInfo> methodParameters = [.. methodInfo.MethodParameters];
        // TODO: refactor, async is hacky.

        sb.AppendLine($"    public static {(async ? "async " : "")}{methodInfo.ReturnType.InteropTypeSyntax} {methodInfo.Name}({Render(methodParameters)})");
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

        if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
        {
            sb.AppendLine($"        return {(async ? "await " : "")}{methodInvocation};");
        }
        else
        {
            sb.AppendLine($"       {(async ? "await " : "")}{methodInvocation};");
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