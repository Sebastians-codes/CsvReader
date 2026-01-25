namespace CsvReaderCore.Models;

/// <summary>
/// Represents the mapping between a CSV column and a property.
/// </summary>
/// <remarks>
/// A column can be identified either by name (when HasHeaderRow is true) or by index (when HasHeaderRow is false).
/// The ColumnIndex provides a fallback when header mapping is not used.
/// </remarks>
/// <example>
/// <code>
/// // Map by column name (used when HasHeaderRow = true)
/// new ColumnMapping("Email", 2)
/// 
/// // Map by index only (used when HasHeaderRow = false)
/// new ColumnMapping("2", 2)
/// </code>
/// </example>
/// <param name="ColumnIdentifier">The column identifier (name when using headers, or numeric string when using indices).</param>
/// <param name="ColumnIndex">The zero-based column index. Default is -1 (will be resolved from header).</param>
public record ColumnMapping(string ColumnIdentifier, int ColumnIndex = -1);
