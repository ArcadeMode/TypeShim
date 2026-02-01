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

            DeferredExpressionRenderer returnValueExpression = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, GetInvocationExpression());

            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) _ctx.Append("return ");
            returnValueExpression.Render();
            _ctx.AppendLine(";");
        }

        _ctx.AppendLine("}");

        string GetInvocationExpression() // TODO: use ctx to render
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
            DeferredExpressionRenderer typedValueExpressionRenderer = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, valueExpression: $"{accessedObject}.{propertyInfo.Name}");
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
            {
                _ctx.Append($"return ");
                typedValueExpressionRenderer.Render();
                _ctx.AppendLine(";");
            }
            else // setter
            {
                string valueVarName = _ctx.LocalScope.GetAccessorExpression(methodInfo.Parameters.First(p => !p.IsInjectedInstanceParameter)); // TODO: get rid of IsInjectedInstanceParameter
                typedValueExpressionRenderer.Render();
                _ctx.Append(" = ").Append(valueVarName).AppendLine(";");                
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
        Dictionary<PropertyInfo, DeferredExpressionRenderer> propertyToConvertedVarDict = RenderNonInlinableTypeConversions(propertiesInMapper);

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
                if (propertyToConvertedVarDict.TryGetValue(propertyInfo, out DeferredExpressionRenderer? expressionRenderer))
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
                        _conversionRenderer.RenderInlineTypeDownConversion(propertyInfo.Type, propertyRetrievalExpression);
                    else
                        _ctx.Append(propertyRetrievalExpression);
                }
                _ctx.AppendLine(",");
            }
        }
        _ctx.AppendLine("};");

        Dictionary<PropertyInfo, DeferredExpressionRenderer> RenderNonInlinableTypeConversions(PropertyInfo[] properties)
        {
            Dictionary<PropertyInfo, DeferredExpressionRenderer> convertedTaskExpressionDict = [];
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.Type is { IsNullableType: true, TypeArgument.IsTaskType: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new DeferredExpressionRenderer(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsTaskType: true, RequiresTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.Append($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")")
                        .Append($" ?? throw new ArgumentException(\"Non-nullable property '{propertyInfo.Name}' missing or of invalid type\", nameof(jsObject))")
                        .AppendLine(";");
                    string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new DeferredExpressionRenderer(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsNullableType: true, RequiresTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    convertedTaskExpressionDict.Add(propertyInfo, new DeferredExpressionRenderer(() => _conversionRenderer.RenderInlineTypeDownConversion(propertyInfo.Type, tmpVarName)));
                }
            }

            return convertedTaskExpressionDict;
        }
    }
}
