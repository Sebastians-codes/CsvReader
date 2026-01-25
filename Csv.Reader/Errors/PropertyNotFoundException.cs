namespace Csv.Reader.Errors;

/// <summary>
/// Exception thrown when a property is not found on the target type.
/// </summary>
internal class PropertyNotFoundException(string propertyName, Type targetType)
    : CsvParseException($"Property '{propertyName}' not found on type {targetType.Name}")
{
    public string PropertyName { get; } = propertyName;
    public Type TargetType { get; } = targetType;
}
