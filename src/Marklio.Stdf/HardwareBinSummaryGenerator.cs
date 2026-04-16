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
    public static async IAsyncEnumerable<StdfRecord> GenerateHardwareBinSummaries(
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
    /// Synchronous version of <see cref="GenerateHardwareBinSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> GenerateHardwareBinSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        var records = source.ToList();
        return EmitWithSummaries(records, scope);
    }

    private static IEnumerable<StdfRecord> EmitWithSummaries(List<StdfRecord> records, SummaryScope scope)
    {
        var existingHbrs = new HashSet<(byte Head, byte Site, ushort Bin)>();
        foreach (var rec in records)
        {
            if (rec is Hbr hbr)
                existingHbrs.Add((hbr.HeadNumber, hbr.SiteNumber, hbr.HardwareBin));
        }

        var generated = GenerateHbrs(records, scope, existingHbrs);
        if (generated.Count == 0)
        {
            foreach (var rec in records)
                yield return rec;
            yield break;
        }

        int insertionIndex = SummaryInsertionPoint.Find(records);
        for (int i = 0; i < insertionIndex; i++)
            yield return records[i];
        foreach (var hbr in generated)
            yield return hbr;
        for (int i = insertionIndex; i < records.Count; i++)
            yield return records[i];
    }

    internal static List<Hbr> GenerateHbrs(
        List<StdfRecord> records, SummaryScope scope, HashSet<(byte Head, byte Site, ushort Bin)> existing)
    {
        var prrs = new List<Prr>();
        foreach (var rec in records)
        {
            if (rec is Prr prr)
                prrs.Add(prr);
        }

        if (prrs.Count == 0)
            return [];

        var result = new List<Hbr>();

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            var groups = prrs.GroupBy(p => (p.HeadNumber, p.SiteNumber, p.HardwareBin));
            foreach (var g in groups)
            {
                if (existing.Contains(g.Key))
                    continue;
                result.Add(BuildHbr(g.Key.HeadNumber, g.Key.SiteNumber, g.Key.HardwareBin, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var groups = prrs.GroupBy(p => (p.HeadNumber, p.HardwareBin));
            foreach (var g in groups)
            {
                if (existing.Contains((g.Key.HeadNumber, 255, g.Key.HardwareBin)))
                    continue;
                result.Add(BuildHbr(g.Key.HeadNumber, 255, g.Key.HardwareBin, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var groups = prrs.GroupBy(p => p.HardwareBin);
            foreach (var g in groups)
            {
                if (existing.Contains((255, 255, g.Key)))
                    continue;
                result.Add(BuildHbr(255, 255, g.Key, g.ToList()));
            }
        }

        return result;
    }

    private static Hbr BuildHbr(byte head, byte site, ushort bin, List<Prr> prrs)
    {
        bool anyFailed = false;
        bool allPassed = true;
        foreach (var prr in prrs)
        {
            var flags = (PartResultFlags)prr.PartFlag;
            bool failed = (flags & PartResultFlags.Failed) != 0;
            bool noIndication = (flags & PartResultFlags.NoPassFailIndication) != 0;
            if (failed)
                anyFailed = true;
            if (failed || noIndication)
                allPassed = false;
        }

        char passFail = anyFailed ? 'F' : allPassed ? 'P' : ' ';

        return new Hbr
        {
            HeadNumber = head,
            SiteNumber = site,
            HardwareBin = bin,
            BinCount = (uint)prrs.Count,
            BinPassFail = passFail,
            BinName = null,
        };
    }
}
