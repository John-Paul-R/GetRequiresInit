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
            ProcessProperty(prop);
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

    private void ProcessProperty(PropertyDefinition propertyDefinition)
    {
        var initFlagField = CreateInitFlagField(propertyDefinition);
        propertyDefinition.DeclaringType.Fields.Add(initFlagField);
        ProcessSetter(propertyDefinition, initFlagField);
        ProcessGetter(propertyDefinition, initFlagField);
    }

    private FieldDefinition CreateInitFlagField(PropertyDefinition propertyDefinition)
    {
        var fieldName = $"_initFlag_{propertyDefinition.Name}";
        return new FieldDefinition(fieldName, FieldAttributes.Private, TypeSystem.BooleanReference);
    }

    private void ProcessGetter(PropertyDefinition propertyDefinition, FieldDefinition initFlagField)
    {
        var getterIlProcessor = propertyDefinition.GetMethod.Body.GetILProcessor();

        getterIlProcessor.Body.SimplifyMacros();
        var getInstructions = getterIlProcessor.Body.Instructions.ToList();
        getterIlProcessor.Body.Instructions.Clear();
        getterIlProcessor.Emit(OpCodes.Nop);
        getterIlProcessor.Emit(OpCodes.Ldarg_0);
        getterIlProcessor.Emit(OpCodes.Ldfld, initFlagField);
        getterIlProcessor.Emit(OpCodes.Ldc_I4_0);
        getterIlProcessor.Emit(OpCodes.Ceq);
        // TODO: IL seems to include these, but when I add, I get errors. Verify these are not needed.
        // getterIlProcessor.Emit(OpCodes.Stloc_0);
        // getterIlProcessor.Emit(OpCodes.Ldloc_0);
        var lbl_elseEntryPoint_6 = getterIlProcessor.Create(OpCodes.Nop);
        getterIlProcessor.Emit(OpCodes.Brfalse_S, lbl_elseEntryPoint_6);

        //if body

        //throw new InvalidOperationException();
        getterIlProcessor.Emit(OpCodes.Newobj, ModuleDefinition.Assembly.MainModule.ImportReference(TypeHelpers.ResolveMethod("System.InvalidOperationException", ".ctor", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public, "")));
        getterIlProcessor.Emit(OpCodes.Throw);
        var lbl_elseEnd_8 = getterIlProcessor.Create(OpCodes.Nop);
        getterIlProcessor.Append(lbl_elseEntryPoint_6);
        getterIlProcessor.Append(lbl_elseEnd_8);
        // end if (!_initFlag_CheckedIntField)

        //return _checkedIntField;
        foreach (var instruction in getInstructions) {
            getterIlProcessor.Append(instruction);
        }

        getterIlProcessor.Body.OptimizeMacros();
    }

    private static void ProcessSetter(PropertyDefinition propertyDefinition, FieldDefinition initFlagField)
    {
        var setterIlProcessor = propertyDefinition.SetMethod.Body.GetILProcessor();

        setterIlProcessor.Body.SimplifyMacros();
        var retInstruction = propertyDefinition.SetMethod.Body.Instructions.Last();
        setterIlProcessor.Remove(retInstruction);
        setterIlProcessor.Emit(OpCodes.Ldarg_0);
        setterIlProcessor.Emit(OpCodes.Ldc_I4_1);
        setterIlProcessor.Emit(OpCodes.Stfld, initFlagField);
        setterIlProcessor.Append(retInstruction);
        setterIlProcessor.Body.OptimizeMacros();
    }

    #region ShouldCleanReference
    public override bool ShouldCleanReference => true;
    #endregion
}

#endregion