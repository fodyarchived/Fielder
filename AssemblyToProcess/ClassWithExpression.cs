using System;
using System.Linq.Expressions;

public class ClassWithExpression
{
    public string Member = "InitialValue";

    public ClassWithExpression Execute()
    {
        return Select(x => new ClassWithExpression { Member = "d"} );
    }

    public ClassWithExpression Select(Expression<Func<ClassWithExpression, ClassWithExpression>> selector)
    {
        return selector.Compile()(this);
    }
}