# CsvReader

A powerful, type-safe CSV parsing library for .NET with comprehensive error handling and flexible configuration options.

## Features

- ✅ **Strongly-typed** - Deserialize CSV data directly into C# objects
- ✅ **Flexible Error Handling** - Choose between strict (fail-fast) or lenient (collect errors) modes
- ✅ **Custom Delimiters** - Support for comma, semicolon, tab, pipe, and any custom delimiter
- ✅ **Header Mapping** - Automatic column-to-property mapping via headers or indices
- ✅ **Type Conversion** - Built-in support for common types (int, string, bool, DateTime, Guid, etc.)
- ✅ **Quote Handling** - RFC 4180 compliant quoted field parsing with escaped quotes
- ✅ **Custom Boolean Values** - Configure your own truthy/falsy values
- ✅ **Comprehensive Error Details** - Line numbers, error messages, and original content preserved
- ✅ **Well Documented** - Full XML documentation for IntelliSense support

## Installation

```bash
# Clone or copy the CsvReaderCore project into your solution
# Add a project reference to CsvReaderCore.csproj
```

## Quick Start

### 1. Define Your Model

```csharp
using CsvReaderCore.Models;
```

## Quick Start

### 1. Define Your Model

```csharp
using CsvReaderCore.Models;

public class Person : IMapped
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
```

### 2. Parse CSV Data

```csharp
using CsvReaderCore;
using CsvReaderCore.Models;

var csvLines = new[]
{
    "Name,Age,Email",
    "John Doe,30,john@example.com",
    "Jane Smith,25,jane@example.com"
};

var options = new CsvParserOptions();
var reader = new CsvReader<Person>(options);
var results = reader.DeserializeLines(csvLines);
```

### 2. Parse CSV Data

```csharp
using CsvReaderCore;
using CsvReaderCore.Models;

public enum TaskStatus { Active, Inactive, Pending }

public class Task : IMapped
{
    public Guid TaskId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TaskStatus Status { get; set; }
    
    // Automatic mapping by index/order is used if GetColumnMapping is not overridden
    // or returns an empty dictionary.
}

var csv = new[]
{
    "TaskId,Name,CreatedAt,CompletedAt,Status",
    "550e8400-e29b-41d4-a716-446655440000,My Task,2024-01-15,2024-01-20,Active",
    "660e8400-e29b-41d4-a716-446655440001,Other Task,2024-02-01,,Pending"
};

var reader = new CsvReader<Task>();
var results = reader.DeserializeLines(csv);

// Enums are case-insensitive: "active", "Active", "ACTIVE" all work
```

### Logging All Errors

```csharp
var results = reader.DeserializeLines(csvLines);

foreach (var error in results.Errors)
{
    Console.WriteLine(
        $"CSV parse error at line {error.LineNumber}: {error.ErrorMessage}. Content: {error.LineContent}"
    );
}

var records = results.Records;
```

## Supported Types

The library supports automatic type conversion for:

- `string`
- `int`, `long`, `decimal`, `double`
- `bool` (with customizable truthy/falsy values)
- `char`
- `DateTime`
- `Guid`
- `Enum` (case-insensitive, supports numeric values)
- Nullable versions of all above types

## Custom Exceptions

The library uses a unified exception hierarchy for error handling:

- **`CsvParseException`** - The primary exception thrown for all CSV parsing errors.

In **Strict Mode**, `CsvParseException` is thrown immediately on the first error encountered. The exception object includes:
- `LineNumber`: The 1-based line number where the error occurred.
- `Message`: A descriptive error message.
- `InnerException`: The underlying cause (e.g., a `FormatException` or `ArgumentException`).

In **Lenient Mode**, errors are captured as `CsvParseError` objects within the result, which provide similar contextual information without stopping the parsing process.

## Best Practices

### 1. Always Check for Errors in Lenient Mode

```csharp
var results = reader.DeserializeLines(csvLines);

// ✅ Good - check for errors
if (results.HasErrors)
{
    // Handle or log results.Errors
}
var records = results.Records;

// ❌ Bad - will throw ErrorsNotHandledException (internal but caught as CsvParseException)
// if there are any errors and you haven't checked HasErrors or Errors first.
var records = results.Records;
```

### 2. Use Strict Mode for Critical Data

```csharp
// For financial data, user credentials, etc.
var options = new CsvParserOptions { StrictMode = true };
```

### 3. Use Lenient Mode for Large Imports

```csharp
// For bulk data imports where partial success is acceptable
var options = new CsvParserOptions { StrictMode = false };
```

### 4. Configure Boolean Values for Your Data

```csharp
// If your CSV uses Y/N instead of true/false
var options = new CsvParserOptions
{
    BooleanTruthyValues = new HashSet<string> { "Y", "Yes", "1" },
    BooleanFalsyValues = new HashSet<string> { "N", "No", "0" }
};
```

### 5. Return Default Implementation for Auto-Mapping

```csharp
public class SimplePerson : IMapped
{
    public string Name { get; set; }
    public int Age { get; set; }
}
```

## Error Handling Patterns

### Pattern 1: Log and Continue

```csharp
var results = reader.DeserializeLines(csvLines);

foreach (var error in results.Errors)
{
    _logger.LogWarning($"Line {error.LineNumber}: {error.ErrorMessage}");
}

ProcessRecords(results.Records);
```

### Pattern 2: Fail if Too Many Errors

```csharp
var results = reader.DeserializeLines(csvLines);

if (results.Errors.Count > MAX_ALLOWED_ERRORS)
{
    throw new Exception($"Too many errors: {results.Errors.Count}");
}

ProcessRecords(results.Records);
```

### Pattern 3: Collect Error Report

```csharp
var results = reader.DeserializeLines(csvLines);

var errorReport = results.Errors.Select(e => new
{
    e.LineNumber,
    e.ErrorMessage,
    Preview = e.LineContent.Substring(0, Math.Min(50, e.LineContent.Length))
});

SaveErrorReport(errorReport);
ProcessRecords(results.Records);
```

## License

[Add your license information here]

## Contributing

[Add contribution guidelines here]

## Support

[Add support contact/links here]
