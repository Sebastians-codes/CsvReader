using CsvReader.Models;

namespace CsvReader.Mapping;

public class MappingResolver
{
    public int ResolveColumnIndex(
        ColumnMapping mapping,
        Dictionary<string, int>? headerMap)
    {
        if (headerMap != null)
        {
            if (!headerMap.TryGetValue(mapping.ColumnIdentifier, out int index))
            {
                throw new InvalidOperationException(
                    $"Column '{mapping.ColumnIdentifier}' not found in CSV headers");
            }

            return index;
        }

        if (mapping.ColumnIndex >= 0)
        {
            return mapping.ColumnIndex;
        }

        if (!int.TryParse(mapping.ColumnIdentifier, out int columnIndex))
        {
            throw new InvalidOperationException(
                $"Column identifier '{mapping.ColumnIdentifier}' must be numeric when HasHeaderRow is false");
        }

        return columnIndex;
    }

    public void ValidateColumnIndex(int columnIndex, int fieldCount)
    {
        if (columnIndex < 0 || columnIndex >= fieldCount)
        {
            throw new IndexOutOfRangeException(
                $"Column index {columnIndex} is out of range. Row has {fieldCount} columns.");
        }
    }
}
