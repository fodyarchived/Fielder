using Mono.Cecil;

public class ForwardedField
{
    public MethodDefinition Get;
    public MethodDefinition Set;

    public bool IsReadOnly;

    public TypeReference FieldType;

    public TypeDefinition DeclaringType;
    public PropertyDefinition PropertyDefinition;
}