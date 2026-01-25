using CsvReader.Errors;
using CsvReader.Models;

namespace CsvReader.Mapping;

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

            throw new TypeConversionException("", targetType);
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
                _ => underlyingType.IsEnum 
                    ? Enum.Parse(underlyingType, value, ignoreCase: true)
                    : throw new TypeConversionException(value, underlyingType)
            };
        }
        catch (FormatException ex)
        {
            throw new TypeConversionException(value, underlyingType, ex);
        }
        catch (ArgumentException ex)
        {
            throw new TypeConversionException(value, underlyingType, ex);
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

        throw new TypeConversionException(value, typeof(bool));
    }

    public static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }
}
