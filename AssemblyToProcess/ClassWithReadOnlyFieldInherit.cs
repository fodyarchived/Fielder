using System.Diagnostics;

public class ClassWithReadOnlyFieldInherit:ClassWithReadOnlyField
{

    public ClassWithReadOnlyFieldInherit()
    {
        Debug.WriteLine(Member);
    }
    public void Method()
    {
        Debug.WriteLine(Member);
    }
}