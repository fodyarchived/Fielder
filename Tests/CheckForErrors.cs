using System.Linq;
using Fody;
using Xunit;
#pragma warning disable 618

public class CheckForErrors
{
    [Fact]
    public void VerifyRefError()
    {
        var weavingTask = new ModuleWeaver();
        var testResult = weavingTask.ExecuteTestRun("AssemblyToProcessWithErrors.dll", false);
        var errors = testResult.Errors.Select(x=>x.Text).ToList();
        Assert.Contains("Method 'ClassUsingOutParam.Method' uses member 'ClassWithField.Member' as a 'ref' or 'out' parameter. This is not supported by Fielder. Please convert this field to a property manually.", errors);
        Assert.Contains("Method 'ClassUsingRefParam.Method' uses member 'ClassWithField.Member' as a 'ref' or 'out' parameter. This is not supported by Fielder. Please convert this field to a property manually.", errors);
        Assert.Equal(2, errors.Count);
    }
}