using CsvReader.Models;

namespace CsvReader.IntegrationTests;

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
        var options = new CsvParserOptions { Delimiter = delimiter };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
        Assert.Equal(30, results[0].Age);
        Assert.True(results[0].Active);
    }

    // ========== HasHeaderRow Option Tests ==========

    [Fact]
    public void HasHeaderRow_True_UsesHeaderMapping()
    {
        var csv = new[] { "Name,Age,Active", "John,30,true" };
        var options = new CsvParserOptions { HasHeaderRow = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
    }

    [Fact]
    public void HasHeaderRow_False_UsesIndexMapping()
    {
        var csv = new[] { "John,30,true", "Jane,25,false" };
        var options = new CsvParserOptions { HasHeaderRow = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("John", results[0].Name);
        Assert.Equal("Jane", results[1].Name);
    }

    // ========== SkipEmptyLines Option Tests ==========

    [Fact]
    public void SkipEmptyLines_True_SkipsEmptyLines()
    {
        var csv = new[] { "Name,Age,Active", "", "John,30,true", "   ", "Jane,25,false" };
        var options = new CsvParserOptions { SkipEmptyLines = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void SkipEmptyLines_False_LenientMode_SkipsEmptyLinesWithLogging()
    {
        var logFile = Path.GetTempFileName();
        try
        {
            var csv = new[] { "Name,Age,Active", "", "John,30,true" };
            var options = new CsvParserOptions
            {
                SkipEmptyLines = false,
                StrictMode = false,
                ErrorLogFile = logFile
            };
            var reader = new CsvReader<TestPerson>(options);
            var results = reader.DeserializeLines(csv).ToList();

            Assert.Single(results);
            var log = File.ReadAllText(logFile);
            Assert.Contains("Empty line", log);
        }
        finally
        {
            if (File.Exists(logFile)) File.Delete(logFile);
        }
    }

    // ========== TrimFields Option Tests ==========

    [Fact]
    public void TrimFields_True_TrimsWhitespace()
    {
        var csv = new[] { "Name,Age,Active", "  John  ,  30  ,  true  " };
        var options = new CsvParserOptions { TrimFields = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
        Assert.Equal(30, results[0].Age);
    }

    [Fact]
    public void TrimFields_False_PreservesWhitespace()
    {
        var csv = new[] { "Name,Age,Active", "  John  ,30,true" };
        var options = new CsvParserOptions { TrimFields = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("  John  ", results[0].Name);
    }

    // ========== CaseInsensitiveHeaders Option Tests ==========

    [Fact]
    public void CaseInsensitiveHeaders_True_MatchesAnyCase()
    {
        var csv = new[] { "NAME,AGE,ACTIVE", "John,30,true" };
        var options = new CsvParserOptions { CaseInsensitiveHeaders = true };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
    }

    [Fact]
    public void CaseInsensitiveHeaders_False_StrictMode_RequiresExactCase()
    {
        var csv = new[] { "NAME,AGE,ACTIVE", "John,30,true" };
        var options = new CsvParserOptions
        {
            CaseInsensitiveHeaders = false,
            StrictMode = true
        };
        var reader = new CsvReader<TestPerson>(options);

        // Should fail to find columns with exact case matching in strict mode
        var exception = Assert.Throws<InvalidOperationException>(() =>
            reader.DeserializeLines(csv).ToList());

        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public void CaseInsensitiveHeaders_False_ExactCaseMatch_Works()
    {
        var csv = new[] { "Name,Age,Active", "John,30,true" };
        var options = new CsvParserOptions { CaseInsensitiveHeaders = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
    }

    // ========== StrictMode Option Tests ==========

    [Fact]
    public void StrictMode_False_SkipsErrors()
    {
        var csv = new[] { "Name,Age,Active", "John,30,true", "Jane,invalid,false" };
        var options = new CsvParserOptions { StrictMode = false };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
        Assert.Equal("John", results[0].Name);
    }

    [Fact]
    public void StrictMode_True_ThrowsOnError()
    {
        var csv = new[] { "Name,Age,Active", "John,30,true", "Jane,invalid,false" };
        var options = new CsvParserOptions { StrictMode = true };
        var reader = new CsvReader<TestPerson>(options);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            reader.DeserializeLines(csv).ToList());

        Assert.Contains("Line 3", exception.Message);
    }

    // ========== ErrorLogFile Option Tests ==========

    [Fact]
    public void ErrorLogFile_Null_NoLogging()
    {
        var csv = new[] { "Name,Age,Active", "John,30,true", "Jane,invalid,false" };
        var options = new CsvParserOptions { StrictMode = false, ErrorLogFile = null };
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Single(results);
    }

    [Fact]
    public void ErrorLogFile_Set_LogsErrors()
    {
        var logFile = Path.GetTempFileName();
        try
        {
            var csv = new[] { "Name,Age,Active", "John,30,true", "Jane,invalid,false" };
            var options = new CsvParserOptions
            {
                StrictMode = false,
                ErrorLogFile = logFile
            };
            var reader = new CsvReader<TestPerson>(options);
            var results = reader.DeserializeLines(csv).ToList();

            Assert.Single(results);
            Assert.True(File.Exists(logFile));
            var log = File.ReadAllText(logFile);
            Assert.Contains("Line 3", log);
        }
        finally
        {
            if (File.Exists(logFile)) File.Delete(logFile);
        }
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
        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(3, results.Count);
        Assert.True(results[0].Active);
        Assert.True(results[1].Active);
        Assert.True(results[2].Active);
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
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.Active));
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
        var options = new CsvParserOptions();
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.False(r.Active));
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
        var reader = new CsvReader<TestPerson>(options);
        var results = reader.DeserializeLines(csv).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.False(r.Active));
    }

    // ========== Combined Options Tests ==========

    [Fact]
    public void CombinedOptions_AllTogether_WorksCorrectly()
    {
        var logFile = Path.GetTempFileName();
        try
        {
            var csv = new[]
            {
                "NAME;AGE;ACTIVE",
                "",
                "  John  ;  30  ;  Y  ",
                "  Jane  ;  invalid  ;  N  ",
                "  Bob  ;  35  ;  Off  "
            };
            var options = new CsvParserOptions
            {
                Delimiter = ';',
                HasHeaderRow = true,
                SkipEmptyLines = true,
                TrimFields = true,
                CaseInsensitiveHeaders = true,
                StrictMode = false,
                ErrorLogFile = logFile,
                BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Y", "On" },
                BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "N", "Off" }
            };
            var reader = new CsvReader<TestPerson>(options);
            var results = reader.DeserializeLines(csv).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal("John", results[0].Name);
            Assert.Equal(30, results[0].Age);
            Assert.True(results[0].Active);
            Assert.Equal("Bob", results[1].Name);
            Assert.False(results[1].Active);

            var log = File.ReadAllText(logFile);
            Assert.Contains("Line 4", log);
        }
        finally
        {
            if (File.Exists(logFile)) File.Delete(logFile);
        }
    }
}
