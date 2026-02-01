using Microsoft.CodeAnalysis;
using System.Diagnostics;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpTypeConversionRenderer(RenderContext _ctx)
{
    internal void RenderParameterTypeConversion(MethodParameterInfo parameterInfo)
    {
        if (!parameterInfo.Type.RequiresTypeConversion)
            return;

        string varName = _ctx.LocalScope.GetAccessorExpression(parameterInfo);
        string newVarName = $"typed_{_ctx.LocalScope.GetAccessorExpression(parameterInfo)}";
        // task pattern differs from other conversions, hence their fully separated rendering.
        if (parameterInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true }) // Task<T>?
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(parameterInfo.Type, varName, varName);
            _ctx.AppendLine($"{parameterInfo.Type.CSharpTypeSyntax} {newVarName} = {convertedTaskExpression};");
        }
        else if (parameterInfo.Type.IsTaskType) // Task<T>
        {
            string convertedTaskExpression = RenderTaskTypeConversion(parameterInfo.Type, varName, varName);
            _ctx.AppendLine($"{parameterInfo.Type.CSharpTypeSyntax} {newVarName} = {convertedTaskExpression};");
        }
        else
        {
            _ctx.Append($"{parameterInfo.Type.CSharpTypeSyntax} {newVarName} = ");
            RenderInlineTypeDownConversion(parameterInfo.Type, varName, forceCovariantConversion: parameterInfo.IsInjectedInstanceParameter);
            _ctx.AppendLine(";");
        }

        _ctx.LocalScope.UpdateAccessorExpression(parameterInfo, newVarName);
    }

    /// <summary>
    /// Renders any lines that may be required to convert the return type from interop to managed. Then returns a delegate to render the expression to access the converted value.
    /// </summary>
    /// <param name="returnType"></param>
    /// <param name="valueExpression"></param>
    /// <returns></returns>
    internal DeferredExpressionRenderer RenderReturnTypeConversion(InteropTypeInfo returnType, string valueExpression)
    {
        if (!returnType.RequiresTypeConversion)
            return DeferredExpressionRenderer.From(() => _ctx.Append(valueExpression));
        
        if (returnType is { IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>
        {
            string convertedValueExpression = RenderTaskTypeConversion(returnType.AsInteropTypeInfo(), "retVal", valueExpression);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedValueExpression));
        }
        else if (returnType is { IsNullableType: true, TypeArgument.IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>?
        {
            string convertedValueExpression = RenderNullableTaskTypeConversion(returnType.AsInteropTypeInfo(), "retVal", valueExpression);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedValueExpression));

        }
        else if (returnType.IsDelegateType() && returnType.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            // Note: its important that we store retVal first, to avoid multiple evaluations of valueExpression, as it can be a method call
            _ctx.Append(returnType.CSharpTypeSyntax).Append(" retVal = ").Append(valueExpression).AppendLine(";");
            return DeferredExpressionRenderer.From(() => RenderInlineDelegateTypeUpConversion(returnType, "retVal", argumentInfo));
        }
        else
        {
            return DeferredExpressionRenderer.From(() => RenderInlineCovariantTypeUpConversion(returnType, valueExpression));
        }
    }

    internal void RenderInlineTypeDownConversion(InteropTypeInfo typeInfo, string varName, bool forceCovariantConversion = false)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        if (forceCovariantConversion)
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, varName);
        }
        else if (typeInfo.IsArrayType)
        {
            RenderInlineArrayTypeDownConversion(typeInfo, varName);
        }
        else if (typeInfo.IsNullableType)
        {
            RenderInlineNullableTypeDownConversion(typeInfo, varName);
        }
        else if (typeInfo.ManagedType is KnownManagedType.Object)
        {
            RenderInlineObjectTypeDownConversion(typeInfo, varName);
        }
        else if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            RenderInlineDelegateTypeDownConversion(typeInfo, varName, argumentInfo);
        }
        else
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CSharpTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineCovariantTypeDownConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        _ctx.Append($"({typeInfo.CSharpTypeSyntax}){parameterName}");
    }
    
    private void RenderInlineCovariantTypeUpConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        _ctx.Append($"({typeInfo.CSharpInteropTypeSyntax}){parameterName}");
    }

    private void RenderInlineObjectTypeDownConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType == KnownManagedType.Object, "Attempting object type conversion with non-object");

        if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion)
        {
            ClassInfo exportedClass = _ctx.SymbolMap.GetClassInfo(typeInfo);
            string targetInteropClass = RenderConstants.InteropClassName(exportedClass);
            _ctx.Append($"{targetInteropClass}.{RenderConstants.FromObject}({parameterName})");
        }
        else
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, parameterName);
        }
    }

    private void RenderInlineNullableTypeDownConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.IsNullableType, "Type must be nullable for nullable type conversion.");
        Debug.Assert(typeInfo.TypeArgument != null, "Nullable type must have a type argument.");

        _ctx.Append($"{parameterName} != null ? ");
        RenderInlineTypeDownConversion(typeInfo.TypeArgument, parameterName);
        _ctx.Append(" : null");
    }

    private void RenderInlineArrayTypeDownConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.TypeArgument != null, "Array type must have a type argument.");
        if (typeInfo.TypeArgument.IsTSExport == false)
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, parameterName);
            // no special conversion possible for non-exported types
        }
        else
        {
            _ctx.Append($"Array.ConvertAll({parameterName}, e => ");
            RenderInlineTypeDownConversion(typeInfo.TypeArgument, "e");
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
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CSharpTypeSyntax}> {tcsVarName} = new();")
            .AppendLine($"{sourceTaskExpression}.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
            _ctx.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeDownConversion(taskTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");
        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
    }
    
    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private string RenderTaskTypeConversionCore(InteropTypeInfo targetTaskType, string sourceVarName, string sourceTaskExpression)
    {
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CSharpTypeSyntax}> {tcsVarName} = new();")
            .AppendLine($"{sourceTaskExpression}.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
            _ctx.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeDownConversion(taskTypeParamInfo, "t.Result");
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
        _ctx.AppendLine($"TaskCompletionSource<{taskReturnTypeParamInfo.CSharpTypeSyntax}>? {tcsVarName} = {sourceTaskExpression} != null ? new() : null;");
        _ctx.AppendLine($"{sourceTaskExpression}?.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}!.SetException(t.Exception.InnerExceptions);")
                .AppendLine($"else if (t.IsCanceled) {tcsVarName}!.SetCanceled();");

            _ctx.Append($"else {tcsVarName}!.SetResult(");
            RenderInlineTypeDownConversion(taskReturnTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");

        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }

    private void RenderInlineDelegateTypeDownConversion(InteropTypeInfo typeInfo, string varName, DelegateArgumentInfo argumentInfo)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        // TODO: render conversion of return type
        _ctx.Append(") => ").Append(varName).Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            // in body of upcasted delegate, to invoke original delegate we simply pass to downcast the parameter types
            _ctx.Append("arg").Append(i);
        }
        _ctx.Append(')');
    }

    private void RenderInlineDelegateTypeUpConversion(InteropTypeInfo typeInfo, string varName, DelegateArgumentInfo argumentInfo)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0)
                _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpInteropTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        // TODO: render conversion of return type
        _ctx.Append(") => ").Append(varName).Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0)
                _ctx.Append(", ");
            // in body of downcasted delegate, to invoke original delegate we must upcast the types again
            RenderInlineTypeDownConversion(argumentInfo.ParameterTypes[i], $"arg{i}");
        }
        _ctx.Append(')');
    }
}