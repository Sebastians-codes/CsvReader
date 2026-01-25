namespace CsvReader.Errors;

/// <summary>
/// Exception thrown when a CSV line contains an unclosed quote character.
/// </summary>
/// <remarks>
/// This exception is thrown when the CSV parser encounters a quote character (")
/// that is not properly closed. This typically happens when:
/// - A quoted field is missing its closing quote
/// - Quote escaping is incorrect (e.g., using single quotes instead of double quotes "")
/// </remarks>
/// <example>
/// Examples of lines that would throw this exception:
/// <code>
/// "Name,Age
/// John,"unclosed quote,30
/// </code>
/// </example>
public class UnclosedQuoteException(string line) : CsvParseException($"Unclosed quote in CSV line: {line}")
{
    /// <summary>
    /// Gets the CSV line content that contains the unclosed quote.
    /// </summary>
    public string Line { get; } = line;
}
