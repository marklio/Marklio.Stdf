using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for generating missing PCR (Part Count Record) summary records
/// from PRR (Part Results Record) data in an STDF stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class PartCountGenerator
{
    /// <summary>
    /// Re-emits the STDF stream with missing PCR records inserted before the first WRR
    /// (or MRR, or at end). PCR records are computed from PRR data at the requested scope levels.
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
    public static async IAsyncEnumerable<StdfRecord> GeneratePartCounts(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var accumulators = new Dictionary<(byte Head, byte Site), PartCountAccumulator>();
        var existing = new HashSet<(byte Head, byte Site)>();
        bool flushed = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec is Pcr pcr)
                existing.Add((pcr.HeadNumber, pcr.SiteNumber));

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
    /// Synchronous version of <see cref="GeneratePartCounts(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
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
    public static IEnumerable<StdfRecord> GeneratePartCounts(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        return Core(source, scope);

        static IEnumerable<StdfRecord> Core(IEnumerable<StdfRecord> source, SummaryScope scope)
        {
            var accumulators = new Dictionary<(byte Head, byte Site), PartCountAccumulator>();
            var existing = new HashSet<(byte Head, byte Site)>();
            bool flushed = false;

            foreach (var rec in source)
            {
                if (rec is Pcr pcr)
                    existing.Add((pcr.HeadNumber, pcr.SiteNumber));

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

    private static void AccumulatePrr(Prr prr, Dictionary<(byte Head, byte Site), PartCountAccumulator> accumulators)
    {
        var key = (prr.HeadNumber, prr.SiteNumber);
        if (!accumulators.TryGetValue(key, out var acc))
            accumulators[key] = acc = new PartCountAccumulator();
        acc.PartCount++;
        var flags = (PartResultFlags)prr.PartFlag;
        if ((flags & PartResultFlags.AbnormalEnd) != 0)
            acc.AbortCount++;
        if ((flags & PartResultFlags.Failed) == 0 &&
            (flags & PartResultFlags.NoPassFailIndication) == 0 &&
            (flags & PartResultFlags.AbnormalEnd) == 0)
            acc.GoodCount++;
    }

    private static IEnumerable<Pcr> BuildSummaries(
        Dictionary<(byte Head, byte Site), PartCountAccumulator> accumulators,
        HashSet<(byte Head, byte Site)> existing,
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
                yield return BuildPcr(key.Head, key.Site, acc);
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var headRollups = new Dictionary<byte, PartCountAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                if (!headRollups.TryGetValue(key.Head, out var rollup))
                    headRollups[key.Head] = rollup = new PartCountAccumulator();
                rollup.PartCount += acc.PartCount;
                rollup.AbortCount += acc.AbortCount;
                rollup.GoodCount += acc.GoodCount;
            }
            foreach (var (head, acc) in headRollups)
            {
                if (existing.Contains((head, 255)))
                    continue;
                yield return BuildPcr(head, 255, acc);
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            if (!existing.Contains((255, 255)))
            {
                var overall = new PartCountAccumulator();
                foreach (var acc in accumulators.Values)
                {
                    overall.PartCount += acc.PartCount;
                    overall.AbortCount += acc.AbortCount;
                    overall.GoodCount += acc.GoodCount;
                }
                yield return BuildPcr(255, 255, overall);
            }
        }
    }

    private static Pcr BuildPcr(byte head, byte site, PartCountAccumulator acc)
    {
        return new Pcr
        {
            HeadNumber = head,
            SiteNumber = site,
            PartCount = acc.PartCount,
            RetestCount = null,
            AbortCount = acc.AbortCount,
            GoodCount = acc.GoodCount,
            FunctionalCount = null,
        };
    }

    private sealed class PartCountAccumulator
    {
        public uint PartCount;
        public uint AbortCount;
        public uint GoodCount;
    }
}
