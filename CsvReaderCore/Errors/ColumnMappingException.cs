namespace CsvReaderCore.Errors;

/// <summary>
/// Base exception for column mapping related errors.
/// </summary>
internal class ColumnMappingException : CsvParseException
{
    public string? ColumnName { get; }
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
