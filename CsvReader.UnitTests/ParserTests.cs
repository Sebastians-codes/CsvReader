using CsvReader.Core;
using CsvReader.Errors;

namespace CsvReader.Tests;

public class ParserTests
{
    private readonly Parser _parser = new();

    [Fact]
    public void ParseLine_SingleField_ReturnsSingleField()
    {
        var result = _parser.ParseLine("value");

        Assert.Single(result);
        Assert.Equal("value", result[0]);
    }

    [Fact]
    public void ParseLine_TwoFields_ReturnsTwoFields()
    {
        var result = _parser.ParseLine("field1,field2");

        Assert.Equal(2, result.Length);
        Assert.Equal("field1", result[0]);
        Assert.Equal("field2", result[1]);
    }

    [Fact]
    public void ParseLine_MultipleFields_ReturnsAllFields()
    {
        var result = _parser.ParseLine("one,two,three,four,five");

        Assert.Equal(5, result.Length);
        Assert.Equal("one", result[0]);
        Assert.Equal("two", result[1]);
        Assert.Equal("three", result[2]);
        Assert.Equal("four", result[3]);
        Assert.Equal("five", result[4]);
    }

    // ========== Empty Field Tests ==========

    [Fact]
    public void ParseLine_EmptyString_ReturnsSingleEmptyField()
    {
        var result = _parser.ParseLine("");

        Assert.Single(result);
        Assert.Equal("", result[0]);
    }

    [Fact]
    public void ParseLine_TwoEmptyFields_ReturnsTwoEmptyFields()
    {
        var result = _parser.ParseLine(",");

        Assert.Equal(2, result.Length);
        Assert.Equal("", result[0]);
        Assert.Equal("", result[1]);
    }

    [Fact]
    public void ParseLine_EmptyFieldAtStart_ReturnsEmptyFirstField()
    {
        var result = _parser.ParseLine(",value");

        Assert.Equal(2, result.Length);
        Assert.Equal("", result[0]);
        Assert.Equal("value", result[1]);
    }

    [Fact]
    public void ParseLine_EmptyFieldAtEnd_ReturnsEmptyLastField()
    {
        var result = _parser.ParseLine("value,");

        Assert.Equal(2, result.Length);
        Assert.Equal("value", result[0]);
        Assert.Equal("", result[1]);
    }

