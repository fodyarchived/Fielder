using System.Diagnostics;

public class ClassWithReadOnlyFieldInherit :
    ClassWithReadOnlyField
{
    public ClassWithReadOnlyFieldInherit()
    {
        Debug.WriteLine(Member);
    }
#pragma warning disable 108,114
    public void Method()
#pragma warning restore 108,114
    {
        Debug.WriteLine(Member);
    }
}