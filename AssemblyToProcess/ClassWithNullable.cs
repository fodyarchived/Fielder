public class ClassWithNullable
{
    public int? Field;
    public static int Foo()
    {
        var classWithNullable = new ClassWithNullable();
        return classWithNullable.Field.Value;
    }
}