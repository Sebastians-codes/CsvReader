namespace CsvReader.Models;

/// <summary>
/// Configuration options for CSV parsing behavior.
/// </summary>
/// <remarks>
/// This class provides comprehensive control over how CSV data is parsed, including
/// delimiter selection, header handling, field trimming, error handling mode, and
/// boolean value interpretation.
/// </remarks>
public class CsvParserOptions
{
    /// <summary>
    /// Gets or sets the delimiter character used to separate fields.
    /// </summary>
    /// <remarks>
    /// Common delimiters include comma (,), semicolon (;), tab (\t), and pipe (|).
    /// Default is comma (,).
    /// </remarks>
    public char Delimiter { get; set; } = ',';

    /// <summary>
    /// Gets or sets a value indicating whether the first line contains column headers.
    /// </summary>
    /// <remarks>
    /// When true, the first line is used to map column names to properties.
    /// When false, columns are mapped by index (0-based).
    /// Default is true.
    /// </remarks>
    public bool HasHeaderRow { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to skip empty or whitespace-only lines.
    /// </summary>
    /// <remarks>
    /// When true, empty lines are ignored during parsing.
    /// When false in strict mode, empty lines throw an exception.
    /// When false in lenient mode, empty lines are added to the error collection.
    /// Default is true.
    /// </remarks>
    public bool SkipEmptyLines { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to trim leading and trailing whitespace from field values.
    /// </summary>
    /// <remarks>
    /// When true, whitespace is removed from the beginning and end of each field.
    /// When false, whitespace is preserved exactly as it appears in the CSV.
    /// Default is true.
    /// </remarks>
    public bool TrimFields { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether column header matching should be case-insensitive.
    /// </summary>
    /// <remarks>
    /// When true, "Name", "NAME", and "name" are all treated as the same column.
    /// When false, header names must match exactly (case-sensitive).
    /// Default is true.
    /// </remarks>
    public bool CaseInsensitiveHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use strict mode error handling.
    /// </summary>
    /// <remarks>
    /// Strict Mode (true):
    /// - Throws an exception immediately on the first error
    /// - Use when you need guaranteed data integrity
    /// - No partial results are returned
    /// 
    /// Lenient Mode (false):
    /// - Collects errors and continues parsing
    /// - Returns successfully parsed records along with error details
    /// - Use when you want to process as much valid data as possible
    /// - Requires checking HasErrors or Errors before accessing Records
    /// 
    /// Default is false (lenient mode).
    /// </remarks>
    public bool StrictMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the set of string values that should be interpreted as boolean true.
    /// </summary>
    /// <remarks>
    /// The comparison is case-insensitive by default (using OrdinalIgnoreCase).
    /// Default values: "true", "1", "yes"
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new CsvParserOptions
    /// {
    ///     BooleanTruthyValues = new HashSet&lt;string&gt;(StringComparer.OrdinalIgnoreCase)
    ///     {
    ///         "Y", "Yes", "T", "True", "On", "Enabled"
    ///     }
    /// };
    /// </code>
    /// </example>
    public HashSet<string> BooleanTruthyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "true", "1", "yes"
    };

    /// <summary>
    /// Gets or sets the set of string values that should be interpreted as boolean false.
    /// </summary>
    /// <remarks>
    /// The comparison is case-insensitive by default (using OrdinalIgnoreCase).
    /// Default values: "false", "0", "no"
    /// </remarks>
    /// <example>
    /// <code>
    /// var options = new CsvParserOptions
    /// {
    ///     BooleanFalsyValues = new HashSet&lt;string&gt;(StringComparer.OrdinalIgnoreCase)
    ///     {
    ///         "N", "No", "F", "False", "Off", "Disabled"
    ///     }
    /// };
    /// </code>
    /// </example>
    public HashSet<string> BooleanFalsyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "false", "0", "no"
    };
}