    [Fact]
    public void ParseLine_EmptyFieldInMiddle_ReturnsEmptyMiddleField()
    {
        var result = _parser.ParseLine("first,,last");

        Assert.Equal(3, result.Length);
        Assert.Equal("first", result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("last", result[2]);
    }

    [Fact]
    public void ParseLine_MultipleConsecutiveDelimiters_ReturnsMultipleEmptyFields()
    {
        var result = _parser.ParseLine("a,,,b");

        Assert.Equal(4, result.Length);
        Assert.Equal("a", result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("", result[2]);
        Assert.Equal("b", result[3]);
    }

    // ========== Quoted Field Tests ==========

    [Fact]
    public void ParseLine_SingleQuotedField_ReturnsFieldWithoutQuotes()
    {
        var result = _parser.ParseLine("\"quoted\"");

        Assert.Single(result);
        Assert.Equal("quoted", result[0]);
    }

    [Fact]
    public void ParseLine_QuotedFieldWithComma_ReturnsFieldWithComma()
    {
        var result = _parser.ParseLine("\"value,with,commas\"");

        Assert.Single(result);
        Assert.Equal("value,with,commas", result[0]);
    }

    [Fact]
    public void ParseLine_MixedQuotedAndUnquotedFields_ReturnsAllFields()
    {
        var result = _parser.ParseLine("normal,\"quoted\",another");

        Assert.Equal(3, result.Length);
        Assert.Equal("normal", result[0]);
        Assert.Equal("quoted", result[1]);
        Assert.Equal("another", result[2]);
    }

    [Fact]
    public void ParseLine_QuotedFieldWithDelimiter_TreatsDelimiterAsLiteral()
    {
        var result = _parser.ParseLine("before,\"has,delimiter\",after");

        Assert.Equal(3, result.Length);
        Assert.Equal("before", result[0]);
        Assert.Equal("has,delimiter", result[1]);
        Assert.Equal("after", result[2]);
    }

    [Fact]
    public void ParseLine_EmptyQuotedField_ReturnsEmptyString()
    {
        var result = _parser.ParseLine("\"\"");

        Assert.Single(result);
        Assert.Equal("", result[0]);
    }

    [Fact]
    public void ParseLine_EmptyQuotedFieldBetweenValues_ReturnsEmptyField()
    {
        var result = _parser.ParseLine("first,\"\",last");

        Assert.Equal(3, result.Length);
        Assert.Equal("first", result[0]);
        Assert.Equal("", result[1]);
        Assert.Equal("last", result[2]);
    }

    // ========== Escaped Quote Tests ==========

    [Fact]
    public void ParseLine_EscapedQuoteInQuotedField_ReturnsSingleQuote()
    {
        var result = _parser.ParseLine("\"value \"\"with\"\" quotes\"");

        Assert.Single(result);
        Assert.Equal("value \"with\" quotes", result[0]);
    }

    [Fact]
    public void ParseLine_MultipleEscapedQuotes_ReturnsMultipleSingleQuotes()
    {
        var result = _parser.ParseLine("\"\"\"quoted\"\" \"\"text\"\"\"");

        Assert.Single(result);
        Assert.Equal("\"quoted\" \"text\"", result[0]);
    }

    [Fact]
    public void ParseLine_EscapedQuoteAtStart_ReturnsSingleQuote()
    {
        var result = _parser.ParseLine("\"\"\"start\"");

        Assert.Single(result);
        Assert.Equal("\"start", result[0]);
    }

    [Fact]
    public void ParseLine_EscapedQuoteAtEnd_ReturnsSingleQuote()
    {
        var result = _parser.ParseLine("\"end\"\"\"");

        Assert.Single(result);
        Assert.Equal("end\"", result[0]);
    }

    [Fact]
    public void ParseLine_OnlyEscapedQuotes_ReturnsSingleQuote()
    {
        var result = _parser.ParseLine("\"\"\"\"");

        Assert.Single(result);
        Assert.Equal("\"", result[0]);
    }

    [Fact]
    public void ParseLine_ComplexEscapedQuotesWithCommas_ParsesCorrectly()
    {
        var result = _parser.ParseLine("\"He said \"\"Hello, world!\"\"\"");

        Assert.Single(result);
        Assert.Equal("He said \"Hello, world!\"", result[0]);
    }

    // ========== Custom Delimiter Tests ==========

    [Theory]
    [InlineData(',', "a,b,c", new[] { "a", "b", "c" })]
    [InlineData(';', "field1;field2;field3", new[] { "field1", "field2", "field3" })]
    [InlineData('|', "a|b|c", new[] { "a", "b", "c" })]
    [InlineData('\t', "col1\tcol2\tcol3", new[] { "col1", "col2", "col3" })]
    [InlineData(':', "one:two:three", new[] { "one", "two", "three" })]
    [InlineData(' ', "x y z", new[] { "x", "y", "z" })]
    public void ParseLine_CustomDelimiters_ParseCorrectly(char delimiter, string input, string[] expected)
    {
        var result = _parser.ParseLine(input, delimiter);

        Assert.Equal(expected.Length, result.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], result[i]);
        }
    }

