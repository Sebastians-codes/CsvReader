namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when a required column is not found in the CSV header row.
/// </summary>
/// <remarks>
/// This exception is thrown when column mapping attempts to find a column by name
/// in the header row, but the column doesn't exist. Common causes include:
/// - The CSV header doesn't contain the expected column name
/// - Column names have different casing and CaseInsensitiveHeaders is false
/// - The column mapping configuration references a non-existent column
/// - Typos in column names
/// </remarks>
/// <example>
/// <code>
/// // CSV Header: "Name,Age"
/// // Mapping expects: "Email" column
/// // Throws: Column 'Email' not found in CSV. Available columns: Name, Age
/// </code>
/// </example>
public class ColumnNotFoundException(string columnName, string[] availableColumns) 
    : ColumnMappingException($"Column '{columnName}' not found in CSV. Available columns: {string.Join(", ", availableColumns)}", columnName)
{
}
