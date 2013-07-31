class ClassUsingRefParam
{
    void Method()
    {
        var test = new ClassWithField();
        MethodWithRef(ref test.Member);
    }

    void MethodWithRef(ref string a)
    {
    }
}