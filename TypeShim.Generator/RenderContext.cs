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

internal sealed class RenderContext(ClassInfo targetClass, IEnumerable<ClassInfo> allClasses, int indentSpaces)
{
    internal ClassInfo Class => targetClass;
    internal LocalScope LocalScope => _localScope ?? throw new InvalidOperationException("No active method in context");

    private readonly Dictionary<InteropTypeInfo, ClassInfo> typeToClassDict = allClasses.ToDictionary(c => c.Type);
    private readonly StringBuilder sb = new();
    private int depth = 0;
    private bool isNewLine = true;

    private LocalScope? _localScope;

    internal ClassInfo? GetClassInfo(InteropTypeInfo type)
    {
        typeToClassDict.TryGetValue(type, out ClassInfo? info);
        return info;
    }

    // TODO: check after interop cache factory if by chance was added during factory call. >> CACHE CLASS

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
        depth++;
        return new ActionOnDisposeDisposable(() => depth--);
    }

    internal RenderContext AppendLine(string line)
    {
        AppendIndentIfNewLine();
        sb.AppendLine(line);
        isNewLine = true;
        return this;
    }

    internal RenderContext Append(string text)
    {
        AppendIndentIfNewLine();
        sb.Append(text);
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
        sb.Append(text);
        return this;
    }

    private void AppendIndentIfNewLine()
    {
        if (!isNewLine) return;

        sb.Append(' ', indentSpaces * depth);
        isNewLine = false;
    }

    internal string Render() // TODO: consider different api? at least dont let renderer call tostring directly
    {
        return sb.ToString();
    }

    internal class ActionOnDisposeDisposable(Action onDisposal) : IDisposable
    {
        public void Dispose()
        {
            onDisposal.Invoke();
        }
    }
}
