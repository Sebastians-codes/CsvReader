namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when attempting to access a column index that doesn't exist in the CSV line.
/// </summary>
/// <remarks>
/// This exception is thrown when a column mapping references a column index that is greater than
/// or equal to the number of fields in the current line. This typically happens when:
/// - The CSV data has fewer columns than expected
/// - A line is malformed and missing fields
/// - The column mapping configuration is incorrect
/// </remarks>
/// <example>
/// <code>
/// // Mapping expects 3 columns (indices 0, 1, 2)
/// // CSV line has only 2 fields: "John,30"
/// // Throws: Column index 2 is out of range. Line has 2 columns
/// </code>
/// </example>
public class ColumnIndexOutOfRangeException(int requestedIndex, int availableColumns, int lineNumber)
    : CsvParseException($"Column index {requestedIndex} is out of range. Line has {availableColumns} columns", lineNumber)
{
    /// <summary>
    /// Gets the column index that was requested but doesn't exist.
    /// </summary>
    public int RequestedIndex { get; } = requestedIndex;

    /// <summary>
    /// Gets the actual number of columns available in the line.
    /// </summary>
    public int AvailableColumns { get; } = availableColumns;
}
