using System.Reflection;

namespace FileDataReader;

internal class FieldCache<T>
{
    private readonly FieldInfo Field;

    public readonly bool IsArray;

    public readonly bool IsLocalisedString;

    public readonly bool IsNonInlineRelation;

    public readonly Action<T, object> Setter;

    public readonly LocaleAttribute LocaleInfo;

    public readonly Type FieldType;

    public readonly Type MetaDataFieldType;

    public bool IndexMapField { get; set; }

    public int Cardinality { get; set; } = 1;


    public FieldCache(FieldInfo field)
    {
        Field = field;
        IsArray = field.FieldType.IsArray;
        IsLocalisedString = GetStringInfo(field, out LocaleInfo);
        Setter = field.GetSetter<T>();
        Cardinality = GetCardinality(field);
        IndexMapField = ((IndexAttribute)Attribute.GetCustomAttribute(field, typeof(IndexAttribute)))?.NonInline ?? false;
        NonInlineRelationAttribute nonInlineRelationAttribute = (NonInlineRelationAttribute)Attribute.GetCustomAttribute(field, typeof(NonInlineRelationAttribute));
        IsNonInlineRelation = nonInlineRelationAttribute != null;
        FieldType = field.FieldType;
        MetaDataFieldType = (IsNonInlineRelation ? nonInlineRelationAttribute.FieldType : FieldType);
    }

    private int GetCardinality(FieldInfo field)
    {
        int? num = field.GetAttribute<CardinalityAttribute>()?.Count;
        if (!num.HasValue || !(num > 0))
        {
            return 1;
        }

        return num.Value;
    }

    private bool GetStringInfo(FieldInfo field, out LocaleAttribute attribute)
    {
        return (attribute = field.GetAttribute<LocaleAttribute>()) != null;
    }
}