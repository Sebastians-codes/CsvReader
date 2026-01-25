using CsvReaderCore;
using CsvReaderCore.Errors;
using CsvReaderCore.Models;

namespace CsvReader.IntegrationTests;

public class CsvReaderIntegrationTests
{
    private class TestPerson : IMapped
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(Name), new ColumnMapping("Name", 0) },
                { nameof(Age), new ColumnMapping("Age", 1) },
                { nameof(Email), new ColumnMapping("Email", 2) }
            };
        }
    }

    private class TestProduct : IMapped
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool InStock { get; set; }

        public Dictionary<string, ColumnMapping> GetColumnMapping()
        {
            return new Dictionary<string, ColumnMapping>
            {
                { nameof(ProductName), new ColumnMapping("ProductName", 0) },
                { nameof(Price), new ColumnMapping("Price", 1) },
                { nameof(Quantity), new ColumnMapping("Quantity", 2) },
                { nameof(InStock), new ColumnMapping("InStock", 3) }
            };
        }
    }

    // ========== End-to-End Parsing Tests ==========

    [Fact]
    public void ParseFile_ValidCSV_ReturnsAllRecords()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "John Doe,30,john@example.com",
            "Jane Smith,25,jane@example.com",
            "Bob Johnson,35,bob@example.com"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.Equal("John Doe", records[0].Name);
        Assert.Equal(30, records[0].Age);
        Assert.Equal("john@example.com", records[0].Email);
    }

    [Fact]
    public void ParseFile_NoHeaderRow_UsesColumnIndices()
    {
        var csv = new[]
        {
            "John Doe,30,john@example.com",
            "Jane Smith,25,jane@example.com"
        };

        var options = new CsvParserOptions { HasHeaderRow = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John Doe", records[0].Name);
        Assert.Equal("Jane Smith", records[1].Name);
    }

    [Fact]
    public void ParseFile_CustomDelimiter_ParsesCorrectly()
    {
        var csv = new[]
        {
            "Name;Age;Email",
            "John Doe;30;john@example.com",
            "Jane Smith;25;jane@example.com"
        };

        var options = new CsvParserOptions { Delimiter = ';' };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John Doe", records[0].Name);
        Assert.Equal(30, records[0].Age);
    }

    [Fact]
    public void ParseFile_QuotedFields_HandlesCorrectly()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "\"Doe, John\",30,john@example.com",
            "\"Smith, Jane\",25,jane@example.com"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("Doe, John", records[0].Name);
        Assert.Equal("Smith, Jane", records[1].Name);
    }

    [Fact]
    public void ParseFile_EscapedQuotes_HandlesCorrectly()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "\"John \"\"The Boss\"\" Doe\",30,john@example.com"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John \"The Boss\" Doe", records[0].Name);
    }

    [Fact]
    public void ParseFile_EmptyFields_HandlesCorrectly()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "John Doe,30,",
            ",25,jane@example.com",
            "Bob,35,"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.Equal("", records[0].Email);
        Assert.Equal("", records[1].Name);
        Assert.Equal("", records[2].Email);
    }

    [Fact]
    public void ParseFile_TrimFields_TrimsWhitespace()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "  John Doe  ,30,  john@example.com  "
        };

        var options = new CsvParserOptions { TrimFields = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John Doe", records[0].Name);
        Assert.Equal("john@example.com", records[0].Email);
    }

    [Fact]
    public void ParseFile_NoTrimFields_PreservesWhitespace()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "  John Doe  ,30,  john@example.com  "
        };

        var options = new CsvParserOptions { TrimFields = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("  John Doe  ", records[0].Name);
        Assert.Equal("  john@example.com  ", records[0].Email);
    }

    [Fact]
    public void ParseFile_SkipEmptyLines_SkipsEmptyLines()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "John Doe,30,john@example.com",
            "",
            "Jane Smith,25,jane@example.com",
            "   ",
            "Bob Johnson,35,bob@example.com"
        };

        var options = new CsvParserOptions { SkipEmptyLines = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
    }

    [Fact]
    public void ParseFile_MultipleTypes_ParsesCorrectly()
    {
        var csv = new[]
        {
            "ProductName,Price,Quantity,InStock",
            "Widget,19.99,100,true",
            "Gadget,29.99,50,false",
            "Doohickey,9.99,200,yes"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestProduct>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.Equal("Widget", records[0].ProductName);
        Assert.Equal(19.99m, records[0].Price);
        Assert.Equal(100, records[0].Quantity);
        Assert.True(records[0].InStock);
        Assert.False(records[1].InStock);
        Assert.True(records[2].InStock);
    }

    // ========== Error Handling Integration Tests ==========

    [Fact]
    public void ParseFile_LenientMode_WithMixedErrors_ParsesValidLines()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "John Doe,30,john@example.com",
            "\"Unclosed,25,error@example.com",
            "Jane Smith,25,jane@example.com",
            "Bob,not-a-number,bob@example.com",
            "Charlie Brown,35,charlie@example.com"
        };

        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.Equal("John Doe", records[0].Name);
        Assert.Equal("Jane Smith", records[1].Name);
        Assert.Equal("Charlie Brown", records[2].Name);
    }

    [Fact]
    public void ParseFile_StrictMode_WithError_ThrowsImmediately()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "John Doe,30,john@example.com",
            "\"Unclosed,25,error@example.com"
        };

        var options = new CsvParserOptions { StrictMode = true };
        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<CsvParseException>(() =>
            reader.DeserializeLines(csv));

        Assert.Contains("Line 3", exception.Message);
    }

    // ========== CaseInsensitiveHeaders Tests ==========

    [Fact]
    public void ParseFile_CaseInsensitiveHeaders_MatchesAnyCase()
    {
        var csv = new[]
        {
            "NAME,AGE,EMAIL",
            "John Doe,30,john@example.com"
        };

        var options = new CsvParserOptions { CaseInsensitiveHeaders = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John Doe", records[0].Name);
    }

    [Fact]
    public void ParseFile_CaseInsensitiveHeaders_MixedCase_Works()
    {
        var csv = new[]
        {
            "nAmE,AgE,eMaIl",
            "John Doe,30,john@example.com"
        };

        var options = new CsvParserOptions { CaseInsensitiveHeaders = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal(30, records[0].Age);
    }

    // ========== Large Dataset Tests ==========

    [Fact]
    public void ParseFile_LargeDataset_ProcessesAll()
    {
        var csv = new List<string> { "Name,Age,Email" };

        for (int i = 1; i <= 1000; i++)
        {
            csv.Add($"Person{i},{20 + (i % 50)},person{i}@example.com");
        }

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(1000, records.Count);
        Assert.Equal("Person1", records[0].Name);
        Assert.Equal("Person1000", records[999].Name);
    }

    [Fact]
    public void ParseFile_VeryLongFields_HandlesCorrectly()
    {
        var longValue = new string('A', 10000);
        var csv = new[]
        {
            "Name,Age,Email",
            $"{longValue},30,email@example.com"
        };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal(longValue, records[0].Name);
    }

    // ========== Edge Case Integration Tests ==========

    [Fact]
    public void ParseFile_OnlyHeader_ReturnsEmptyList()
    {
        var csv = new[] { "Name,Age,Email" };

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Empty(records);
    }

    [Fact]
    public void ParseFile_EmptyCSV_ReturnsEmptyList()
    {
        var csv = Array.Empty<string>();

        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Empty(records);
    }

    [Fact]
    public void ParseFile_AllEmptyLines_ReturnsEmptyList()
    {
        var csv = new[] { "", "   ", "\t" };

        var options = new CsvParserOptions { SkipEmptyLines = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Empty(records);
    }

    [Fact]
    public void ParseFile_ComplexRealWorldScenario_WorksCorrectly()
    {
        var csv = new[]
        {
            "Name,Age,Email",
            "\"Doe, John\",30,john.doe@company.com",
            "Jane Smith,25,jane@example.com",
            "",
            "\"Bob \"\"The Builder\"\" Johnson\",35,bob@builder.com",
            "  Alice Williams  ,28,  alice@example.com  ",
            "Charlie Brown,32,charlie@peanuts.com"
        };

        var options = new CsvParserOptions
        {
            TrimFields = true,
            SkipEmptyLines = true,
            CaseInsensitiveHeaders = true
        };

        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(5, records.Count);
        Assert.Equal("Doe, John", records[0].Name);
        Assert.Equal("Bob \"The Builder\" Johnson", records[2].Name);
        Assert.Equal("Alice Williams", records[3].Name);
        Assert.Equal("alice@example.com", records[3].Email);
    }
}
