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
