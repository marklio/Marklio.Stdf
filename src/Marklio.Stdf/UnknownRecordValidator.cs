using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods that flag <see cref="UnknownRecord"/> instances in the
/// record stream by injecting <see cref="ErrorRecord"/> instances before them.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class UnknownRecordValidator
{
    /// <summary>
    /// Yields an <see cref="ErrorRecord"/> before each <see cref="UnknownRecord"/>.
    /// All original records (including the unknown ones) pass through unchanged.
    /// </summary>
    /// <param name="source">The STDF record stream to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of <see cref="StdfRecord"/> that includes all original records plus an <see cref="ErrorRecord"/> before each <see cref="UnknownRecord"/>.</returns>
    /// <remarks>
    /// Unknown records are record types not recognized by the parser (vendor-specific or future types).
    /// Each <see cref="UnknownRecord"/> gets an <see cref="ErrorRecord"/> with severity
    /// <see cref="ErrorSeverity.Error"/> emitted immediately before it. The unknown record itself
    /// is still yielded so downstream processing can inspect or round-trip it.
    /// </remarks>
    public static async IAsyncEnumerable<StdfRecord> RejectUnknownRecords(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec is UnknownRecord unknown)
            {
                yield return new ErrorRecord
                {
                    Severity = ErrorSeverity.Error,
                    Code = "STDF_UNKNOWN_RECORD",
                    Message = $"Unknown record type ({unknown.RecordType}, {unknown.RecordSubType}).",
                    SourceRecord = unknown,
                };
            }

            yield return rec;
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="RejectUnknownRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    /// <inheritdoc cref="RejectUnknownRecords(IAsyncEnumerable{StdfRecord}, CancellationToken)" path="/remarks"/>
    /// <param name="source">The STDF record stream to process.</param>
    /// <returns>An enumerable of <see cref="StdfRecord"/> that includes all original records plus an <see cref="ErrorRecord"/> before each <see cref="UnknownRecord"/>.</returns>
    public static IEnumerable<StdfRecord> RejectUnknownRecords(this IEnumerable<StdfRecord> source)
    {
        foreach (var rec in source)
        {
            if (rec is UnknownRecord unknown)
            {
                yield return new ErrorRecord
                {
                    Severity = ErrorSeverity.Error,
                    Code = "STDF_UNKNOWN_RECORD",
                    Message = $"Unknown record type ({unknown.RecordType}, {unknown.RecordSubType}).",
                    SourceRecord = unknown,
                };
            }

            yield return rec;
        }
    }
}
