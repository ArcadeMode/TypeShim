using System.Diagnostics;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpTypeConversionRenderer(RenderContext _ctx)
{
    internal void RenderParameterTypeConversion(MethodParameterInfo parameterInfo)
    {
        if (!parameterInfo.Type.RequiresCLRTypeConversion)
            return;

        string newVarName = $"typed_{_ctx.LocalScope.GetAccessorExpression(parameterInfo)}";
        // task pattern differs from other conversions, hence their fully separated rendering.
        if (parameterInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true }) // Task<T>?
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            _ctx.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {newVarName} = {convertedTaskExpression};");
        }
        else if (parameterInfo.Type.IsTaskType) // Task<T>
        {
            string convertedTaskExpression = RenderTaskTypeConversion(parameterInfo.Type, parameterInfo.Name, parameterInfo.Name);
            _ctx.AppendLine($"{parameterInfo.Type.CLRTypeSyntax} {newVarName} = {convertedTaskExpression};");
        }
        else
        {
            _ctx.Append($"{parameterInfo.Type.CLRTypeSyntax} {newVarName} = ");
            RenderInlineTypeConversion(parameterInfo.Type, parameterInfo.Name, forceCovariantConversion: parameterInfo.IsInjectedInstanceParameter);
            _ctx.AppendLine(";");
        }

        _ctx.LocalScope.UpdateAccessorExpression(parameterInfo, newVarName); // TODO: let methodctx decide naming
    }

    internal void RenderInlineTypeConversion(InteropTypeInfo typeInfo, string varName, bool forceCovariantConversion = false)
    {
        if (forceCovariantConversion)
        {
            RenderInlineCovariantTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.IsArrayType)
        {
            RenderInlineArrayTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.IsNullableType)
        {
            RenderInlineNullableTypeConversion(typeInfo, varName);
        }
        else if (typeInfo.ManagedType is KnownManagedType.Object)
        {
            RenderInlineObjectTypeConversion(typeInfo, varName);
        }
        else // Tests guard against this case. Anyway, here is a state-of-the-art regression detector.
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CLRTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineCovariantTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType is KnownManagedType.Object or KnownManagedType.Array, "Unexpected non-object or non-array type with required type conversion");
        _ctx.Append($"({typeInfo.CLRTypeSyntax}){parameterName}");
    }

    private void RenderInlineObjectTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType == KnownManagedType.Object, "Attempting object type conversion with non-object");

        if (_ctx.GetClassInfo(typeInfo) is ClassInfo exportedClass)
        {
            string targetInteropClass = _ctx.GetInteropClassName(exportedClass);
            _ctx.Append($"{targetInteropClass}.{RenderConstants.FromObjectMethodName}({parameterName})");
        }
        else
        {
            RenderInlineCovariantTypeConversion(typeInfo, parameterName);
        }
    }

    private void RenderInlineNullableTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.IsNullableType, "Type must be nullable for nullable type conversion.");
        Debug.Assert(typeInfo.TypeArgument != null, "Nullable type must have a type argument.");

        _ctx.Append($"{parameterName} != null ? ");
        RenderInlineTypeConversion(typeInfo.TypeArgument, parameterName);
        _ctx.Append(" : null");
    }

    private void RenderInlineArrayTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.TypeArgument != null, "Array type must have a type argument.");
        InteropTypeInfo elementTypeInfo = typeInfo.TypeArgument ?? throw new InvalidOperationException("Array type must have a type argument for conversion.");
        if (typeInfo.TypeArgument.IsTSExport == false)
        {
            RenderInlineCovariantTypeConversion(typeInfo, parameterName);
            // no special conversion possible for non-exported types
        }
        else
        {
            _ctx.Append($"Array.ConvertAll({parameterName}, e => ");
            RenderInlineTypeConversion(typeInfo.TypeArgument, "e");
            _ctx.Append(')');
        }
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal string RenderTaskTypeConversion(InteropTypeInfo targetTaskType, string sourceVarName, string sourceTaskExpression)
    {
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CLRTypeSyntax}> {tcsVarName} = new();")
            .AppendLine($"{sourceTaskExpression}.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
            _ctx.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeConversion(taskTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");
        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
    }

    internal string RenderNullableTaskTypeConversion(InteropTypeInfo targetNullableTaskType, string sourceVarName, string sourceTaskExpression)
    {
        InteropTypeInfo taskTypeParamInfo = targetNullableTaskType.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument for conversion.");
        InteropTypeInfo taskReturnTypeParamInfo = taskTypeParamInfo.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskReturnTypeParamInfo.CLRTypeSyntax}>? {tcsVarName} = {sourceTaskExpression} != null ? new() : null;");
        _ctx.AppendLine($"{sourceTaskExpression}?.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);")
                .AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");

            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeConversion(taskReturnTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");

        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }
}