using System.Diagnostics;

public class ClassWithConstField
{
    public const string Member = "InitialValue";


    public void Method()
    {
        Debug.WriteLine(Member);
    }
}