namespace Marklio.Stdf;

/// <summary>
/// Exception thrown by <see cref="ErrorRecordExtensions.ThrowOnError"/> when
/// an <see cref="ErrorRecord"/> is encountered in the record stream.
/// </summary>
public class StdfValidationException : Exception
{
    /// <summary>The error record that triggered the exception.</summary>
    public ErrorRecord ErrorRecord { get; }

    /// <summary>Creates a new <see cref="StdfValidationException"/>.</summary>
    /// <param name="errorRecord">The error record that caused the exception.</param>
    /// <remarks>
    /// This exception is thrown by <see cref="ErrorRecordExtensions"/> <c>ThrowOnError</c> when an
    /// <see cref="ErrorRecord"/> at or above the minimum severity is encountered in the stream.
    /// </remarks>
    public StdfValidationException(ErrorRecord errorRecord)
        : base(errorRecord.Message)
    {
        ErrorRecord = errorRecord;
    }

    /// <summary>Creates a new <see cref="StdfValidationException"/> with a custom message.</summary>
    /// <param name="message">The exception message.</param>
    /// <param name="errorRecord">The error record that caused the exception.</param>
    /// <remarks>
    /// This exception is thrown by <see cref="ErrorRecordExtensions"/> <c>ThrowOnError</c> when an
    /// <see cref="ErrorRecord"/> at or above the minimum severity is encountered in the stream.
    /// </remarks>
    public StdfValidationException(string message, ErrorRecord errorRecord)
        : base(message)
    {
        ErrorRecord = errorRecord;
    }
}
