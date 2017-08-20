using System;
using System.Linq;
using Mono.Cecil;

public class ModuleWeaver
{
    public Action<string> LogError { get; set; }
    public Action<string> LogInfo { get; set; }
    public Action<string> LogWarning { get; set; }
    public ModuleDefinition ModuleDefinition { get; set; }

    public ModuleWeaver()
    {
        LogError = s => { };
        LogInfo = s => { };
        LogWarning = s => { };
    }

    public void Execute()
    {
        var msCoreReferenceFinder = new MsCoreReferenceFinder(this, ModuleDefinition.AssemblyResolver, LogInfo);
        msCoreReferenceFinder.Execute();
        var allTypes = ModuleDefinition.GetTypes().ToList();

        var finder = new MethodFinder(allTypes);
        finder.Execute();
        var converter = new FieldToPropertyConverter(this, msCoreReferenceFinder, ModuleDefinition.TypeSystem, allTypes);
        converter.Execute();
        var forwarder = new FieldToPropertyForwarder(this, converter, msCoreReferenceFinder, finder);
        forwarder.Execute();
    }
}