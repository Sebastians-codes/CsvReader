using Csv.Reader.Models;

namespace Csv.Reader.IntegrationTests;

public class ErrorHandlingTests
{
    private class TestPerson : IMapped
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool Active { get; set; }

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(Name), new ColumnMapping("Name", 0) },
                { nameof(Age), new ColumnMapping("Age", 1) },
                { nameof(Active), new ColumnMapping("Active", 2) }
            };
        }
    }

    // ========== Error Count Tests ==========

    [Fact]
    public void LenientMode_MultipleErrors_CollectsAllErrors()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",           // Valid - line 2
            "\"Unclosed,40,true",     // Error: unclosed quote - line 3
            "Jane,25,false",          // Valid - line 4
            "Bob,not-a-number,true",  // Error: type conversion - line 5
            "Alice,35,true"           // Valid - line 6
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        // Check errors before accessing records
        Assert.True(results.HasErrors);
        Assert.Equal(2, results.Errors.Count);

        // Verify we got the valid records
        var records = results.Records.ToList();
        Assert.Equal(3, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal("Jane", records[1].Name);
        Assert.Equal("Alice", records[2].Name);
    }

    [Fact]
    public void LenientMode_NoErrors_ErrorsListIsEmpty()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Jane,25,false"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.False(results.HasErrors);
        Assert.Empty(results.Errors);

        var records = results.Records.ToList();
        Assert.Equal(2, records.Count);
    }

    [Fact]
    public void LenientMode_AllLinesHaveErrors_NoRecordsButManyErrors()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "\"Unclosed1,30,true",     // Error - line 2
            "\"Unclosed2,25,false",    // Error - line 3
            "Bob,not-a-number,true"    // Error - line 4
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Equal(3, results.Errors.Count);

        var records = results.Records.ToList();
        Assert.Empty(records);
    }

    // ========== Line Number Accuracy Tests ==========

    [Fact]
    public void LenientMode_ErrorLineNumbers_AreAccurate()
    {
        var csv = new[]
        {
            "Name,Age,Active",         // Line 1 (header)
            "John,30,true",            // Line 2 (valid)
            "\"Unclosed,40,true",      // Line 3 (error)
            "Jane,25,false",           // Line 4 (valid)
            "Bob,not-a-number,true",   // Line 5 (error)
            "Alice,35,false",          // Line 6 (valid)
            "Dave,bad,true"            // Line 7 (error)
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.Equal(3, results.Errors.Count);

        // Verify line numbers are correct
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Equal(5, results.Errors[1].LineNumber);
        Assert.Equal(7, results.Errors[2].LineNumber);
    }

    [Fact]
    public void LenientMode_WithEmptyLines_LineNumbersAccountForSkipped()
    {
        var csv = new[]
        {
            "Name,Age,Active",         // Line 1
            "John,30,true",            // Line 2
            "",                        // Line 3 (skipped)
            "Bad,not-a-number,true",   // Line 4 (error)
            "   ",                     // Line 5 (skipped)
            "Jane,25,false"            // Line 6 (valid)
        };

        var options = new CsvParserOptions
        {
            StrictMode = false,
            SkipEmptyLines = true
        };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);
        Assert.Equal(4, results.Errors[0].LineNumber);
    }

    [Fact]
    public void LenientMode_WithoutSkippingEmptyLines_EmptyLinesCreateErrors()
    {
        var csv = new[]
        {
            "Name,Age,Active",         // Line 1
            "John,30,true",            // Line 2
            "",                        // Line 3 (error: empty line)
            "Jane,25,false"            // Line 4 (valid)
        };

        var options = new CsvParserOptions
        {
            StrictMode = false,
            SkipEmptyLines = false
        };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Contains("Empty line", results.Errors[0].ErrorMessage);
    }

    // ========== Error Message Content Tests ==========

    [Fact]
    public void LenientMode_UnclosedQuoteError_ContainsCorrectInformation()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "\"Unclosed quote,30,true"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);

        var error = results.Errors[0];
        Assert.Equal(2, error.LineNumber);
        Assert.Contains("Unclosed quote", error.ErrorMessage);
        Assert.Equal("\"Unclosed quote,30,true", error.LineContent);
    }

    [Fact]
    public void LenientMode_TypeConversionError_ContainsCorrectInformation()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,not-a-number,true"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);

        var error = results.Errors[0];
        Assert.Equal(2, error.LineNumber);
        Assert.Contains("Cannot convert", error.ErrorMessage);
        Assert.Equal("John,not-a-number,true", error.LineContent);
    }

    [Fact]
    public void LenientMode_ColumnMismatchError_ContainsCorrectInformation()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "OnlyTwoFields,30"  // Missing third field
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);

        var error = results.Errors[0];
        Assert.Equal(3, error.LineNumber);
        Assert.Contains("out of range", error.ErrorMessage);
        Assert.Equal("OnlyTwoFields,30", error.LineContent);
    }

    // ========== Mixed Error Types Tests ==========

    [Fact]
    public void LenientMode_MixedErrorTypes_AllErrorsCollected()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",            // Valid
            "\"Unclosed,40,true",      // Parse error
            "Jane,not-a-number,false", // Type conversion error
            "Bob,35",                  // Column mismatch
            "Alice,45,true"            // Valid
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Equal(3, results.Errors.Count);

        // Verify each error type
        Assert.Contains("Unclosed", results.Errors[0].ErrorMessage);
        Assert.Contains("Cannot convert", results.Errors[1].ErrorMessage);
        Assert.Contains("out of range", results.Errors[2].ErrorMessage);

        // Verify line numbers
        Assert.Equal(3, results.Errors[0].LineNumber);
        Assert.Equal(4, results.Errors[1].LineNumber);
        Assert.Equal(5, results.Errors[2].LineNumber);

        // Verify we got the valid records
        var records = results.Records.ToList();
        Assert.Equal(2, records.Count);
    }

    [Fact]
    public void LenientMode_ErrorsPreserveOriginalLine()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",                     // Valid
            "  Jane  ,25,false",                // Valid with spaces
            "Bad,not-a-number,true"             // Error - preserve exact original line
        };

        var options = new CsvParserOptions { StrictMode = false, TrimFields = true };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        Assert.True(results.HasErrors);
        Assert.Single(results.Errors);

        // Verify the original line is preserved in the error (not trimmed)
        var error = results.Errors[0];
        Assert.Equal("Bad,not-a-number,true", error.LineContent);
        Assert.Equal(4, error.LineNumber);
    }

    // ========== Error Access Pattern Tests ==========

    [Fact]
    public void LenientMode_AccessErrorsFirst_ThenRecords_Works()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Bad,not-a-number,true"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        // Access Errors first (marks errors as handled)
        var errors = results.Errors;
        Assert.Single(errors);

        // Then access Records (should work because Errors was accessed)
        var records = results.Records.ToList();
        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
    }

    [Fact]
    public void LenientMode_CheckHasErrorsFirst_ThenRecords_Works()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Bad,not-a-number,true"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        // Check HasErrors first (marks errors as handled)
        Assert.True(results.HasErrors);

        // Then access Records (should work)
        var records = results.Records.ToList();
        Assert.Single(records);
    }

    [Fact]
    public void LenientMode_LogAllErrors_Pattern()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Bad1,not-a-number,true",
            "Jane,25,false",
            "Bad2,also-bad,true"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader(options);
        var results = reader.DeserializeLines<TestPerson>(csv);

        // Common pattern: log all errors
        var errorLog = new List<string>();
        foreach (var error in results.Errors)
        {
            errorLog.Add($"Line {error.LineNumber}: {error.ErrorMessage}");
        }

        Assert.Equal(2, errorLog.Count);
        Assert.All(errorLog, log => Assert.Contains("Line ", log));
        Assert.All(errorLog, log => Assert.Contains("Cannot convert", log));

        // Then process records
        var records = results.Records.ToList();
        Assert.Equal(2, records.Count);
    }
}
