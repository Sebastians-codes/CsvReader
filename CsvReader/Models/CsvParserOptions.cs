namespace CsvReader.Models;

public class CsvParserOptions
{
    public char Delimiter { get; set; } = ',';
    public bool HasHeaderRow { get; set; } = true;
    public bool SkipEmptyLines { get; set; } = true;
    public bool TrimFields { get; set; } = true;
    public bool CaseInsensitiveHeaders { get; set; } = true;
    public bool StrictMode { get; set; } = false;
    public string? ErrorLogFile { get; set; } = null;

    public HashSet<string> BooleanTruthyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "true", "1", "yes"
    };

    public HashSet<string> BooleanFalsyValues { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "false", "0", "no"
    };
}
