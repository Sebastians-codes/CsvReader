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

Install via NuGet:

```bash
dotnet add package CsvReaderCore
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
var reader = new CsvReader(options);
var results = reader.DeserializeLines<Person>(csvLines);

// Check for errors before accessing records
if (results.HasErrors)
{
    foreach (var error in results.Errors)
    {
        Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
    }
}

// Access the parsed records
foreach (var person in results.Records)
{
    Console.WriteLine($"{person.Name} is {person.Age} years old");
}
```

## Configuration Options

### CsvParserOptions

```csharp
var options = new CsvParserOptions
{
    // Delimiter character (default: ',')
    Delimiter = ',',
    
    // First line contains headers (default: true)
    HasHeaderRow = true,
    
    // Skip empty/whitespace lines (default: true)
    SkipEmptyLines = true,
    
    // Trim whitespace from fields (default: true)
    TrimFields = true,
    
    // Case-insensitive header matching (default: true)
    CaseInsensitiveHeaders = true,
    
    // Error handling mode (default: false)
    // false = lenient mode (collect errors)
    // true = strict mode (throw on first error)
    StrictMode = false,
    
    // Custom boolean true values (default: "true", "1", "yes")
    BooleanTruthyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "Y", "Yes", "T", "True", "On", "1"
    },
    
    // Custom boolean false values (default: "false", "0", "no")
    BooleanFalsyValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "N", "No", "F", "False", "Off", "0"
    }
};
```

## Error Handling Modes

### Lenient Mode (Default)

Collects all errors and continues parsing valid lines. Perfect for processing large files where you want to get as much data as possible.

```csharp
var options = new CsvParserOptions { StrictMode = false };
var reader = new CsvReader(options);
var results = reader.DeserializeLines<Person>(csvLines);

// Must check for errors before accessing records
if (results.HasErrors)
{
    Console.WriteLine($"Found {results.Errors.Count} errors:");
    foreach (var error in results.Errors)
    {
        Console.WriteLine($"  Line {error.LineNumber}: {error.ErrorMessage}");
        Console.WriteLine($"  Content: {error.LineContent}");
    }
}

// Get successfully parsed records (may be partial if errors occurred)
var records = results.Records;
```

### Strict Mode

Throws an exception immediately on the first error. Use when data integrity is critical.

```csharp
var options = new CsvParserOptions { StrictMode = true };
var reader = new CsvReader(options);

try
{
    var results = reader.DeserializeLines<Person>(csvLines);
    var records = results.Records; // Can access directly in strict mode
}
catch (CsvParseException ex)
{
    Console.WriteLine($"Error at line {ex.LineNumber}: {ex.Message}");
}
```

## Advanced Examples

### Custom Delimiter (Semicolon)

```csharp
var csv = new[]
{
    "Name;Age;Active",
    "John;30;true",
    "Jane;25;false"
};

var options = new CsvParserOptions { Delimiter = ';' };
var reader = new CsvReader(options);
var results = reader.DeserializeLines<Person>(csv);
```

### No Header Row (Index-Based Mapping)

```csharp
var csv = new[]
{
    "John,30,john@example.com",
    "Jane,25,jane@example.com"
};

var options = new CsvParserOptions { HasHeaderRow = false };
var reader = new CsvReader(options);
var results = reader.DeserializeLines<Person>(csv);
```

### Reading from File

```csharp
var csvLines = File.ReadLines("data.csv");
var reader = new CsvReader();
var results = reader.DeserializeLines<Person>(csvLines);
```

### Using Enums, Guids, and DateTime

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

var reader = new CsvReader();
var results = reader.DeserializeLines<Task>(csv);

// Enums are case-insensitive: "active", "Active", "ACTIVE" all work
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

## Error Handling Patterns

### Pattern 1: Log and Continue

```csharp
var results = reader.DeserializeLines(csvLines);

foreach (var error in results.Errors)
{
    Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
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

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Support

If you find any bugs or have feature requests, please create an issue on [GitHub](https://github.com/sebastians-codes/csvreader/issues).
