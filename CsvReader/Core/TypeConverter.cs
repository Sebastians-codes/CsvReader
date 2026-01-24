using CsvReader.Models;

namespace CsvReader.Core;

public class TypeConverter
{
    public object? ConvertValue(string value, Type targetType, CsvParserOptions options)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (IsNullableType(targetType))
            {
                return null;
            }

            if (targetType == typeof(string))
            {
                return string.Empty;
            }

            throw new InvalidOperationException(
                $"Cannot convert empty string to non-nullable type {targetType.Name}");
        }

        Type underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            return underlyingType.Name switch
            {
                nameof(Guid) => Guid.Parse(value),
                nameof(DateTime) => DateTime.Parse(value),
                nameof(String) => value,
                nameof(Char) => char.Parse(value),
                nameof(Int32) => int.Parse(value),
                nameof(Int64) => long.Parse(value),
                nameof(Double) => double.Parse(value),
                nameof(Decimal) => decimal.Parse(value),
                nameof(Boolean) => ParseBoolean(value, options),
                _ => throw new NotSupportedException(
                    $"Type {underlyingType.Name} is not supported")
            };
        }
        catch (FormatException ex)
        {
            throw new FormatException(
                $"Failed to convert '{value}' to type {underlyingType.Name}", ex);
        }
    }

    public static bool ParseBoolean(string value, CsvParserOptions options)
    {
        if (options.BooleanTruthyValues.Contains(value))
        {
            return true;
        }

        if (options.BooleanFalsyValues.Contains(value))
        {
            return false;
        }

        var truthyList = string.Join(", ", options.BooleanTruthyValues.Select(v => $"\"{v}\""));
        var falsyList = string.Join(", ", options.BooleanFalsyValues.Select(v => $"\"{v}\""));

        throw new FormatException(
            $"Cannot convert '{value}' to Boolean. " +
            $"Expected truthy values: {truthyList} " +
            $"or falsy values: {falsyList}");
    }

    public static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }
}
