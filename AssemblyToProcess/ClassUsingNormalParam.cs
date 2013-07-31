class ClassUsingNormalParam
{
    void Method()
    {
        var test = new ClassWithField();
        MethodWithRef(test.Member);
    }

    void MethodWithRef(string a)
    {
    }
}