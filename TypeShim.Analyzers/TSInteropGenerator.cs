using System;
using System.Collections.Generic;
using System.Text;

namespace TypeScriptExportGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using TypeScriptExport;

[Generator]
public class TSInteropGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("TestDebug.g.cs", SourceText.From("namespace TestDebug { public class Dummy {} }", Encoding.UTF8));
        });

        // Find all class declarations in the project
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node)
            .Where(static c => c != null);

        // Combine class declarations with the Compilation to get semantic info (attributes)
        IncrementalValueProvider<ImmutableArray<ISymbol?>> exportedClasses = context.CompilationProvider
            .Combine(classDeclarations.Collect())
            .Select((pair, _) =>
            {
                var (compilation, classDecls) = (pair.Left, pair.Right);
                var marked = classDecls
                    .Select(c => compilation.GetSemanticModel(c.SyntaxTree).GetDeclaredSymbol(c))
                    .ToImmutableArray();
                return marked;
            });

        // Output: the TS code in a .g.cs file
        context.RegisterSourceOutput(exportedClasses, (spc, symbols) =>
        {
            //System.Diagnostics.Debugger.Launch();

            IEnumerable<INamedTypeSymbol> namedSymbolsWithAttribute = symbols
                .Where(sym => sym != null && sym.GetAttributes().Any(attr => attr.AttributeClass?.Name == "TsExportAttribute"))
                .Cast<INamedTypeSymbol>();

            var sb = new StringBuilder();
            foreach (INamedTypeSymbol sym in namedSymbolsWithAttribute)
            {
                DiagnosticDescriptor descriptor = new("TSGEN01", "Type info", "Found class: {0}", "TsExport", DiagnosticSeverity.Info, true);
                spc.ReportDiagnostic(Diagnostic.Create(descriptor, Location.None, sym.Name));

                if (GetInteropClass(spc, sym) is SourceText tsSource)
                {
                    spc.AddSource($"{sym.Name}Interop.g.ts", tsSource);
                }
            }
        });
    }

    private static SourceText? GetInteropClass(SourceProductionContext spc, INamedTypeSymbol classSymbol)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated TypeScript interop definitions");
        sb.AppendLine("using System.Runtime.InteropServices.JavaScript;");
        sb.AppendLine("using TypeScriptExport;");
        
        
        if (classSymbol.ContainingNamespace?.ToDisplayString() is not string nsName)
        {
            spc.ReportDiagnostic(Diagnostic.Create(
                new DiagnosticDescriptor("TSGEN02", "Namespace info", "Type {0} must be contained in a namespace", "TsExport", DiagnosticSeverity.Error, true),
                classSymbol.Locations.FirstOrDefault() ?? Location.None,
                classSymbol.Name));
            return null;
        }
        sb.AppendLine($@"namespace {nsName};");

        sb.AppendLine($"public class {classSymbol.Name}Interop");
        sb.AppendLine("{");

        // TODO:
        // - PARAMETER TYPE MARSHALING FOR CUSTOMCLASSES
        // - 

        foreach (IMethodSymbol staticMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && m.IsStatic))
        {
            sb.AppendLine("    [JSExport]");

            if (IsCustomClass(staticMethod.ReturnType))
            {
                sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
            }

            string parameters = string.Join(", ", staticMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            sb.AppendLine($"    public static {staticMethod.ReturnType.ToDisplayString()} {staticMethod.Name}({parameters}) => {classSymbol.Name}.{staticMethod.Name}({string.Join(", ", staticMethod.Parameters.Select(p => p.Name))});");
        }
        
        foreach (IMethodSymbol memberMethod in classSymbol.GetMembers().OfType<IMethodSymbol>().Where(m => m.MethodKind == MethodKind.Ordinary && !m.IsStatic)) // todo dynamic members exported through interop type as static with instance parameter
        {

            sb.AppendLine("    [JSExport]");

            if (IsCustomClass(memberMethod.ReturnType))
            {
                sb.AppendLine("    [return: JSMarshalAs<JSType.Any>]");
            }

            string parameters = string.Join(", ", memberMethod.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
            string instanceParameter = $"[JSMarshalAs<JSType.Any>, TsExportAs<{classSymbol.Name}>] object instance";

            if (!string.IsNullOrEmpty(parameters))
            {
                parameters = $"{instanceParameter}, {parameters}";
            }
            else
            {
                parameters = instanceParameter;
            }

            sb.AppendLine($"    public static {memberMethod.ReturnType.ToDisplayString()} {memberMethod.Name}({parameters})");
            sb.AppendLine("    {");
            sb.AppendLine($"        {classSymbol.Name} typedInstance = ({classSymbol.Name})instance;");
            string memberInvocation = $"typedInstance.{memberMethod.Name}({string.Join(", ", memberMethod.Parameters.Select(p => p.Name))})";
            if (memberMethod.ReturnType.SpecialType != SpecialType.System_Void)
            {
                sb.AppendLine($"        return {memberInvocation};");
                sb.AppendLine("    }");
                continue;
            } else
            {
                sb.AppendLine($"        {memberInvocation};");
                sb.AppendLine("    }");
            }

                
        }

        sb.AppendLine("}");




        // property diagnostics??
        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.DeclaredAccessibility == Accessibility.Public)
            {
                spc.ReportDiagnostic(Diagnostic.Create(
                    new DiagnosticDescriptor("TSGEN03", "Property info", "Public properties are not exported to interop", "TsExport", DiagnosticSeverity.Info, true),
                    member.Locations.FirstOrDefault() ?? Location.None,
                    member.Name,
                    member.Type.Name));
            }
        }


        //sb.AppendLine("}");
        return SourceText.From(sb.ToString(), Encoding.UTF8);
    }

    private static bool IsCustomClass(ITypeSymbol type)
    {
        // Exclude primitives
        if (type.SpecialType != SpecialType.None)
            return false;

        // Exclude strings (not considered primitive in Roslyn)
        if (type is INamedTypeSymbol nts && nts.ToDisplayString() == "string")
            return false;

        // Exclude enums, delegates, interfaces, structs, arrays
        if (type.TypeKind != TypeKind.Class)
            return false;

        // Exclude common framework types by namespace (System.*, Microsoft.*)
        var ns = type.ContainingNamespace?.ToDisplayString();
        if (ns != null &&
            (ns.StartsWith("System.") || ns == "System" || ns.StartsWith("Microsoft.")))
            return false;

        // Optionally, exclude specific types (Task, Span, ReadOnlySpan)
        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        if (fullName.StartsWith("global::System.Threading.Tasks.Task") ||
            fullName.StartsWith("global::System.Span") ||
            fullName.StartsWith("global::System.ReadOnlySpan"))
            return false;

        // Optionally, restrict to only types defined in the user's assembly
        // if (type.ContainingAssembly?.Name == "YourProjectAssemblyName")
        //     return true;

        // If it passed all filters, treat as custom
        return true;
    }
}