using System.Diagnostics;

public class ClassWithFieldInherit : ClassWithField
{
    public ClassWithFieldInherit()
    {
        Member = "Foo";
        Debug.WriteLine(Member);
    }
    public void Method2()
    {
        Member = "Foo";
        Debug.WriteLine(Member);
    }
}