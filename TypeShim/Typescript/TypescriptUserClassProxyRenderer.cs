using System.Text;
using TypeShim.Parsing;

namespace DotnetWasmTypescript.InteropGenerator.Typescript;

internal class TypescriptUserClassProxyRenderer(ClassInfo classInfo, TypeScriptMethodRenderer methodRenderer, TypescriptClassNameBuilder classNameBuilder)
{
    //TODO: render proxy class implementing the interface rendered by TypescriptUserClassInterfaceRenderer

    // PURPOSE:
    // - glue between interop interface for a single class instance, enabling dynamic method invocation

    // CONSTRUCTOR:
    // - constructor takes ref to managedObject (js runtime) to pass as instance parameter to interop calls
    // --- IF the original class has non-static methods
    // - contructor takes ref to exports interface by TypescriptWasmExportsInterfaceClassInfoRenderer

    private readonly StringBuilder sb = new();

    internal string Render()
    {
        string proxyClassName = classNameBuilder.GetUserClassProxyName(classInfo);
        string interopInterfaceName = classNameBuilder.GetModuleInteropClassName();
        RenderProxyClass(proxyClassName, interopInterfaceName, classInfo.Methods.Where(m => !m.IsStatic));


        string staticsClassName = classNameBuilder.GetUserClassStaticsName(classInfo);
        RenderStaticsClass(staticsClassName, interopInterfaceName, classInfo.Methods.Where(m => m.IsStatic));
        return sb.ToString();
    }

    private void RenderStaticsClass(string className, string interopInterfaceName, IEnumerable<MethodInfo> methods)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript statics class. Source class: {classInfo.Namespace}.{classInfo.Name}");
        
        sb.AppendLine($"export class {className} {{");
        sb.AppendLine($"{indent}private interop: {interopInterfaceName};");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();
        foreach (MethodInfo methodInfo in methods)
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignature(methodInfo)} {{");
            RenderInteropInvocation(indent, methodInfo);

            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }
        sb.AppendLine($"}}");
    }

    private void RenderInteropInvocation(string indent, MethodInfo methodInfo)
    {
        if (classNameBuilder.GetUserClassProxyNameForReturnType(methodInfo) is string proxyClassName) // user class return type, wrap in proxy
        {
            sb.AppendLine($"{indent}{indent}return new {classNameBuilder.GetUserClassProxyNameForReturnType(methodInfo)}(this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({methodRenderer.RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance")}), this.interop);");
        }
        else // primitive return type or void
        {
            string optionalReturn = methodInfo.ReturnKnownType == KnownManagedType.Void ? string.Empty : "return ";
            sb.AppendLine($"{indent}{indent}{optionalReturn}this.interop.{ResolveInteropMethodAccessor(classInfo, methodInfo)}({methodRenderer.RenderMethodCallParametersWithInstanceParameterExpression(methodInfo, "this.instance")});");
        }
    }

    private void RenderProxyClass(string className, string interopInterfaceName, IEnumerable<MethodInfo> methods)
    {
        string indent = "  ";
        sb.AppendLine($"// Auto-generated TypeScript proxy class. Source class: {classInfo.Namespace}.{classInfo.Name}");

        sb.AppendLine($"export class {className} implements {classInfo.Name} {{");
        sb.AppendLine($"{indent}private interop: {interopInterfaceName};");
        sb.AppendLine($"{indent}private instance: object;");
        sb.AppendLine();
        sb.AppendLine($"{indent}constructor(instance: object, interop: {interopInterfaceName}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}{indent}this.instance = instance;");
        sb.AppendLine($"{indent}}}");
        sb.AppendLine();
        foreach (MethodInfo methodInfo in methods)
        {
            sb.AppendLine($"{indent}public {methodRenderer.RenderMethodSignature(methodInfo.WithoutInstanceParameter())} {{"); // skip instance parameter, its provided by the proxy class
            RenderInteropInvocation(indent, methodInfo);
            sb.AppendLine($"{indent}}}");
            sb.AppendLine();
        }
        sb.AppendLine($"}}");
    }

    private string ResolveInteropMethodAccessor(ClassInfo classInfo, MethodInfo methodInfo)
    {
        return $"{classInfo.Namespace}.{classNameBuilder.GetInteropInterfaceName(classInfo)}.{methodInfo.Name}";
    }
}
