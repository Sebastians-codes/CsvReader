namespace Csv.Reader.Errors;

/// <summary>
/// Exception thrown when an empty line is encountered.
/// </summary>
internal class EmptyLineException(int lineNumber) : CsvParseException($"Empty line encountered at line {lineNumber}", lineNumber)
{
}
