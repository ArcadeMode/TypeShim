using Microsoft.CodeAnalysis;

namespace DotnetWasmTypescript.InteropGenerator;

internal sealed class ClassInfo
{
    internal required string Namespace { get; init; }
    internal required string Name { get; init; }
    internal required IEnumerable<MethodInfo> Methods { get; init; }
}
