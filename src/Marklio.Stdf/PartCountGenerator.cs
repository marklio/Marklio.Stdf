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
    public static async IAsyncEnumerable<StdfRecord> GeneratePartCounts(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var records = new List<StdfRecord>();
        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            records.Add(rec);

        foreach (var rec in EmitWithSummaries(records, scope))
            yield return rec;
    }

    /// <summary>
    /// Synchronous version of <see cref="GeneratePartCounts(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> GeneratePartCounts(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        return Core(source, scope);

        static IEnumerable<StdfRecord> Core(IEnumerable<StdfRecord> source, SummaryScope scope)
        {
            var records = source.ToList();
            foreach (var rec in EmitWithSummaries(records, scope))
                yield return rec;
        }
    }

    private static IEnumerable<StdfRecord> EmitWithSummaries(List<StdfRecord> records, SummaryScope scope)
    {
        var existingPcrs = new HashSet<(byte Head, byte Site)>();
        foreach (var rec in records)
        {
            if (rec is Pcr pcr)
                existingPcrs.Add((pcr.HeadNumber, pcr.SiteNumber));
        }

        var generated = GeneratePcrs(records, scope, existingPcrs);
        if (generated.Count == 0)
        {
            foreach (var rec in records)
                yield return rec;
            yield break;
        }

        int insertionIndex = SummaryInsertionPoint.Find(records);
        for (int i = 0; i < insertionIndex; i++)
            yield return records[i];
        foreach (var pcr in generated)
            yield return pcr;
        for (int i = insertionIndex; i < records.Count; i++)
            yield return records[i];
    }

    internal static List<Pcr> GeneratePcrs(
        List<StdfRecord> records, SummaryScope scope, HashSet<(byte Head, byte Site)> existing)
    {
        var prrs = new List<Prr>();
        foreach (var rec in records)
        {
            if (rec is Prr prr)
                prrs.Add(prr);
        }

        if (prrs.Count == 0)
            return [];

        var result = new List<Pcr>();

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            var groups = prrs.GroupBy(p => (p.HeadNumber, p.SiteNumber));
            foreach (var g in groups)
            {
                if (existing.Contains(g.Key))
                    continue;
                result.Add(BuildPcr(g.Key.HeadNumber, g.Key.SiteNumber, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var groups = prrs.GroupBy(p => p.HeadNumber);
            foreach (var g in groups)
            {
                if (existing.Contains((g.Key, 255)))
                    continue;
                result.Add(BuildPcr(g.Key, 255, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            if (!existing.Contains((255, 255)))
                result.Add(BuildPcr(255, 255, prrs));
        }

        return result;
    }

    private static Pcr BuildPcr(byte head, byte site, List<Prr> prrs)
    {
        uint abortCount = 0;
        uint goodCount = 0;
        foreach (var prr in prrs)
        {
            var flags = (PartResultFlags)prr.PartFlag;
            if ((flags & PartResultFlags.AbnormalEnd) != 0)
                abortCount++;
            if ((flags & PartResultFlags.Failed) == 0 &&
                (flags & PartResultFlags.NoPassFailIndication) == 0 &&
                (flags & PartResultFlags.AbnormalEnd) == 0)
                goodCount++;
        }

        return new Pcr
        {
            HeadNumber = head,
            SiteNumber = site,
            PartCount = (uint)prrs.Count,
            RetestCount = null,
            AbortCount = abortCount,
            GoodCount = goodCount,
            FunctionalCount = null,
        };
    }
}
