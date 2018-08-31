using System.Collections.Generic;
using System.Linq;
using Fody;

public class ModuleWeaver:BaseModuleWeaver
{
    public override void Execute()
    {
        var referenceFinder = new ReferenceFinder(ModuleDefinition, FindType);
        referenceFinder.Execute();
        var allTypes = ModuleDefinition.GetTypes().ToList();

        var finder = new MethodFinder(allTypes);
        finder.Execute();
        var converter = new FieldToPropertyConverter(this, referenceFinder, allTypes);
        converter.Execute();
        var forwarder = new FieldToPropertyForwarder(this, converter, referenceFinder, finder);
        forwarder.Execute();
    }

    public override IEnumerable<string> GetAssembliesForScanning()
    {
        yield return "System.Reflection";
        yield return "System.Linq.Expressions";
    }

    public override bool ShouldCleanReference => true;
}