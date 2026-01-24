using CsvReader.Core;
using CsvReader.Models;

namespace CsvReader.UnitTests;

public class TypeConverterTests
{
    private readonly TypeConverter _converter = new();
    private readonly CsvParserOptions _defaultOptions = new();

    // ========== String Conversion Tests ==========

    [Fact]
    public void ConvertValue_StringType_ReturnsOriginalValue()
    {
        var result = _converter.ConvertValue("test value", typeof(string), _defaultOptions);

        Assert.Equal("test value", result);
    }

    [Fact]
    public void ConvertValue_EmptyStringToString_ReturnsEmptyString()
    {
        var result = _converter.ConvertValue("", typeof(string), _defaultOptions);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ConvertValue_WhitespaceToString_ReturnsEmptyString()
    {
        var result = _converter.ConvertValue("   ", typeof(string), _defaultOptions);

        Assert.Equal(string.Empty, result);
    }

    // ========== Integer Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidIntegerString_ReturnsInt()
    {
        var result = _converter.ConvertValue("42", typeof(int), _defaultOptions);

        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertValue_NegativeInteger_ReturnsNegativeInt()
    {
        var result = _converter.ConvertValue("-100", typeof(int), _defaultOptions);

        Assert.Equal(-100, result);
    }

    [Fact]
    public void ConvertValue_ZeroInteger_ReturnsZero()
    {
        var result = _converter.ConvertValue("0", typeof(int), _defaultOptions);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ConvertValue_InvalidIntegerString_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            _converter.ConvertValue("not-a-number", typeof(int), _defaultOptions));

        Assert.Contains("Failed to convert", exception.Message);
        Assert.Contains("Int32", exception.Message);
    }

    [Fact]
    public void ConvertValue_EmptyStringToInt_ThrowsInvalidOperationException()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _converter.ConvertValue("", typeof(int), _defaultOptions));

