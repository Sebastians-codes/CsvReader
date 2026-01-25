namespace CsvReaderCore.Models;

/// <summary>
/// Interface for types that can be deserialized from CSV data.
/// </summary>
/// <remarks>
/// Implement this interface on your classes to define how CSV columns map to properties.
/// If GetColumnMapping returns an empty dictionary, the library will automatically create
/// mappings based on property order (index-based mapping).
/// </remarks>
/// <example>
/// <code>
/// public class Person : IMapped
/// {
///     public string Name { get; set; }
///     public int Age { get; set; }
///     public string Email { get; set; }
///     
///     public Dictionary&lt;string, ColumnMapping&gt; GetColumnMapping()
///     {
///         return new Dictionary&lt;string, ColumnMapping&gt;
///         {
///             { nameof(Name), new ColumnMapping("Name", 0) },
///             { nameof(Age), new ColumnMapping("Age", 1) },
///             { nameof(Email), new ColumnMapping("Email", 2) }
///         };
///     }
/// }
/// </code>
/// </example>
public interface IMapped
{
    /// <summary>
    /// Gets the mapping between property names and CSV columns.
    /// </summary>
    /// <returns>
    /// A dictionary where keys are property names and values are column mappings.
    /// Return an empty dictionary to use automatic index-based mapping.
    /// </returns>
    Dictionary<string, ColumnMapping> GetColumnMapping() => [];
}
