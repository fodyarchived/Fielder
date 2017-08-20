// ReSharper disable UnusedMember.Local
class ClassUsingOutParam
{
    void Method()
    {
        var test = new ClassWithField();
        MethodWithRef(out test.Member);
    }

    void MethodWithRef(out string a)
    {
        a = null;
    }
}