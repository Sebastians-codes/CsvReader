namespace CsvReaderCore.Errors;

/// <summary>
/// Exception thrown when a required column is not found in the CSV header.
/// </summary>
internal class ColumnNotFoundException(string columnName, string[] availableColumns)
    : ColumnMappingException($"Column '{columnName}' not found in CSV. Available columns: {string.Join(", ", availableColumns)}", columnName)
{
}
