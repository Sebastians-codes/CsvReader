namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when a property referenced in column mapping doesn't exist on the target type.
/// </summary>
/// <remarks>
/// This exception indicates a programming error where the column mapping configuration
/// references a property name that doesn't exist on the target class. This is caught
/// at runtime during deserialization.
/// </remarks>
/// <example>
/// <code>
/// // Target class has: Name, Age
/// // Mapping references: "Email" property that doesn't exist
/// // Throws: Property 'Email' not found on type TestPerson
/// </code>
/// </example>
public class PropertyNotFoundException(string propertyName, Type targetType) 
    : CsvParseException($"Property '{propertyName}' not found on type {targetType.Name}")
{
    /// <summary>
    /// Gets the name of the property that was not found.
    /// </summary>
    public string PropertyName { get; } = propertyName;
    
    /// <summary>
    /// Gets the type that was searched for the property.
    /// </summary>
    public Type TargetType { get; } = targetType;
}
