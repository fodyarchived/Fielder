class ClassUsingNormalParam
{
    void Method()
    {
        var test = new ClassWithField();
        MethodWithRef(test.Member);
    }

    // ReSharper disable once UnusedParameter.Local
    void MethodWithRef(string a)
    {
    }
}