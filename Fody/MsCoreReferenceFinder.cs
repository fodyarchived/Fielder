using System.IO;
using System.Linq;
using Mono.Cecil;

public class MsCoreReferenceFinder
{
    ModuleWeaver moduleWeaver;
    IAssemblyResolver assemblyResolver;
    public MethodReference GetMethodFromHandle;
    public TypeReference MethodInfoTypeReference;
    public MethodReference PropertyReference;
    public MethodReference CompilerGeneratedReference;

    public MsCoreReferenceFinder(ModuleWeaver moduleWeaver, IAssemblyResolver assemblyResolver)
    {
        this.moduleWeaver = moduleWeaver;
        this.assemblyResolver = assemblyResolver;
    }


    public void Execute()
    {
        var msCoreLibDefinition = assemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
        var msCoreTypes = msCoreLibDefinition.MainModule.Types;

        var objectDefinition = msCoreTypes.FirstOrDefault(x => x.Name == "Object");
        if (objectDefinition == null)
        {
            ExecuteWinRT();
            return;
        }
        var module = moduleWeaver.ModuleDefinition;

        var methodBaseDefinition = msCoreTypes.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = module.ImportReference(methodBaseDefinition.Methods.First(x => x.Name == "GetMethodFromHandle"));

        var methodInfo = msCoreTypes.FirstOrDefault(x => x.Name == "MethodInfo");
        MethodInfoTypeReference = module.ImportReference(methodInfo);

        var compilerGeneratedDefinition = msCoreTypes.First(x => x.Name == "CompilerGeneratedAttribute");
        CompilerGeneratedReference = module.ImportReference(compilerGeneratedDefinition.Methods.First(x => x.IsConstructor));

        var systemCoreDefinition = GetSystemCoreDefinition();


        var expressionTypeDefinition = systemCoreDefinition.MainModule.Types.First(x => x.Name == "Expression");
        var propertyMethodDefinition =
            expressionTypeDefinition.Methods.First(
                x => x.Name == "Property" && x.Parameters.Last().ParameterType.Name == "MethodInfo");
        PropertyReference = module.ImportReference(propertyMethodDefinition);

    }

    public void ExecuteWinRT()
    {
        var systemRuntime = assemblyResolver.Resolve(new AssemblyNameReference("System.Runtime", null));
        var systemRuntimeTypes = systemRuntime.MainModule.Types;

        var module = moduleWeaver.ModuleDefinition;

        var compilerGeneratedDefinition = systemRuntimeTypes.First(x => x.Name == "CompilerGeneratedAttribute");
        CompilerGeneratedReference = module.ImportReference(compilerGeneratedDefinition.Methods.First(x => x.IsConstructor));

        var systemReflection = assemblyResolver.Resolve(new AssemblyNameReference("System.Reflection", null));
        var methodBaseDefinition = systemReflection.MainModule.Types.First(x => x.Name == "MethodBase");
        GetMethodFromHandle = module.ImportReference(methodBaseDefinition.Methods.First(x => x.Name == "GetMethodFromHandle"));

        var methodInfo = systemReflection.MainModule.Types.FirstOrDefault(x => x.Name == "MethodInfo");
        MethodInfoTypeReference = module.ImportReference(methodInfo);

        var systemLinqExpressions = assemblyResolver.Resolve(new AssemblyNameReference("System.Linq.Expressions", null));
        var expressionTypeDefinition = systemLinqExpressions.MainModule.Types.First(x => x.Name == "Expression");
        var propertyMethodDefinition = expressionTypeDefinition.Methods.First(x => x.Name == "Property" && x.Parameters.Last().ParameterType.Name == "MethodInfo");
        PropertyReference = module.ImportReference(propertyMethodDefinition);
    }


    AssemblyDefinition GetSystemCoreDefinition()
    {
        try
        {
            return assemblyResolver.Resolve(new AssemblyNameReference("System.Core", null));
        }
        catch (FileNotFoundException)
        {
            throw new WeavingException(
                "Could not resolve System.Core. Please ensure you are using .net 3.5 or higher.");
        }
    }
}