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
            RenderInlineTypeConversion(parameterInfo.Type, varName, forceCovariantConversion: parameterInfo.IsInjectedInstanceParameter);
            _ctx.AppendLine(";");
        }

        _ctx.LocalScope.UpdateAccessorExpression(parameterInfo, newVarName);
    }

    internal void RenderReturnTypeConversion(InteropTypeInfo returnType, string varName)
    {
        if (!returnType.RequiresTypeConversion)
            return;
        
        if (returnType.IsDelegateType() && returnType.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            RenderInlineDelegateTypeDownConversion(returnType, varName, argumentInfo);
        }
        else
        {
            RenderInlineTypeConversion(returnType, varName);
        }
        
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
        else if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            RenderInlineDelegateTypeUpConversion(typeInfo, varName, argumentInfo);
        }
        else // Tests guard against this case. Anyway, here is a state-of-the-art regression detector.
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CSharpTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineDelegateTypeDownConversion(InteropTypeInfo typeInfo, string varName, DelegateArgumentInfo argumentInfo)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0)
                _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpInteropTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        _ctx.Append(") => ").Append(varName).Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0)
                _ctx.Append(", ");
            RenderInlineTypeConversion(argumentInfo.ParameterTypes[i], $"arg{i}");
        }
        _ctx.Append(')');
    }
    
    private void RenderInlineDelegateTypeUpConversion(InteropTypeInfo typeInfo, string varName, DelegateArgumentInfo argumentInfo)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        _ctx.Append(") => ").Append(varName).Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            _ctx.Append("arg").Append(i); // simply pass to upcast the type
        }
        _ctx.Append(')');
    }

    private void RenderInlineCovariantTypeConversion(InteropTypeInfo typeInfo, string parameterName)
    {
        Debug.Assert(typeInfo.ManagedType is KnownManagedType.Object or KnownManagedType.Array, "Unexpected non-covariant type in cast conversion");
        _ctx.Append($"({typeInfo.CSharpTypeSyntax}){parameterName}");
    }

    private void RenderInlineObjectTypeConversion(InteropTypeInfo typeInfo, string parameterName)
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
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CSharpTypeSyntax}> {tcsVarName} = new();")
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
        _ctx.AppendLine($"TaskCompletionSource<{taskReturnTypeParamInfo.CSharpTypeSyntax}>? {tcsVarName} = {sourceTaskExpression} != null ? new() : null;");
        _ctx.AppendLine($"{sourceTaskExpression}?.ContinueWith(t => {{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}!.SetException(t.Exception.InnerExceptions);")
                .AppendLine($"else if (t.IsCanceled) {tcsVarName}!.SetCanceled();");

            _ctx.Append($"else {tcsVarName}!.SetResult(");
            RenderInlineTypeConversion(taskReturnTypeParamInfo, "t.Result");
            _ctx.AppendLine(");");

        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }
}