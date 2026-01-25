namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when a CSV field value cannot be converted to the target property type.
/// </summary>
/// <remarks>
/// This exception is thrown during type conversion when a CSV field value is incompatible
/// with the target property type. Common scenarios include:
/// - Invalid numeric formats (e.g., "abc" to int)
/// - Invalid date formats (e.g., "not-a-date" to DateTime)
/// - Invalid boolean values that don't match configured truthy/falsy values
/// - Unsupported types that the library doesn't know how to convert
/// </remarks>
/// <example>
/// Examples that would throw this exception:
/// <code>
/// // CSV: "Name,Age"
/// // Data: "John,not-a-number"
/// // Throws: Cannot convert value 'not-a-number' to type Int32
/// </code>
/// </example>
public class TypeConversionException : CsvParseException
{
    /// <summary>
    /// Gets the string value that failed to convert.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets the target type that the value was being converted to.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Gets the name of the property being assigned, if available.
    /// </summary>
    public string? PropertyName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConversionException"/> class.
    /// </summary>
    /// <param name="value">The string value that failed to convert.</param>
    /// <param name="targetType">The target type that the value was being converted to.</param>
    /// <param name="propertyName">The name of the property being assigned (optional).</param>
    public TypeConversionException(string value, Type targetType, string? propertyName = null)
        : base($"Cannot convert value '{value}' to type {targetType.Name}" +
               (propertyName != null ? $" for property '{propertyName}'" : ""))
    {
        Value = value;
        TargetType = targetType;
        PropertyName = propertyName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypeConversionException"/> class with an inner exception.
    /// </summary>
    /// <param name="value">The string value that failed to convert.</param>
    /// <param name="targetType">The target type that the value was being converted to.</param>
    /// <param name="innerException">The exception that caused the conversion to fail.</param>
    /// <param name="propertyName">The name of the property being assigned (optional).</param>
    public TypeConversionException(string value, Type targetType, Exception innerException, string? propertyName = null)
        : base($"Cannot convert value '{value}' to type {targetType.Name}" +
               (propertyName != null ? $" for property '{propertyName}'" : ""), innerException)
    {
        Value = value;
        TargetType = targetType;
        PropertyName = propertyName;
    }
}
