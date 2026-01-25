using CsvReaderCore.Errors;

namespace CsvReaderCore.Models;

/// <summary>
/// Represents the result of a CSV parsing operation, containing both successfully parsed records and any errors encountered.
/// </summary>
/// <typeparam name="T">The type of records being parsed.</typeparam>
/// <remarks>
/// This class enforces a safe error handling pattern in lenient mode:
/// - In strict mode (StrictMode = true): Errors throw exceptions immediately during parsing
/// - In lenient mode (StrictMode = false): Errors are collected, and you must check HasErrors or Errors before accessing Records
/// 
/// The error handling enforcement prevents accidental data loss by requiring explicit acknowledgment
/// that parsing errors may have occurred.
/// </remarks>
/// <example>
/// <code>
/// var results = reader.DeserializeLines(csvLines);
/// 
/// // Pattern 1: Check for errors
/// if (results.HasErrors) 
/// {
///     Console.WriteLine($"Found {results.Errors.Count} errors");
/// }
/// var records = results.Records;
/// 
/// // Pattern 2: Log all errors
/// foreach (var error in results.Errors) 
/// {
///     _logger.LogWarning($"Line {error.LineNumber}: {error.ErrorMessage}");
/// }
/// var records = results.Records;
/// </code>
/// </example>
public sealed class CsvParseResult<T>(
    IList<T> records,
    IList<CsvParseError> errors,
    bool isStrictMode
)
{
    /// <summary>
    /// Gets a value indicating whether any errors occurred during parsing.
    /// </summary>
    /// <remarks>
    /// Accessing this property marks errors as handled, allowing subsequent access to the Records property.
    /// </remarks>
    public bool HasErrors => HandleHasErrors();

    /// <summary>
    /// Gets the list of errors that occurred during parsing.
    /// </summary>
    /// <remarks>
    /// Accessing this property marks errors as handled, allowing subsequent access to the Records property.
    /// Each error contains the line number, line content, and error message.
    /// </remarks>
    public IList<CsvParseError> Errors => GetErrors();

    /// <summary>
    /// Gets the list of successfully parsed records.
    /// </summary>
    /// <remarks>
    /// In lenient mode, you must check HasErrors or access Errors before accessing this property,
    /// otherwise an <see cref="ErrorsNotHandledException"/> will be thrown.
    /// In strict mode, this property can be accessed directly since errors would have thrown during parsing.
    /// </remarks>
    /// <exception cref="ErrorsNotHandledException">
    /// Thrown in lenient mode when attempting to access records without first checking for errors.
    /// </exception>
    public IList<T> Records => GetRecords();
    private IList<T> _records = records;
    private IList<CsvParseError> _errors = errors;
    private bool _hasHandledErrors = false;

    private bool HandleHasErrors()
    {
        _hasHandledErrors = true;
        return Errors.Count > 0;
    }

    private IList<CsvParseError> GetErrors()
    {
        _hasHandledErrors = true;
        return _errors;
    }

    private IList<T> GetRecords()
    {
        EnsureErrorsHandled();
        return _records;
    }

    private void EnsureErrorsHandled()
    {
        if (!_hasHandledErrors && !isStrictMode)
        {
            throw new ErrorsNotHandledException();
        }
    }
};
