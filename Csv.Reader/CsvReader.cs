using System.Reflection;
using System.Collections.Concurrent;
using Csv.Reader.Core;
using Csv.Reader.Errors;
using Csv.Reader.Mapping;
using Csv.Reader.Models;

namespace Csv.Reader;

/// <summary>
/// A CSV reader that deserializes CSV data into strongly-typed objects.
/// </summary>
/// <remarks>
/// This CSV reader supports:
/// - Custom delimiters (comma, semicolon, tab, pipe, etc.)
/// - Quoted fields with escaped quotes
/// - Header row mapping or index-based mapping
/// - Case-insensitive header matching
/// - Field trimming
/// - Empty line handling
/// - Strict and lenient error handling modes
/// - Custom boolean value mappings
/// 
/// Error Handling Modes:
/// - Strict Mode (StrictMode = true): Throws exceptions immediately on first error
/// - Lenient Mode (StrictMode = false): Collects errors and continues parsing valid lines
/// </remarks>
/// <example>
/// <code>
/// // Basic usage with default options
/// var options = new CsvParserOptions();
/// var reader = new CsvReader(options);
/// var results = reader.DeserializeLines&lt;Person&gt;(csvLines);
/// 
/// if (results.HasErrors) 
/// {
///     foreach (var error in results.Errors)
///         Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
/// }
/// 
/// var people = results.Records;
/// </code>
/// </example>
public static class CsvReader
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, ColumnMapping>> _mappingCache = new();
    private static readonly MappingResolver _mappingResolver = new();
    private static readonly TypeConverter _typeConverter = new();
    private static readonly Parser _parser = new();

    /// <summary>
    /// Deserializes CSV lines into strongly-typed objects.
    /// </summary>
    /// <typeparam name="T">The type to deserialize CSV rows into. Must implement <see cref="IMapped"/> and have a parameterless constructor.</typeparam>
    /// <param name="lines">The CSV lines to parse. First line may be a header depending on HasHeaderRow option.</param>
    /// <returns>
    /// A <see cref="CsvParseResult{T}"/> containing successfully parsed records and any errors encountered.
    /// </returns>
    /// <exception cref="CsvParseException">
    /// Thrown in strict mode when any parsing error occurs (unclosed quotes, type conversion errors, etc.).
    /// </exception>
    /// <remarks>
    /// In strict mode, this method throws on the first error encountered.
    /// In lenient mode, errors are collected in the result and parsing continues.
    /// </remarks>
    public static CsvParseResult<T> DeserializeLines<T>(IEnumerable<string> lines) where T : IMapped, new()
    {
        var _options = new CsvParserOptions();
        return Process<T>(lines, _options);
    }

    /// <summary>
    /// Deserializes CSV lines into strongly-typed objects.
    /// </summary>
    /// <typeparam name="T">The type to deserialize CSV rows into. Must implement <see cref="IMapped"/> and have a parameterless constructor.</typeparam>
    /// <param name="lines">The CSV lines to parse. First line may be a header depending on HasHeaderRow option.</param>
    /// <param name="options">Sets the options for how the strings should be parsed.</param>
    /// <returns>
    /// A <see cref="CsvParseResult{T}"/> containing successfully parsed records and any errors encountered.
    /// </returns>
    /// <exception cref="CsvParseException">
    /// Thrown in strict mode when any parsing error occurs (unclosed quotes, type conversion errors, etc.).
    /// </exception>
    /// <remarks>
    /// In strict mode, this method throws on the first error encountered.
    /// In lenient mode, errors are collected in the result and parsing continues.
    /// </remarks>
    public static CsvParseResult<T> DeserializeLines<T>(IEnumerable<string> lines, CsvParserOptions options) where T : IMapped, new()
    {
        return Process<T>(lines, options);
    }

    private static CsvParseResult<T> Process<T>(IEnumerable<string> lines, CsvParserOptions _options) where T : IMapped, new()
    {
        var records = new List<T>();
        var errors = new List<CsvParseError>();
        var columnMapping = GetOrCreateMapping<T>();
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
                    throw new EmptyLineException(lineNumber);
                }

                errors.Add(new CsvParseError(lineNumber, line, "Empty line"));
                continue;
            }

            string[] fields;
            try
            {
                fields = _parser.ParseLine(line, _options.Delimiter);
            }
            catch (CsvParseException ex)
            {
                if (_options.StrictMode)
                {
                    throw new CsvParseException(
                        $"Line {lineNumber}: {ex.Message}", lineNumber, ex);
                }

                errors.Add(new CsvParseError(lineNumber, line, ex.Message));
                continue;
            }

            if (_options.TrimFields)
            {
                fields = [.. fields.Select(f => f.Trim())];
            }

            if (isFirstLine && _options.HasHeaderRow)
            {
                headerMap = BuildHeaderMap(fields, _options);
                isFirstLine = false;
                continue;
            }

            isFirstLine = false;

            T obj;
            try
            {
                obj = DeserializeLine<T>(fields, headerMap, lineNumber, columnMapping, _options);
            }
            catch (CsvParseException ex)
            {
                if (_options.StrictMode)
                {
                    throw new CsvParseException(
                        $"Line {lineNumber}: {ex.Message}", lineNumber, ex);
                }

                errors.Add(new CsvParseError(lineNumber, line, ex.Message));
                continue;
            }

            records.Add(obj);
        }

        return new CsvParseResult<T>(records, errors, _options.StrictMode);
    }

    private static Dictionary<string, int> BuildHeaderMap(string[] headers, CsvParserOptions options)
    {
        var map = new Dictionary<string, int>(
            options.CaseInsensitiveHeaders
                ? StringComparer.OrdinalIgnoreCase
                : StringComparer.Ordinal);

        for (int i = 0; i < headers.Length; i++)
        {
            map[headers[i]] = i;
        }

        return map;
    }

    private static T DeserializeLine<T>(string[] fields, Dictionary<string, int>? headerMap, int lineNumber, Dictionary<string, ColumnMapping> columnMapping, CsvParserOptions options) where T : IMapped, new()
    {
        var obj = new T();
        var type = typeof(T);

        foreach (var mapping in columnMapping)
        {
            string propertyName = mapping.Key;
            ColumnMapping columnMappingEntry = mapping.Value;

            PropertyInfo? property = type.GetProperty(propertyName) ??
                throw new PropertyNotFoundException(propertyName, type);

            int columnIndex = _mappingResolver.ResolveColumnIndex(columnMappingEntry, headerMap);

            _mappingResolver.ValidateColumnIndex(columnIndex, fields.Length, columnMapping.Count, options.StrictMode, lineNumber);

            string fieldValue = fields[columnIndex];

            try
            {
                object? convertedValue = _typeConverter.ConvertValue(fieldValue, property.PropertyType, options);
                property.SetValue(obj, convertedValue);
            }
            catch (TypeConversionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new TypeConversionException(fieldValue, property.PropertyType, ex, propertyName);
            }
        }

        return obj;
    }

    internal static Dictionary<string, ColumnMapping> GetOrCreateMapping<T>() where T : IMapped, new()
    {
        return _mappingCache.GetOrAdd(typeof(T), _ =>
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
        });
    }
}

