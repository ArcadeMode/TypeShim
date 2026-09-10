using Microsoft.CodeAnalysis;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpMethodRenderer(RenderContext _ctx, CSharpTypeConversionRenderer _conversionRenderer, JSObjectMethodResolver _methodResolver)
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
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(_ctx);
        marshalAsAttributeRenderer.RenderJSExportAttribute();
        _ctx.AppendLine();
        marshalAsAttributeRenderer.RenderReturnAttribute(constructorInfo.Type.JSTypeSyntax);
        _ctx.AppendLine();

        MethodParameterInfo[] allParameters = constructorInfo.GetParametersIncludingInitializerObject();
        RenderMethodSignature(constructorInfo.Name, constructorInfo.Type, allParameters);
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            if (constructorInfo.InitializerObject is MethodParameterInfo initializerParamInfo)
            {
                _ctx.Append("using var _ = ").Append(initializerParamInfo.Name).AppendLine(";");
            }

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
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(_ctx);
        marshalAsAttributeRenderer.RenderJSExportAttribute();
        _ctx.AppendLine();
        marshalAsAttributeRenderer.RenderReturnAttribute(methodInfo.ReturnType.JSTypeSyntax);
        _ctx.AppendLine();

        RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParametersIncludingInstanceParameter());
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.GetParametersIncludingInstanceParameter())
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }

            DeferredExpressionRenderer returnValueExpression = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, DeferredExpressionRenderer.FromUnary(RenderInvocationExpression));
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
                _ctx.Append(_ctx.LocalScope.GetAccessorExpression(methodInfo.InstanceParameter!));
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
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(_ctx);
        marshalAsAttributeRenderer.RenderJSExportAttribute();
        _ctx.AppendLine();
        marshalAsAttributeRenderer.RenderReturnAttribute(methodInfo.ReturnType.JSTypeSyntax);
        _ctx.AppendLine();

        RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.GetParametersIncludingInstanceParameter());
        _ctx.AppendLine("{");

        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.GetParametersIncludingInstanceParameter())
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }

            string accessedObject = methodInfo.IsStatic ? _ctx.Class.Name : _ctx.LocalScope.GetAccessorExpression(methodInfo.InstanceParameter!);
            DeferredExpressionRenderer untypedValueExpressionRenderer = DeferredExpressionRenderer.FromUnary(() => _ctx.Append(accessedObject).Append('.').Append(propertyInfo.Name));
            DeferredExpressionRenderer typedValueExpressionRenderer = _conversionRenderer.RenderReturnTypeConversion(methodInfo.ReturnType, untypedValueExpressionRenderer);
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
            {
                _ctx.Append("return ");
                typedValueExpressionRenderer.Render();
                _ctx.AppendLine(";");
            }
            else // setter
            {
                string valueVarName = _ctx.LocalScope.GetAccessorExpression(methodInfo.Parameters.First());
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
                JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(_ctx);
                marshalAsAttributeRenderer.RenderParameterAttribute(parameterInfo.Type.JSTypeSyntax);
                _ctx.Append(' ')
                    .Append(parameterInfo.Type.CSharpInteropTypeSyntax)
                    .Append(' ')
                    .Append(parameterInfo.Name);
                isFirst = false;
            }
        }
    }

    internal void RenderFromObjectMapper()
    {
        _ctx.Append("public static ").Append(_ctx.Class.Type.CSharpTypeSyntax.ToString()).Append(' ').Append(RenderConstants.FromObject).AppendLine("(object obj)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine("return obj switch");
            _ctx.AppendLine("{");
            using (_ctx.Indent())
            {
                _ctx.Append(_ctx.Class.Type.CSharpTypeSyntax.ToString()).AppendLine(" instance => instance,");
                if (_ctx.Class is { Constructor: { AcceptsInitializer: true, IsParameterless: true } })
                {
                    _ctx.Append("JSObject jsObj => ").Append(RenderConstants.FromJSObject).AppendLine("(jsObj),");
                }
                _ctx.AppendLine("_ => throw new ArgumentException($\"Invalid object type {obj?.GetType().ToString() ?? \"null\"}\", nameof(obj)),");
            }
            _ctx.AppendLine("};");
        }
        _ctx.AppendLine("}");
    }

    internal void RenderFromJSObjectMapper(ConstructorInfo constructorInfo, MethodParameterInfo initializerParameter)
    {
        _ctx.Append("public static ").Append(_ctx.Class.Type.CSharpTypeSyntax).Append(' ').Append(RenderConstants.FromJSObject).Append("(JSObject ").Append(initializerParameter.Name).AppendLine(")")
            .AppendLine("{");

        using (_ctx.Indent())
        {
            _ctx.Append("using var _ = ").Append(initializerParameter.Name).AppendLine(";");
            RenderConstructorInvocation(constructorInfo);
        }
        _ctx.AppendLine("}");
    }

    private void RenderConstructorInvocation(ConstructorInfo constructorInfo)
    {
        if (!(constructorInfo.AcceptsInitializer && constructorInfo.InitializerObject is MethodParameterInfo initializerParameter))
        {
            _ctx.Append("return ").Append(RenderConstants.UnsafeAccessorConstructorMethod).Append('(');
            RenderPositionalArguments(constructorInfo);
            _ctx.AppendLine(");");
            return;
        }

        _ctx.Append("var instance = ").Append(RenderConstants.UnsafeAccessorConstructorMethod).Append('(');
        RenderPositionalArguments(constructorInfo);
        _ctx.AppendLine(");");

        foreach (PropertyInfo propertyInfo in constructorInfo.MemberInitializers)
        {
            _ctx.Append("if (").Append(initializerParameter.Name).Append(".HasProperty(\"").Append(propertyInfo.Name).AppendLine("\"))");
            _ctx.AppendLine("{");
            using (_ctx.Indent())
            {
                DeferredExpressionRenderer valueRenderer = RenderMemberValueExpression(propertyInfo, initializerParameter);
                _ctx.Append(RenderConstants.UnsafeAccessorSetMethod(propertyInfo)).Append("(instance, ");
                valueRenderer.Render();
                _ctx.AppendLine(");");
            }
            _ctx.AppendLine("}");

            if (propertyInfo.IsRequired)
            {
                _ctx.AppendLine("else");
                _ctx.AppendLine("{");
                using (_ctx.Indent())
                {
                    _ctx.Append("throw new ArgumentException(\"Required property '")
                        .Append(propertyInfo.Name)
                        .Append("' was not provided\", nameof(").Append(initializerParameter.Name).AppendLine("));");
                }
                _ctx.AppendLine("}");
            }
        }

        _ctx.AppendLine("return instance;");

        void RenderPositionalArguments(ConstructorInfo constructorInfo)
        {
            bool isFirst = true;
            foreach (MethodParameterInfo param in constructorInfo.Parameters)
            {
                if (!isFirst) _ctx.Append(", ");
                _ctx.Append(_ctx.LocalScope.GetAccessorExpression(param));
                isFirst = false;
            }
        }

        DeferredExpressionRenderer RenderMemberValueExpression(PropertyInfo propertyInfo, MethodParameterInfo initializerParameter)
        {
            DeferredExpressionRenderer valueRetrievalExpressionRenderer = DeferredExpressionRenderer.FromUnary(() => {
                _ctx.Append(initializerParameter.Name).Append(".").Append(_methodResolver.ResolveJSObjectMethodName(propertyInfo.Type))
                    .Append("(\"").Append(propertyInfo.Name).Append("\")");
            });

            if (!propertyInfo.Type.RequiresTypeConversion)
            {
                return valueRetrievalExpressionRenderer;
            }

            if (propertyInfo.Type.IsDelegateType())
            {
                // delegates with conversion requirements need to be stored in a temporary variable to avoid multiple invocations of the JSObject method from the wrapper delegate
                _ctx.Append(propertyInfo.Type.CSharpInteropTypeSyntax).Append(" tmp").Append(propertyInfo.Name).Append(" = ");
                valueRetrievalExpressionRenderer.Render();
                _ctx.AppendLine(";");
                valueRetrievalExpressionRenderer = DeferredExpressionRenderer.FromUnary(() => {
                    _ctx.Append("tmp").Append(propertyInfo.Name);
                });
            }

            return _conversionRenderer.RenderVarTypeConversion(propertyInfo.Type, propertyInfo.Name, valueRetrievalExpressionRenderer);
        }
    }

    internal void RenderMemberInitializerAccessors(ConstructorInfo constructorInfo)
    {
        _ctx.AppendLine();
        _ctx.AppendLine("[UnsafeAccessor(UnsafeAccessorKind.Constructor)]");
        _ctx.Append("private static extern ").Append(constructorInfo.Type.CSharpTypeSyntax).Append(' ')
            .Append(RenderConstants.UnsafeAccessorConstructorMethod).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo param in constructorInfo.Parameters)
        {
            if (!isFirst) _ctx.Append(", ");
            _ctx.Append(param.Type.CSharpTypeSyntax).Append(' ').Append(param.Name);
            isFirst = false;
        }
        _ctx.AppendLine(");");

        foreach (PropertyInfo propertyInfo in constructorInfo.MemberInitializers)
        {
            _ctx.AppendLine();
            _ctx.Append("[UnsafeAccessor(UnsafeAccessorKind.Method, Name = \"set_")
                .Append(propertyInfo.Name).AppendLine("\")]");
            _ctx.Append("private static extern void ").Append(RenderConstants.UnsafeAccessorSetMethod(propertyInfo)).Append('(')
                .Append(constructorInfo.Type.CSharpTypeSyntax).Append(" target, ")
                .Append(propertyInfo.Type.CSharpTypeSyntax).AppendLine(" value);");
        }
    }
}
