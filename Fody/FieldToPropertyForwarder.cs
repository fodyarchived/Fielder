using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;

public class FieldToPropertyForwarder
{
    ModuleWeaver moduleWeaver;
    MsCoreReferenceFinder msCoreReferenceFinder;
    MethodFinder methodFinder;
    Dictionary<FieldDefinition, ForwardedField> forwardedFields;

    public FieldToPropertyForwarder(ModuleWeaver moduleWeaver, FieldToPropertyConverter fieldToPropertyConverter, MsCoreReferenceFinder msCoreReferenceFinder, MethodFinder methodFinder)
    {
        this.moduleWeaver = moduleWeaver;
        this.msCoreReferenceFinder = msCoreReferenceFinder;
        this.methodFinder = methodFinder;
        forwardedFields = fieldToPropertyConverter.ForwardedFields;
    }

    public void Execute()
    {
        foreach (var methodDefinition in methodFinder.MethodsToProcess)
        {
            Replace(methodDefinition);
        }
    }

    void Replace(MethodDefinition methodDefinition)
    {
        if (methodDefinition == null)
        {
            return;
        }
        if (methodDefinition.IsAbstract)
        {
            return;
        }
        //for delegates
        if (methodDefinition.Body == null)
        {
            return;
        }
        methodDefinition.Body.SimplifyMacros();
        var actions = new List<Action<Collection<Instruction>>>();
        var instructions = methodDefinition.Body.Instructions;
        foreach (var instruction in instructions)
        {
            var fieldDefinition = instruction.Operand as FieldDefinition;
            if (fieldDefinition == null)
            {
                continue;
            }
            if (instruction.OpCode == OpCodes.Ldfld)
            {
                ForwardedField forwardedField;
                if (forwardedFields.TryGetValue(fieldDefinition, out forwardedField))
                {
                    instruction.OpCode = OpCodes.Callvirt;
                    instruction.Operand = forwardedField.Get;
                }
                continue;
            }
            if (instruction.OpCode == OpCodes.Ldflda)
            {
                ForwardedField forwardedField;
                if (forwardedFields.TryGetValue(fieldDefinition, out forwardedField))
                {
                    if (instruction.Next.IsRefOrOut())
                    {
                        var format = string.Format("Method '{0}.{1}' uses member '{2}.{3}' as a 'ref' or 'out' parameter. This is not supported by Fielder. Please convert this field to a property manually.", methodDefinition.DeclaringType.Name, methodDefinition.Name, forwardedField.DeclaringType.Name, forwardedField.PropertyDefinition.Name);
                        moduleWeaver.LogError(format);
                        continue;
                    }
                    methodDefinition.Body.InitLocals = true;
                    var variableDefinition = new VariableDefinition(forwardedField.FieldType);
                    methodDefinition.Body.Variables.Add(variableDefinition);

                    instruction.OpCode = OpCodes.Callvirt;
                    instruction.Operand = forwardedField.Get;
                    var localCopy = instruction;
                    actions.Add(collection =>
                        {
                            var indexOf = collection.IndexOf(localCopy) + 1;
                            collection.Insert(indexOf, Instruction.Create(OpCodes.Stloc, variableDefinition));
                            collection.Insert(indexOf + 1, Instruction.Create(OpCodes.Ldloca, variableDefinition));
                        });
                }
                continue;
            }

            if (instruction.OpCode == OpCodes.Ldtoken)
            {
                actions.Add(ProcessLdToken(instruction, fieldDefinition));

                continue;
            }

            if (instruction.OpCode == OpCodes.Stfld)
            {
                ForwardedField forwardedField;
                if (forwardedFields.TryGetValue(fieldDefinition, out forwardedField))
                {
                    if (forwardedField.IsReadOnly)
                    {
                        continue;
                    }
                    if (methodDefinition.IsConstructor && forwardedField.DeclaringType == methodDefinition.DeclaringType)
                    {
                        continue;
                    }
                    instruction.OpCode = OpCodes.Callvirt;
                    instruction.Operand = forwardedField.Set;
                }
                continue;
            }
        }
        foreach (var action in actions)
        {
            action(instructions);
        }
        methodDefinition.Body.OptimizeMacros();
    }

    Action<Collection<Instruction>> ProcessLdToken(Instruction instruction, FieldDefinition fieldDefinition)
    {
        ForwardedField forwardedField;
        if (!forwardedFields.TryGetValue(fieldDefinition, out forwardedField))
        {
            return collection => { };
        }

        instruction.Operand = forwardedField.Get;
        var next = instruction.Next;
        if (next == null)
        {
            return collection => { };
        }
        var nextNext = next.Next;
        if (nextNext == null)
        {
            return collection => { };
        }
        if (next.OpCode != OpCodes.Call || nextNext.OpCode != OpCodes.Call)
        {
            return collection => { };
        }
        var nextMethod = next.Operand as MethodReference;
        if (nextMethod == null)
        {
            return collection => { };
        }
        var nextNextMethod = nextNext.Operand as MethodReference;
        if (nextNextMethod == null)
        {
            return collection => { };
        }
        if (nextMethod.FullName != "System.Reflection.FieldInfo System.Reflection.FieldInfo::GetFieldFromHandle(System.RuntimeFieldHandle)")
        {
            return collection => { };
        }
        if (nextNextMethod.FullName != "System.Linq.Expressions.MemberExpression System.Linq.Expressions.Expression::Field(System.Linq.Expressions.Expression,System.Reflection.FieldInfo)")
        {
            return collection => { };
        }
        next.Operand = msCoreReferenceFinder.GetMethodFromHandle;
        Action<Collection<Instruction>> processLdToken = collection =>
                                                             {
                                                                 var indexOf = collection.IndexOf(nextNext);
                                                                 collection.Insert(indexOf, Instruction.Create(OpCodes.Castclass, msCoreReferenceFinder.MethodInfoTypeReference));
                                                             };
        //nextNext.Next
        nextNext.Operand = msCoreReferenceFinder.PropertyReference;
        return processLdToken;
    }
}