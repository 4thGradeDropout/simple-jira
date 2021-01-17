using System.Reflection;

namespace SimpleJira.Interface.Metadata
{
    public interface IJiraFieldMetadata
    {
        string FieldName { get; }
        PropertyInfo Property { get; }
    }
}