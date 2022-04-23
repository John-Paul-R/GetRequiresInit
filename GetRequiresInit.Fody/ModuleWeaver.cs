using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Fody;
using GetRequiresInit.Fody;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using FieldAttributes = Mono.Cecil.FieldAttributes;

#region ModuleWeaver

public class ModuleWeaver :
    BaseModuleWeaver
{
    #region Execute

    public override void Execute()
    {
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
            .SelectMany(t => t.Properties)
            .ToList();

        var annotatedProps = ModuleDefinition.Types
            .SelectMany(t => t.Properties)
            .Where(p => p.HasCustomAttributes && p.CustomAttributes.Any(AttributeSelector));
        
        propsToProcess.AddRange(annotatedProps);

        // TODO: Implement or remove `removeAttributes` config option
        bool removeAttributes = false;
        if (removeAttributes) {
            foreach (var typeToProcess in typesToProcess) {
                typeToProcess.CustomAttributes.Remove(typeToProcess.CustomAttributes.Single(AttributeSelector));
            }
        }

        foreach (var prop in propsToProcess) {
            AddInitField(prop);
        }
        
        WriteInfo("Added init check fields, and modified annotated property get/set methods.");
    }

    #endregion

    #region GetAssembliesForScanning

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "netstandard";
        yield return "mscorlib";
    }

    #endregion

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
        getterIlProcessor.Emit(OpCodes.Newobj, ModuleDefinition.Assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod("System.InvalidOperationException", ".ctor",BindingFlags.Default|BindingFlags.Instance|BindingFlags.Public,"")));
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

    #region ShouldCleanReference
    public override bool ShouldCleanReference => true;
    #endregion
}

#endregion