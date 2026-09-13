using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo _classInfo;
    private readonly RenderContext _ctx;
    private readonly JSObjectMethodResolver _methodResolver;
    private readonly CSharpTypeConversionRenderer _conversionRenderer;
    private readonly CSharpMethodRenderer _methodRenderer;

    public CSharpInteropClassRenderer(ClassInfo classInfo, RenderContext context, JSObjectMethodResolver methodResolver)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        ArgumentNullException.ThrowIfNull(context);
        if (classInfo.IsTSExport && !classInfo.Methods.Any() && !classInfo.Properties.Any() && classInfo.NestedTypes.Count == 0)
        {
            throw new ArgumentException("Interop class must have at least one method, property or nested type to render.", nameof(classInfo));
        }
        _classInfo = classInfo;
        _ctx = context;
        _methodResolver = methodResolver;
        _conversionRenderer = new CSharpTypeConversionRenderer(context);
        _methodRenderer = new CSharpMethodRenderer(context, _conversionRenderer, methodResolver);
    }

    internal string Render()
    {
        if (!_classInfo.IsTSExport)
        {
            return string.Empty;
        }

        _ctx.AppendLine("#nullable enable")
            .AppendLine("// TypeShim generated TypeScript interop definitions")
            .AppendLine("using System;")
            .AppendLine("using System.Runtime.CompilerServices;")
            .AppendLine("using System.Runtime.InteropServices.JavaScript;")
            .AppendLine("using System.Threading.Tasks;")
            .Append("namespace ").Append(_classInfo.Namespace).AppendLine(";");

        RenderClassBlock();
        return _ctx.ToString();
    }

    // Renders `public partial class {Name}Interop { ... }` (without the file header), recursing into nested classes.
    private void RenderClassBlock()
    {
        _ctx.Append("public partial class ").AppendLine(RenderConstants.InteropClassName(_classInfo))
            .AppendLine("{");

        using (_ctx.Indent())
        {
            if (_classInfo.Constructor is not null)
            {
                _methodRenderer.RenderConstructorMethod(_classInfo.Constructor);
            }

            foreach (MethodInfo methodInfo in _classInfo.Methods)
            {
                _methodRenderer.RenderMethod(methodInfo);
            }

            foreach (PropertyInfo propertyInfo in _classInfo.Properties)
            {
                _methodRenderer.RenderPropertyMethod(propertyInfo, propertyInfo.GetMethod);

                if (propertyInfo.SetMethod is null)
                    continue;

                _methodRenderer.RenderPropertyMethod(propertyInfo, propertyInfo.SetMethod);
                // Note: init is not rendered as an interop method.
            }

            if (!_classInfo.IsStatic)
            {
                _methodRenderer.RenderFromObjectMapper();
            }

            if (_classInfo.Constructor is { AcceptsInitializer: true, IsParameterless: true, InitializerObject: { } initializerParameter } constructorMethod)
            {
                _methodRenderer.RenderFromJSObjectMapper(constructorMethod, initializerParameter);
            }

            if (_classInfo.Constructor is not null)
            {
                _methodRenderer.RenderMemberInitializerAccessors(_classInfo.Constructor);
            }

            RenderNestedInteropClasses();
        }

        _ctx.AppendLine("}");
    }

    // Emits nested classes as physically nested partial interop classes so the runtime assemblyExports
    // nest them under their parent (e.g. ShipmentInterop.ManifestInterop). Nested enums cross as numbers
    // and produce no C# interop surface, so they are skipped here.
    private void RenderNestedInteropClasses()
    {
        foreach (ClassInfo nested in _classInfo.NestedTypes.OfType<ClassInfo>())
        {
            RenderContext childCtx = new(nested, _ctx.AllNamedTypes, RenderOptions.CSharp);
            CSharpInteropClassRenderer childRenderer = new(nested, childCtx, _methodResolver);
            childRenderer.RenderClassBlock();
            _ctx.AppendIndentedBlock(childCtx.ToString());
        }
    }
}
