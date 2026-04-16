using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for generating missing HBR (Hardware Bin Record) summary records
/// from PRR data in an STDF stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class HardwareBinSummaryGenerator
{
    /// <summary>
    /// Re-emits the STDF stream with missing HBR records inserted before the first WRR
    /// (or MRR, or at end). HBR records are computed from PRR hardware bin data.
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
    public static async IAsyncEnumerable<StdfRecord> GenerateHardwareBinSummaries(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var accumulators = new Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator>();
        var existing = new HashSet<(byte Head, byte Site, ushort Bin)>();
        bool flushed = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec is Hbr hbr)
                existing.Add((hbr.HeadNumber, hbr.SiteNumber, hbr.HardwareBin));

            if (rec is Prr prr)
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
    /// Synchronous version of <see cref="GenerateHardwareBinSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
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
    public static IEnumerable<StdfRecord> GenerateHardwareBinSummaries(
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
                if (rec is Hbr hbr)
                    existing.Add((hbr.HeadNumber, hbr.SiteNumber, hbr.HardwareBin));

                if (rec is Prr prr)
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
        var key = (prr.HeadNumber, prr.SiteNumber, prr.HardwareBin);
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

    private static IEnumerable<Hbr> BuildSummaries(
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
                yield return BuildHbr(key.Head, key.Site, key.Bin, acc);
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
                yield return BuildHbr(key.Head, 255, key.Bin, acc);
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
                yield return BuildHbr(255, 255, bin, acc);
            }
        }
    }

    private static Hbr BuildHbr(byte head, byte site, ushort bin, BinAccumulator acc)
    {
        char passFail = acc.AnyFailed ? 'F' : acc.AllPassed ? 'P' : ' ';

        return new Hbr
        {
            HeadNumber = head,
            SiteNumber = site,
            HardwareBin = bin,
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
