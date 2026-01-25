namespace CsvReader.Errors;

/// <summary>
/// Base exception for column mapping related errors.
/// </summary>
/// <remarks>
/// This exception serves as a base class for all column mapping related errors.
/// Specific mapping issues derive from this class, such as ColumnNotFoundException.
/// It can be caught to handle any column mapping error generically.
/// </remarks>
public class ColumnMappingException : CsvParseException
{
    /// <summary>
    /// Gets the column name involved in the mapping error, if applicable.
    /// </summary>
    public string? ColumnName { get; }

    /// <summary>
    /// Gets the property name involved in the mapping error, if applicable.
    /// </summary>
    public string? PropertyName { get; }

    public ColumnMappingException(string message) : base(message)
    {
    }

    public ColumnMappingException(string message, string columnName) : base(message)
    {
        ColumnName = columnName;
    }

    public ColumnMappingException(string message, string columnName, string propertyName) : base(message)
    {
        ColumnName = columnName;
        PropertyName = propertyName;
    }
}
