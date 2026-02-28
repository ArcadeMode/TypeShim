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

        DeferredExpressionRenderer convertedValueAccessor = RenderTypeDownConversion(parameterInfo.Type, varName, DeferredExpressionRenderer.FromUnary(() => _ctx.Append(varName)), parameterInfo.IsInjectedInstanceParameter);
        _ctx.Append(parameterInfo.Type.CSharpTypeSyntax.ToString()).Append(' ').Append(newVarName).Append(" = ");
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
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineNullableTaskTypeConversion(typeInfo, up: false, accessorExpressionRenderer));
        }
        else if (typeInfo.IsTaskType) // Task<T>
        {
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineTaskTypeConversion(typeInfo, up: false, accessorExpressionRenderer));
        }
        else
        {
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineTypeDownConversion(typeInfo, accessorName, accessorExpressionRenderer, forceCovariantConversion: isInstanceParameter));
        }
    }

    /// <summary>
    /// Renders any lines that may be required to convert the return type from interop to managed.
    /// </summary>
    /// <param name="returnType"></param>
    /// <param name="valueExpression"></param>
    /// <returns>a delegate to render the expression that returns the converted value.</returns>
    internal DeferredExpressionRenderer RenderReturnTypeConversion(InteropTypeInfo returnType, DeferredExpressionRenderer valueExpressionRenderer)
    {
        if (!returnType.RequiresTypeConversion)
            return valueExpressionRenderer;
        
        if (returnType is { IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>
        {
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineTaskTypeConversion(returnType, up: true, valueExpressionRenderer));
        }
        else if (returnType is { IsNullableType: true, TypeArgument.IsTaskType: true, TypeArgument.RequiresTypeConversion: true }) // Handle Task<T>?
        {
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineNullableTaskTypeConversion(returnType, up: true, valueExpressionRenderer));

        }
        else if (returnType.IsDelegateType() && returnType.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            // Note: for delegates its important that we store retVal first, to avoid multiple evaluations of valueExpression inside the wrapper delegate, as it can be a method call
            _ctx.Append(returnType.CSharpTypeSyntax).Append(" retVal = ");
            valueExpressionRenderer.Render();
            _ctx.AppendLine(";");
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineDelegateTypeUpConversion(returnType, "retVal", argumentInfo));
        }
        else
        {
            return DeferredExpressionRenderer.FromUnary(() => RenderInlineCovariantTypeUpConversion(returnType, valueExpressionRenderer));
        }
    }

    private void RenderInlineTypeDownConversion(InteropTypeInfo typeInfo, string accessorName, DeferredExpressionRenderer accessorExpressionRenderer, bool forceCovariantConversion = false)
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
            RenderInlineNullableTypeDownConversion(typeInfo, accessorName, accessorExpressionRenderer);
        }
        else if (typeInfo.ManagedType is KnownManagedType.Object)
        {
            RenderInlineObjectTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
        else if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo argumentInfo) // Action/Action<T1...Tn>/Func<T1...Tn>
        {
            RenderInlineDelegateTypeDownConversion(argumentInfo, accessorName, accessorExpressionRenderer);
        }
        else
        {
            throw new NotImplementedException($"Type conversion not implemented for type: {typeInfo.CSharpTypeSyntax}. Please file an issue at https://github.com/ArcadeMode/TypeShim");
        }
    }

    private void RenderInlineCovariantTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        _ctx.Append('(').Append(typeInfo.CSharpTypeSyntax.ToString()).Append(')');
        accessorExpressionRenderer.Render();
    }
    
    private void RenderInlineCovariantTypeUpConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer valueExpressionRenderer)
    {
        _ctx.Append('(').Append(typeInfo.CSharpInteropTypeSyntax.ToString()).Append(')');
        valueExpressionRenderer.Render();
    }

    private void RenderInlineObjectTypeDownConversion(InteropTypeInfo typeInfo, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        Debug.Assert(typeInfo.ManagedType == KnownManagedType.Object, "Attempting object type conversion with non-object");

        if (typeInfo.RequiresTypeConversion && typeInfo.SupportsTypeConversion)
        {
            ClassInfo exportedClass = _ctx.SymbolMap.GetClassInfo(typeInfo);
            string targetInteropClass = RenderConstants.InteropClassName(exportedClass);
            _ctx.Append(RenderConstants.InteropClassName(exportedClass)).Append('.').Append(RenderConstants.FromObject).Append('(');
            accessorExpressionRenderer.Render();
            _ctx.Append(")");
        }
        else
        {
            RenderInlineCovariantTypeDownConversion(typeInfo, accessorExpressionRenderer);
        }
    }

    private void RenderInlineNullableTypeDownConversion(InteropTypeInfo typeInfo, string accessorName, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        Debug.Assert(typeInfo.IsNullableType, "Type must be nullable for nullable type conversion.");
        Debug.Assert(typeInfo.TypeArgument != null, "Nullable type must have a type argument.");

        accessorExpressionRenderer.Render();
        _ctx.Append(" is { } ")
            .Append(accessorName).Append("Val")
            .Append(" ? ");
        RenderInlineTypeDownConversion(typeInfo.TypeArgument, accessorName, DeferredExpressionRenderer.FromUnary(() => _ctx.Append(accessorName).Append("Val")));
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
            RenderInlineTypeDownConversion(typeInfo.TypeArgument, "e", DeferredExpressionRenderer.FromUnary(() => _ctx.Append("e")));
            _ctx.Append(')');
        }
    }

    /// <summary>
    /// returns an expression to access the converted task with.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    private void RenderInlineTaskTypeConversion(InteropTypeInfo targetTaskType, bool up, DeferredExpressionRenderer sourceTaskExpressionRenderer)
    {
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        if (sourceTaskExpressionRenderer.IsBinary) _ctx.Append('(');
        sourceTaskExpressionRenderer.Render();
        if (sourceTaskExpressionRenderer.IsBinary) _ctx.Append(')');
        _ctx.Append(".ContinueWith(t => ");
        if (up)
        {
            RenderInlineCovariantTypeUpConversion(taskTypeParamInfo, DeferredExpressionRenderer.FromUnary(() => _ctx.Append("t.Result")));
        } 
        else
        {
            RenderInlineTypeDownConversion(taskTypeParamInfo, "t", DeferredExpressionRenderer.FromUnary(() => _ctx.Append("t.Result")));
        }
        _ctx.Append(", TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)");
    }

    private void RenderInlineNullableTaskTypeConversion(InteropTypeInfo targetNullableTaskType, bool up, DeferredExpressionRenderer sourceTaskExpressionRenderer)
    {
        InteropTypeInfo targetTaskType = targetNullableTaskType.TypeArgument ?? throw new InvalidOperationException("Nullable type must have a type argument for conversion.");
        InteropTypeInfo taskTypeParamInfo = targetTaskType.TypeArgument ?? throw new InvalidOperationException("Task type must have a type argument for conversion.");
        if (sourceTaskExpressionRenderer.IsBinary) _ctx.Append('(');
        sourceTaskExpressionRenderer.Render();
        if (sourceTaskExpressionRenderer.IsBinary) _ctx.Append(')');
        _ctx.Append("?.ContinueWith(t => ");
        if (up)
        {
            RenderInlineCovariantTypeUpConversion(taskTypeParamInfo, DeferredExpressionRenderer.FromUnary(() => _ctx.Append("t.Result")));
        }
        else
        {
            RenderInlineTypeDownConversion(taskTypeParamInfo, "t", DeferredExpressionRenderer.FromUnary(() => _ctx.Append("t.Result")));
        }
        _ctx.Append(", TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously)");
    }

    private void RenderInlineDelegateTypeDownConversion(DelegateArgumentInfo argumentInfo, string accessorName, DeferredExpressionRenderer accessorExpressionRenderer)
    {
        _ctx.Append('(');
        for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
        {
            if (i > 0) _ctx.Append(", ");
            _ctx.Append(argumentInfo.ParameterTypes[i].CSharpTypeSyntax).Append(' ').Append("arg").Append(i);
        }
        _ctx.Append(") => ");

        DeferredExpressionRenderer invocationExpressionRenderer = DeferredExpressionRenderer.FromUnary(() =>
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
            RenderInlineTypeDownConversion(argumentInfo.ReturnType, accessorName, invocationExpressionRenderer);
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

        DeferredExpressionRenderer invocationExpressionRenderer = DeferredExpressionRenderer.FromUnary(() =>
        {
            _ctx.Append(varName).Append('(');
            for (int i = 0; i < argumentInfo.ParameterTypes.Length; i++)
            {
                if (i > 0) _ctx.Append(", ");

                // in body of downcasted delegate, to invoke original delegate we must upcast the types again
                DeferredExpressionRenderer argNameRenderer = DeferredExpressionRenderer.FromUnary(() => _ctx.Append("arg").Append(i));
                if (argumentInfo.ParameterTypes[i].RequiresTypeConversion)
                {
                    RenderInlineTypeDownConversion(argumentInfo.ParameterTypes[i], $"arg{i}", argNameRenderer); // TODO: refactor delegatearginfo to contain methodparameterinfo for names
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