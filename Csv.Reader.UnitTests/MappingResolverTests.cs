using Csv.Reader.Errors;
using Csv.Reader.Mapping;
using Csv.Reader.Models;

namespace Csv.Reader.UnitTests;

public class MappingResolverTests
{
    private readonly MappingResolver _resolver = new();

    // ========== ResolveColumnIndex with Header Map Tests ==========

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_ColumnExists_ReturnsCorrectIndex()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "Name", 0 },
            { "Age", 1 },
            { "Email", 2 }
        };
        var mapping = new ColumnMapping("Age", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(1, result);
    }

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_ColumnNotFound_ThrowsInvalidOperationException()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "Name", 0 },
            { "Age", 1 }
        };
        var mapping = new ColumnMapping("Email", -1);

        var exception = Assert.Throws<ColumnNotFoundException>(() =>
            _resolver.ResolveColumnIndex(mapping, headerMap));

        Assert.Contains("Column 'Email' not found", exception.Message);
    }

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_CaseInsensitive_ReturnsCorrectIndex()
    {
        var headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "name", 0 },
            { "age", 1 }
        };
        var mapping = new ColumnMapping("NAME", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_FirstColumn_ReturnsZero()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "Name", 0 }
        };
        var mapping = new ColumnMapping("Name", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_LastColumn_ReturnsLastIndex()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "Name", 0 },
            { "Age", 1 },
            { "Email", 2 },
            { "Phone", 3 }
        };
        var mapping = new ColumnMapping("Phone", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(3, result);
    }

    // ========== ResolveColumnIndex without Header Map Tests ==========

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithPositiveColumnIndex_ReturnsColumnIndex()
    {
        var mapping = new ColumnMapping("Column", 2);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithZeroIndex_ReturnsZero()
    {
        var mapping = new ColumnMapping("Column", 0);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithNumericIdentifier_ReturnsIndex()
    {
        var mapping = new ColumnMapping("3", -1);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(3, result);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithZeroIdentifier_ReturnsZero()
    {
        var mapping = new ColumnMapping("0", -1);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithNonNumericIdentifier_ThrowsInvalidOperationException()
    {
        var mapping = new ColumnMapping("Name", -1);

        var exception = Assert.Throws<ColumnMappingException>(() =>
            _resolver.ResolveColumnIndex(mapping, null));

        Assert.Contains("must be numeric", exception.Message);
        Assert.Contains("Name", exception.Message);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_WithNegativeColumnIndex_UsesNumericIdentifier()
    {
        var mapping = new ColumnMapping("5", -1);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(5, result);
    }

    // ========== ValidateColumnIndex Tests ==========

    [Fact]
    public void ValidateColumnIndex_ValidIndex_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(0, 3, 3, false, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateColumnIndex_IndexZero_WithOneField_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(0, 1, 1, false, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateColumnIndex_LastIndex_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(4, 5, 5, false, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateColumnIndex_NegativeIndex_ThrowsIndexOutOfRangeException()
    {
        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(-1, 3, 3, false, 1));

        Assert.Contains("out of range", exception.Message);
        Assert.Contains("-1", exception.Message);
    }

    [Fact]
    public void ValidateColumnIndex_IndexEqualToFieldCount_ThrowsIndexOutOfRangeException()
    {
        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(3, 3, 3, false, 1));

        Assert.Contains("out of range", exception.Message);
        Assert.Contains("3", exception.Message);
    }

    [Fact]
    public void ValidateColumnIndex_IndexGreaterThanFieldCount_ThrowsIndexOutOfRangeException()
    {
        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(5, 3, 3, false, 1));

        Assert.Contains("out of range", exception.Message);
        Assert.Contains("has 3 columns", exception.Message);
    }

    [Fact]
    public void ValidateColumnIndex_ZeroFields_AnyIndex_ThrowsIndexOutOfRangeException()
    {
        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(0, 0, 0, false, 1));

        Assert.Contains("out of range", exception.Message);
        Assert.Contains("has 0 columns", exception.Message);
    }

    // ========== Edge Case Tests ==========

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_EmptyIdentifier_ReturnsIndexIfExists()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "", 0 },
            { "Name", 1 }
        };
        var mapping = new ColumnMapping("", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ResolveColumnIndex_WithHeaderMap_SpecialCharacters_ReturnsCorrectIndex()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "First-Name", 0 },
            { "Email@Address", 1 },
            { "Phone#", 2 }
        };
        var mapping = new ColumnMapping("Email@Address", -1);

        var result = _resolver.ResolveColumnIndex(mapping, headerMap);

        Assert.Equal(1, result);
    }

    [Fact]
    public void ResolveColumnIndex_NoHeaderMap_LargeNumericIdentifier_ReturnsLargeIndex()
    {
        var mapping = new ColumnMapping("999", -1);

        var result = _resolver.ResolveColumnIndex(mapping, null);

        Assert.Equal(999, result);
    }

    [Fact]
    public void ValidateColumnIndex_LargeFieldCount_ValidIndex_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(999, 1000, 1000, false, 1));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateColumnIndex_SingleField_IndexOne_ThrowsException()
    {
        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(1, 1, 1, false, 1));

        Assert.Contains("out of range", exception.Message);
    }

    // ========== Integration Tests ==========

    [Fact]
    public void ResolveAndValidate_HeaderMode_ValidColumn_Succeeds()
    {
        var headerMap = new Dictionary<string, int>
        {
            { "Name", 0 },
            { "Age", 1 },
            { "Email", 2 }
        };
        var mapping = new ColumnMapping("Age", -1);

        var index = _resolver.ResolveColumnIndex(mapping, headerMap);

        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(index, 3, 3, false, 1));

        Assert.Equal(1, index);
        Assert.Null(exception);
    }

    [Fact]
    public void ResolveAndValidate_IndexMode_ValidColumn_Succeeds()
    {
        var mapping = new ColumnMapping("Column", 1);

        var index = _resolver.ResolveColumnIndex(mapping, null);

        var exception = Record.Exception(() =>
            _resolver.ValidateColumnIndex(index, 3, 3, false, 1));

        Assert.Equal(1, index);
        Assert.Null(exception);
    }

    [Fact]
    public void ResolveAndValidate_IndexMode_IndexOutOfRange_ThrowsAfterValidation()
    {
        var mapping = new ColumnMapping("Column", 5);

        var index = _resolver.ResolveColumnIndex(mapping, null);

        var exception = Assert.Throws<ColumnIndexOutOfRangeException>(() =>
            _resolver.ValidateColumnIndex(index, 3, 3, false, 1));

        Assert.Equal(5, index);
        Assert.Contains("out of range", exception.Message);
    }
}
