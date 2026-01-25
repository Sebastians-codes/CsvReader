using Csv.Reader.Errors;
using Csv.Reader.Models;

namespace Csv.Reader.IntegrationTests;

public class EnumAndComplexTypeTests
{
    public enum Status
    {
        Active,
        Inactive,
        Pending,
        Archived
    }

    public enum Priority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    private class TaskRecord : IMapped
    {
        public string Name { get; set; } = string.Empty;
        public Guid TaskId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Status Status { get; set; }
        public Priority Priority { get; set; }
        public Status? OptionalStatus { get; set; }

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(Name), new ColumnMapping("Name", 0) },
                { nameof(TaskId), new ColumnMapping("TaskId", 1) },
                { nameof(CreatedAt), new ColumnMapping("CreatedAt", 2) },
                { nameof(CompletedAt), new ColumnMapping("CompletedAt", 3) },
                { nameof(Status), new ColumnMapping("Status", 4) },
                { nameof(Priority), new ColumnMapping("Priority", 5) },
                { nameof(OptionalStatus), new ColumnMapping("OptionalStatus", 6) }
            };
        }
    }

    // ========== Enum Conversion Tests ==========

    [Fact]
    public void ParseCSV_WithValidEnums_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,Active,High,Pending",
            $"Task2,{guid},2024-01-20,2024-01-25,Inactive,Low,"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);

        // First record
        Assert.Equal(Status.Active, records[0].Status);
        Assert.Equal(Priority.High, records[0].Priority);
        Assert.Equal(Status.Pending, records[0].OptionalStatus);

        // Second record
        Assert.Equal(Status.Inactive, records[1].Status);
        Assert.Equal(Priority.Low, records[1].Priority);
        Assert.Null(records[1].OptionalStatus);
    }

    [Fact]
    public void ParseCSV_WithEnumCaseInsensitive_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,active,HIGH,",
            $"Task2,{guid},2024-01-20,,PENDING,low,"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal(Status.Active, records[0].Status);
        Assert.Equal(Priority.High, records[0].Priority);
        Assert.Equal(Status.Pending, records[1].Status);
        Assert.Equal(Priority.Low, records[1].Priority);
    }

    [Fact]
    public void ParseCSV_WithEnumNumericValues_ParsesCorrectly()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,Active,1,",
            $"Task2,{guid},2024-01-20,,Pending,4,"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal(Priority.Low, records[0].Priority);
        Assert.Equal(Priority.Critical, records[1].Priority);
    }

    [Fact]
    public void ParseCSV_WithInvalidEnum_CollectsError()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,Active,High,",
            $"Task2,{guid},2024-01-20,,InvalidStatus,Low,"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Contains("Cannot convert", results.Errors[0].ErrorMessage);

        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("Task1", records[0].Name);
    }

    [Fact]
    public void ParseCSV_WithNullableEnum_EmptyString_ParsesAsNull()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,Active,High,",
            $"Task2,{guid},2024-01-20,,Pending,Low,"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Null(records[0].OptionalStatus);
        Assert.Null(records[1].OptionalStatus);
    }

    // ========== Combined Guid, DateTime, Enum Tests ==========

    [Fact]
    public void ParseCSV_AllComplexTypes_ParsesCorrectly()
    {
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid1},2024-01-15,2024-01-20,Active,High,Pending",
            $"Task2,{guid2},2024-02-01,,Inactive,Low,",
            $"Task3,{Guid.Empty},2024-03-15,2024-03-20,Archived,Critical,Active"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.False(results.HasErrors);

        // Verify all types parsed correctly
        Assert.Equal(guid1, records[0].TaskId);
        Assert.Equal(new DateTime(2024, 1, 15), records[0].CreatedAt);
        Assert.Equal(new DateTime(2024, 1, 20), records[0].CompletedAt);
        Assert.Equal(Status.Active, records[0].Status);
        Assert.Equal(Priority.High, records[0].Priority);
        Assert.Equal(Status.Pending, records[0].OptionalStatus);

        Assert.Equal(guid2, records[1].TaskId);
        Assert.Null(records[1].CompletedAt);
        Assert.Equal(Status.Inactive, records[1].Status);
        Assert.Null(records[1].OptionalStatus);

        Assert.Equal(Guid.Empty, records[2].TaskId);
        Assert.Equal(Status.Archived, records[2].Status);
        Assert.Equal(Priority.Critical, records[2].Priority);
    }

    [Fact]
    public void ParseCSV_MixedComplexTypeErrors_CollectsAllErrors()
    {
        var validGuid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{validGuid},2024-01-15,,Active,High,",
            "Task2,bad-guid,2024-02-01,,Active,Low,",       // Bad Guid
            $"Task3,{validGuid},bad-date,,Active,Medium,",  // Bad DateTime
            $"Task4,{validGuid},2024-03-01,,BadEnum,Low,"   // Bad Enum
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        Assert.True(results.HasErrors);
        Assert.Equal(3, results.Errors.Count);

        // Verify each error line number
        Assert.Equal(3, results.Errors[0].LineNumber); // Bad Guid
        Assert.Equal(4, results.Errors[1].LineNumber); // Bad DateTime
        Assert.Equal(5, results.Errors[2].LineNumber); // Bad Enum name

        // Only first record should be valid
        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("Task1", records[0].Name);
    }

    [Fact]
    public void ParseCSV_StrictMode_InvalidEnum_ThrowsImmediately()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,InvalidStatus,High,"
        };

        var options = new CsvParserOptions { StrictMode = true };
        var reader = new CsvReader(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines<TaskRecord>(csv));

        Assert.Contains("Line 2", exception.Message);
        Assert.Contains("Cannot convert", exception.Message);
    }

    [Fact]
    public void ParseCSV_EnumWithSpaces_TrimsAndParses()
    {
        var guid = Guid.NewGuid();

        var csv = new[]
        {
            "Name,TaskId,CreatedAt,CompletedAt,Status,Priority,OptionalStatus",
            $"Task1,{guid},2024-01-15,,  Active  ,  High  ,"
        };

        var options = new CsvParserOptions { TrimFields = true };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TaskRecord>(csv);

        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal(Status.Active, records[0].Status);
        Assert.Equal(Priority.High, records[0].Priority);
    }
}
