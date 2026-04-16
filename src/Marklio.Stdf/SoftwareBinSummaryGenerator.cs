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
    public static async IAsyncEnumerable<StdfRecord> GenerateSoftwareBinSummaries(
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
    /// Synchronous version of <see cref="GenerateSoftwareBinSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> GenerateSoftwareBinSummaries(
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
        var existingSbrs = new HashSet<(byte Head, byte Site, ushort Bin)>();
        foreach (var rec in records)
        {
            if (rec is Sbr sbr)
                existingSbrs.Add((sbr.HeadNumber, sbr.SiteNumber, sbr.SoftwareBin));
        }

        var generated = GenerateSbrs(records, scope, existingSbrs);
        if (generated.Count == 0)
        {
            foreach (var rec in records)
                yield return rec;
            yield break;
        }

        int insertionIndex = SummaryInsertionPoint.Find(records);
        for (int i = 0; i < insertionIndex; i++)
            yield return records[i];
        foreach (var sbr in generated)
            yield return sbr;
        for (int i = insertionIndex; i < records.Count; i++)
            yield return records[i];
    }

    internal static List<Sbr> GenerateSbrs(
        List<StdfRecord> records, SummaryScope scope, HashSet<(byte Head, byte Site, ushort Bin)> existing)
    {
        // Only include PRRs that have a SoftwareBin set
        var prrs = new List<Prr>();
        foreach (var rec in records)
        {
            if (rec is Prr prr && prr.SoftwareBin.HasValue)
                prrs.Add(prr);
        }

        if (prrs.Count == 0)
            return [];

        var result = new List<Sbr>();

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            var groups = prrs.GroupBy(p => (p.HeadNumber, p.SiteNumber, p.SoftwareBin!.Value));
            foreach (var g in groups)
            {
                if (existing.Contains(g.Key))
                    continue;
                result.Add(BuildSbr(g.Key.HeadNumber, g.Key.SiteNumber, g.Key.Value, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var groups = prrs.GroupBy(p => (p.HeadNumber, p.SoftwareBin!.Value));
            foreach (var g in groups)
            {
                if (existing.Contains((g.Key.HeadNumber, 255, g.Key.Value)))
                    continue;
                result.Add(BuildSbr(g.Key.HeadNumber, 255, g.Key.Value, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var groups = prrs.GroupBy(p => p.SoftwareBin!.Value);
            foreach (var g in groups)
            {
                if (existing.Contains((255, 255, g.Key)))
                    continue;
                result.Add(BuildSbr(255, 255, g.Key, g.ToList()));
            }
        }

        return result;
    }

    private static Sbr BuildSbr(byte head, byte site, ushort bin, List<Prr> prrs)
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

        return new Sbr
        {
            HeadNumber = head,
            SiteNumber = site,
            SoftwareBin = bin,
            BinCount = (uint)prrs.Count,
            BinPassFail = passFail,
            BinName = null,
        };
    }
}
