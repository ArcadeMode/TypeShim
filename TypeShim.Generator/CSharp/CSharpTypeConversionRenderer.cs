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

        DeferredExpressionRenderer convertedValueAccessor = RenderTypeDownConversion(parameterInfo.Type, varName, DeferredExpressionRenderer.From(() => _ctx.Append(varName)), parameterInfo.IsInjectedInstanceParameter);
        _ctx.Append($"{parameterInfo.Type.CSharpTypeSyntax} {newVarName} = ");
        convertedValueAccessor.Render();
        _ctx.AppendLine(";");
        _ctx.LocalScope.UpdateAccessorExpression(parameterInfo, newVarName);
    }

    internal DeferredExpressionRenderer RenderVarTypeConversion(InteropTypeInfo typeInfo, string varName, DeferredExpressionRenderer valueExpressionRenderer)
    {
        return RenderTypeDownConversion(typeInfo, varName, valueExpressionRenderer, false);
    }

    private DeferredExpressionRenderer RenderTypeDownConversion(InteropTypeInfo typeInfo, string accessorName, DeferredExpressionRenderer accessorExpressionRenderer, bool isInstanceParameter)
    {
        if (!typeInfo.RequiresTypeConversion)
            return accessorExpressionRenderer;

        if (typeInfo is { IsNullableType: true, TypeArgument.IsTaskType: true }) // Task<T>?
        {
            string convertedTaskExpression = RenderNullableTaskTypeConversion(typeInfo, accessorName, accessorExpressionRenderer);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedTaskExpression));
        }
        else if (typeInfo.IsTaskType) // Task<T>
        {
            string convertedTaskExpression = RenderTaskTypeConversion(typeInfo, accessorName, accessorExpressionRenderer);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedTaskExpression));
        }
        else
        {
            return DeferredExpressionRenderer.From(() => RenderInlineTypeDownConversion(typeInfo, accessorExpressionRenderer, forceCovariantConversion: isInstanceParameter));
        }
    }

    /// <summary>
    /// Renders any lines that may be required to convert the return type from interop to managed. Then returns a delegate to render the expression to access the converted value.
    /// </summary>
    /// <param name="returnType"></param>
    /// <param name="valueExpression"></param>
    /// <returns></returns>
    internal DeferredExpressionRenderer RenderReturnTypeConversion(InteropTypeInfo returnType, DeferredExpressionRenderer valueExpressionRenderer)
    {
        if (!returnType.RequiresTypeConversion)
            return valueExpressionRenderer; // DeferredExpressionRenderer.From(() => _ctx.Append(valueExpression));
        
        if (returnType is { IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>
        {
            string convertedValueExpression = RenderTaskTypeConversion(returnType.AsInteropTypeInfo(), "retVal", valueExpressionRenderer);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedValueExpression));
        }
        else if (returnType is { IsNullableType: true, TypeArgument.IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>?
        {
            string convertedValueExpression = RenderNullableTaskTypeConversion(returnType.AsInteropTypeInfo(), "retVal", valueExpressionRenderer);
            return DeferredExpressionRenderer.From(() => _ctx.Append(convertedValueExpression));

        }
        else if (returnType.IsDelegateType() && returnType.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            // Note: for delegates its important that we store retVal first, to avoid multiple evaluations of valueExpression inside the wrapper delegate, as it can be a method call
            _ctx.Append(returnType.CSharpTypeSyntax).Append(" retVal = ");
            valueExpressionRenderer.Render();
            _ctx.AppendLine(";");
            return DeferredExpressionRenderer.From(() => RenderInlineDelegateTypeUpConversion(returnType, "retVal", argumentInfo));
        }
        else
        {
            return DeferredExpressionRenderer.From(() => RenderInlineCovariantTypeUpConversion(returnType, valueExpressionRenderer));
        }
    }

    private void RenderInlineTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer, bool forceCovariantConversion = false)
    {
        ArgumentNullException.ThrowIfNull(typeInfo, nameof(typeInfo));
        if (forceCovariantConversion)
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
        else if (typeInfo.IsArrayType)
        {
            RenderInlineArrayTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
        else if (typeInfo.IsNullableType)
        {
            RenderInlineNullableTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
        else if (typeInfo.ManagedType is KnownManagedType.Object)
        {
            RenderInlineObjectTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
        else if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            RenderInlineDelegateTypeDownConversion(typeInfo, accessorExpressionRenderer, argumentInfo);
        }
        else
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CSharpTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineCovariantTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        _ctx.Append($"({typeInfo.CSharpTypeSyntax})");
        accessorExpressionRenderer.Render();
    }
    
    private void RenderInlineCovariantTypeUpConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer valueExpressionRenderer)
    {
        _ctx.Append($"({typeInfo.CSharpInteropTypeSyntax})");
        valueExpressionRenderer.Render();
    }

    private void RenderInlineObjectTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        Debug.Assert(typeInfo.ManagedType == KnownManagedType.Object, "Attempting object type conversion with non-object");

        if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion)
        {
            ClassInfo exportedClass = _ctx.SymbolMap.GetClassInfo(typeInfo);
            string targetInteropClass = RenderConstants.InteropClassName(exportedClass);
            _ctx.Append($"{targetInteropClass}.{RenderConstants.FromObject}(");
            accessorExpressionRenderer.Render();
            _ctx.Append(")");
        }
        else
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
    }

    private void RenderInlineNullableTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        Debug.Assert(typeInfo.IsNullableType, "Type must be nullable for nullable type conversion.");
        Debug.Assert(typeInfo.TypeArgument != null, "Nullable type must have a type argument.");

        accessorExpressionRenderer.Render();
        _ctx.Append(" != null ? ");
        RenderInlineTypeDownConversion(typeInfo.TypeArgument, accessorExpressionRenderer); // TODO: consider pattern expression? expression is { } notNullVar, and pass deferred renderer that appends notNullVar
        _ctx.Append(" : null");
    }

    private void RenderInlineArrayTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        Debug.Assert(typeInfo.TypeArgument != null, "Array type must have a type argument.");
        if (typeInfo.TypeArgument.IsTSExport == false)
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, accessorExpressionRenderer);
            // no special conversion possible for non-exported types
        }
        else
        {
            _ctx.Append("Array.ConvertAll(");
            accessorExpressionRenderer.Render();
            _ctx.Append(", e => ");
            RenderInlineTypeDownConversion(typeInfo.TypeArgument, DeferredExpressionRenderer.From(() => _ctx.Append("e")));
            _ctx.Append(')');
        }
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private string RenderTaskTypeConversion(InteropTypeInfo targetTaskType, string sourceVarName, DeferredExpressionRenderer sourceTaskExpressionRenderer)
    {
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.AppendLine($"TaskCompletionSource<{taskTypeParamInfo.CSharpTypeSyntax}> {tcsVarName} = new();");

        _ctx.Append('(');
        sourceTaskExpressionRenderer.Render();
        _ctx.AppendLine(").ContinueWith(t => {");
        
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}.SetException(t.Exception.InnerExceptions);");
            _ctx.AppendLine($"else if (t.IsCanceled) {tcsVarName}.SetCanceled();");
            _ctx.Append($"else {tcsVarName}.SetResult(");
            RenderInlineTypeDownConversion(taskTypeParamInfo, DeferredExpressionRenderer.From(() => _ctx.Append("t.Result")));
            _ctx.AppendLine(");");
        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}.Task";
    }

    private string RenderNullableTaskTypeConversion(InteropTypeInfo targetNullableTaskType, string sourceVarName, DeferredExpressionRenderer sourceTaskExpressionRenderer)
    {
        InteropTypeInfo taskTypeParamInfo = targetNullableTaskType.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument for conversion.");
        InteropTypeInfo taskReturnTypeParamInfo = taskTypeParamInfo.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        
        string tcsVarName = $"{sourceVarName}Tcs";
        _ctx.Append("TaskCompletionSource<").Append(taskReturnTypeParamInfo.CSharpTypeSyntax.ToString()).Append(">? ").Append(tcsVarName).Append(" = ");
        sourceTaskExpressionRenderer.Render();
        _ctx.AppendLine(" != null ? new() : null;");
        sourceTaskExpressionRenderer.Render();
        _ctx.AppendLine("?.ContinueWith(t => {");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"if (t.IsFaulted) {tcsVarName}!.SetException(t.Exception.InnerExceptions);")
                .AppendLine($"else if (t.IsCanceled) {tcsVarName}!.SetCanceled();");

            _ctx.Append($"else {tcsVarName}!.SetResult(");
            RenderInlineTypeDownConversion(taskReturnTypeParamInfo, DeferredExpressionRenderer.From(() => _ctx.Append("t.Result")));
            _ctx.AppendLine(");");

        }
        _ctx.AppendLine("}, TaskContinuationOptions.ExecuteSynchronously);");
        return $"{tcsVarName}?.Task";
    }

    private void RenderInlineDelegateTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer, DelegateArgumentInfo argumentInfo)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        _ctx.Append(") => ");

        DeferredExpressionRenderer invocationExpressionRenderer = DeferredExpressionRenderer.From(() =>
        {
            accessorExpressionRenderer.Render();
            _ctx.Append('(');
            for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
            {
                if (i > 0) _ctx.Append(", ");
                // in body of upcasted delegate, to invoke original delegate we simply pass to downcast the parameter types
                _ctx.Append("arg").Append(i);
            }
            _ctx.Append(')');
        });

        if (argumentInfo.ReturnType.RequiresTypeConversion)
        {
            RenderInlineTypeDownConversion(argumentInfo.ReturnType, invocationExpressionRenderer);
        }
        else
        {
            invocationExpressionRenderer.Render();
        }
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
        _ctx.Append(") => ");

        DeferredExpressionRenderer invocationExpressionRenderer = DeferredExpressionRenderer.From(() =>
        {
            _ctx.Append(varName).Append('(');
            for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
            {
                if (i > 0) _ctx.Append(", ");

                // in body of downcasted delegate, to invoke original delegate we must upcast the types again
                DeferredExpressionRenderer argNameRenderer = DeferredExpressionRenderer.From(() => _ctx.Append("arg").Append(i));
                if (argumentInfo.ParameterTypes[i].RequiresTypeConversion)
                {
                    RenderInlineTypeDownConversion(argumentInfo.ParameterTypes[i], argNameRenderer);
                }
                else
                {
                    argNameRenderer.Render();
                }
            }
            _ctx.Append(')');
        });

        if (argumentInfo.ReturnType.RequiresTypeConversion)
        {
            RenderInlineCovariantTypeUpConversion(argumentInfo.ReturnType, invocationExpressionRenderer);
        }
        else
        {
            invocationExpressionRenderer.Render();
        }
    }
}