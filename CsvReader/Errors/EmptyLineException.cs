namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when an empty line is encountered in strict mode with SkipEmptyLines set to false.
/// </summary>
/// <remarks>
/// This exception is only thrown in strict mode when an empty or whitespace-only line is encountered
/// and the SkipEmptyLines option is set to false. In lenient mode, empty lines are added to the
/// error collection instead of throwing an exception.
/// </remarks>
/// <example>
/// <code>
/// var csv = new[] { "Name,Age", "", "John,30" };
/// var options = new CsvParserOptions 
/// { 
///     StrictMode = true, 
///     SkipEmptyLines = false 
/// };
/// // Throws: Empty line encountered at line 2
/// </code>
/// </example>
public class EmptyLineException(int lineNumber) : CsvParseException($"Empty line encountered at line {lineNumber}", lineNumber)
{
}
