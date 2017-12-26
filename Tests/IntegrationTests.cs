using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fody;
using Xunit;
#pragma warning disable 618

public class IntegrationTests
{
    TestResult testResult;

    public IntegrationTests()
    {
//#if NETCOREAPP2_0
//            var directory = Assembly.Load("netstandard").Location;
//            assemblyResolver.AddSearchDirectory(directory);
//#endif
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll");
    }

    [Fact]
    public void ClassWithField()
    {
        var instance = testResult.Assembly.GetInstance("ClassWithField");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("InitialValue", instance.Member);
    }

    [Fact]
    public void EnsureCompilerGeneratedOnField()
    {
        var type = testResult.Assembly.GetType("ClassWithField", true);
        var fieldInfo = type.GetField("<Member>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(fieldInfo.GetCustomAttribute<CompilerGeneratedAttribute>());
    }

    [Fact]
    public void ClassWithFieldInherit()
    {
        var instance = testResult.Assembly.GetInstance("ClassWithFieldInherit");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("Foo", instance.Member);
    }

    [Fact]
    public void ClassWithReadOnlyField()
    {
        var instance = testResult.Assembly.GetInstance("ClassWithReadOnlyField");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("InitialValue", instance.Member);
    }

    [Fact]
    public void ClassWithReadOnlyFieldInherit()
    {
        var instance = testResult.Assembly.GetInstance("ClassWithReadOnlyFieldInherit");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("InitialValue", instance.Member);
    }

    [Fact]
    public void ClassWithConstField()
    {
        var instance = testResult.Assembly.GetInstance("ClassWithConstField");

        Type type = instance.GetType();
        var fieldInfo = type.GetField("Member");
        Assert.NotNull(fieldInfo);
        Assert.Equal("InitialValue", fieldInfo.GetValue(null));
    }

    [Fact]
    public void StructWithFields()
    {
        var type = testResult.Assembly.GetType("StructWithFields");
        Assert.NotNull(type.GetField("Member"));
    }
}