using System;
using System.Threading.Tasks;
using TypeShim.E2E.External;

namespace TypeShim.E2E.Wasm;

/// <summary>
/// Exercises references to <see cref="ExternalClass"/> and <see cref="ExternalEnum"/> (declared in a
/// different namespace) across every shape the generator must qualify: properties, method parameters
/// and return types, and the reference nested inside Task, nullable, array and delegate types.
/// </summary>
[TSExport]
public class CrossNamespaceClass
{
    public ExternalClass Reference { get; set; } = new();
    public ExternalClass? NullableReference { get; set; }
    public ExternalClass[] ReferenceArray { get; set; } = [];
    public required Func<ExternalClass, ExternalClass> ReferenceFunc { get; set; }

    public ExternalEnum EnumReference { get; set; }
    public ExternalEnum? NullableEnumReference { get; set; }
    public ExternalEnum[] EnumReferenceArray { get; set; } = [];

    public ExternalClass Echo(ExternalClass value) => value;

    public ExternalClass? EchoNullable(ExternalClass? value) => value;

    public ExternalClass[] EchoArray(ExternalClass[] values) => values;

    public Task<ExternalClass> EchoAsync(ExternalClass value) => Task.FromResult(value);

    public ExternalClass InvokeFunc(Func<ExternalClass, ExternalClass> func, ExternalClass value) => func(value);

    public ExternalEnum EchoEnum(ExternalEnum value) => value;

    public ExternalEnum? EchoNullableEnum(ExternalEnum? value) => value;

    public ExternalEnum[] EchoEnumArray(ExternalEnum[] values) => values;

    public Task<ExternalEnum> EchoEnumAsync(ExternalEnum value) => Task.FromResult(value);

    public ExternalEnum InvokeEnumFunc(Func<ExternalEnum, ExternalEnum> func, ExternalEnum value) => func(value);
}
