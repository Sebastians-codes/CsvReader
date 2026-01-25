using Csv.Reader.Errors;
using Csv.Reader.Models;

namespace Csv.Reader.IntegrationTests;

public class OptionsTests
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

    // ========== Delimiter Option Tests ==========

    [Theory]
    [InlineData(',', "Name,Age,Active", "John,30,true")]
    [InlineData(';', "Name;Age;Active", "John;30;true")]
    [InlineData('|', "Name|Age|Active", "John|30|true")]
    [InlineData('\t', "Name\tAge\tActive", "John\t30\ttrue")]
    [InlineData(':', "Name:Age:Active", "John:30:true")]
    public void Delimiter_VariousDelimiters_Work(char delimiter, string header, string dataLine)
    {
        var csv = new[] { header, dataLine };
        var options = new CsvParserOptions
        {
            Delimiter = delimiter
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
        Assert.Equal(30, records[0].Age);
        Assert.True(records[0].Active);
    }

    // ========== HasHeaderRow Option Tests ==========

    [Fact]
    public void HasHeaderRow_True_UsesHeaderMapping()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
    }

    [Fact]
    public void HasHeaderRow_False_UsesIndexMapping()
    {
        var csv = new[]
        {
            "John,30,true",
            "Jane,25,false"
        };

        var options = new CsvParserOptions { HasHeaderRow = false };

        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
        Assert.Equal("John", records[0].Name);
        Assert.Equal("Jane", records[1].Name);
    }

    // ========== SkipEmptyLines Option Tests ==========

    [Fact]
    public void SkipEmptyLines_True_SkipsEmptyLines()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "",
            "John,30,true",
            "   ",
            "Jane,25,false"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(2, records.Count);
    }

    // ========== TrimFields Option Tests ==========

    [Fact]
    public void TrimFields_True_TrimsWhitespace()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "  John  ,  30  ,  true  "
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
        Assert.Equal(30, records[0].Age);
    }

    [Fact]
    public void TrimFields_False_PreservesWhitespace()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "  John  ,30,true"
        };

        var options = new CsvParserOptions { TrimFields = false };
        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("  John  ", records[0].Name);
    }

    // ========== CaseInsensitiveHeaders Option Tests ==========

    [Fact]
    public void CaseInsensitiveHeaders_True_MatchesAnyCase()
    {
        var csv = new[]
        {
            "NAME,AGE,ACTIVE",
            "John,30,true"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
    }

    [Fact]
    public void CaseInsensitiveHeaders_False_StrictMode_RequiresExactCase()
    {
        var csv = new[]
        {
            "NAME,AGE,ACTIVE",
            "John,30,true"
        };

        var options = new CsvParserOptions
        {
            CaseInsensitiveHeaders = false,
            StrictMode = true
        };

        // Should fail to find columns with exact case matching in strict mode
        var exception = Assert.Throws<CsvParseException>(() =>
            CsvReader.DeserializeLines<TestPerson>(csv, options));

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void CaseInsensitiveHeaders_False_ExactCaseMatch_Works()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true"
        };

        var options = new CsvParserOptions { CaseInsensitiveHeaders = false };
        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
    }

    // ========== StrictMode Option Tests ==========

    [Fact]
    public void StrictMode_False_SkipsErrors()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Jane,invalid,false"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Single(records);
        Assert.Equal("John", records[0].Name);
    }

    [Fact]
    public void StrictMode_True_ThrowsOnError()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Jane,invalid,false"
        };

        var options = new CsvParserOptions { StrictMode = true };

        var exception = Assert.Throws<CsvParseException>(() =>
            CsvReader.DeserializeLines<TestPerson>(csv, options));

        Assert.Contains("Line 3", exception.Message);
    }

    // ========== BooleanTruthyValues Option Tests ==========

    [Fact]
    public void BooleanTruthyValues_Default_ParsesStandardValues()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,true",
            "Jane,25,1",
            "Bob,35,yes"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.True(records[0].Active);
        Assert.True(records[1].Active);
        Assert.True(records[2].Active);
    }

    [Fact]
    public void BooleanTruthyValues_Custom_ParsesCustomValues()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,Y",
            "Jane,25,On",
            "Bob,35,Enabled"
        };

        var options = new CsvParserOptions
        {
            BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Y", "On", "Enabled"
            },
            BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "N", "Off", "Disabled"
            }
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.All(records, r => Assert.True(r.Active));
    }

    // ========== BooleanFalsyValues Option Tests ==========

    [Fact]
    public void BooleanFalsyValues_Default_ParsesStandardValues()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,false",
            "Jane,25,0",
            "Bob,35,no"
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.All(records, r => Assert.False(r.Active));
    }

    [Fact]
    public void BooleanFalsyValues_Custom_ParsesCustomValues()
    {
        var csv = new[]
        {
            "Name,Age,Active",
            "John,30,N",
            "Jane,25,Off",
            "Bob,35,Disabled"
        };

        var options = new CsvParserOptions
        {
            BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Y", "On", "Enabled"
            },
            BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "N", "Off", "Disabled"
            }
        };

        var results = CsvReader.DeserializeLines<TestPerson>(csv, options);
        _ = results.HasErrors;
        var records = results.Records.ToList();

        Assert.Equal(3, records.Count);
        Assert.All(records, r => Assert.False(r.Active));
    }
}
