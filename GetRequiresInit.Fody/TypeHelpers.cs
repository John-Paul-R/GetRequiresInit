#nullable disable
namespace GetRequiresInit.Fody;

using System;
using System.Linq;
using System.Reflection;

public static class TypeHelpers
{
    public static MethodBase ResolveMethod(string declaringTypeName, string methodName, BindingFlags bindingFlags, string typeArgumentList, params string[] paramTypes)
    {
        var declaringType = Type.GetType(declaringTypeName);
        if (declaringType == null)
        {
            throw new InvalidOperationException($"Failed to resolve type [{declaringTypeName}].");
        }
        
        if (declaringType.IsGenericType)
        {
            var typeArguments = typeArgumentList.Split(',');
            declaringType = declaringType.MakeGenericType(typeArguments.Select(Type.GetType).ToArray());
        }

        if (methodName == ".ctor")
        {
            var resolvedCtor = declaringType.GetConstructor(
                bindingFlags,
                binder: null,
                paramTypes.Select(Type.GetType).ToArray(),
                modifiers: null);

            if (resolvedCtor == null)
            {
                throw new InvalidOperationException($"Failed to resolve ctor [{declaringType}({string.Join("","", paramTypes)})");
            }
                
            return resolvedCtor;
        }
            
        var resolvedMethod = declaringType.GetMethod(methodName,
            bindingFlags,
            binder: null,
            paramTypes.Select(Type.GetType).ToArray(),
            modifiers: null);

        if (resolvedMethod == null)
        {
            throw new InvalidOperationException($"Failed to resolve method {declaringType}.{methodName}({string.Join("","", paramTypes)})");
        }
            
        return resolvedMethod;
    }
}