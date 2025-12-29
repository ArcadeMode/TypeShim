using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal static class RenderConstants
{
    internal const string FromJSObjectMethodName = "FromJSObject";
    internal const string FromObjectMethodName = "FromObject";
}

internal sealed class RenderOptions
{
    internal required int IndentSpaces { get; init; }

    internal static RenderOptions CSharp = new()
    {
        IndentSpaces = 4
    };

    internal static RenderOptions TypeScript = new()
    {
        IndentSpaces = 2
    };
}

internal sealed class RenderContext(ClassInfo? targetClass, IEnumerable<ClassInfo> allClasses, RenderOptions options)
{
    internal ClassInfo Class => targetClass ?? throw new InvalidOperationException("Not rendering any particular class"); // TODO: improve api to avoid needing this
    internal LocalScope LocalScope => _localScope ?? throw new InvalidOperationException("No active method in context");

    private readonly Dictionary<InteropTypeInfo, ClassInfo> _typeToClassDict = allClasses.ToDictionary(c => c.Type);
    private readonly StringBuilder _sb = new();

    private int _currentDepth = 0;
    private bool _isNewLine = true;
    private LocalScope? _localScope;

    internal ClassInfo? GetClassInfo(InteropTypeInfo type)
    {
        _typeToClassDict.TryGetValue(type, out ClassInfo? info);
        return info;
    }

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

    internal string GetInteropClassName(ClassInfo classInfo) => $"{classInfo.Name}Interop";

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
        if (!string.IsNullOrEmpty(line))AppendIndentIfNewLine();
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

    internal string Render() // TODO: consider different api? at least dont let renderer call tostring directly
    {
        return _sb.ToString();
    }

    internal class ActionOnDisposeDisposable(Action onDisposal) : IDisposable
    {
        public void Dispose()
        {
            onDisposal.Invoke();
        }
    }
}
