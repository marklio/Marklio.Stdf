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
    private sealed class OrderingState
    {
        public FileState State = FileState.ExpectFar;
        public int BpsDepth;
    }

    public static async IAsyncEnumerable<StdfRecord> ValidateOrdering(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var st = new OrderingState();
        var openPirs = new HashSet<(byte head, byte site)>();

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            var errors = Validate(rec, st, openPirs);
            if (errors != null)
                foreach (var error in errors)
                    yield return error;

            yield return rec;
        }

        foreach (var error in ValidateEndOfStream(st, openPirs))
            yield return error;
    }

    /// <summary>
    /// Synchronous version of <see cref="ValidateOrdering(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> ValidateOrdering(this IEnumerable<StdfRecord> source)
    {
        var st = new OrderingState();
        var openPirs = new HashSet<(byte head, byte site)>();

        foreach (var rec in source)
        {
            var errors = Validate(rec, st, openPirs);
            if (errors != null)
                foreach (var error in errors)
                    yield return error;

            yield return rec;
        }

        foreach (var error in ValidateEndOfStream(st, openPirs))
            yield return error;
    }

    private static List<ErrorRecord>? Validate(
        StdfRecord rec, OrderingState st,
        HashSet<(byte head, byte site)> openPirs)
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

        if (st.State == FileState.Done)
        {
            AddError("STDF_ORDER_RECORD_AFTER_MRR", $"Record {rec.GetType().Name} appeared after MRR.");
            return errors;
        }

        switch (st.State)
        {
            case FileState.ExpectFar:
                if (rec is not Far)
                {
                    AddError("STDF_ORDER_NO_FAR", $"Expected FAR as first record, got {rec.GetType().Name}.");
                    st.State = FileState.ExpectMir;
                }
                else
                {
                    st.State = FileState.ExpectMir;
                    return null;
                }
                break;

            case FileState.ExpectMir:
                if (rec is Mir)
                {
                    st.State = FileState.TestData;
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
                if (st.State == FileState.Summaries)
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
                if (st.State == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "PTR appeared after summary records.");
                if (!openPirs.Contains((ptr.HeadNumber, ptr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"PTR without open PIR for head {ptr.HeadNumber}, site {ptr.SiteNumber}.");
                break;

            case Ftr ftr:
                if (st.State == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "FTR appeared after summary records.");
                if (!openPirs.Contains((ftr.HeadNumber, ftr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"FTR without open PIR for head {ftr.HeadNumber}, site {ftr.SiteNumber}.");
                break;

            case Mpr mpr:
                if (st.State == FileState.Summaries)
                    AddError("STDF_ORDER_TEST_AFTER_SUMMARY", "MPR appeared after summary records.");
                if (!openPirs.Contains((mpr.HeadNumber, mpr.SiteNumber)))
                    AddError("STDF_ORDER_NO_PIR", $"MPR without open PIR for head {mpr.HeadNumber}, site {mpr.SiteNumber}.");
                break;

            case Bps:
                st.BpsDepth++;
                break;

            case Eps:
                if (st.BpsDepth <= 0)
                    AddError("STDF_ORDER_UNPAIRED_BPS", "EPS without matching BPS.");
                else
                    st.BpsDepth--;
                break;

            case Pcr or Hbr or Sbr or Tsr or Wrr:
                if (st.State == FileState.TestData)
                    st.State = FileState.Summaries;
                break;

            case Mrr:
                st.State = FileState.Done;
                break;
        }

        return errors;
    }

    private static IEnumerable<StdfRecord> ValidateEndOfStream(
        OrderingState st, HashSet<(byte head, byte site)> openPirs)
    {
        if (st.State == FileState.ExpectFar)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_ORDER_NO_FAR",
                Message = "Stream ended without a FAR record.",
            };
        }
        else if (st.State == FileState.ExpectMir)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_ORDER_MIR_EXPECTED",
                Message = "Stream ended without a MIR record.",
            };
        }

        foreach (var (head, site) in openPirs)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_ORDER_UNCLOSED_PIR",
                Message = $"PIR for head {head}, site {site} was never closed by a matching PRR.",
            };
        }

        if (st.BpsDepth > 0)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Error,
                Code = "STDF_ORDER_UNPAIRED_BPS",
                Message = $"BPS was opened but never closed by a matching EPS ({st.BpsDepth} level(s) deep).",
            };
        }

        if (st.State is not FileState.Done and not FileState.ExpectFar and not FileState.ExpectMir)
        {
            yield return new ErrorRecord
            {
                Severity = ErrorSeverity.Warning,
                Code = "STDF_ORDER_NO_MRR",
                Message = "Stream ended without an MRR record.",
            };
        }
    }
}
