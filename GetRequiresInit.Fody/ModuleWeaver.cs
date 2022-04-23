using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using TypeAttributes = Mono.Cecil.TypeAttributes;

#region ModuleWeaver

public class ModuleWeaver :
    BaseModuleWeaver
{
    #region Execute

    public override void Execute()
    {
        var ns = GetNamespace();
        var type = new TypeDefinition(ns, "Hello", TypeAttributes.Public, TypeSystem.ObjectReference);

        AddConstructor(type);

        AddHelloWorld(type);

        var getRequiresInitAttributeDefinition = ModuleDefinition.GetCustomAttributes()
            .FirstOrDefault(a => 
                a.AttributeType.FullName == "GetRequiresInitAttribute"
                || a.AttributeType.Name == "GetRequiresInitAttribute");

        if (getRequiresInitAttributeDefinition is null) {
            throw new NullReferenceException("Could not locate GetRequiresInitAttribute in assembly");
        }

        bool AttributeSelector(CustomAttribute attribute)
            => attribute.AttributeType == getRequiresInitAttributeDefinition.AttributeType;
        
        var typesToProcess = ModuleDefinition.Types
            .Where(t => t.HasCustomAttributes && t.CustomAttributes.Any(AttributeSelector))
            .ToList();
        var propsToProcess = typesToProcess
            .SelectMany(t => t.Properties);
        
        foreach (var typeToProcess in typesToProcess) {
            typeToProcess.CustomAttributes.Remove(typeToProcess.CustomAttributes.Single(AttributeSelector));
        }

        foreach (var prop in propsToProcess) {
            AddInitField(prop);
        }
        
        ModuleDefinition.Types.Add(type);
        WriteInfo("Added type 'Hello' with method 'World'.");
    }

    #endregion

    #region GetAssembliesForScanning

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }

    #endregion

    string? GetNamespace()
    {
        var namespaceFromConfig = GetNamespaceFromConfig();
        var namespaceFromAttribute = GetNamespaceFromAttribute();
        if (namespaceFromConfig != null && namespaceFromAttribute != null)
        {
            throw new WeavingException("Configuring namespace from both Config and Attribute is not supported.");
        }

        if (namespaceFromAttribute != null)
        {
            return namespaceFromAttribute;
        }

        return namespaceFromConfig;
    }

    string? GetNamespaceFromConfig()
    {
        var attribute = Config?.Attribute("Namespace");
        if (attribute == null)
        {
            return null;
        }

        var value = attribute.Value;
        ValidateNamespace(value);
        return value;
    }

    string? GetNamespaceFromAttribute()
    {
        var attributes = ModuleDefinition.Assembly.CustomAttributes;
        var namespaceAttribute = attributes
            .SingleOrDefault(x => x.AttributeType.FullName == "NamespaceAttribute");
        if (namespaceAttribute == null)
        {
            return null;
        }

        attributes.Remove(namespaceAttribute);
        var value = (string)namespaceAttribute.ConstructorArguments.First().Value;
        ValidateNamespace(value);
        return value;
    }

    static void ValidateNamespace(string? value)
    {
        if (value is null || string.IsNullOrWhiteSpace(value))
        {
            throw new WeavingException("Invalid namespace");
        }
    }

    void AddConstructor(TypeDefinition newType)
    {
        var attributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        var method = new MethodDefinition(".ctor", attributes, TypeSystem.VoidReference);
        var objectConstructor = ModuleDefinition.ImportReference(TypeSystem.ObjectDefinition.GetConstructors().First());
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldarg_0);
        processor.Emit(OpCodes.Call, objectConstructor);
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
    }

    void AddHelloWorld(TypeDefinition newType)
    {
        var method = new MethodDefinition("World", MethodAttributes.Public, TypeSystem.StringReference);
        var processor = method.Body.GetILProcessor();
        processor.Emit(OpCodes.Ldstr, "Hello World");
        processor.Emit(OpCodes.Ret);
        newType.Methods.Add(method);
        
    }

    void AddInitField(PropertyDefinition propertyDefinition)
    {
        var fieldName = $"_initFlag_{propertyDefinition.Name}";
        var initFlagField = new FieldDefinition(fieldName, FieldAttributes.Private, TypeSystem.BooleanReference);
        propertyDefinition.DeclaringType.Fields.Add(initFlagField);

        var setterIlProcessor = propertyDefinition.SetMethod.Body.GetILProcessor();
        var retInstruction = propertyDefinition.SetMethod.Body.Instructions.Last();
        setterIlProcessor.Remove(retInstruction);
        setterIlProcessor.Emit(OpCodes.Ldarg_0);
        setterIlProcessor.Emit(OpCodes.Ldc_I4_1);
        setterIlProcessor.Emit(OpCodes.Stfld, initFlagField);
        setterIlProcessor.Append(retInstruction);

        var getterIlProcessor = propertyDefinition.GetMethod.Body.GetILProcessor();
        var getInstructions = getterIlProcessor.Body.Instructions.ToList();
        getterIlProcessor.Body.Instructions.Clear();
        getterIlProcessor.Emit(OpCodes.Nop);
        getterIlProcessor.Emit(OpCodes.Ldarg_0);
        getterIlProcessor.Emit(OpCodes.Ldfld, initFlagField);
        getterIlProcessor.Emit(OpCodes.Ldc_I4_0);
        getterIlProcessor.Emit(OpCodes.Ceq);
        var lbl_elseEntryPoint_6 = getterIlProcessor.Create(OpCodes.Nop);
        getterIlProcessor.Emit(OpCodes.Brfalse, lbl_elseEntryPoint_6);
        
        //if body
        
        //throw new InvalidOperationException();
        getterIlProcessor.Emit(OpCodes.Newobj, ModuleDefinition.Assembly.MainModule.ImportReference(ResolveMethod("System.InvalidOperationException", ".ctor",BindingFlags.Default|BindingFlags.Instance|BindingFlags.Public,"")));
        getterIlProcessor.Emit(OpCodes.Throw);
        var lbl_elseEnd_8 = getterIlProcessor.Create(OpCodes.Nop);
        getterIlProcessor.Append(lbl_elseEntryPoint_6);
        getterIlProcessor.Append(lbl_elseEnd_8);
        getterIlProcessor.Body.OptimizeMacros();
        // end if (!_initFlag_CheckedIntField)
        
        //return _checkedIntField;
        foreach (var instruction in getInstructions) {
            getterIlProcessor.Append(instruction);
        }


    }
    
    
    #nullable disable
    public static MethodBase ResolveMethod(string declaringTypeName, string methodName, BindingFlags bindingFlags, string typeArgumentList, params string[] paramTypes)
    {
        var declaringType = Type.GetType(declaringTypeName);
        if (declaringType.IsGenericType)
        {
            var typeArguments = typeArgumentList.Split(',');
            declaringType = declaringType.MakeGenericType(typeArguments.Select(Type.GetType).ToArray());
        }

        if (methodName == ".ctor")
        {
            var resolvedCtor = declaringType.GetConstructor(
                bindingFlags,
                null,
                paramTypes.Select(Type.GetType).ToArray(),
                null);

            if (resolvedCtor == null)
            {
                throw new InvalidOperationException($"Failed to resolve ctor [{declaringType}({string.Join("","", paramTypes)})");
            }
                
            return resolvedCtor;
        }
            
        var resolvedMethod = declaringType.GetMethod(methodName,
            bindingFlags,
            null,
            paramTypes.Select(Type.GetType).ToArray(),
            null);

        if (resolvedMethod == null)
        {
            throw new InvalidOperationException($"Failed to resolve method {declaringType}.{methodName}({string.Join("","", paramTypes)})");
        }
            
        return resolvedMethod;
    }
    #nullable enable


    #region ShouldCleanReference
    public override bool ShouldCleanReference => true;
    #endregion
}

#endregion