    [Theory]
    [InlineData(';', "normal;\"has;delimiter\";after", new[] { "normal", "has;delimiter", "after" })]
    [InlineData('|', "a|\"b|c\"|d", new[] { "a", "b|c", "d" })]
    [InlineData('\t', "x\t\"y\tz\"\tw", new[] { "x", "y\tz", "w" })]
    public void ParseLine_QuotedFieldWithCustomDelimiter_TreatsDelimiterAsLiteral(char delimiter, string input, string[] expected)
    {
        var result = _parser.ParseLine(input, delimiter);

        Assert.Equal(expected.Length, result.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], result[i]);
        }
    }

    // ========== Unclosed Quote Tests (Error Cases) ==========

    [Fact]
    public void ParseLine_UnclosedQuoteAtStart_ThrowsFormatException()
    {
        var exception = Assert.Throws<UnclosedQuoteException>(() =>
            _parser.ParseLine("\"unclosed"));

        Assert.Contains("Unclosed quote", exception.Message);
    }

    [Fact]
    public void ParseLine_UnclosedQuoteInMiddle_ThrowsFormatException()
    {
        var exception = Assert.Throws<UnclosedQuoteException>(() =>
            _parser.ParseLine("normal,\"unclosed,value"));

        Assert.Contains("Unclosed quote", exception.Message);
    }

    [Fact]
    public void ParseLine_UnclosedQuoteAtEnd_ThrowsFormatException()
    {
        var exception = Assert.Throws<UnclosedQuoteException>(() =>
            _parser.ParseLine("normal,\"unclosed"));

        Assert.Contains("Unclosed quote", exception.Message);
    }

    [Fact]
    public void ParseLine_OddNumberOfQuotes_ThrowsFormatException()
    {
        var exception = Assert.Throws<UnclosedQuoteException>(() =>
            _parser.ParseLine("\"quoted\" text with \" extra quote"));

        Assert.Contains("Unclosed quote", exception.Message);
    }

    // ========== Whitespace Handling Tests ==========

    [Fact]
    public void ParseLine_LeadingWhitespace_PreservesWhitespace()
    {
        var result = _parser.ParseLine("  value");

        Assert.Single(result);
        Assert.Equal("  value", result[0]);
    }

    [Fact]
    public void ParseLine_TrailingWhitespace_PreservesWhitespace()
    {
        var result = _parser.ParseLine("value  ");

        Assert.Single(result);
        Assert.Equal("value  ", result[0]);
    }

    [Fact]
    public void ParseLine_WhitespaceAroundDelimiter_PreservesWhitespace()
    {
        var result = _parser.ParseLine("value1 , value2");

        Assert.Equal(2, result.Length);
        Assert.Equal("value1 ", result[0]);
        Assert.Equal(" value2", result[1]);
    }

    [Fact]
    public void ParseLine_WhitespaceInQuotedField_PreservesWhitespace()
    {
        var result = _parser.ParseLine("\"  spaces  \"");

        Assert.Single(result);
        Assert.Equal("  spaces  ", result[0]);
    }

    [Fact]
    public void ParseLine_WhitespaceOutsideQuotes_PreservesWhitespace()
    {
        var result = _parser.ParseLine("  \"quoted\"  ");

        Assert.Single(result);
        Assert.Equal("  quoted  ", result[0]);
    }

    // ========== Special Character Tests ==========

    [Fact]
    public void ParseLine_TabCharacterInQuotedField_PreservesTab()
    {
        var result = _parser.ParseLine("\"has\ttab\"");

        Assert.Single(result);
        Assert.Equal("has\ttab", result[0]);
    }

    [Fact]
    public void ParseLine_NewlineCharacterInUnquotedField_PreservesNewline()
    {
        var result = _parser.ParseLine("has\nnewline");

        Assert.Single(result);
        Assert.Equal("has\nnewline", result[0]);
    }

    [Fact]
    public void ParseLine_SpecialCharactersInQuotedField_PreservesAll()
    {
        var result = _parser.ParseLine("\"!@#$%^&*()\"");

        Assert.Single(result);
        Assert.Equal("!@#$%^&*()", result[0]);
    }

    [Fact]
    public void ParseLine_UnicodeCharacters_PreservesUnicode()
    {
        var result = _parser.ParseLine("日本語,Ελληνικά,Русский");

        Assert.Equal(3, result.Length);
        Assert.Equal("日本語", result[0]);
        Assert.Equal("Ελληνικά", result[1]);
        Assert.Equal("Русский", result[2]);
    }

    // ========== Edge Cases ==========

    [Fact]
    public void ParseLine_OnlyDelimiters_ReturnsAllEmptyFields()
    {
        var result = _parser.ParseLine(",,,");

        Assert.Equal(4, result.Length);
        Assert.All(result, field => Assert.Equal("", field));
    }

    [Fact]
    public void ParseLine_VeryLongField_ParsesCorrectly()
    {
        var longValue = new string('a', 10000);
        var result = _parser.ParseLine(longValue);

        Assert.Single(result);
        Assert.Equal(longValue, result[0]);
    }

    [Fact]
    public void ParseLine_ManyFields_ParsesAllCorrectly()
    {
        var line = string.Join(",", Enumerable.Range(1, 100).Select(i => $"field{i}"));
        var result = _parser.ParseLine(line);

        Assert.Equal(100, result.Length);
        for (int i = 0; i < 100; i++)
        {
            Assert.Equal($"field{i + 1}", result[i]);
        }
    }

    [Fact]
    public void ParseLine_QuotedFieldFollowedByDelimiter_ParsesCorrectly()
    {
        var result = _parser.ParseLine("\"quoted\",next");

        Assert.Equal(2, result.Length);
        Assert.Equal("quoted", result[0]);
        Assert.Equal("next", result[1]);
    }

    [Fact]
    public void ParseLine_DelimiterFollowedByQuotedField_ParsesCorrectly()
    {
        var result = _parser.ParseLine("first,\"quoted\"");

        Assert.Equal(2, result.Length);
        Assert.Equal("first", result[0]);
        Assert.Equal("quoted", result[1]);
    }

    [Fact]
    public void ParseLine_MultipleQuotedFieldsInRow_ParsesAllCorrectly()
    {
        var result = _parser.ParseLine("\"first\",\"second\",\"third\"");

        Assert.Equal(3, result.Length);
        Assert.Equal("first", result[0]);
        Assert.Equal("second", result[1]);
        Assert.Equal("third", result[2]);
    }

    // ========== RFC 4180 Compliance Tests ==========

    [Fact]
    public void ParseLine_RFC4180Example1_ParsesCorrectly()
    {
        // Example from RFC 4180: aaa,bbb,ccc
        var result = _parser.ParseLine("aaa,bbb,ccc");

        Assert.Equal(3, result.Length);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("bbb", result[1]);
        Assert.Equal("ccc", result[2]);
    }

    [Fact]
    public void ParseLine_RFC4180Example2_ParsesCorrectly()
    {
        // Example from RFC 4180: "aaa","bbb","ccc"
        var result = _parser.ParseLine("\"aaa\",\"bbb\",\"ccc\"");

        Assert.Equal(3, result.Length);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("bbb", result[1]);
        Assert.Equal("ccc", result[2]);
    }

    [Fact]
    public void ParseLine_RFC4180Example3_ParsesCorrectly()
    {
        // Example from RFC 4180: "aaa","b,bb","ccc"
        var result = _parser.ParseLine("\"aaa\",\"b,bb\",\"ccc\"");

        Assert.Equal(3, result.Length);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("b,bb", result[1]);
        Assert.Equal("ccc", result[2]);
    }

    [Fact]
    public void ParseLine_RFC4180Example4_ParsesCorrectly()
    {
        // Example from RFC 4180: "aaa","b""bb","ccc"
        var result = _parser.ParseLine("\"aaa\",\"b\"\"bb\",\"ccc\"");

        Assert.Equal(3, result.Length);
        Assert.Equal("aaa", result[0]);
        Assert.Equal("b\"bb", result[1]);
        Assert.Equal("ccc", result[2]);
    }
}
