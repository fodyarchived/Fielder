#if (DEBUG)
#endif
using System;
using System.Reflection;
using NUnit.Framework;

public abstract class BaseTaskTests
{
    string projectPath;
    Assembly assembly;

    protected BaseTaskTests(string projectPath)
    {

#if (!DEBUG)

            projectPath = projectPath.Replace("Debug", "Release");
#endif
        this.projectPath = projectPath;
    }

    [TestFixtureSetUp]
    public void Setup()
    {
        var weaverHelper = new WeaverHelper(projectPath);
        assembly = weaverHelper.Assembly;
    }


    [Test]
    public void ClassWithFields()
    {
        var instance = assembly.GetInstance("ClassWithFields");

        Type condition = instance.GetType();
        Assert.IsNotNull(condition.GetProperty("Member"));
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