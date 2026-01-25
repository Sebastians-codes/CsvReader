namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when attempting to access records without first acknowledging errors in lenient mode.
/// </summary>
/// <remarks>
/// This exception enforces a safe error handling pattern in lenient mode (StrictMode = false).
/// Before accessing the Records property, you must acknowledge that errors may have occurred by either:
/// - Checking the HasErrors property
/// - Accessing the Errors property to inspect them
/// 
/// This prevents silent data loss by forcing explicit acknowledgment of potential errors.
/// In strict mode, this exception is never thrown because errors cause immediate exceptions during parsing.
/// </remarks>
/// <example>
/// <code>
/// // ❌ Wrong - will throw ErrorsNotHandledException
/// var results = reader.DeserializeLines(csv);
/// var records = results.Records;
/// 
/// // ✅ Correct - check for errors first
/// var results = reader.DeserializeLines(csv);
/// if (results.HasErrors) 
/// {
///     foreach (var error in results.Errors)
///         Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
/// }
/// var records = results.Records;
/// </code>
/// </example>
public class ErrorsNotHandledException : CsvParseException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorsNotHandledException"/> class.
    /// </summary>
    public ErrorsNotHandledException()
        : base("Errors have not been handled. Please check for errors before accessing the result.")
    {
    }
}
