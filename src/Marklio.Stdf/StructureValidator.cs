using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods that validate structural invariants of an STDF record stream.
/// Lighter than <see cref="OrderingValidator"/>: checks PIR/PRR pairing and
/// required record presence without enforcing full ordering rules.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class StructureValidator
{
    /// <summary>
    /// Validates structural invariants of the STDF record stream. Yields
    /// <see cref="ErrorRecord"/> instances inline (before offending records or at
    /// end of stream) when violations are detected. All original records pass through.
    /// </summary>
    public static async IAsyncEnumerable<StdfRecord> ValidateStructure(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var openPirs = new HashSet<(byte head, byte site)>();
        bool sawFar = false, sawMir = false, sawMrr = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var error = Validate(rec, openPirs, ref sawFar, ref sawMir, ref sawMrr);
            if (error != null)
                yield return error;

            yield return rec;
        }

        foreach (var error in EndOfStream(openPirs, sawFar, sawMir, sawMrr))
            yield return error;
    }

    /// <summary>
    /// Synchronous version of <see cref="ValidateStructure(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> ValidateStructure(this IEnumerable<StdfRecord> source)
    {
        var openPirs = new HashSet<(byte head, byte site)>();
        bool sawFar = false, sawMir = false, sawMrr = false;

        foreach (var rec in source)
        {
            var error = Validate(rec, openPirs, ref sawFar, ref sawMir, ref sawMrr);
            if (error != null)
                yield return error;

            yield return rec;
        }

        foreach (var error in EndOfStream(openPirs, sawFar, sawMir, sawMrr))
            yield return error;
    }

    private static ErrorRecord? Validate(
        StdfRecord rec,
        HashSet<(byte head, byte site)> openPirs,
        ref bool sawFar, ref bool sawMir, ref bool sawMrr)
    {
        if (rec is ErrorRecord)
            return null;

        switch (rec)
        {
            case Far:
                sawFar = true;
                break;

            case Mir:
                sawMir = true;
                break;

            case Mrr:
                sawMrr = true;
                break;

            case Pir pir:
                var pirKey = (pir.HeadNumber, pir.SiteNumber);
                if (!openPirs.Add(pirKey))
                {
                    return new ErrorRecord
                    {
                        Severity = ErrorSeverity.Error,
                        Code = "STDF_STRUCT_DUPLICATE_PIR",
                        Message = $"Duplicate PIR for head {pir.HeadNumber}, site {pir.SiteNumber}.",
                        SourceRecord = rec,
                    };
                }
                break;

            case Prr prr:
                var prrKey = (prr.HeadNumber, prr.SiteNumber);
                if (!openPirs.Remove(prrKey))
                {
                    return new ErrorRecord
                    {
                        Severity = ErrorSeverity.Error,
                        Code = "STDF_STRUCT_NO_MATCHING_PIR",
                        Message = $"PRR without matching PIR for head {prr.HeadNumber}, site {prr.SiteNumber}.",
                        SourceRecord = rec,
                    };
                }
                break;
        }

        return null;
    }

    private static IEnumerable<ErrorRecord> EndOfStream(
        HashSet<(byte head, byte site)> openPirs,
        bool sawFar, bool sawMir, bool sawMrr)
    {
        foreach (var (head, site) in openPirs)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_STRUCT_UNCLOSED_PIR",
                Message = $"Unclosed PIR for head {head}, site {site}.",
            };
        }

        if (!sawFar)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_STRUCT_NO_FAR",
                Message = "No FAR record found in stream.",
            };
        }

        if (!sawMir)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_STRUCT_NO_MIR",
                Message = "No MIR record found in stream.",
            };
        }

        if (!sawMrr)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_STRUCT_NO_MRR",
                Message = "No MRR record found in stream.",
            };
        }
    }
}
