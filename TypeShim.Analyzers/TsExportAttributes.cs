
namespace TypeScriptExport;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum)]
public class TsExportAttribute : Attribute {
}

[AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
public class TsExportAsAttribute<TSGeneratorTypeHint> : Attribute { }