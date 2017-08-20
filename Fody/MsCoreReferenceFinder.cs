using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

public class MsCoreReferenceFinder
{
    ModuleWeaver moduleWeaver;
    IAssemblyResolver assemblyResolver;
    Action<string> logInfo;
    public MethodReference GetMethodFromHandle;
    public TypeReference MethodInfoTypeReference;
    public MethodReference PropertyReference;
    public MethodReference CompilerGeneratedReference;

    public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver, Action<string> logInfo)
    {
        this.moduleWeaver = moduleWeaver;
        this.assemblyResolver = assemblyResolver;
        this.logInfo = logInfo;
    }

    void AddAssemblyIfExists(string name, List<TypeDefinition> types)
    {
        try
        {
            var msCoreLibDefinition = assemblyResolver.Resolve(new AssemblyNameReference(name, null));

            if (msCoreLibDefinition != null)
            {
                types.AddRange(msCoreLibDefinition.MainModule.Types);
            }
        }
        catch (AssemblyResolutionException)
        {
            logInfo($"Failed to resolve '{name}'. So skipping its types.");
        }
    }

    public void Execute()
    {

        var types = new List<TypeDefinition>();

        AddAssemblyIfExists("mscorlib", types);
        AddAssemblyIfExists("System.Core", types);
        AddAssemblyIfExists("System.Runtime", types);
        AddAssemblyIfExists("System.Reflection", types);
        AddAssemblyIfExists("System.Linq.Expressions", types);
        AddAssemblyIfExists("netstandard", types);

        var module = moduleWeaver.ModuleDefinition;

        var methodBaseDefinition = types.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = module.ImportReference(methodBaseDefinition.Methods.First(x => x.Name == "GetMethodFromHandle"));

        var methodInfo = types.First(x => x.Name == "MethodInfo");
        MethodInfoTypeReference = module.ImportReference(methodInfo);

        var compilerGeneratedDefinition = types.First(x => x.Name == "CompilerGeneratedAttribute");
        CompilerGeneratedReference = module.ImportReference(compilerGeneratedDefinition.Methods.First(x => x.IsConstructor));

        var expressionTypeDefinition = types.First(x => x.Name == "Expression");
        var propertyMethodDefinition =
            expressionTypeDefinition.Methods.First(
                x => x.Name == "Property" && x.Parameters.Last().ParameterType.Name == "MethodInfo");
        PropertyReference = module.ImportReference(propertyMethodDefinition);
    }

}