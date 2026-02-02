using Microsoft.CodeAnalysis;
using System.Data;
using System.Diagnostics;
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

            DeferredExpressionRenderer returnValueExpression = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, DeferredExpressionRenderer.From(RenderInvocationExpression));
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) _ctx.Append("return ");
            returnValueExpression.Render();
            _ctx.AppendLine(";");
        }
        _ctx.AppendLine("}");

        void RenderInvocationExpression()
        {
            IReadOnlyCollection<MethodParameterInfo> parameters = methodInfo.Parameters;
            if (methodInfo.IsStatic)
            {
                _ctx.Append(_ctx.Class.Name);
            }
            else
            {
                _ctx.Append(_ctx.LocalScope.GetAccessorExpression(methodInfo.Parameters.First(p => p.IsInjectedInstanceParameter)));
                parameters = [.. methodInfo.Parameters.Skip(1)];
            }

            _ctx.Append('.').Append(methodInfo.Name).Append('(');
            bool isFirst = true;
            foreach (MethodParameterInfo param in parameters)
            {
                if (!isFirst) _ctx.Append(", ");
                _ctx.Append(_ctx.LocalScope.GetAccessorExpression(param));
                isFirst = false;
            }
            _ctx.Append(")");
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
            DeferredExpressionRenderer untypedValueExpressionRenderer = DeferredExpressionRenderer.From(() => _ctx.Append($"{accessedObject}.{propertyInfo.Name}"));
            DeferredExpressionRenderer typedValueExpressionRenderer = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, untypedValueExpressionRenderer);
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
        Dictionary<PropertyInfo, DeferredExpressionRenderer> propertyToAccessorDict = RenderJSObjectPropertyRetrievalWithTypeConversions(propertiesInMapper);
        Debug.Assert(propertyToAccessorDict.Count == propertiesInMapper.Length, "Property count differs from renderer count");

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
            foreach ((PropertyInfo propertyInfo, DeferredExpressionRenderer expressionRenderer) in propertyToAccessorDict)
            {
                _ctx.Append(propertyInfo.Name).Append(" = ");
                expressionRenderer.Render();
                _ctx.AppendLine(",");
            }
        }
        _ctx.AppendLine("};");

        Dictionary<PropertyInfo, DeferredExpressionRenderer> RenderJSObjectPropertyRetrievalWithTypeConversions(PropertyInfo[] properties)
        {
            Dictionary<PropertyInfo, DeferredExpressionRenderer> convertedTaskExpressionDict = [];
            foreach (PropertyInfo propertyInfo in properties)
            {
                DeferredExpressionRenderer valueRetrievalExpressionRenderer = DeferredExpressionRenderer.From(() => {
                    _ctx.Append("jsObject.").Append(JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type))
                        .Append("(\"").Append(propertyInfo.Name).Append("\")");
                    if (!propertyInfo.Type.IsNullableType)
                    {
                        _ctx.Append(" ?? throw new ArgumentException(\"Non-nullable property '")
                            .Append(propertyInfo.Name)
                            .Append("' missing or of invalid type\", nameof(jsObject))");
                    }
                });

                if (!propertyInfo.Type.RequiresTypeConversion)
                {
                    convertedTaskExpressionDict.Add(propertyInfo, valueRetrievalExpressionRenderer);

                } 
                else if (propertyInfo.Type is { IsNullableType: true })
                {
                    // TODO: if nullable type conversion can be done without a temp variable, this if statement can be removed
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.Append("var ").Append(tmpVarName).Append(" = ");
                    valueRetrievalExpressionRenderer.Render();
                    _ctx.AppendLine(";");

                    valueRetrievalExpressionRenderer = DeferredExpressionRenderer.From(() => _ctx.Append(tmpVarName));
                    DeferredExpressionRenderer convertedValueAccessorRenderer = _conversionRenderer.RenderVarTypeConversion(propertyInfo.Type, tmpVarName, valueRetrievalExpressionRenderer);
                    convertedTaskExpressionDict.Add(propertyInfo, convertedValueAccessorRenderer);
                }
                else
                {
                    DeferredExpressionRenderer convertedValueAccessorRenderer = _conversionRenderer.RenderVarTypeConversion(propertyInfo.Type, propertyInfo.Name, valueRetrievalExpressionRenderer);
                    convertedTaskExpressionDict.Add(propertyInfo, convertedValueAccessorRenderer);
                }
            }

            return convertedTaskExpressionDict;
        }
    }
}
