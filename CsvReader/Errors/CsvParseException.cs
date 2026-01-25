namespace CsvReader.Errors;

/// <summary>
/// Base exception for all CSV parsing related errors.
/// This is the base class for all custom CSV parsing exceptions and can be caught
/// to handle any CSV-related error.
/// </summary>
/// <remarks>
/// All CSV parsing exceptions inherit from this class, making it easy to catch
/// all CSV-related errors with a single catch block. The exception includes
/// an optional line number to help identify where the error occurred.
/// </remarks>
public class CsvParseException : Exception
{
    /// <summary>
    /// Gets the line number where the error occurred, if available.
    /// Line numbers start at 1 (not 0).
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvParseException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CsvParseException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvParseException"/> class with a specified error message and line number.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="lineNumber">The line number where the error occurred (1-based).</param>
    public CsvParseException(string message, int lineNumber) : base(message)
    {
        LineNumber = lineNumber;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvParseException"/> class with a specified error message and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CsvParseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvParseException"/> class with a specified error message, line number, and inner exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="lineNumber">The line number where the error occurred (1-based).</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CsvParseException(string message, int lineNumber, Exception innerException) : base(message, innerException)
    {
        LineNumber = lineNumber;
    }
}
