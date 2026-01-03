using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class SymbolMap(IEnumerable<ClassInfo> allClasses)
{
    private readonly Dictionary<InteropTypeInfo, ClassInfo> _typeToClassDict = allClasses.ToDictionary(c => c.Type);

    internal ClassInfo GetClassInfo(InteropTypeInfo type)
    {
        _typeToClassDict.TryGetValue(type, out ClassInfo? info);
        return info ?? throw new NotFoundClassInfoException($"Could not find ClassInfo for type: {type.CSharpTypeSyntax}");
    }

    internal string GetUserClassSymbolName(InteropTypeInfo type, TypeShimSymbolType flags)
    {
        ClassInfo classInfo = GetClassInfo(type.GetInnermostType());
        return GetUserClassSymbolNameCore(type, classInfo.Type, flags);
    }

    internal string GetUserClassSymbolName(ClassInfo classInfo, TypeShimSymbolType flags)
    {
        return GetUserClassSymbolNameCore(classInfo.Type, classInfo.Type, flags);
    }

    internal string GetUserClassSymbolName(ClassInfo classInfo, InteropTypeInfo useSiteTypeInfo, TypeShimSymbolType flags)
    {
        return GetUserClassSymbolNameCore(useSiteTypeInfo, classInfo.Type, flags);
    }

    private static string GetUserClassSymbolNameCore(InteropTypeInfo useSiteTypeInfo, InteropTypeInfo userTypeInfo, TypeShimSymbolType flags)
    {
        return (flags) switch
        {
            TypeShimSymbolType.Proxy => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
            TypeShimSymbolType.Namespace => useSiteTypeInfo.TypeScriptTypeSyntax.Render(),
            TypeShimSymbolType.Snapshot => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Properties}"),
            TypeShimSymbolType.Initializer => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}"),
            TypeShimSymbolType.ProxyInitializerUnion => useSiteTypeInfo.TypeScriptTypeSyntax.Render(suffix: $" | {userTypeInfo.TypeScriptTypeSyntax.Render(suffix: $".{RenderConstants.Initializer}")}"),
            _ => throw new NotImplementedException(),
        };
    }
}

internal sealed class RenderContext(ClassInfo? targetClass, IEnumerable<ClassInfo> allClasses, RenderOptions options)
{
    internal ClassInfo Class => targetClass ?? throw new InvalidOperationException("Not rendering any particular class");
    internal LocalScope LocalScope => _localScope ?? throw new InvalidOperationException("No active method in context");
    internal SymbolMap SymbolMap => _symbolMap?? throw new InvalidOperationException("No active method in context");

    private readonly Dictionary<InteropTypeInfo, ClassInfo> _typeToClassDict = allClasses.ToDictionary(c => c.Type);
    
    private readonly SymbolMap _symbolMap = new(allClasses);
    private readonly StringBuilder _sb = new();

    private int _currentDepth = 0;
    private bool _isNewLine = true;
    private LocalScope? _localScope;

    internal ClassInfo GetClassInfo(InteropTypeInfo type)
    {
        _typeToClassDict.TryGetValue(type, out ClassInfo? info);
        return info ?? throw new NotFoundClassInfoException($"Could not find ClassInfo for type: {type.CSharpTypeSyntax}");
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
