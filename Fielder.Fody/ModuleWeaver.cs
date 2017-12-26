using System.Collections.Generic;
using System.Linq;
using Fody;

public class ModuleWeaver:BaseModuleWeaver
{
    public override void Execute()
    {
        var msCoreReferenceFinder = new MsCoreReferenceFinder(ModuleDefinition, FindType);
        msCoreReferenceFinder.Execute();
        var allTypes = ModuleDefinition.GetTypes().ToList();

        var finder = new MethodFinder(allTypes);
        finder.Execute();
        var converter = new FieldToPropertyConverter(this, msCoreReferenceFinder, ModuleDefinition.TypeSystem, allTypes);
        converter.Execute();
        var forwarder = new FieldToPropertyForwarder(this, converter, msCoreReferenceFinder, finder);
        forwarder.Execute();
        CleanReferences();
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "mscorlib";
        yield return "System.Core";
        yield return "System.Runtime";
        yield return "System.Reflection";
        yield return "System.Linq.Expressions";
        yield return "netstandard";
    }

    public void CleanReferences()
    {
        var referenceToRemove = ModuleDefinition.AssemblyReferences.FirstOrDefault(x => x.Name == "Fielder");
        if (referenceToRemove == null)
        {
            LogDebug("\tNo reference to 'Fielder' found. References not modified.");
            return;
        }

        ModuleDefinition.AssemblyReferences.Remove(referenceToRemove);
        LogInfo("\tRemoving reference to 'Fielder'.");
    }
}