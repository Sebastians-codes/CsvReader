namespace CsvReaderCore.Errors;

/// <summary>
/// Exception thrown when a field value cannot be converted to the target type.
/// </summary>
internal class TypeConversionException : CsvParseException
{
    public string Value { get; }
    public Type TargetType { get; }
    public string? PropertyName { get; }

    public TypeConversionException(string value, Type targetType, string? propertyName = null)
        : base($"Cannot convert value '{value}' to type {targetType.Name}" +
               (propertyName != null ? $" for property '{propertyName}'" : ""))
    {
        Value = value;
        TargetType = targetType;
        PropertyName = propertyName;
    }

    public TypeConversionException(string value, Type targetType, Exception innerException, string? propertyName = null)
        : base($"Cannot convert value '{value}' to type {targetType.Name}" +
               (propertyName != null ? $" for property '{propertyName}'" : ""), innerException)
    {
        Value = value;
        TargetType = targetType;
        PropertyName = propertyName;
    }
}
