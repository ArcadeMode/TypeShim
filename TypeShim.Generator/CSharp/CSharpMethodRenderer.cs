using Microsoft.CodeAnalysis;
using System.Data;
using System.Reflection;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpMethodRenderer(RenderContext _ctx, CSharpTypeConversionRenderer _conversionRenderer)
{
    internal void RenderConstructorMethod(ConstructorInfo constructorInfo)
    {
        try
        {
            _ctx.EnterScope(constructorInfo);
            RenderConstructorMethodCore(constructorInfo);
        }
        finally
        {
            _ctx.LeaveScope();
        }
    }

    internal void RenderPropertyMethod(PropertyInfo propertyInfo, MethodInfo methodInfo)
    {
        try
        {
            _ctx.EnterScope(methodInfo);
            RenderPropertyMethodCore(propertyInfo, methodInfo);
        } 
        finally
        {
            _ctx.LeaveScope();
        }
    }

    internal void RenderMethod(MethodInfo methodInfo)
    {
        try
        {
            _ctx.EnterScope(methodInfo);
            RenderMethodCore(methodInfo);
        }
        finally
        {
            _ctx.LeaveScope();
        }
    }

    private void RenderConstructorMethodCore(ConstructorInfo constructorInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(constructorInfo.Type);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString())
            .AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        MethodParameterInfo[] allParameters = constructorInfo.GetParametersIncludingInitializerObject();
        RenderMethodSignature(constructorInfo.Name, constructorInfo.Type, allParameters);
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in allParameters)
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }
            RenderConstructorInvocation(constructorInfo);
        }

        _ctx.AppendLine("}");
    }

    private void RenderMethodCore(MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString())
            .AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.Parameters);
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.Parameters)
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }

            if (methodInfo.ReturnType.IsDelegateType())
            {
                _ctx.Append(methodInfo.ReturnType.CSharpTypeSyntax).Append(" retVal = ");
            }
            else if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
            {
                _ctx.Append("return ");
            }

            RenderUserMethodInvocation(methodInfo);
            _ctx.AppendLine(";");
            if (methodInfo.ReturnType.IsDelegateType())
            {
                _ctx.Append("return ");
                _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, "retVal");
                _ctx.AppendLine(";");
            }
        }

        _ctx.AppendLine("}");

        void RenderUserMethodInvocation(MethodInfo methodInfo)
        {
            // Handle Task<T> return conversion for conversion requiring types
            if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true, TypeArgument.RequiresTypeConversion: true })
            {
                string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
                _ctx.Append(convertedTaskExpression);
            }
            else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresTypeConversion: true })
            {
                string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
                _ctx.Append(convertedTaskExpression);
            }
            else // direct return handling or void invocations
            {
                _ctx.Append(GetInvocationExpression());
            }
        }

        string GetInvocationExpression()
        {
            if (!methodInfo.IsStatic)
            {
                MethodParameterInfo instanceParam = methodInfo.Parameters.ElementAt(0);
                List<MethodParameterInfo> memberParams = [.. methodInfo.Parameters.Skip(1)];
                return $"{_ctx.LocalScope.GetAccessorExpression(instanceParam)}.{methodInfo.Name}({string.Join(", ", memberParams.Select(_ctx.LocalScope.GetAccessorExpression))})";
            }
            else
            {
                return $"{_ctx.Class.Name}.{methodInfo.Name}({string.Join(", ", methodInfo.Parameters.Select(_ctx.LocalScope.GetAccessorExpression))})";
            }
        }
    }

    private void RenderPropertyMethodCore(PropertyInfo propertyInfo, MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.Parameters);

        _ctx.AppendLine("{");

        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.Parameters)
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }

            string accessedObject = methodInfo.IsStatic ? _ctx.Class.Name : _ctx.LocalScope.GetAccessorExpression(methodInfo.Parameters.ElementAt(0));
            string accessorExpression = $"{accessedObject}.{propertyInfo.Name}";

            if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
            {
                // Handle Task<T>? property conversion to interop type Task<object>?
                string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }
            else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresTypeConversion: true })
            {
                // Handle Task<T> property conversion to interop type Task<object>
                string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }

            //TODO: move task conversion above to conversion renderer and use RenderReturnTypeConversion (extend localscope with method for return value)

            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
            {
                _ctx.AppendLine($"return {accessorExpression};");
            }
            else // setter
            {
                string valueVarName = _ctx.LocalScope.GetAccessorExpression(methodInfo.Parameters.First(p => !p.IsInjectedInstanceParameter));
                _ctx.AppendLine($"{accessorExpression} = {valueVarName};");
            }
        }

        _ctx.AppendLine("}");
    }

    private void RenderMethodSignature(string name, InteropTypeInfo returnType, IEnumerable<MethodParameterInfo> parameterInfos)
    {
        _ctx.Append("public static ")
            .Append(returnType.CSharpInteropTypeSyntax)
            .Append(' ')
            .Append(name)
            .Append('(');
        RenderMethodParameterList();
        _ctx.AppendLine(")");

        void RenderMethodParameterList()
        {
            if (!parameterInfos.Any())
                return;

            bool isFirst = true;
            foreach (MethodParameterInfo parameterInfo in parameterInfos)
            {
                if (!isFirst) _ctx.Append(", ");

                JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(parameterInfo.Type);
                _ctx.Append(marshalAsAttributeRenderer.RenderParameterAttribute().NormalizeWhitespace().ToFullString())
                    .Append(' ')
                    .Append(parameterInfo.Type.CSharpInteropTypeSyntax)
                    .Append(' ')
                    .Append(parameterInfo.Name);
                isFirst = false;
            }
        }
    }

    internal void RenderFromObjectMapper()
    {
        _ctx.AppendLine($"public static {_ctx.Class.Type.CSharpTypeSyntax} {RenderConstants.FromObject}(object obj)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"return obj switch");
            _ctx.AppendLine("{");
            using (_ctx.Indent())
            {
                _ctx.AppendLine($"{_ctx.Class.Type.CSharpTypeSyntax} instance => instance,");
                if (_ctx.Class is { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
                {
                    _ctx.AppendLine($"JSObject jsObj => {RenderConstants.FromJSObject}(jsObj),");
                }
                _ctx.AppendLine($"_ => throw new ArgumentException($\"Invalid object type {{obj?.GetType().ToString() ?? \"null\"}}\", nameof(obj)),");
            }
            _ctx.AppendLine("};");
        }
        _ctx.AppendLine("}");
    }

    internal void RenderFromJSObjectMapper(ConstructorInfo constructorInfo)
    {
        _ctx.AppendLine($"public static {_ctx.Class.Type.CSharpTypeSyntax} {RenderConstants.FromJSObject}(JSObject jsObject)")
            .AppendLine("{");

        using (_ctx.Indent())
        {
            RenderConstructorInvocation(constructorInfo);
        }
        _ctx.AppendLine("}");
    }

    private void RenderConstructorInvocation(ConstructorInfo constructorInfo)
    {
        PropertyInfo[] propertiesInMapper = [.. constructorInfo.MemberInitializers];
        Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> propertyToConvertedVarDict = RenderNonInlinableTypeConversions(propertiesInMapper);

        _ctx.Append("return new ").Append(constructorInfo.Type.CSharpTypeSyntax).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo param in constructorInfo.Parameters)
        {
            if (!isFirst) _ctx.Append(", ");
            _ctx.Append(_ctx.LocalScope.GetAccessorExpression(param));
            isFirst = false;
        }

        if (!constructorInfo.AcceptsInitializer)
        {
            _ctx.AppendLine(");");
            return;
        }

        _ctx.AppendLine(")");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (PropertyInfo propertyInfo in propertiesInMapper)
            {
                if (propertyToConvertedVarDict.TryGetValue(propertyInfo, out TypeConversionExpressionRenderDelegate? expressionRenderer))
                {
                    _ctx.Append($"{propertyInfo.Name} = ");
                    expressionRenderer.Render();
                }
                else
                {
                    _ctx.Append($"{propertyInfo.Name} = ");
                    string propertyRetrievalExpression = $"jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")";
                    if (propertyInfo.Type is { IsNullableType: false })
                    {
                        propertyRetrievalExpression = $"({propertyRetrievalExpression} ?? throw new ArgumentException(\"Non-nullable property '{propertyInfo.Name}' missing or of invalid type\", nameof(jsObject)))";
                    }

                    if (propertyInfo.Type.RequiresTypeConversion)
                        _conversionRenderer.RenderInlineTypeConversion(propertyInfo.Type, propertyRetrievalExpression);
                    else
                        _ctx.Append(propertyRetrievalExpression);
                }
                _ctx.AppendLine(",");
            }
        }
        _ctx.AppendLine("};");

        Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> RenderNonInlinableTypeConversions(PropertyInfo[] properties)
        {
            Dictionary<PropertyInfo, TypeConversionExpressionRenderDelegate> convertedTaskExpressionDict = [];
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsTaskType: true, RequiresTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.Append($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")")
                        .Append($" ?? throw new ArgumentException(\"Non-nullable property '{propertyInfo.Name}' missing or of invalid type\", nameof(jsObject))")
                        .AppendLine(";");
                    string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsNullableType: true, RequiresTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _conversionRenderer.RenderInlineTypeConversion(propertyInfo.Type, tmpVarName)));
                }
            }

            return convertedTaskExpressionDict;
        }
    }

    private class TypeConversionExpressionRenderDelegate(Action renderAction)
    {
        internal void Render() => renderAction();

        public static implicit operator TypeConversionExpressionRenderDelegate(Action renderAction)
        {
            return new TypeConversionExpressionRenderDelegate(renderAction);
        }
    }
}
