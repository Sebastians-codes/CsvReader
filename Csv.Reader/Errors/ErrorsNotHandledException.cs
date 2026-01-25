namespace Csv.Reader.Errors;

/// <summary>
/// Exception thrown when attempting to access records without handling errors.
/// </summary>
internal class ErrorsNotHandledException : CsvParseException
{
    public ErrorsNotHandledException()
        : base("Errors have not been handled. Please check for errors before accessing the result.")
    {
    }
}
