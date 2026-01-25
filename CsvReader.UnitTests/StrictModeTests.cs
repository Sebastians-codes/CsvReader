using CsvReaderCore;
using CsvReaderCore.Errors;
using CsvReaderCore.Models;

namespace CsvReader.UnitTests;

public class StrictModeTests
{
    private class TestPerson : IMapped
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(Name), new ColumnMapping("Name", 0) },
                { nameof(Age), new ColumnMapping("Age", 1) }
            };
        }
    }

    [Fact]
    public void LenientMode_WithMalformedQuote_SkipsLineAndContinues()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "\"Unclosed,40",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = false
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal(30, records[0].Age);
        Assert.Equal("Jane", records[1].Name);
        Assert.Equal(25, records[1].Age);
    }

    [Fact]
    public void StrictMode_WithMalformedQuote_ThrowsException()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "\"Unclosed,40",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Line 3", exception.Message);
        Assert.Contains("Unclosed quote", exception.Message);
    }

    [Fact]
    public void LenientMode_WithColumnMismatch_SkipsLineAndContinues()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "MissingColumn",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = false
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal("Jane", records[1].Name);
    }

    [Fact]
    public void StrictMode_WithColumnMismatch_ThrowsException()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "MissingColumn",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Line 3", exception.Message);
        Assert.Contains("out of range", exception.Message);
    }

    [Fact]
    public void LenientMode_WithTypeConversionError_SkipsLineAndContinues()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "Jane,not-a-number",
            "Bob,35"
        };

        var options = new CsvParserOptions
        {
            StrictMode = false
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal(30, records[0].Age);
        Assert.Equal("Bob", records[1].Name);
        Assert.Equal(35, records[1].Age);
    }

    [Fact]
    public void StrictMode_WithTypeConversionError_ThrowsException()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "Jane,not-a-number",
            "Bob,35"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Line 3", exception.Message);
    }

    [Fact]
    public void LenientMode_WithEmptyLine_WhenSkipEmptyLinesFalse_SkipsAndContinues()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = false,
            SkipEmptyLines = false
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal("Jane", records[1].Name);
    }

    [Fact]
    public void StrictMode_WithEmptyLine_WhenSkipEmptyLinesFalse_ThrowsException()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true,
            SkipEmptyLines = false
        };

        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<EmptyLineException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Empty line", exception.Message);
        Assert.Contains("Empty line", exception.Message);
    }

    [Fact]
    public void StrictMode_WithEmptyLine_WhenSkipEmptyLinesTrue_SkipsAndContinues()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "",
            "Jane,25"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true,
            SkipEmptyLines = true
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal("Jane", records[1].Name);
    }

    [Fact]
    public void StrictMode_WithMultipleErrors_ThrowsOnFirstError()
    {
        var csv = new[]
        {
            "Name,Age",
            "John,30",
            "\"Unclosed,40",
            "Jane,not-a-number",
            "Bob,35"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Line 3", exception.Message);
    }
}
