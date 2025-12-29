using Microsoft.CodeAnalysis;
using System;
using System.Reflection;
using TypeShim.Generator.Parsing;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo _classInfo;
    private readonly RenderContext _ctx;
    private readonly CSharpTypeConversionRenderer _conversionRenderer;
    private readonly CSharpMethodRenderer _methodRenderer;

    public CSharpInteropClassRenderer(ClassInfo classInfo, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        ArgumentNullException.ThrowIfNull(context);
        if (!classInfo.Methods.Any() && !classInfo.Properties.Any())
        {
            throw new ArgumentException("Interop class must have at least one method or property to render.", nameof(classInfo));
        }
        _classInfo = classInfo;
        _ctx = context;
        _conversionRenderer = new CSharpTypeConversionRenderer(context);
        _methodRenderer = new CSharpMethodRenderer(context, _conversionRenderer);
    }

    internal string Render()
    {
        _ctx.AppendLine("// Auto-generated TypeScript interop definitions")
            .AppendLine("using System;")
            .AppendLine("using System.Runtime.InteropServices.JavaScript;")
            .AppendLine("using System.Threading.Tasks;")
            .Append("namespace ").Append(_classInfo.Namespace).AppendLine(";")
            .Append("public partial class ").AppendLine(_ctx.GetInteropClassName(_classInfo))
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

            if (_classInfo.Constructor is { AcceptsInitializer: true, IsParameterless: true } constructorMethod)
            {
                _methodRenderer.RenderFromJSObjectMapper(constructorMethod);
            }
        }
        
        _ctx.AppendLine("}");
        return _ctx.Render();
    }
}
