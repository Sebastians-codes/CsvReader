namespace Csv.Reader.Errors;

/// <summary>
/// Exception thrown when a column index is out of range.
/// </summary>
internal class ColumnIndexOutOfRangeException(int requestedIndex, int availableColumns, int lineNumber)
    : CsvParseException($"Column index {requestedIndex} is out of range. Line has {availableColumns} columns", lineNumber)
{
    public int RequestedIndex { get; } = requestedIndex;
    public int AvailableColumns { get; } = availableColumns;
}
