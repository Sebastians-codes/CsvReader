using Csv.Reader.Errors;
using Csv.Reader.Models;

namespace Csv.Reader.IntegrationTests;

public class TypeConversionIntegrationTests
{
    // Test model with various types
    private class AdvancedRecord : IMapped
    {
        public string Name { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? OptionalId { get; set; }

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(Name), new ColumnMapping("Name", 0) },
                { nameof(Id), new ColumnMapping("Id", 1) },
                { nameof(CreatedAt), new ColumnMapping("CreatedAt", 2) },
                { nameof(UpdatedAt), new ColumnMapping("UpdatedAt", 3) },
                { nameof(OptionalId), new ColumnMapping("OptionalId", 4) }
            };
        }
    }

    // ========== Guid Conversion Tests ==========

    [Fact]
    public void ParseCSV_WithValidGuids_ParsesCorrectly()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid1},2024-01-15,2024-01-20,{guid2}",
            $"Record2,{guid2},2024-02-01,,{Guid.Empty}"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);

        // First record
        Assert.Equal("Record1", records[0].Name);
        Assert.Equal(guid1, records[0].Id);
        Assert.Equal(guid2, records[0].OptionalId);

        // Second record
        Assert.Equal("Record2", records[1].Name);
        Assert.Equal(guid2, records[1].Id);
        Assert.Equal(Guid.Empty, records[1].OptionalId);
    }

    [Fact]
    public void ParseCSV_WithInvalidGuid_CollectsError()
    {
        var validGuid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{validGuid},2024-01-15,,",
            "Record2,not-a-valid-guid,2024-02-01,,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Contains("Cannot convert", results.Errors[0].ErrorMessage);
        Assert.Contains("Guid", results.Errors[0].ErrorMessage);

        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("Record1", records[0].Name);
    }

    [Fact]
    public void ParseCSV_WithNullableGuid_EmptyString_ParsesAsNull()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid},2024-01-15,2024-01-20,",
            $"Record2,{guid},2024-02-01,,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Null(records[0].OptionalId);
        Assert.Null(records[1].OptionalId);
    }

    // ========== DateTime Conversion Tests ==========

    [Fact]
    public void ParseCSV_WithVariousDateTimeFormats_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid},2024-01-15,2024-01-20 14:30:00,",
            $"Record2,{guid},2024-02-01T10:15:30,,",
            $"Record3,{guid},01/15/2024,02/20/2024,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);

        // First record - ISO date
        Assert.Equal(new DateTime(2024, 1, 15), records[0].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 20, 14, 30, 0), records[0].UpdatedAt);

        // Second record - ISO datetime with T
        Assert.Equal(new DateTime(2024, 2, 1, 10, 15, 30), records[1].CreatedAt);
        Assert.Null(records[1].UpdatedAt);

        // Third record - US date format
        Assert.Equal(new DateTime(2024, 1, 15), records[2].CreatedAt);
        Assert.Equal(new DateTime(2024, 2, 20), records[2].UpdatedAt);
    }

    [Fact]
    public void ParseCSV_WithInvalidDateTime_CollectsError()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid},2024-01-15,,",
            $"Record2,{guid},not-a-date,,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Contains("Cannot convert", results.Errors[0].ErrorMessage);

        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("Record1", records[0].Name);
    }

    [Fact]
    public void ParseCSV_WithNullableDateTime_EmptyString_ParsesAsNull()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid},2024-01-15,,",
            $"Record2,{guid},2024-02-01,,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Null(records[0].UpdatedAt);
        Assert.Null(records[1].UpdatedAt);
    }

    // ========== Mixed Type Conversion Tests ==========

    [Fact]
    public void ParseCSV_WithMixedTypeErrors_CollectsAllErrors()
    {
        var validGuid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{validGuid},2024-01-15,,",
            "Record2,bad-guid,2024-02-01,,",      // Bad Guid
            $"Record3,{validGuid},bad-date,,",    // Bad DateTime
            "Record4,bad-guid,bad-date,,"         // Both bad
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        Assert.True(results.HasErrors);
        Assert.Equal(3, results.Errors.Count);

        // Verify error line numbers
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Equal(4, results.Errors[1].LineNumber);
        Assert.Equal(5, results.Errors[2].LineNumber);

        // Only first record should be valid
        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("Record1", records[0].Name);
    }

    [Fact]
    public void ParseCSV_LargeDataset_WithGuidsAndDates_ParsesEfficiently()
    {
        var csvLines = new List<string>
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId"
        };

        // Generate 1000 records
        for (int i = 0; i < 1000; i++)
        {
            var guid = Guid.NewGuid();
            var date = new DateTime(2024, 1, 1).AddDays(i);
            csvLines.Add($"Record{i},{guid},{date:yyyy-MM-dd},,");
        }

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csvLines);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(1000, records.Count);
        Assert.False(results.HasErrors);

        // Verify first and last records
        Assert.Equal("Record0", records[0].Name);
        Assert.Equal(new DateTime(2024, 1, 1), records[0].CreatedAt);

        Assert.Equal("Record999", records[999].Name);
        Assert.Equal(new DateTime(2024, 1, 1).AddDays(999), records[999].CreatedAt);
    }

    [Fact]
    public void ParseCSV_WithWhitespaceAroundGuids_TrimsCorrectly()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,  {guid}  ,2024-01-15,,"
        };

        var results = CsvReader.DeserializeLines<AdvancedRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal(guid, records[0].Id);
    }

    [Fact]
    public void ParseCSV_StrictMode_InvalidGuid_ThrowsImmediately()
    {
        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            "Record1,not-a-guid,2024-01-15,,"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var exception = Assert.Throws<CsvParseException>(() =>
            CsvReader.DeserializeLines<AdvancedRecord>(csv, options));

        Assert.Contains("Line 2", exception.Message);
        Assert.Contains("Cannot convert", exception.Message);
    }

    [Fact]
    public void ParseCSV_StrictMode_InvalidDateTime_ThrowsImmediately()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,Id,CreatedAt,UpdatedAt,OptionalId",
            $"Record1,{guid},not-a-date,,"
        };

        var options = new CsvParserOptions
        {
            StrictMode = true
        };

        var exception = Assert.Throws<CsvParseException>(() =>
            CsvReader.DeserializeLines<AdvancedRecord>(csv, options));

        Assert.Contains("Line 2", exception.Message);
        Assert.Contains("Cannot convert", exception.Message);
    }
}
