namespace CsvReaderCore.Models;

/// <summary>
/// Represents a parsing error that occurred on a specific CSV line.
/// </summary>
/// <remarks>
/// This record is used in lenient mode to collect errors without stopping the parsing process.
/// Each error contains the line number, original line content, and a descriptive error message.
/// </remarks>
/// <example>
/// <code>
/// foreach (var error in results.Errors) 
/// {
///     Console.WriteLine($"Error at line {error.LineNumber}:");
///     Console.WriteLine($"  Content: {error.LineContent}");
///     Console.WriteLine($"  Message: {error.ErrorMessage}");
/// }
/// </code>
/// </example>
/// <param name="LineNumber">The line number where the error occurred (1-based, includes header row).</param>
/// <param name="LineContent">The original content of the line that caused the error.</param>
/// <param name="ErrorMessage">A descriptive message explaining what went wrong.</param>
public record CsvParseError(
    int LineNumber,
    string LineContent,
    string ErrorMessage
);
