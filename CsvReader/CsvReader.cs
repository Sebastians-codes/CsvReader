using System.Reflection;
using CsvReader.Mapping;
using CsvReader.Core;
using CsvReader.Models;

namespace CsvReader;

public class CsvReader<T>(
    CsvParserOptions? options = null
    ) where T : IMapped, new()
{
    private readonly Dictionary<string, ColumnMapping> _columnMapping = GetOrCreateMapping();
    private readonly CsvParserOptions _options = options ?? new CsvParserOptions();
    private readonly MappingResolver _mappingResolver = new();
    private readonly TypeConverter _typeConverter = new();
    private readonly Parser _parser = new();

    public IEnumerable<T> ParseFile(string filePath)
    {
        var lines = File.ReadLines(filePath);
        return DeserializeLines(lines);
    }

    public IEnumerable<T> DeserializeLines(IEnumerable<string> lines)
    {
        Dictionary<string, int>? headerMap = null;
        bool isFirstLine = true;
        int lineNumber = 0;

        foreach (var line in lines)
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                if (_options.SkipEmptyLines)
                {
                    continue;
                }

                if (_options.StrictMode)
                {
                    throw new InvalidOperationException($"Line {lineNumber}: Empty line encountered");
                }

                LogError(lineNumber, line, "Empty line");
                continue;
            }

            string[] fields;
            try
            {
                fields = _parser.ParseLine(line, _options.Delimiter);
            }
            catch (FormatException ex)
            {
                if (_options.StrictMode)
                {
                    throw new InvalidOperationException(
                        $"Line {lineNumber}: {ex.Message}", ex);
                }

                LogError(lineNumber, line, ex.Message);
                continue;
            }

            if (_options.TrimFields)
            {
                fields = fields.Select(f => f.Trim()).ToArray();
            }

            if (isFirstLine && _options.HasHeaderRow)
            {
                headerMap = BuildHeaderMap(fields);
                isFirstLine = false;
                continue;
            }

            isFirstLine = false;

            T obj;
            try
            {
                obj = DeserializeLine(fields, headerMap);
            }
            catch (Exception ex)
            {
                if (_options.StrictMode)
                {
                    throw new InvalidOperationException(
                        $"Line {lineNumber}: {ex.Message}", ex);
                }

                LogError(lineNumber, line, ex.Message);
                continue;
            }

            yield return obj;
        }
    }

    private Dictionary<string, int> BuildHeaderMap(string[] headers)
    {
        var map = new Dictionary<string, int>(
            _options.CaseInsensitiveHeaders
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

        for (int i = 0; i < headers.Length; i++)
        {
            map[headers[i]] = i;
        }

        return map;
    }

    private T DeserializeLine(string[] fields, Dictionary<string, int>? headerMap)
    {
        var obj = new T();
        var type = typeof(T);

        foreach (var mapping in _columnMapping)
        {
            string propertyName = mapping.Key;
            ColumnMapping columnMapping = mapping.Value;

            PropertyInfo? property = type.GetProperty(propertyName) ??
                throw new InvalidOperationException($"Property '{propertyName}' not found on type {type.Name}");

            int columnIndex = _mappingResolver.ResolveColumnIndex(columnMapping, headerMap);

            _mappingResolver.ValidateColumnIndex(columnIndex, fields.Length);

            string fieldValue = fields[columnIndex];

            try
            {
                object? convertedValue = _typeConverter.ConvertValue(fieldValue, property.PropertyType, _options);
                property.SetValue(obj, convertedValue);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to convert column '{columnMapping.ColumnIdentifier}' (value: '{fieldValue}') " +
                    $"to property '{propertyName}' of type {property.PropertyType.Name}: {ex.Message}",
                    ex);
            }
        }

        return obj;
    }

    public static Dictionary<string, ColumnMapping> GetOrCreateMapping()
    {
        var mapping = new T().GetColumnMapping();

        if (mapping.Count == 0)
        {
            var properties = typeof(T).GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                var propName = properties[i].Name;
                mapping[propName] = new ColumnMapping(propName, i);
            }
        }

        return mapping;
    }

    private void LogError(int lineNumber, string line, string error)
    {
        if (string.IsNullOrEmpty(_options.ErrorLogFile))
        {
            return;
        }

        try
        {
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Line {lineNumber}: {error}\n";
            File.AppendAllText(_options.ErrorLogFile, logEntry);
        }
        catch
        {
        }
    }
}
