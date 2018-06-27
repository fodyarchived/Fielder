using System;
using System.Linq.Expressions;

public class ClassWithExpression
{
    public string Member = "InitialValue";

    public ClassWithExpression Execute()
    {
        return Select(()=> new ClassWithExpression { Member = "d"} );
    }

    public ClassWithExpression Select(Expression<Func<ClassWithExpression>> selector)
    {
        return selector.Compile()();
    }
}