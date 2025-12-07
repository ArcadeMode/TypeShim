using System;
using System.Collections.Generic;
using System.Text;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.Typescript;

/// <summary>
/// Renders the 'Module' provided to the user. Ties together the object references + interop interface + class proxies.
/// 
/// Provides the user's static methods as methods on the module class.
/// </summary>
internal class TypescriptUserModuleClassRenderer(ModuleInfo moduleInfo, TypescriptClassNameBuilder classNameBuilder)
{
    private readonly StringBuilder sb = new();

    internal string Render()
    {
        string indent = "  ";
        sb.AppendLine($"export class {classNameBuilder.GetModuleClassName()} {{");
        sb.AppendLine($"{indent}private interop: {classNameBuilder.GetModuleInteropClassName()}");
        sb.AppendLine($"{indent}constructor(interop: {classNameBuilder.GetModuleInteropClassName()}) {{");
        sb.AppendLine($"{indent}{indent}this.interop = interop;");
        sb.AppendLine($"{indent}}}");

        // Render static methods for each exported class
        // TODO: add private-class-like structure to deal with duplicate class names across namespaces
        // so not module.ClassA but module.Namespace.Sub.ClassA()
        // TODO: swap to properties instead of methods to drop unnecessary parentheses
        foreach (ClassInfo classInfo in moduleInfo.ExportedClasses.Where(c => c.Methods.Any(m => m.IsStatic) || c.Properties.Any(p => p.IsStatic)))
        {
            string staticsClassName = classNameBuilder.GetUserClassStaticsName(classInfo);
            sb.AppendLine($"{indent}public get {classInfo.Name}(): {staticsClassName} {{");
            sb.AppendLine($"{indent}{indent}return new {staticsClassName}(this.interop);");
            sb.AppendLine($"{indent}}}");
        }

        sb.AppendLine($"}}");
        return sb.ToString();
    }
}
