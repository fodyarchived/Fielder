using System.Diagnostics;

public class ClassWithReadOnlyField
{
	public readonly string Member = "InitialValue";

    public ClassWithReadOnlyField()
    {
        Debug.WriteLine(Member);
    }
// ReSharper disable UnusedParameter.Local
    public ClassWithReadOnlyField(int foo)
// ReSharper restore UnusedParameter.Local
    {
        Member = "sdfsdf";
        Debug.WriteLine(Member);
    }

    public void Method()
    {
        Debug.WriteLine(Member);
    }
}