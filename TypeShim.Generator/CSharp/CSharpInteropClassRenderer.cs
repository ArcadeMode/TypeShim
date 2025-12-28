using Microsoft.CodeAnalysis;
using System;
using System.Data;
using System.Reflection;
using TypeShim.Generator.Parsing;
using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal sealed class CSharpInteropClassRenderer
{
    private readonly ClassInfo classInfo;
    private readonly RenderContext _ctx;
    private readonly CSharpTypeConversionRenderer _conversionRenderer;

    public CSharpInteropClassRenderer(ClassInfo classInfo, RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(classInfo);
        ArgumentNullException.ThrowIfNull(context);
        if (!classInfo.Methods.Any() && !classInfo.Properties.Any())
        {
            throw new ArgumentException("Interop class must have at least one method or property to render.", nameof(classInfo));
        }
        this.classInfo = classInfo;
        this._ctx = context;
        _conversionRenderer = new CSharpTypeConversionRenderer(context);
    }

    internal string Render()
    {
        _ctx.AppendLine("// Auto-generated TypeScript interop definitions")
            .AppendLine("using System;")
            .AppendLine("using System.Runtime.InteropServices.JavaScript;")
            .AppendLine("using System.Threading.Tasks;")
            .Append("namespace ").Append(classInfo.Namespace).AppendLine(";")
            .Append("public partial class ").AppendLine(_ctx.GetInteropClassName(classInfo))
            .AppendLine("{");

        using (_ctx.Indent())
        {
            if (classInfo.Constructor is not null)
            {
                RenderConstructor(classInfo.Constructor);
            }

            foreach (MethodInfo methodInfo in classInfo.Methods)
            {
                RenderMethod(methodInfo);
            }

            foreach (PropertyInfo propertyInfo in classInfo.Properties)
            {
                RenderProperty(propertyInfo);
            }
            
            if (!classInfo.IsStatic)
            {
                RenderFromObjectMapper();
            }

            if (classInfo.Constructor is { AcceptsInitializer: true, IsParameterless: true } constructorMethod)
            {
                RenderFromJSObjectMapper(constructorMethod);
            }
        }
        
        _ctx.AppendLine("}");
        return _ctx.Render();
    }

    private void RenderConstructor(ConstructorInfo constructorInfo)
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

    private void RenderProperty(PropertyInfo propertyInfo)
    {
        RenderPropertyMethod(propertyInfo, propertyInfo.GetMethod);
        if (propertyInfo.SetMethod is null)
            return;
        RenderPropertyMethod(propertyInfo, propertyInfo.SetMethod);
    }

    private void RenderPropertyMethod(PropertyInfo propertyInfo, MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString());
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo);

        _ctx.AppendLine("{");

        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }

            string accessedObject = methodInfo.IsStatic ? classInfo.Name : _ctx.GetTypedParameterName(methodInfo.MethodParameters.ElementAt(0));
            string accessorExpression = $"{accessedObject}.{propertyInfo.Name}";

            if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true })
            {
                // Handle Task<T>? property conversion to interop type Task<object>?
                string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }
            else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
            {
                // Handle Task<T> property conversion to interop type Task<object>
                string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", accessorExpression);
                accessorExpression = convertedTaskExpression; // continue with the converted expression
            }

            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void) // getter
            {
                _ctx.AppendLine($"return {accessorExpression};");
            }
            else // setter
            {
                string valueVarName = _ctx.GetTypedParameterName(methodInfo.MethodParameters.ElementAt(1));
                _ctx.AppendLine($"{accessorExpression} = {valueVarName};");
            }
        }
        
        _ctx.AppendLine("}");
    }

    private void RenderMethod(MethodInfo methodInfo)
    {
        JSMarshalAsAttributeRenderer marshalAsAttributeRenderer = new(methodInfo.ReturnType);
        _ctx.AppendLine(marshalAsAttributeRenderer.RenderJSExportAttribute().NormalizeWhitespace().ToFullString())
            .AppendLine(marshalAsAttributeRenderer.RenderReturnAttribute().NormalizeWhitespace().ToFullString());

        RenderMethodSignature(methodInfo);        
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            foreach (MethodParameterInfo originalParamInfo in methodInfo.MethodParameters)
            {
                _conversionRenderer.RenderParameterTypeConversion(originalParamInfo);
            }
            RenderUserMethodInvocation(methodInfo);
        }
            
        _ctx.AppendLine("}");
    }

    private void RenderMethodSignature(MethodInfo methodInfo) 
        => RenderMethodSignature(methodInfo.Name, methodInfo.ReturnType, methodInfo.MethodParameters);

    private void RenderMethodSignature(string name, InteropTypeInfo returnType, IEnumerable<MethodParameterInfo> parameterInfos)
    {
        _ctx.Append("public static ")
            .Append(returnType.InteropTypeSyntax)
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
                    .Append(parameterInfo.Type.InteropTypeSyntax)
                    .Append(' ')
                    .Append(parameterInfo.Name);
                isFirst = false;
            }
        }
    }

    private void RenderUserMethodInvocation(MethodInfo methodInfo)
    {
        // Handle Task<T> return conversion for conversion requiring types
        if (methodInfo.ReturnType is { IsNullableType: true, TypeArgument.IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            _ctx.Append("return ").Append(convertedTaskExpression).AppendLine(";");
        }
        else if (methodInfo.ReturnType is { IsTaskType: true, TypeArgument.RequiresCLRTypeConversion: true })
        {
            string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(methodInfo.ReturnType.AsInteropTypeInfo(), "retVal", GetInvocationExpression());
            _ctx.Append("return ").Append(convertedTaskExpression).AppendLine(";");
        }
        else // direct return handling or void invocations
        {
            if (methodInfo.ReturnType.ManagedType != KnownManagedType.Void)
            {
                _ctx.Append("return ");
            }
            _ctx.Append(GetInvocationExpression())
                .AppendLine(";");
        }

        string GetInvocationExpression()
        {
            if (!methodInfo.IsStatic)
            {
                MethodParameterInfo instanceParam = methodInfo.MethodParameters.ElementAt(0);
                List<MethodParameterInfo> memberParams = [.. methodInfo.MethodParameters.Skip(1)];
                return $"{_ctx.GetTypedParameterName(instanceParam)}.{methodInfo.Name}({string.Join(", ", memberParams.Select(_ctx.GetTypedParameterName))})";
            }
            else
            {
                return $"{classInfo.Name}.{methodInfo.Name}({string.Join(", ", methodInfo.MethodParameters.Select(_ctx.GetTypedParameterName))})";
            }
        }
    }

    void RenderFromObjectMapper()
    {
        _ctx.AppendLine($"public static {classInfo.Type.CLRTypeSyntax} {RenderContext.FromObjectMethodName}(object obj)");
        _ctx.AppendLine("{");
        using (_ctx.Indent())
        {
            _ctx.AppendLine($"return obj switch");
            _ctx.AppendLine("{");
            using (_ctx.Indent())
            {
                _ctx.AppendLine($"{classInfo.Type.CLRTypeSyntax} instance => instance,");
                if (classInfo.IsSnapshotCompatible())
                {
                    _ctx.AppendLine($"JSObject jsObj => {RenderContext.FromJSObjectMethodName}(jsObj),");
                }
                _ctx.AppendLine($"_ => throw new ArgumentException($\"Invalid object type {{obj?.GetType().ToString() ?? \"null\"}}\", nameof(obj)),");
            }
            _ctx.AppendLine("};");
        }
        _ctx.AppendLine("}");
    }

    void RenderFromJSObjectMapper(ConstructorInfo constructorInfo)
    {
        _ctx.AppendLine($"public static {classInfo.Type.CLRTypeSyntax} {RenderContext.FromJSObjectMethodName}(JSObject jsObject)")
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
        
        _ctx.Append("return new ").Append(constructorInfo.Type.CLRTypeSyntax).Append('(');
        bool isFirst = true;
        foreach (MethodParameterInfo param in constructorInfo.Parameters)
        {
            if (!isFirst) _ctx.Append(", ");
            _ctx.Append(param.Name);
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
            // TODO: support init properties
            foreach (PropertyInfo propertyInfo in propertiesInMapper)
            {
                if (propertyToConvertedVarDict.TryGetValue(propertyInfo, out TypeConversionExpressionRenderDelegate? expressionRenderer))
                {
                    _ctx.Append($"{propertyInfo.Name} = ");
                    expressionRenderer.Render();
                }
                else if (propertyInfo.Type.RequiresCLRTypeConversion)
                {
                    string propertyRetrievalExpression = $"jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")";
                    _ctx.Append($"{propertyInfo.Name} = ");
                    _conversionRenderer.RenderInlineTypeConversion(propertyInfo.Type, propertyRetrievalExpression);
                }
                else
                {
                    _ctx.Append($"{propertyInfo.Name} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\")"); // TODO: error handling? (null / missing props?)
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
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = _conversionRenderer.RenderNullableTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsTaskType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type)}(\"{propertyInfo.Name}\");");
                    string convertedTaskExpression = _conversionRenderer.RenderTaskTypeConversion(propertyInfo.Type, propertyInfo.Name, tmpVarName);
                    convertedTaskExpressionDict.Add(propertyInfo, new TypeConversionExpressionRenderDelegate(() => _ctx.Append(convertedTaskExpression)));
                }
                else if (propertyInfo.Type is { IsNullableType: true, RequiresCLRTypeConversion: true })
                {
                    string tmpVarName = $"{propertyInfo.Name}Tmp";
                    _ctx.AppendLine($"var {tmpVarName} = jsObject.{JSObjectMethodResolver.ResolveJSObjectMethodName(propertyInfo.Type.TypeArgument!)}(\"{propertyInfo.Name}\");");
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
