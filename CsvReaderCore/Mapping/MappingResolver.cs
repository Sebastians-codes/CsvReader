using CsvReaderCore.Errors;
using CsvReaderCore.Models;

namespace CsvReaderCore.Mapping;

internal class MappingResolver
{
    internal int ResolveColumnIndex(
        ColumnMapping mapping,
        Dictionary<string, int>? headerMap)
    {
        if (headerMap != null)
        {
            if (!headerMap.TryGetValue(mapping.ColumnIdentifier, out int index))
            {
                throw new ColumnNotFoundException(mapping.ColumnIdentifier, headerMap.Keys.ToArray());
            }

            return index;
        }

        if (mapping.ColumnIndex >= 0)
        {
            return mapping.ColumnIndex;
        }

        if (!int.TryParse(mapping.ColumnIdentifier, out int columnIndex))
        {
            throw new ColumnMappingException(
                $"Column identifier '{mapping.ColumnIdentifier}' must be numeric when HasHeaderRow is false",
                mapping.ColumnIdentifier);
        }

        return columnIndex;
    }

    internal void ValidateColumnIndex(int columnIndex, int fieldCount, int mappingCount, bool strictMode, int lineNumber)
    {
        if (columnIndex < 0 || columnIndex >= fieldCount)
        {
            throw new ColumnIndexOutOfRangeException(columnIndex, fieldCount, lineNumber);
        }

        if (strictMode && fieldCount > mappingCount)
        {
            throw new ColumnMappingException(
                $"Amount of fields {fieldCount} exceeds the number of mappings ({mappingCount}).");
        }
    }
}
