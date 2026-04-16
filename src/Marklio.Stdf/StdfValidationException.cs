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
    public StdfValidationException(ErrorRecord errorRecord)
        : base(errorRecord.Message)
    {
        ErrorRecord = errorRecord;
    }

    /// <summary>Creates a new <see cref="StdfValidationException"/> with a custom message.</summary>
    public StdfValidationException(string message, ErrorRecord errorRecord)
        : base(message)
    {
        ErrorRecord = errorRecord;
    }
}
