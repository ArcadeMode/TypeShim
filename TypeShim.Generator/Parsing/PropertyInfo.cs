using TypeShim.Shared;
using TypeShim.Generator.Parsing;

internal sealed class PropertyInfo
{
    internal required string Name { get; init; }
    internal required bool IsStatic { get; init; }
    internal required bool IsRequired { get; init; }
    internal required InteropTypeInfo Type { get; init; }

    internal required MethodInfo GetMethod { get; init; }
    internal required MethodInfo? SetMethod { get; init; }
    internal required MethodInfo? InitMethod { get; init; }

    public bool IsSnapshotCompatible() => !IsStatic && Type.IsSnapshotCompatible;

    public PropertyInfo WithInteropTypeInfo()
    {
        return new PropertyInfo
        {
            Name = this.Name,
            IsStatic = this.IsStatic,
            IsRequired = this.IsRequired,
            Type = this.Type.AsInteropTypeInfo(),
            GetMethod = this.GetMethod.WithInteropTypeInfo(),
            SetMethod = this.SetMethod?.WithInteropTypeInfo(),
            InitMethod = this.InitMethod?.WithInteropTypeInfo(),
        };
    }
}