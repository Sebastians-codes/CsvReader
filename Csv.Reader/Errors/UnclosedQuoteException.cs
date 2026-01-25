namespace Csv.Reader.Errors;

/// <summary>
/// Exception thrown when a CSV line contains an unclosed quote.
/// </summary>
internal class UnclosedQuoteException(string line) : CsvParseException($"Unclosed quote in CSV line: {line}")
{
    public string Line { get; } = line;
}
