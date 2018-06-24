using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using TypeSystem = Fody.TypeSystem;

public class FieldToPropertyConverter
{
    ReferenceFinder referenceFinder;
    TypeSystem typeSystem;
    List<TypeDefinition> allTypes;

    public Dictionary<FieldDefinition, ForwardedField> ForwardedFields = new Dictionary<FieldDefinition, ForwardedField>();

    ModuleWeaver moduleWeaver;

    public FieldToPropertyConverter(ModuleWeaver moduleWeaver, ReferenceFinder referenceFinder, Fody.TypeSystem typeSystem, List<TypeDefinition> allTypes)
    {
        this.moduleWeaver = moduleWeaver;
        this.referenceFinder = referenceFinder;
        this.typeSystem = typeSystem;
        this.allTypes = allTypes;
    }

    void Process(TypeDefinition typeDefinition)
    {
        foreach (var field in typeDefinition.Fields)
        {
            ProcessField(typeDefinition, field);
        }
    }

    void ProcessField(TypeDefinition typeDefinition, FieldDefinition field)
    {
        var name = field.Name;
        if (!field.IsPublic || field.IsStatic || !char.IsUpper(name, 0))
        {
            return;
        }

        if (typeDefinition.HasGenericParameters)
        {
            var message =
                $"Skipped public field '{typeDefinition.Name}.{field.Name}' because generic types are not currently supported. You should make this a public property instead.";
            moduleWeaver.LogWarning(message);
            return;
        }

        field.Name = $"<{name}>k__BackingField";
        field.IsPublic = false;
        field.IsPrivate = true;
        var get = GetGet(field, name);
        typeDefinition.Methods.Add(get);

        var propertyDefinition = new PropertyDefinition(name, PropertyAttributes.None, field.FieldType)
            {GetMethod = get};
        var forwardedField = new ForwardedField
        {
            Get = get,
            FieldType = field.FieldType,
            DeclaringType = field.DeclaringType,
            PropertyDefinition = propertyDefinition,
        };

        var isReadOnly = field.Attributes.HasFlag(FieldAttributes.InitOnly);
        if (!isReadOnly)
        {
            var set = GetSet(field, name);
            forwardedField.Set = set;
            typeDefinition.Methods.Add(set);
            propertyDefinition.SetMethod = set;
        }

        forwardedField.IsReadOnly = isReadOnly;
        foreach (var customAttribute in field.CustomAttributes)
        {
            propertyDefinition.CustomAttributes.Add(customAttribute);
        }

        field.CustomAttributes.Add(new CustomAttribute(referenceFinder.CompilerGeneratedReference));
        typeDefinition.Properties.Add(propertyDefinition);

        ForwardedFields.Add(field, forwardedField);
    }

    MethodDefinition GetGet(FieldDefinition field, string name)
    {
        var get = new MethodDefinition("get_" + name,
            MethodAttributes.Public | MethodAttributes.SpecialName |
            MethodAttributes.HideBySig, field.FieldType);
        var instructions = get.Body.Instructions;
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldfld, field));
        instructions.Add(Instruction.Create(OpCodes.Stloc_0));
        var inst = Instruction.Create(OpCodes.Ldloc_0);
        instructions.Add(Instruction.Create(OpCodes.Br_S, inst));
        instructions.Add(inst);
        instructions.Add(Instruction.Create(OpCodes.Ret));
        get.Body.Variables.Add(new VariableDefinition(field.FieldType));
        get.Body.InitLocals = true;
        get.SemanticsAttributes = MethodSemanticsAttributes.Getter;
        get.CustomAttributes.Add(new CustomAttribute(referenceFinder.CompilerGeneratedReference));
        return get;
    }

    MethodDefinition GetSet(FieldDefinition field, string name)
    {
        var set = new MethodDefinition("set_" + name,
            MethodAttributes.Public | MethodAttributes.SpecialName |
            MethodAttributes.HideBySig, typeSystem.VoidReference);
        var instructions = set.Body.Instructions;
        instructions.Add(Instruction.Create(OpCodes.Ldarg_0));
        instructions.Add(Instruction.Create(OpCodes.Ldarg_1));
        instructions.Add(Instruction.Create(OpCodes.Stfld, field));
        instructions.Add(Instruction.Create(OpCodes.Ret));
        set.Parameters.Add(new ParameterDefinition("value", ParameterAttributes.None, field.FieldType));
        set.SemanticsAttributes = MethodSemanticsAttributes.Setter;
        set.CustomAttributes.Add(new CustomAttribute(referenceFinder.CompilerGeneratedReference));
        return set;
    }

    public void Execute()
    {
        foreach (var type in allTypes)
        {
            if (type.IsInterface)
            {
                continue;
            }

            if (type.IsValueType)
            {
                continue;
            }

            if (type.IsEnum)
            {
                continue;
            }

            Process(type);
        }
    }
}