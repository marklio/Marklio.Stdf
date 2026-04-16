using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for generating missing SBR (Software Bin Record) summary records
/// from PRR data in an STDF stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class SoftwareBinSummaryGenerator
{
    /// <summary>
    /// Re-emits the STDF stream with missing SBR records inserted before the first WRR
    /// (or MRR, or at end). SBR records are computed from PRR software bin data.
    /// PRRs without a SoftwareBin value are skipped.
    /// </summary>
    /// <remarks>
    /// This method streams records through without buffering. It accumulates statistics from
    /// PRR records as they pass, and inserts generated summaries before the first WRR or MRR
    /// record encountered. If neither appears, summaries are appended at the end of the stream.
    /// <para>
    /// For correct deduplication, this method assumes summary records in the input appear
    /// before the first WRR/MRR, consistent with STDF V4 ordering rules.
    /// </para>
    /// </remarks>
    public static async IAsyncEnumerable<StdfRecord> GenerateSoftwareBinSummaries(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var accumulators = new Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator>();
        var existing = new HashSet<(byte Head, byte Site, ushort Bin)>();
        bool flushed = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec is Sbr sbr)
                existing.Add((sbr.HeadNumber, sbr.SiteNumber, sbr.SoftwareBin));

            if (rec is Prr prr && prr.SoftwareBin.HasValue)
                AccumulatePrr(prr, accumulators);

            if (!flushed && rec is Wrr or Mrr)
            {
                foreach (var summary in BuildSummaries(accumulators, existing, scope))
                    yield return summary;
                flushed = true;
            }

            yield return rec;
        }

        if (!flushed)
        {
            foreach (var summary in BuildSummaries(accumulators, existing, scope))
                yield return summary;
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="GenerateSoftwareBinSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// This method streams records through without buffering. It accumulates statistics from
    /// PRR records as they pass, and inserts generated summaries before the first WRR or MRR
    /// record encountered. If neither appears, summaries are appended at the end of the stream.
    /// <para>
    /// For correct deduplication, this method assumes summary records in the input appear
    /// before the first WRR/MRR, consistent with STDF V4 ordering rules.
    /// </para>
    /// </remarks>
    public static IEnumerable<StdfRecord> GenerateSoftwareBinSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        return Core(source, scope);

        static IEnumerable<StdfRecord> Core(IEnumerable<StdfRecord> source, SummaryScope scope)
        {
            var accumulators = new Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator>();
            var existing = new HashSet<(byte Head, byte Site, ushort Bin)>();
            bool flushed = false;

            foreach (var rec in source)
            {
                if (rec is Sbr sbr)
                    existing.Add((sbr.HeadNumber, sbr.SiteNumber, sbr.SoftwareBin));

                if (rec is Prr prr && prr.SoftwareBin.HasValue)
                    AccumulatePrr(prr, accumulators);

                if (!flushed && rec is Wrr or Mrr)
                {
                    foreach (var summary in BuildSummaries(accumulators, existing, scope))
                        yield return summary;
                    flushed = true;
                }

                yield return rec;
            }

            if (!flushed)
            {
                foreach (var summary in BuildSummaries(accumulators, existing, scope))
                    yield return summary;
            }
        }
    }

    private static void AccumulatePrr(Prr prr, Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator> accumulators)
    {
        var key = (prr.HeadNumber, prr.SiteNumber, prr.SoftwareBin!.Value);
        if (!accumulators.TryGetValue(key, out var acc))
            accumulators[key] = acc = new BinAccumulator();
        acc.Count++;
        var flags = (PartResultFlags)prr.PartFlag;
        bool failed = (flags & PartResultFlags.Failed) != 0;
        bool noIndication = (flags & PartResultFlags.NoPassFailIndication) != 0;
        if (failed)
            acc.AnyFailed = true;
        if (failed || noIndication)
            acc.AllPassed = false;
    }

    private static IEnumerable<Sbr> BuildSummaries(
        Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator> accumulators,
        HashSet<(byte Head, byte Site, ushort Bin)> existing,
        SummaryScope scope)
    {
        if (accumulators.Count == 0)
            yield break;

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            foreach (var (key, acc) in accumulators)
            {
                if (existing.Contains(key))
                    continue;
                yield return BuildSbr(key.Head, key.Site, key.Bin, acc);
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var headRollups = new Dictionary<(byte Head, ushort Bin), BinAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                var rollupKey = (key.Head, key.Bin);
                if (!headRollups.TryGetValue(rollupKey, out var rollup))
                    headRollups[rollupKey] = rollup = new BinAccumulator();
                rollup.Count += acc.Count;
                if (acc.AnyFailed) rollup.AnyFailed = true;
                if (!acc.AllPassed) rollup.AllPassed = false;
            }
            foreach (var (key, acc) in headRollups)
            {
                if (existing.Contains((key.Head, 255, key.Bin)))
                    continue;
                yield return BuildSbr(key.Head, 255, key.Bin, acc);
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var overallRollups = new Dictionary<ushort, BinAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                if (!overallRollups.TryGetValue(key.Bin, out var rollup))
                    overallRollups[key.Bin] = rollup = new BinAccumulator();
                rollup.Count += acc.Count;
                if (acc.AnyFailed) rollup.AnyFailed = true;
                if (!acc.AllPassed) rollup.AllPassed = false;
            }
            foreach (var (bin, acc) in overallRollups)
            {
                if (existing.Contains((255, 255, bin)))
                    continue;
                yield return BuildSbr(255, 255, bin, acc);
            }
        }
    }

    private static Sbr BuildSbr(byte head, byte site, ushort bin, BinAccumulator acc)
    {
        char passFail = acc.AnyFailed ? 'F' : acc.AllPassed ? 'P' : ' ';

        return new Sbr
        {
            HeadNumber = head,
            SiteNumber = site,
            SoftwareBin = bin,
            BinCount = acc.Count,
            BinPassFail = passFail,
            BinName = null,
        };
    }

    private sealed class BinAccumulator
    {
        public uint Count;
        public bool AnyFailed;
        public bool AllPassed = true;
    }
}
