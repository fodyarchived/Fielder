using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using NUnit.Framework;

[TestFixture]
public class IntegrationTests
{
    Assembly assembly;
    string afterAssemblyPath;
    string beforeAssemblyPath;

    public IntegrationTests()
    {
        beforeAssemblyPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\AssemblyToProcess\bin\Debug\AssemblyToProcess.dll"));
#if (!DEBUG)

        beforeAssemblyPath = beforeAssemblyPath.Replace("Debug", "Release");
#endif

        afterAssemblyPath = beforeAssemblyPath.Replace(".dll", "2.dll");
        File.Copy(beforeAssemblyPath, afterAssemblyPath, true);

        using (var moduleDefinition = ModuleDefinition.ReadModule(beforeAssemblyPath))
        {
            var weavingTask = new ModuleWeaver
            {
                ModuleDefinition = moduleDefinition,
            };

            weavingTask.Execute();
            moduleDefinition.Write(afterAssemblyPath);
        }

        assembly = Assembly.LoadFile(afterAssemblyPath);
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
    public void EnsureCompilerGeneratedOnField()
    {
        var type = assembly.GetType("ClassWithField", true);
        var fieldInfo = type.GetField("<Member>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>());
    }

    [Test]
    public void ClassWithFieldInherit()
    {
        var instance = assembly.GetInstance("ClassWithFieldInherit");

        Type type = instance.GetType();
        Assert.IsNotNull(type.GetProperty("Member"));
        Assert.AreEqual("Foo", instance.Member);
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
    public void ClassWithReadOnlyFieldInherit()
    {
        var instance = assembly.GetInstance("ClassWithReadOnlyFieldInherit");

        Type type = instance.GetType();
        Assert.IsNotNull(type.GetProperty("Member"));
        Assert.AreEqual("InitialValue", instance.Member);
    }

    [Test]
    public void ClassWithConstField()
    {
        var instance = assembly.GetInstance("ClassWithConstField");

        Type type = instance.GetType();
        var fieldInfo = type.GetField("Member");
        Assert.IsNotNull(fieldInfo);
        Assert.AreEqual("InitialValue", fieldInfo.GetValue(null));

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
        Verifier.Verify(beforeAssemblyPath, afterAssemblyPath);
    }

#endif

}