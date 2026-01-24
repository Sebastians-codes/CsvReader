namespace CsvReader.Models;

public interface IMapped
{
    Dictionary<string, ColumnMapping> GetColumnMapping() => [];
}
