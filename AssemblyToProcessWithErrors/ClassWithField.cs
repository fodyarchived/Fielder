using System.Diagnostics;

public class ClassWithField 
{
    public string Member = "InitialValue";
    public void Method()
    {
        Member = "Foo";
        Debug.WriteLine(Member);
    }
}