namespace Marklio.Stdf;

/// <summary>
/// Severity level for <see cref="ErrorRecord"/> instances.
/// </summary>
public enum ErrorSeverity
{
    /// <summary>A non-fatal issue that may indicate data quality problems.</summary>
    Warning,

    /// <summary>A spec violation or structural error.</summary>
    Error,
}
