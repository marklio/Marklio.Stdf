namespace Marklio.Stdf;

/// <summary>
/// Exception thrown when an STDF file cannot be parsed.
/// </summary>
public class StdfParseException : Exception
{
    /// <summary>Gets the approximate byte offset in the stream where the error occurred.</summary>
    public long Offset { get; }

    /// <inheritdoc/>
    public StdfParseException(string message, long offset) : base(message)
    {
        Offset = offset;
    }

    /// <inheritdoc/>
    public StdfParseException(string message, long offset, Exception innerException) : base(message, innerException)
    {
        Offset = offset;
    }
}