        Assert.Contains("Cannot convert empty string", exception.Message);
    }

    // ========== Long Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidLongString_ReturnsLong()
    {
        var result = _converter.ConvertValue("9223372036854775807", typeof(long), _defaultOptions);

        Assert.Equal(9223372036854775807L, result);
    }

    [Fact]
    public void ConvertValue_NegativeLong_ReturnsNegativeLong()
    {
        var result = _converter.ConvertValue("-9223372036854775808", typeof(long), _defaultOptions);

        Assert.Equal(-9223372036854775808L, result);
    }

    // ========== Decimal Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidDecimalString_ReturnsDecimal()
    {
        var result = _converter.ConvertValue("123.45", typeof(decimal), _defaultOptions);

        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ConvertValue_NegativeDecimal_ReturnsNegativeDecimal()
    {
        var result = _converter.ConvertValue("-99.99", typeof(decimal), _defaultOptions);

        Assert.Equal(-99.99m, result);
    }

    [Fact]
    public void ConvertValue_DecimalWithManyDigits_PreservesPrecision()
    {
        var result = _converter.ConvertValue("123.456789", typeof(decimal), _defaultOptions);

        Assert.Equal(123.456789m, result);
    }

    // ========== Double Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidDoubleString_ReturnsDouble()
    {
        var result = _converter.ConvertValue("3.14159", typeof(double), _defaultOptions);

        Assert.Equal(3.14159, result);
    }

    [Fact]
    public void ConvertValue_ScientificNotation_ReturnsDouble()
    {
        var result = _converter.ConvertValue("1.23E+10", typeof(double), _defaultOptions);

        Assert.Equal(1.23E+10, result);
    }

    // ========== Boolean Conversion Tests ==========

    [Fact]
    public void ConvertValue_TrueString_ReturnsTrue()
    {
        var result = _converter.ConvertValue("true", typeof(bool), _defaultOptions);

        Assert.True((bool)result!);
    }

    [Fact]
    public void ConvertValue_FalseString_ReturnsFalse()
    {
        var result = _converter.ConvertValue("false", typeof(bool), _defaultOptions);

        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertValue_OneString_ReturnsTrue()
    {
        var result = _converter.ConvertValue("1", typeof(bool), _defaultOptions);

        Assert.True((bool)result!);
    }

    [Fact]
    public void ConvertValue_ZeroString_ReturnsFalse()
    {
        var result = _converter.ConvertValue("0", typeof(bool), _defaultOptions);

        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertValue_YesString_ReturnsTrue()
    {
        var result = _converter.ConvertValue("yes", typeof(bool), _defaultOptions);

        Assert.True((bool)result!);
    }

    [Fact]
    public void ConvertValue_NoString_ReturnsFalse()
    {
        var result = _converter.ConvertValue("no", typeof(bool), _defaultOptions);

        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertValue_CaseInsensitiveBooleanTrue_ReturnsTrue()
    {
        var result = _converter.ConvertValue("TRUE", typeof(bool), _defaultOptions);

        Assert.True((bool)result!);
    }

    [Fact]
    public void ConvertValue_CaseInsensitiveBooleanFalse_ReturnsFalse()
    {
        var result = _converter.ConvertValue("FALSE", typeof(bool), _defaultOptions);

        Assert.False((bool)result!);
    }

    [Fact]
    public void ConvertValue_InvalidBooleanString_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            _converter.ConvertValue("maybe", typeof(bool), _defaultOptions));

        Assert.Contains("Failed to convert 'maybe'", exception.Message);
    }

    [Fact]
    public void ConvertValue_CustomBooleanValues_ReturnsCorrectBoolean()
    {
        var options = new CsvParserOptions
        {
            BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Y", "T" },
            BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "N", "F" }
        };

        var resultTrue = _converter.ConvertValue("Y", typeof(bool), options);
        var resultFalse = _converter.ConvertValue("N", typeof(bool), options);

        Assert.True((bool)resultTrue!);
        Assert.False((bool)resultFalse!);
    }

    // ========== Character Conversion Tests ==========

    [Fact]
    public void ConvertValue_SingleCharacter_ReturnsChar()
    {
        var result = _converter.ConvertValue("A", typeof(char), _defaultOptions);

        Assert.Equal('A', result);
    }

    [Fact]
    public void ConvertValue_MultipleCharacters_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            _converter.ConvertValue("AB", typeof(char), _defaultOptions));

        Assert.Contains("Failed to convert", exception.Message);
    }

    // ========== DateTime Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidDateTimeString_ReturnsDateTime()
    {
        var result = _converter.ConvertValue("2024-01-15", typeof(DateTime), _defaultOptions);

        Assert.IsType<DateTime>(result);
        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void ConvertValue_DateTimeWithTime_ReturnsDateTime()
    {
        var result = _converter.ConvertValue("2024-01-15 14:30:00", typeof(DateTime), _defaultOptions);

        Assert.IsType<DateTime>(result);
    }

    [Fact]
    public void ConvertValue_InvalidDateTime_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            _converter.ConvertValue("not-a-date", typeof(DateTime), _defaultOptions));

        Assert.Contains("Failed to convert", exception.Message);
    }

    // ========== Guid Conversion Tests ==========

    [Fact]
    public void ConvertValue_ValidGuidString_ReturnsGuid()
    {
        var guidString = "550e8400-e29b-41d4-a716-446655440000";
        var result = _converter.ConvertValue(guidString, typeof(Guid), _defaultOptions);

        Assert.Equal(Guid.Parse(guidString), result);
    }

    [Fact]
    public void ConvertValue_InvalidGuid_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            _converter.ConvertValue("not-a-guid", typeof(Guid), _defaultOptions));

        Assert.Contains("Failed to convert", exception.Message);
    }

    // ========== Nullable Type Tests ==========

    [Fact]
    public void ConvertValue_EmptyStringToNullableInt_ReturnsNull()
    {
        var result = _converter.ConvertValue("", typeof(int?), _defaultOptions);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertValue_WhitespaceToNullableInt_ReturnsNull()
    {
        var result = _converter.ConvertValue("   ", typeof(int?), _defaultOptions);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertValue_ValidValueToNullableInt_ReturnsInt()
    {
        var result = _converter.ConvertValue("42", typeof(int?), _defaultOptions);

        Assert.Equal(42, result);
    }

    [Fact]
    public void ConvertValue_EmptyStringToNullableDecimal_ReturnsNull()
    {
        var result = _converter.ConvertValue("", typeof(decimal?), _defaultOptions);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertValue_ValidValueToNullableDecimal_ReturnsDecimal()
    {
        var result = _converter.ConvertValue("123.45", typeof(decimal?), _defaultOptions);

        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void ConvertValue_EmptyStringToNullableDateTime_ReturnsNull()
    {
        var result = _converter.ConvertValue("", typeof(DateTime?), _defaultOptions);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertValue_ValidDateToNullableDateTime_ReturnsDateTime()
    {
        var result = _converter.ConvertValue("2024-01-15", typeof(DateTime?), _defaultOptions);

        Assert.Equal(new DateTime(2024, 1, 15), result);
    }

    [Fact]
    public void ConvertValue_EmptyStringToNullableBoolean_ReturnsNull()
    {
        var result = _converter.ConvertValue("", typeof(bool?), _defaultOptions);

        Assert.Null(result);
    }

    [Fact]
    public void ConvertValue_ValidBoolToNullableBoolean_ReturnsBoolean()
    {
        var result = _converter.ConvertValue("true", typeof(bool?), _defaultOptions);

        Assert.True((bool)result!);
    }

    // ========== Unsupported Type Tests ==========

    [Fact]
    public void ConvertValue_UnsupportedType_ThrowsNotSupportedException()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            _converter.ConvertValue("test", typeof(System.Drawing.Point), _defaultOptions));

        Assert.Contains("not supported", exception.Message);
    }

    // ========== ParseBoolean Tests ==========

    [Fact]
    public void ParseBoolean_DefaultTruthyValues_ReturnsTrue()
    {
        Assert.True(TypeConverter.ParseBoolean("true", _defaultOptions));
        Assert.True(TypeConverter.ParseBoolean("1", _defaultOptions));
        Assert.True(TypeConverter.ParseBoolean("yes", _defaultOptions));
    }

    [Fact]
    public void ParseBoolean_DefaultFalsyValues_ReturnsFalse()
    {
        Assert.False(TypeConverter.ParseBoolean("false", _defaultOptions));
        Assert.False(TypeConverter.ParseBoolean("0", _defaultOptions));
        Assert.False(TypeConverter.ParseBoolean("no", _defaultOptions));
    }

    [Fact]
    public void ParseBoolean_CaseInsensitive_WorksCorrectly()
    {
        Assert.True(TypeConverter.ParseBoolean("TRUE", _defaultOptions));
        Assert.True(TypeConverter.ParseBoolean("Yes", _defaultOptions));
        Assert.False(TypeConverter.ParseBoolean("FALSE", _defaultOptions));
        Assert.False(TypeConverter.ParseBoolean("No", _defaultOptions));
    }

    [Fact]
    public void ParseBoolean_InvalidValue_ThrowsFormatException()
    {
        var exception = Assert.Throws<FormatException>(() =>
            TypeConverter.ParseBoolean("invalid", _defaultOptions));

        Assert.Contains("Cannot convert 'invalid' to Boolean", exception.Message);
        Assert.Contains("truthy values", exception.Message);
        Assert.Contains("falsy values", exception.Message);
    }

    [Fact]
    public void ParseBoolean_CustomValues_WorksCorrectly()
    {
        var options = new CsvParserOptions
        {
            BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "on", "enabled" },
            BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "off", "disabled" }
        };

        Assert.True(TypeConverter.ParseBoolean("on", options));
        Assert.True(TypeConverter.ParseBoolean("enabled", options));
        Assert.False(TypeConverter.ParseBoolean("off", options));
        Assert.False(TypeConverter.ParseBoolean("disabled", options));
    }

    // ========== IsNullableType Tests ==========

    [Fact]
    public void IsNullableType_NullableInt_ReturnsTrue()
    {
        Assert.True(TypeConverter.IsNullableType(typeof(int?)));
    }

    [Fact]
    public void IsNullableType_NullableDecimal_ReturnsTrue()
    {
        Assert.True(TypeConverter.IsNullableType(typeof(decimal?)));
    }

    [Fact]
    public void IsNullableType_NullableDateTime_ReturnsTrue()
    {
        Assert.True(TypeConverter.IsNullableType(typeof(DateTime?)));
    }

    [Fact]
    public void IsNullableType_NonNullableInt_ReturnsFalse()
    {
        Assert.False(TypeConverter.IsNullableType(typeof(int)));
    }

    [Fact]
    public void IsNullableType_String_ReturnsFalse()
    {
        Assert.False(TypeConverter.IsNullableType(typeof(string)));
    }

    [Fact]
    public void IsNullableType_ReferenceType_ReturnsFalse()
    {
        Assert.False(TypeConverter.IsNullableType(typeof(object)));
    }
}
