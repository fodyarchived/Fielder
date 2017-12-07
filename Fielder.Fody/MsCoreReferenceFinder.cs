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
            var assembly = assemblyResolver.Resolve(new AssemblyNameReference(name, null));

            if (assembly == null)
            {
                return;
            }

            var module = assembly.MainModule;
            types.AddRange(module.Types.Where(x => x.IsPublic));
            var exported = module.ExportedTypes
                .Select(x => x.Resolve())
                .Where(x => x != null && x.IsPublic);
            types.AddRange(exported);
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
        var getMethodFromHandle = methodBaseDefinition.Methods.First(x => x.Name == "GetMethodFromHandle");
        GetMethodFromHandle = module.ImportReference(getMethodFromHandle);

        var methodInfo = types.First(x => x.Name == "MethodInfo");
        MethodInfoTypeReference = module.ImportReference(methodInfo);

        var compilerGeneratedDefinition = types.First(x => x.Name == "CompilerGeneratedAttribute");
        var compilerGenerated = compilerGeneratedDefinition.Methods.First(x => x.IsConstructor);
        CompilerGeneratedReference = module.ImportReference(compilerGenerated);

        var expressionTypeDefinition = types.First(x => x.Name == "Expression");
        var propertyMethodDefinition =
            expressionTypeDefinition.Methods.First(
                x => x.Name == "Property" && x.Parameters.Last().ParameterType.Name == "MethodInfo");
        PropertyReference = module.ImportReference(propertyMethodDefinition);
    }
}