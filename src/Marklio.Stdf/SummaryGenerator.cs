using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for generating all missing summary records (PCR, HBR, SBR, TSR)
/// from test data in an STDF stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class SummaryGenerator
{
    /// <summary>
    /// Re-emits the STDF stream with all missing summary records (PCR, HBR, SBR, TSR)
    /// inserted at the correct position. Buffers the stream once and generates all summaries
    /// in a single pass.
    /// </summary>
    public static async IAsyncEnumerable<StdfRecord> GenerateSummaries(
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
    /// Synchronous version of <see cref="GenerateSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> GenerateSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        var records = source.ToList();
        return EmitWithSummaries(records, scope);
    }

    private static IEnumerable<StdfRecord> EmitWithSummaries(List<StdfRecord> records, SummaryScope scope)
    {
        // Collect existing summaries
        var existingPcrs = new HashSet<(byte Head, byte Site)>();
        var existingHbrs = new HashSet<(byte Head, byte Site, ushort Bin)>();
        var existingSbrs = new HashSet<(byte Head, byte Site, ushort Bin)>();
        var existingTsrs = new HashSet<(byte Head, byte Site, uint TestNum)>();

        foreach (var rec in records)
        {
            switch (rec)
            {
                case Pcr pcr:
                    existingPcrs.Add((pcr.HeadNumber, pcr.SiteNumber));
                    break;
                case Hbr hbr:
                    existingHbrs.Add((hbr.HeadNumber, hbr.SiteNumber, hbr.HardwareBin));
                    break;
                case Sbr sbr:
                    existingSbrs.Add((sbr.HeadNumber, sbr.SiteNumber, sbr.SoftwareBin));
                    break;
                case Tsr tsr when tsr.TestNumber.HasValue:
                    existingTsrs.Add((tsr.HeadNumber, tsr.SiteNumber, tsr.TestNumber.Value));
                    break;
            }
        }

        // Generate all missing summaries
        var allGenerated = new List<StdfRecord>();
        allGenerated.AddRange(PartCountGenerator.GeneratePcrs(records, scope, existingPcrs));
        allGenerated.AddRange(HardwareBinSummaryGenerator.GenerateHbrs(records, scope, existingHbrs));
        allGenerated.AddRange(SoftwareBinSummaryGenerator.GenerateSbrs(records, scope, existingSbrs));
        allGenerated.AddRange(TestSummaryGenerator.GenerateTsrs(records, scope, existingTsrs));

        if (allGenerated.Count == 0)
        {
            foreach (var rec in records)
                yield return rec;
            yield break;
        }

        int insertionIndex = SummaryInsertionPoint.Find(records);
        for (int i = 0; i < insertionIndex; i++)
            yield return records[i];
        foreach (var generated in allGenerated)
            yield return generated;
        for (int i = insertionIndex; i < records.Count; i++)
            yield return records[i];
    }
}
