using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Fody;
using Xunit;
#pragma warning disable 618

public class IntegrationTests
{
    static TestResult testResult;

    static IntegrationTests()
    {
        var weavingTask = new ModuleWeaver();
        testResult = weavingTask.ExecuteTestRun("AssemblyToProcess.dll");
    }
#if DEBUG

    [Fact]
    public void ClassWithExpression()
    {
        var instance = testResult.GetInstance("ClassWithExpression");
        Assert.NotNull(instance.Execute());
    }

#endif

    [Fact]
    public void ClassWithField()
    {
        var instance = testResult.GetInstance("ClassWithField");

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
        var instance = testResult.GetInstance("ClassWithFieldInherit");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("Foo", instance.Member);
    }

    [Fact]
    public void ClassWithReadOnlyField()
    {
        var instance = testResult.GetInstance("ClassWithReadOnlyField");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("InitialValue", instance.Member);
    }

    [Fact]
    public void ClassWithReadOnlyFieldInherit()
    {
        var instance = testResult.GetInstance("ClassWithReadOnlyFieldInherit");

        Type type = instance.GetType();
        Assert.NotNull(type.GetProperty("Member"));
        Assert.Equal("InitialValue", instance.Member);
    }

    [Fact]
    public void ClassWithConstField()
    {
        var instance = testResult.GetInstance("ClassWithConstField");

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