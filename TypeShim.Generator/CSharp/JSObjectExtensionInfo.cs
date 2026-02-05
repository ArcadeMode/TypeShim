using TypeShim.Shared;

namespace TypeShim.Generator.CSharp;

internal record JSObjectExtensionInfo(InteropTypeInfo TypeInfo)
{
    internal string Name = string.Join("", GetManagedTypeListForType(TypeInfo));
        
    internal string GetMarshalAsMethodName()
    {
        return $"MarshalAs{Name}";
    }

    internal string GetGetPropertyAsMethodName()
    {
        return $"GetPropertyAs{Name}Nullable";
    }

    private static IEnumerable<KnownManagedType> GetManagedTypeListForType(InteropTypeInfo typeInfo)
    {
        IEnumerable<KnownManagedType> managedTypes = [];
        BuildManagedTypeEnumerableRecursive();
        return managedTypes;

        void BuildManagedTypeEnumerableRecursive()
        {
            if (typeInfo.IsDelegateType() && typeInfo.ArgumentInfo is DelegateArgumentInfo delegateArgInfo)
            {
                foreach (InteropTypeInfo paramType in delegateArgInfo.ParameterTypes)
                {
                    managedTypes = managedTypes.Concat(GetManagedTypeListForType(paramType));
                }
                managedTypes = managedTypes.Concat(GetManagedTypeListForType(delegateArgInfo.ReturnType));
            }
            else if (typeInfo.TypeArgument != null)
            {
                managedTypes = managedTypes.Concat(GetManagedTypeListForType(typeInfo.TypeArgument));
            }
            managedTypes = managedTypes.Append(typeInfo.ManagedType);
        }
    }
}
