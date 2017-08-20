// ReSharper disable UnusedMember.Local
class ClassUsingRefParam
{
    void Method()
    {
        var test = new ClassWithField();
        MethodWithRef(ref test.Member);
    }

    // ReSharper disable once UnusedParameter.Local
    void MethodWithRef(ref string a)
    {
    }
}