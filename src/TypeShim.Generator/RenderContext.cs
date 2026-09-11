using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class RenderContext(NamedTypeInfo? targetType, IEnumerable<NamedTypeInfo> allNamedTypes, RenderOptions options)
{
    internal ClassInfo Class => targetType as ClassInfo ?? throw new InvalidOperationException("Current type in RenderContext is not a class");
    internal NamedTypeInfo NamedType => targetType ?? throw new InvalidOperationException("No current type in RenderContext");
    internal LocalScope LocalScope => _localScope ?? throw new InvalidOperationException("No active method in context");
    internal SymbolMap SymbolMap { get; } = new(allNamedTypes);

    /// <summary>
    /// Renders a managed-type reference for use in generated C#, qualifying it with <c>global::</c> only when the
    /// type (or a nested type argument / delegate parameter) lives in a different namespace than the type currently
    /// being rendered. Same-namespace references stay minimally qualified, matching hand-written C#.
    /// </summary>
    internal string ManagedTypeReference(InteropTypeInfo type)
        => ReferencesTypeOutsideCurrentNamespace(type)
            ? type.CSharpFullyQualifiedTypeSyntax.ToString()
            : type.CSharpTypeSyntax.ToString();

    /// <summary>
    /// Renders a generated interop-class reference, qualifying it with <c>global::</c> only when the referenced
    /// class lives in a different namespace than the type currently being rendered.
    /// </summary>
    internal string InteropClassReference(ClassInfo classInfo)
        => classInfo.Namespace != NamedType.Namespace
            ? RenderConstants.FullyQualifiedInteropClassName(classInfo)
            : RenderConstants.InteropClassName(classInfo);

    private bool ReferencesTypeOutsideCurrentNamespace(InteropTypeInfo type)
    {
        if (SymbolMap.TryGetNamedTypeInfo(type, out NamedTypeInfo? info) && info!.Namespace != NamedType.Namespace)
        {
            return true;
        }

        if (type.TypeArgument != null && ReferencesTypeOutsideCurrentNamespace(type.TypeArgument))
        {
            return true;
        }

        if (type.ArgumentInfo is DelegateArgumentInfo delegateArgumentInfo)
        {
            if (ReferencesTypeOutsideCurrentNamespace(delegateArgumentInfo.ReturnType))
            {
                return true;
            }

            foreach (InteropTypeInfo parameterType in delegateArgumentInfo.ParameterTypes)
            {
                if (ReferencesTypeOutsideCurrentNamespace(parameterType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private readonly StringBuilder _sb = new(capacity: 16 * 1024);

    private int _currentDepth = 0;
    private bool _isNewLine = true;
    private LocalScope? _localScope;

    internal void EnterScope(MethodInfo methodInfo)
    {
        _localScope = new LocalScope(methodInfo);
    }

    internal void EnterScope(ConstructorInfo constructorInfo)
    {
        _localScope = new LocalScope(constructorInfo);
    }

    internal void LeaveScope()
    {
        _localScope = null;
    }

    /// <summary>
    /// <code>
    /// using (ctx.Indent()) 
    /// { 
    ///     ctx.AppendLine("..."); // prints with one level of indentation
    /// }</code>
    /// </summary>
    /// <returns></returns>
    internal IDisposable Indent()
    {
        _currentDepth++;
        return new ActionOnDisposeDisposable(() => _currentDepth--);
    }

    internal RenderContext AppendLine() => AppendLine(string.Empty);

    internal RenderContext AppendLine(string line)
    {
        if (!string.IsNullOrEmpty(line)) AppendIndentIfNewLine();
        _sb.AppendLine(line);
        _isNewLine = true;
        return this;
    }

    internal RenderContext Append(string text)
    {
        AppendIndentIfNewLine();
        _sb.Append(text);
        return this;
    }

    internal RenderContext Append(object? text)
    {
        if (text == null) return this;
        return Append(text.ToString()!);
    }

    internal RenderContext Append(char text)
    {
        AppendIndentIfNewLine();
        _sb.Append(text);
        return this;
    }

    private void AppendIndentIfNewLine()
    {
        if (!_isNewLine) return;

        _sb.Append(' ', options.IndentSpaces * _currentDepth);
        _isNewLine = false;
    }

    /// <summary>
    /// Materialize the rendered content as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _sb.ToString();
    }

    private class ActionOnDisposeDisposable(Action onDisposal) : IDisposable
    {
        public void Dispose()
        {
            onDisposal.Invoke();
        }
    }
}
