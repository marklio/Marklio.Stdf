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
    /// <remarks>
    /// This method streams records through without buffering. Statistics are accumulated
    /// as records pass through, and generated summaries are inserted before the first
    /// WRR or MRR record. For correct deduplication, input should follow STDF V4 ordering
    /// (summary records before WRR/MRR).
    /// </remarks>
    /// <param name="source">The STDF record stream to process.</param>
    /// <param name="scope">Which summary scope levels to generate. Defaults to <see cref="SummaryScope.All"/>.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of <see cref="StdfRecord"/> that includes all original records plus any generated TSR records.</returns>
    public static async IAsyncEnumerable<StdfRecord> GenerateTestSummaries(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var accumulators = new Dictionary<(byte Head, byte Site, uint TestNumber), TestAccumulator>();
        var existing = new HashSet<(byte Head, byte Site, uint TestNumber)>();
        bool flushed = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec is Tsr tsr && tsr.TestNumber.HasValue)
                existing.Add((tsr.HeadNumber, tsr.SiteNumber, tsr.TestNumber.Value));

            AccumulateTestRecord(rec, accumulators);

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
    /// Synchronous version of <see cref="GenerateTestSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// This method streams records through without buffering. Statistics are accumulated
    /// as records pass through, and generated summaries are inserted before the first
    /// WRR or MRR record. For correct deduplication, input should follow STDF V4 ordering
    /// (summary records before WRR/MRR).
    /// </remarks>
    /// <param name="source">The STDF record stream to process.</param>
    /// <param name="scope">Which summary scope levels to generate. Defaults to <see cref="SummaryScope.All"/>.</param>
    /// <returns>An enumerable of <see cref="StdfRecord"/> that includes all original records plus any generated TSR records.</returns>
    public static IEnumerable<StdfRecord> GenerateTestSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        return Core(source, scope);

        static IEnumerable<StdfRecord> Core(IEnumerable<StdfRecord> source, SummaryScope scope)
        {
            var accumulators = new Dictionary<(byte Head, byte Site, uint TestNumber), TestAccumulator>();
            var existing = new HashSet<(byte Head, byte Site, uint TestNumber)>();
            bool flushed = false;

            foreach (var rec in source)
            {
                if (rec is Tsr tsr && tsr.TestNumber.HasValue)
                    existing.Add((tsr.HeadNumber, tsr.SiteNumber, tsr.TestNumber.Value));

                AccumulateTestRecord(rec, accumulators);

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

    private static void AccumulateTestRecord(
        StdfRecord rec,
        Dictionary<(byte Head, byte Site, uint TestNumber), TestAccumulator> accumulators)
    {
        switch (rec)
        {
            case Ptr ptr:
            {
                var key = (ptr.HeadNumber, ptr.SiteNumber, ptr.TestNumber);
                var acc = GetOrCreateAccumulator(accumulators, key);
                acc.ExecutedCount++;
                acc.TestType = 'P';
                acc.TestName ??= ptr.TestText;
                var flags = (TestResultFlags)ptr.TestFlags;
                if ((flags & TestResultFlags.Failed) != 0) acc.FailedCount++;
                if ((flags & TestResultFlags.Alarm) != 0) acc.AlarmCount++;
                if (ptr.Result.HasValue)
                {
                    double r = ptr.Result.Value;
                    if (r < acc.Min) acc.Min = r;
                    if (r > acc.Max) acc.Max = r;
                    acc.Sum += r;
                    acc.SumOfSquares += r * r;
                    acc.ResultCount++;
                }
                break;
            }
            case Ftr ftr:
            {
                var key = (ftr.HeadNumber, ftr.SiteNumber, ftr.TestNumber);
                var acc = GetOrCreateAccumulator(accumulators, key);
                acc.ExecutedCount++;
                acc.TestType = 'F';
                acc.TestName ??= ftr.TestText;
                var flags = (TestResultFlags)(ftr.TestFlags ?? 0);
                if ((flags & TestResultFlags.Failed) != 0) acc.FailedCount++;
                if ((flags & TestResultFlags.Alarm) != 0) acc.AlarmCount++;
                break;
            }
            case Mpr mpr:
            {
                var key = (mpr.HeadNumber, mpr.SiteNumber, mpr.TestNumber);
                var acc = GetOrCreateAccumulator(accumulators, key);
                acc.ExecutedCount++;
                acc.TestType = 'M';
                acc.TestName ??= mpr.TestText;
                var flags = (TestResultFlags)mpr.TestFlags;
                if ((flags & TestResultFlags.Failed) != 0) acc.FailedCount++;
                if ((flags & TestResultFlags.Alarm) != 0) acc.AlarmCount++;
                if (mpr.ReturnResults != null)
                {
                    foreach (var r in mpr.ReturnResults)
                    {
                        double d = r;
                        if (d < acc.Min) acc.Min = d;
                        if (d > acc.Max) acc.Max = d;
                        acc.Sum += d;
                        acc.SumOfSquares += d * d;
                        acc.ResultCount++;
                    }
                }
                break;
            }
        }
    }

    private static TestAccumulator GetOrCreateAccumulator(
        Dictionary<(byte, byte, uint), TestAccumulator> accumulators,
        (byte, byte, uint) key)
    {
        if (!accumulators.TryGetValue(key, out var acc))
        {
            acc = new TestAccumulator();
            accumulators[key] = acc;
        }
        return acc;
    }

    internal static IEnumerable<Tsr> BuildSummaries(
        Dictionary<(byte Head, byte Site, uint TestNumber), TestAccumulator> accumulators,
        HashSet<(byte Head, byte Site, uint TestNumber)> existing,
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
                yield return BuildTsr(key.Head, key.Site, key.TestNumber, acc);
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var headRollups = new Dictionary<(byte Head, uint TestNumber), TestAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                var rollupKey = (key.Head, key.TestNumber);
                if (!headRollups.TryGetValue(rollupKey, out var rollup))
                {
                    rollup = new TestAccumulator();
                    headRollups[rollupKey] = rollup;
                }
                RollUp(rollup, acc);
            }

            foreach (var (key, acc) in headRollups)
            {
                if (existing.Contains((key.Head, 255, key.TestNumber)))
                    continue;
                yield return BuildTsr(key.Head, 255, key.TestNumber, acc);
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var overallRollups = new Dictionary<uint, TestAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                if (!overallRollups.TryGetValue(key.TestNumber, out var rollup))
                {
                    rollup = new TestAccumulator();
                    overallRollups[key.TestNumber] = rollup;
                }
                RollUp(rollup, acc);
            }

            foreach (var (testNumber, acc) in overallRollups)
            {
                if (existing.Contains((255, 255, testNumber)))
                    continue;
                yield return BuildTsr(255, 255, testNumber, acc);
            }
        }
    }

    private static void RollUp(TestAccumulator target, TestAccumulator source)
    {
        target.ExecutedCount += source.ExecutedCount;
        target.FailedCount += source.FailedCount;
        target.AlarmCount += source.AlarmCount;
        if (source.Min < target.Min) target.Min = source.Min;
        if (source.Max > target.Max) target.Max = source.Max;
        target.Sum += source.Sum;
        target.SumOfSquares += source.SumOfSquares;
        target.ResultCount += source.ResultCount;
        if (target.TestType == '\0') target.TestType = source.TestType;
        target.TestName ??= source.TestName;
    }

    private static Tsr BuildTsr(byte head, byte site, uint testNumber, TestAccumulator acc)
    {
        var tsr = new Tsr
        {
            HeadNumber = head,
            SiteNumber = site,
            TestType = acc.TestType,
            TestNumber = testNumber,
            ExecutedCount = acc.ExecutedCount,
            FailedCount = acc.FailedCount,
            AlarmCount = acc.AlarmCount,
            TestName = acc.TestName,
        };

        if (acc.ResultCount > 0 && acc.TestType != 'F')
        {
            tsr.OptionalFlags = 0; // all stats valid
            tsr.ResultMin = (float)acc.Min;
            tsr.ResultMax = (float)acc.Max;
            tsr.ResultSum = (float)acc.Sum;
            tsr.ResultSumOfSquares = (float)acc.SumOfSquares;
        }
        else
        {
            // FTR has no result values; PTR/MPR with no results also need stats marked invalid
            tsr.OptionalFlags = (byte)(
                TsrOptionalFlags.ResultMinInvalid |
                TsrOptionalFlags.ResultMaxInvalid |
                TsrOptionalFlags.TestTimeInvalid |
                TsrOptionalFlags.ResultSumInvalid |
                TsrOptionalFlags.ResultSumOfSquaresInvalid);
        }

        return tsr;
    }

    internal sealed class TestAccumulator
    {
        public uint ExecutedCount;
        public uint FailedCount;
        public uint AlarmCount;
        public char TestType;
        public string? TestName;
        public double Min = double.MaxValue;
        public double Max = double.MinValue;
        public double Sum;
        public double SumOfSquares;
        public int ResultCount;
    }
}
