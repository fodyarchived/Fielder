using System.Collections.Generic;
using Mono.Cecil;

public class FieldToPropertyFinder
{
    private List<TypeDefinition> allTypes;
    public List<MethodDefinition> MethodsToProcess = new List<MethodDefinition>();

    public FieldToPropertyFinder(List<TypeDefinition> allTypes)
    {
        this.allTypes = allTypes;
    }

    public void Execute()
    {
        foreach (var type in allTypes)
        {
            if (type.IsInterface)
            {
                continue;
            }
            if (type.IsEnum)
            {
                continue;
            }
            foreach (var method in type.Methods)
            {
                if (method.IsGetter || method.IsSetter)
                {
                    continue;
                }
                MethodsToProcess.Add(method);
            }
            foreach (var property in type.Properties)
            {
                MethodsToProcess.Add(property.GetMethod);
                MethodsToProcess.Add(property.SetMethod);
            }
        }
    }
}