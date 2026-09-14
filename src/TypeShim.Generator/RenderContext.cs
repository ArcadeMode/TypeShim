using System;
using System.Collections.Generic;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Generator.Typescript;
using TypeShim.Shared;

namespace TypeShim.Generator;

internal sealed class RenderContext
{
    private readonly NamedTypeInfo? _targetType;
    private readonly CodeBuilder _codeBuilder;

    private LocalScope? _localScope;

    internal RenderContext(NamedTypeInfo? targetType, IEnumerable<NamedTypeInfo> allNamedTypes, RenderOptions options)
    {
        _targetType = targetType;
        AllNamedTypes = allNamedTypes as IReadOnlyList<NamedTypeInfo> ?? [.. allNamedTypes];
        SymbolMap = new(AllNamedTypes);
        _codeBuilder = new CodeBuilder(options);
    }

    private RenderContext(NamedTypeInfo? targetType, IReadOnlyList<NamedTypeInfo> allNamedTypes, SymbolMap symbolMap, CodeBuilder codeBuilder)
    {
        _targetType = targetType;
        AllNamedTypes = allNamedTypes;
        SymbolMap = symbolMap;
        _codeBuilder = codeBuilder;
    }

    internal ClassInfo Class => _targetType as ClassInfo ?? throw new InvalidOperationException("Current type in RenderContext is not a class");
    internal NamedTypeInfo NamedType => _targetType ?? throw new InvalidOperationException("No current type in RenderContext");
    internal LocalScope LocalScope => _localScope ?? throw new InvalidOperationException("No active method in context");
    internal IReadOnlyList<NamedTypeInfo> AllNamedTypes { get; }
    internal SymbolMap SymbolMap { get; }

    /// <summary>
    /// Creates a context targeting a nested type that shares this context's <see cref="CodeBuilder"/>, so the
    /// nested type renders directly into the current output at the current indentation level.
    /// </summary>
    internal RenderContext GetNestedContext(NamedTypeInfo nestedType)
        => new(nestedType, AllNamedTypes, SymbolMap, _codeBuilder);

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
    internal IDisposable Indent() => _codeBuilder.Indent();

    internal RenderContext AppendLine()
    {
        _codeBuilder.AppendLine();
        return this;
    }

    internal RenderContext AppendLine(string line)
    {
        _codeBuilder.AppendLine(line);
        return this;
    }

    /// <summary>
    /// Appends pre-rendered multi-line content, re-indenting each line to the current depth.
    /// </summary>
    internal RenderContext AppendIndentedBlock(string content)
    {
        _codeBuilder.AppendIndentedBlock(content);
        return this;
    }

    internal RenderContext Append(string text)
    {
        _codeBuilder.Append(text);
        return this;
    }

    internal RenderContext Append(object? text)
    {
        _codeBuilder.Append(text);
        return this;
    }

    internal RenderContext Append(char text)
    {
        _codeBuilder.Append(text);
        return this;
    }

    /// <summary>
    /// Materialize the rendered content as a string.
    /// </summary>
    public override string ToString()
    {
        return _codeBuilder.ToString();
    }
}
