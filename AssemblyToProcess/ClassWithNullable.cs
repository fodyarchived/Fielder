using System.Diagnostics;

public class ClassWithNullable
{
    public int? Member;

    public ClassWithNullable()
    {
        Member = 3;
        Debug.WriteLine(Member);
    }
    public void Method()
    {
        Member = 10;
        Debug.WriteLine(Member);
    }
    public static int StaticMethod()
    {
        var classWithNullable = new ClassWithNullable();
        return classWithNullable.Member.Value;
    }
}