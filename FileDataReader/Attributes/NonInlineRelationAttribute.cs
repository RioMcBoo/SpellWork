namespace FileDataReader;

public class NonInlineRelationAttribute : Attribute
{
    public readonly Type FieldType;

    public NonInlineRelationAttribute(Type fieldType)
    {
        FieldType = fieldType;
    }
}