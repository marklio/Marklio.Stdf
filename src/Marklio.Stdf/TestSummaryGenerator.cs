using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for generating missing TSR (Test Synopsis Record) summary records
/// from PTR/FTR/MPR data in an STDF stream.
/// </summary>
[Experimental("STDF0001", UrlFormat = "https://github.com/marklio/Marklio.Stdf")]
public static class TestSummaryGenerator
{
    /// <summary>
    /// Re-emits the STDF stream with missing TSR records inserted before the first WRR
    /// (or MRR, or at end). TSR records are computed from PTR, FTR, and MPR data.
    /// </summary>
    public static async IAsyncEnumerable<StdfRecord> GenerateTestSummaries(
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
    /// Synchronous version of <see cref="GenerateTestSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    public static IEnumerable<StdfRecord> GenerateTestSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        var records = source.ToList();
        return EmitWithSummaries(records, scope);
    }

    private static IEnumerable<StdfRecord> EmitWithSummaries(List<StdfRecord> records, SummaryScope scope)
    {
        var existingTsrs = new HashSet<(byte Head, byte Site, uint TestNum)>();
        foreach (var rec in records)
        {
            if (rec is Tsr tsr && tsr.TestNumber.HasValue)
                existingTsrs.Add((tsr.HeadNumber, tsr.SiteNumber, tsr.TestNumber.Value));
        }

        var generated = GenerateTsrs(records, scope, existingTsrs);
        if (generated.Count == 0)
        {
            foreach (var rec in records)
                yield return rec;
            yield break;
        }

        int insertionIndex = SummaryInsertionPoint.Find(records);
        for (int i = 0; i < insertionIndex; i++)
            yield return records[i];
        foreach (var tsr in generated)
            yield return tsr;
        for (int i = insertionIndex; i < records.Count; i++)
            yield return records[i];
    }

    internal static List<Tsr> GenerateTsrs(
        List<StdfRecord> records, SummaryScope scope, HashSet<(byte Head, byte Site, uint TestNum)> existing)
    {
        var testRecords = new List<ITestRecord>();
        foreach (var rec in records)
        {
            if (rec is ITestRecord tr)
                testRecords.Add(tr);
        }

        if (testRecords.Count == 0)
            return [];

        var result = new List<Tsr>();

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            var groups = testRecords.GroupBy(t => (t.HeadNumber, t.SiteNumber, t.TestNumber));
            foreach (var g in groups)
            {
                if (existing.Contains(g.Key))
                    continue;
                result.Add(BuildTsr(g.Key.HeadNumber, g.Key.SiteNumber, g.Key.TestNumber, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var groups = testRecords.GroupBy(t => (t.HeadNumber, t.TestNumber));
            foreach (var g in groups)
            {
                if (existing.Contains((g.Key.HeadNumber, 255, g.Key.TestNumber)))
                    continue;
                result.Add(BuildTsr(g.Key.HeadNumber, 255, g.Key.TestNumber, g.ToList()));
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var groups = testRecords.GroupBy(t => t.TestNumber);
            foreach (var g in groups)
            {
                if (existing.Contains((255, 255, g.Key)))
                    continue;
                result.Add(BuildTsr(255, 255, g.Key, g.ToList()));
            }
        }

        return result;
    }

    private static Tsr BuildTsr(byte head, byte site, uint testNumber, List<ITestRecord> testRecords)
    {
        uint executedCount = (uint)testRecords.Count;
        uint failedCount = 0;
        uint alarmCount = 0;
        char testType = ' ';
        string? testName = null;

        // Collect results for statistics (PTR has single Result, MPR has ReturnResults)
        var results = new List<float>();

        foreach (var tr in testRecords)
        {
            byte testFlags;
            switch (tr)
            {
                case Ptr ptr:
                    testType = 'P';
                    testFlags = ptr.TestFlags;
                    testName ??= ptr.TestText;
                    if (ptr.Result.HasValue)
                        results.Add(ptr.Result.Value);
                    break;
                case Ftr ftr:
                    testType = 'F';
                    testFlags = ftr.TestFlags ?? 0;
                    testName ??= ftr.TestText;
                    break;
                case Mpr mpr:
                    testType = 'M';
                    testFlags = mpr.TestFlags;
                    testName ??= mpr.TestText;
                    if (mpr.ReturnResults != null)
                    {
                        foreach (var r in mpr.ReturnResults)
                            results.Add(r);
                    }
                    break;
                default:
                    testFlags = 0;
                    break;
            }

            var flags = (TestResultFlags)testFlags;
            if ((flags & TestResultFlags.Failed) != 0)
                failedCount++;
            if ((flags & TestResultFlags.Alarm) != 0)
                alarmCount++;
        }

        var tsr = new Tsr
        {
            HeadNumber = head,
            SiteNumber = site,
            TestType = testType,
            TestNumber = testNumber,
            ExecutedCount = executedCount,
            FailedCount = failedCount,
            AlarmCount = alarmCount,
            TestName = testName,
        };

        if (results.Count > 0 && testType != 'F')
        {
            float min = float.MaxValue;
            float max = float.MinValue;
            float sum = 0;
            float sumSq = 0;
            foreach (var r in results)
            {
                if (r < min) min = r;
                if (r > max) max = r;
                sum += r;
                sumSq += r * r;
            }

            tsr.OptionalFlags = 0; // all stats valid
            tsr.ResultMin = min;
            tsr.ResultMax = max;
            tsr.ResultSum = sum;
            tsr.ResultSumOfSquares = sumSq;
        }
        else if (testType == 'F')
        {
            // FTR has no result values; mark all result stats as invalid
            tsr.OptionalFlags = (byte)(
                TsrOptionalFlags.ResultMinInvalid |
                TsrOptionalFlags.ResultMaxInvalid |
                TsrOptionalFlags.TestTimeInvalid |
                TsrOptionalFlags.ResultSumInvalid |
                TsrOptionalFlags.ResultSumOfSquaresInvalid);
        }

        return tsr;
    }
}
