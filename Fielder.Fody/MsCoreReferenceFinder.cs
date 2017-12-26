using System;
using System.Linq;
using Mono.Cecil;

public class MsCoreReferenceFinder
{
    ModuleDefinition module;
    Func<string, TypeDefinition> findType;
    public MethodReference GetMethodFromHandle;
    public TypeReference MethodInfoTypeReference;
    public MethodReference PropertyReference;
    public MethodReference CompilerGeneratedReference;

    public MsCoreReferenceFinder(ModuleDefinition module, Func<string, TypeDefinition> findType)
    {
        this.module = module;
        this.findType = findType;
    }

    public void Execute()
    {
        var methodBaseDefinition = findType("System.Reflection.MethodBase");
        var getMethodFromHandle = methodBaseDefinition.Methods.First(x => x.Name == "GetMethodFromHandle");
        GetMethodFromHandle = module.ImportReference(getMethodFromHandle);

        var methodInfo = findType("System.Reflection.MethodInfo");
        MethodInfoTypeReference = module.ImportReference(methodInfo);

        var compilerGeneratedDefinition = findType("System.Runtime.CompilerServices.CompilerGeneratedAttribute");
        var compilerGenerated = compilerGeneratedDefinition.Methods.First(x => x.IsConstructor);
        CompilerGeneratedReference = module.ImportReference(compilerGenerated);
        
        var expressionTypeDefinition = findType("System.Linq.Expressions.Expression");
        var propertyMethodDefinition =
            expressionTypeDefinition.Methods.First(
                x => x.Name == "Property" && x.Parameters.Last().ParameterType.Name == "MethodInfo");
        PropertyReference = module.ImportReference(propertyMethodDefinition);
    }
}