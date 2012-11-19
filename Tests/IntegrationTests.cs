using System;
using System.IO;
using System.Reflection;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class IntegrationTests
{
    Assembly assembly;

    public IntegrationTests()
    {
        var assemblyPath = Path.GetFullPath(@"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll");
#if (!DEBUG)

        assemblyPath = assemblyPath.Replace("Debug", "Release");
#endif

        var newAssembly = assemblyPath.Replace(".dll", "2.dll");
        File.Copy(assemblyPath, newAssembly, true);

        var moduleDefinition = ModuleDefinition.ReadModule(newAssembly);
        var weavingTask = new ModuleWeaver
                              {
                                  ModuleDefinition = moduleDefinition,
                              };

        weavingTask.Execute();
        moduleDefinition.Write(newAssembly);

        assembly = Assembly.LoadFile(newAssembly);
    }


    [Test]
    public void ClassWithField()
    {
        var instance = assembly.GetInstance("ClassWithField");

		Type type = instance.GetType();
		Assert.IsNotNull(type.GetProperty("Member"));
		Assert.AreEqual("InitialValue", instance.Member);
    }

    [Test]
	public void ClassWithReadOnlyField()
    {
		var instance = assembly.GetInstance("ClassWithReadOnlyField");

        Type type = instance.GetType();
        Assert.IsNotNull(type.GetProperty("Member"));
		Assert.AreEqual("InitialValue", instance.Member);

    }

    [Test]
    public void StructWithFields()
    {
        var type = assembly.GetType("StructWithFields");
        Assert.IsNotNull(type.GetField("Member"));
    }


#if(DEBUG)
    [Test]
    public void PeVerify()
    {
        Verifier.Verify(assembly.CodeBase.Remove(0, 8));
    }
#endif

}