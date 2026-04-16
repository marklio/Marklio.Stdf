using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods that validate STDF V4 record ordering rules.
/// Emits <see cref="ErrorRecord"/> instances inline for ordering violations
/// while passing all original records through unchanged.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class OrderingValidator
{
    internal enum FileState { ExpectFar, ExpectMir, TestData, Summaries, Done }

    /// <summary>
    /// Validates STDF V4 record ordering rules. Yields <see cref="ErrorRecord"/>
    /// instances before offending records when violations are detected.
    /// All original records are always yielded.
    /// </summary>
    public static async IAsyncEnumerable<StdfRecord> ValidateOrdering(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = FileState.ExpectFar;
        var openPirs = new HashSet<(byte head, byte site)>();
        int bpsDepth = 0;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var errors = Validate(rec, ref state, openPirs, ref bpsDepth);
            if (errors != null)
                foreach (var error in errors)
                    yield return error;

            yield return rec;
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="ValidateOrdering(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> ValidateOrdering(this IEnumerable<StdfRecord> source)
    {
        var state = FileState.ExpectFar;
        var openPirs = new HashSet<(byte head, byte site)>();
        int bpsDepth = 0;

        foreach (var rec in source)
        {
            var errors = Validate(rec, ref state, openPirs, ref bpsDepth);
            if (errors != null)
                foreach (var error in errors)
                    yield return error;

            yield return rec;
        }
    }

    private static List<ErrorRecord>? Validate(
        StdfRecord rec, ref FileState state,
        HashSet<(byte head, byte site)> openPirs, ref int bpsDepth)
    {
        if (rec is ErrorRecord)
            return null;

        List<ErrorRecord>? errors = null;

        void AddError(string code, string message)
        {
            errors ??= new List<ErrorRecord>();
            errors.Add(new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = code,
                Message = message,
                SourceRecord = rec,
            });
        }

        if (state == FileState.Done)
        {
            AddError("STDF_ORDER_RECORD_AFTER_MRR", $"Record {rec.GetType().Name} appeared after MRR.");
            return errors;
        }

        switch (state)
        {
            case FileState.ExpectFar:
                if (rec is not Far)
                {
                    AddError("STDF_ORDER_NO_FAR", $"Expected FAR as first record, got {rec.GetType().Name}.");
                    state = FileState.ExpectMir;
                }
                else
                {
                    state = FileState.ExpectMir;
                    return null;
                }
                break;

            case FileState.ExpectMir:
                if (rec is Mir)
                {
                    state = FileState.TestData;
                    return null;
                }
                if (rec is Atr)
                    return null;
                AddError("STDF_ORDER_MIR_EXPECTED", $"Expected MIR (or ATR) after FAR, got {rec.GetType().Name}.");
                return errors;
        }

        switch (rec)
        {
            case Pir pir:
                if (state == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "PIR appeared after summary records.");
                var pirKey = (pir.HeadNumber, pir.SiteNumber);
                if (!openPirs.Add(pirKey))
                    AddError("STDF_ORDER_DUPLICATE_PIR", $"Duplicate PIR for head {pir.HeadNumber}, site {pir.SiteNumber}.");
                break;

            case Prr prr:
                var prrKey = (prr.HeadNumber, prr.SiteNumber);
                if (!openPirs.Remove(prrKey))
                    AddError("STDF_ORDER_NO_MATCHING_PIR", $"PRR without matching PIR for head {prr.HeadNumber}, site {prr.SiteNumber}.");
                break;

            case Ptr ptr:
                if (state == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "PTR appeared after summary records.");
                if (!openPirs.Contains((ptr.HeadNumber, ptr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"PTR without open PIR for head {ptr.HeadNumber}, site {ptr.SiteNumber}.");
                break;

            case Ftr ftr:
                if (state == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "FTR appeared after summary records.");
                if (!openPirs.Contains((ftr.HeadNumber, ftr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"FTR without open PIR for head {ftr.HeadNumber}, site {ftr.SiteNumber}.");
                break;

            case Mpr mpr:
                if (state == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "MPR appeared after summary records.");
                if (!openPirs.Contains((mpr.HeadNumber, mpr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"MPR without open PIR for head {mpr.HeadNumber}, site {mpr.SiteNumber}.");
                break;

            case Bps:
                bpsDepth++;
                break;

            case Eps:
                if (bpsDepth <= 0)
                    AddError("STDF_ORDER_UNPAIRED_BPS", "EPS without matching BPS.");
                else
                    bpsDepth--;
                break;

            case Pcr or Hbr or Sbr or Tsr or Wrr:
                if (state == FileState.TestData)
                    state = FileState.Summaries;
                break;

            case Mrr:
                state = FileState.Done;
                break;
        }

        return errors;
    }
}
