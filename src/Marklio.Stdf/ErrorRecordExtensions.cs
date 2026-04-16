using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for handling <see cref="ErrorRecord"/> instances in the record stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class ErrorRecordExtensions
{
    /// <summary>
    /// Passes through all records, throwing <see cref="StdfValidationException"/>
    /// when an <see cref="ErrorRecord"/> is encountered.
    /// </summary>
    /// <param name="source">The record stream to monitor.</param>
    /// <param name="minimumSeverity">
    /// The minimum severity that triggers an exception. Defaults to <see cref="ErrorSeverity.Warning"/>
    /// (throws on any error record). Use <see cref="ErrorSeverity.Error"/> to ignore warnings.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async IAsyncEnumerable<StdfRecord> ThrowOnError(
        this IAsyncEnumerable<StdfRecord> source,
        ErrorSeverity minimumSeverity = ErrorSeverity.Warning,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var record in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (record is ErrorRecord error && error.Severity >= minimumSeverity)
                throw new StdfValidationException(error);

            yield return record;
        }
    }

    /// <summary>
    /// Passes through all records, throwing <see cref="StdfValidationException"/>
    /// when an <see cref="ErrorRecord"/> is encountered.
    /// </summary>
    /// <param name="source">The record stream to monitor.</param>
    /// <param name="minimumSeverity">
    /// The minimum severity that triggers an exception. Defaults to <see cref="ErrorSeverity.Warning"/>
    /// (throws on any error record). Use <see cref="ErrorSeverity.Error"/> to ignore warnings.
    /// </param>
    public static IEnumerable<StdfRecord> ThrowOnError(
        this IEnumerable<StdfRecord> source,
        ErrorSeverity minimumSeverity = ErrorSeverity.Warning)
    {
        foreach (var record in source)
        {
            if (record is ErrorRecord error && error.Severity >= minimumSeverity)
                throw new StdfValidationException(error);

            yield return record;
        }
    }
}
