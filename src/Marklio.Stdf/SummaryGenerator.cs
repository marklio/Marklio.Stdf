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
    /// inserted at the correct position. Accumulates all four summary types in a single
    /// streaming pass.
    /// </summary>
    /// <remarks>
    /// This method streams records through without buffering. Statistics are accumulated
    /// as records pass through, and generated summaries are inserted before the first
    /// WRR or MRR record. For correct deduplication, input should follow STDF V4 ordering
    /// (summary records before WRR/MRR).
    /// </remarks>
    public static async IAsyncEnumerable<StdfRecord> GenerateSummaries(
        this IAsyncEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var state = new AccumulatorState();
        bool flushed = false;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            TrackExisting(rec, state);
            Accumulate(rec, state);

            if (!flushed && rec is Wrr or Mrr)
            {
                foreach (var summary in BuildAllSummaries(state, scope))
                    yield return summary;
                flushed = true;
            }

            yield return rec;
        }

        if (!flushed)
        {
            foreach (var summary in BuildAllSummaries(state, scope))
                yield return summary;
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="GenerateSummaries(IAsyncEnumerable{StdfRecord}, SummaryScope, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// This method streams records through without buffering. Statistics are accumulated
    /// as records pass through, and generated summaries are inserted before the first
    /// WRR or MRR record. For correct deduplication, input should follow STDF V4 ordering
    /// (summary records before WRR/MRR).
    /// </remarks>
    public static IEnumerable<StdfRecord> GenerateSummaries(
        this IEnumerable<StdfRecord> source,
        SummaryScope scope = SummaryScope.All)
    {
        return Core(source, scope);

        static IEnumerable<StdfRecord> Core(IEnumerable<StdfRecord> source, SummaryScope scope)
        {
            var state = new AccumulatorState();
            bool flushed = false;

            foreach (var rec in source)
            {
                TrackExisting(rec, state);
                Accumulate(rec, state);

                if (!flushed && rec is Wrr or Mrr)
                {
                    foreach (var summary in BuildAllSummaries(state, scope))
                        yield return summary;
                    flushed = true;
                }

                yield return rec;
            }

            if (!flushed)
            {
                foreach (var summary in BuildAllSummaries(state, scope))
                    yield return summary;
            }
        }
    }

    private static void TrackExisting(StdfRecord rec, AccumulatorState state)
    {
        switch (rec)
        {
            case Pcr pcr:
                state.ExistingPcrs.Add((pcr.HeadNumber, pcr.SiteNumber));
                break;
            case Hbr hbr:
                state.ExistingHbrs.Add((hbr.HeadNumber, hbr.SiteNumber, hbr.HardwareBin));
                break;
            case Sbr sbr:
                state.ExistingSbrs.Add((sbr.HeadNumber, sbr.SiteNumber, sbr.SoftwareBin));
                break;
            case Tsr tsr when tsr.TestNumber.HasValue:
                state.ExistingTsrs.Add((tsr.HeadNumber, tsr.SiteNumber, tsr.TestNumber.Value));
                break;
        }
    }

    private static void Accumulate(StdfRecord rec, AccumulatorState state)
    {
        switch (rec)
        {
            case Prr prr:
                AccumulatePrr(prr, state);
                break;
            case Ptr ptr:
                AccumulatePtr(ptr, state);
                break;
            case Ftr ftr:
                AccumulateFtr(ftr, state);
                break;
            case Mpr mpr:
                AccumulateMpr(mpr, state);
                break;
        }
    }

    private static void AccumulatePrr(Prr prr, AccumulatorState state)
    {
        var pcrKey = (prr.HeadNumber, prr.SiteNumber);
        if (!state.PcrAccumulators.TryGetValue(pcrKey, out var pcrAcc))
        {
            pcrAcc = new PartCountAccumulator();
            state.PcrAccumulators[pcrKey] = pcrAcc;
        }

        var flags = (PartResultFlags)prr.PartFlag;
        pcrAcc.PartCount++;
        if ((flags & PartResultFlags.AbnormalEnd) != 0)
            pcrAcc.AbortCount++;
        if ((flags & PartResultFlags.Failed) == 0 &&
            (flags & PartResultFlags.NoPassFailIndication) == 0 &&
            (flags & PartResultFlags.AbnormalEnd) == 0)
            pcrAcc.GoodCount++;

        // HBR accumulation
        var hbrKey = (prr.HeadNumber, prr.SiteNumber, prr.HardwareBin);
        if (!state.HbrAccumulators.TryGetValue(hbrKey, out var hbrAcc))
        {
            hbrAcc = new BinAccumulator();
            state.HbrAccumulators[hbrKey] = hbrAcc;
        }
        hbrAcc.BinCount++;
        bool failed = (flags & PartResultFlags.Failed) != 0;
        bool noIndication = (flags & PartResultFlags.NoPassFailIndication) != 0;
        if (failed) hbrAcc.AnyFailed = true;
        if (failed || noIndication) hbrAcc.AllPassed = false;

        // SBR accumulation (only if SoftwareBin is present)
        if (prr.SoftwareBin.HasValue)
        {
            var sbrKey = (prr.HeadNumber, prr.SiteNumber, prr.SoftwareBin.Value);
            if (!state.SbrAccumulators.TryGetValue(sbrKey, out var sbrAcc))
            {
                sbrAcc = new BinAccumulator();
                state.SbrAccumulators[sbrKey] = sbrAcc;
            }
            sbrAcc.BinCount++;
            if (failed) sbrAcc.AnyFailed = true;
            if (failed || noIndication) sbrAcc.AllPassed = false;
        }
    }

    private static void AccumulatePtr(Ptr ptr, AccumulatorState state)
    {
        var key = (ptr.HeadNumber, ptr.SiteNumber, ptr.TestNumber);
        var acc = GetOrCreateTestAccumulator(state.TsrAccumulators, key);
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
    }

    private static void AccumulateFtr(Ftr ftr, AccumulatorState state)
    {
        var key = (ftr.HeadNumber, ftr.SiteNumber, ftr.TestNumber);
        var acc = GetOrCreateTestAccumulator(state.TsrAccumulators, key);
        acc.ExecutedCount++;
        acc.TestType = 'F';
        acc.TestName ??= ftr.TestText;
        var flags = (TestResultFlags)(ftr.TestFlags ?? 0);
        if ((flags & TestResultFlags.Failed) != 0) acc.FailedCount++;
        if ((flags & TestResultFlags.Alarm) != 0) acc.AlarmCount++;
    }

    private static void AccumulateMpr(Mpr mpr, AccumulatorState state)
    {
        var key = (mpr.HeadNumber, mpr.SiteNumber, mpr.TestNumber);
        var acc = GetOrCreateTestAccumulator(state.TsrAccumulators, key);
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
    }

    private static TestSummaryGenerator.TestAccumulator GetOrCreateTestAccumulator(
        Dictionary<(byte, byte, uint), TestSummaryGenerator.TestAccumulator> accumulators,
        (byte, byte, uint) key)
    {
        if (!accumulators.TryGetValue(key, out var acc))
        {
            acc = new TestSummaryGenerator.TestAccumulator();
            accumulators[key] = acc;
        }
        return acc;
    }

    private static IEnumerable<StdfRecord> BuildAllSummaries(AccumulatorState state, SummaryScope scope)
    {
        foreach (var pcr in BuildPcrSummaries(state.PcrAccumulators, state.ExistingPcrs, scope))
            yield return pcr;
        foreach (var hbr in BuildBinSummaries(state.HbrAccumulators, state.ExistingHbrs, scope, isHardware: true))
            yield return hbr;
        foreach (var sbr in BuildBinSummaries(state.SbrAccumulators, state.ExistingSbrs, scope, isHardware: false))
            yield return sbr;
        foreach (var tsr in TestSummaryGenerator.BuildSummaries(state.TsrAccumulators, state.ExistingTsrs, scope))
            yield return tsr;
    }

    private static IEnumerable<Pcr> BuildPcrSummaries(
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
                yield return MakePcr(key.Head, key.Site, acc);
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var headRollups = new Dictionary<byte, PartCountAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                if (!headRollups.TryGetValue(key.Head, out var rollup))
                {
                    rollup = new PartCountAccumulator();
                    headRollups[key.Head] = rollup;
                }
                rollup.PartCount += acc.PartCount;
                rollup.AbortCount += acc.AbortCount;
                rollup.GoodCount += acc.GoodCount;
            }

            foreach (var (head, acc) in headRollups)
            {
                if (existing.Contains((head, 255)))
                    continue;
                yield return MakePcr(head, 255, acc);
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
                yield return MakePcr(255, 255, overall);
            }
        }
    }

    private static Pcr MakePcr(byte head, byte site, PartCountAccumulator acc)
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

    private static IEnumerable<StdfRecord> BuildBinSummaries(
        Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator> accumulators,
        HashSet<(byte Head, byte Site, ushort Bin)> existing,
        SummaryScope scope,
        bool isHardware)
    {
        if (accumulators.Count == 0)
            yield break;

        if ((scope & SummaryScope.HeadSite) != 0)
        {
            foreach (var (key, acc) in accumulators)
            {
                if (existing.Contains(key))
                    continue;
                yield return MakeBinRecord(key.Head, key.Site, key.Bin, acc, isHardware);
            }
        }

        if ((scope & SummaryScope.Head) != 0)
        {
            var headRollups = new Dictionary<(byte Head, ushort Bin), BinAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                var rollupKey = (key.Head, key.Bin);
                if (!headRollups.TryGetValue(rollupKey, out var rollup))
                {
                    rollup = new BinAccumulator();
                    headRollups[rollupKey] = rollup;
                }
                RollUpBin(rollup, acc);
            }

            foreach (var (key, acc) in headRollups)
            {
                if (existing.Contains((key.Head, 255, key.Bin)))
                    continue;
                yield return MakeBinRecord(key.Head, 255, key.Bin, acc, isHardware);
            }
        }

        if ((scope & SummaryScope.Overall) != 0)
        {
            var overallRollups = new Dictionary<ushort, BinAccumulator>();
            foreach (var (key, acc) in accumulators)
            {
                if (!overallRollups.TryGetValue(key.Bin, out var rollup))
                {
                    rollup = new BinAccumulator();
                    overallRollups[key.Bin] = rollup;
                }
                RollUpBin(rollup, acc);
            }

            foreach (var (bin, acc) in overallRollups)
            {
                if (existing.Contains((255, 255, bin)))
                    continue;
                yield return MakeBinRecord(255, 255, bin, acc, isHardware);
            }
        }
    }

    private static void RollUpBin(BinAccumulator target, BinAccumulator source)
    {
        target.BinCount += source.BinCount;
        if (source.AnyFailed) target.AnyFailed = true;
        if (!source.AllPassed) target.AllPassed = false;
    }

    private static StdfRecord MakeBinRecord(byte head, byte site, ushort bin, BinAccumulator acc, bool isHardware)
    {
        char passFail = acc.AnyFailed ? 'F' : acc.AllPassed ? 'P' : ' ';

        if (isHardware)
        {
            return new Hbr
            {
                HeadNumber = head,
                SiteNumber = site,
                HardwareBin = bin,
                BinCount = acc.BinCount,
                BinPassFail = passFail,
                BinName = null,
            };
        }
        else
        {
            return new Sbr
            {
                HeadNumber = head,
                SiteNumber = site,
                SoftwareBin = bin,
                BinCount = acc.BinCount,
                BinPassFail = passFail,
                BinName = null,
            };
        }
    }

    private sealed class AccumulatorState
    {
        public readonly Dictionary<(byte Head, byte Site), PartCountAccumulator> PcrAccumulators = new();
        public readonly Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator> HbrAccumulators = new();
        public readonly Dictionary<(byte Head, byte Site, ushort Bin), BinAccumulator> SbrAccumulators = new();
        public readonly Dictionary<(byte Head, byte Site, uint TestNumber), TestSummaryGenerator.TestAccumulator> TsrAccumulators = new();
        public readonly HashSet<(byte Head, byte Site)> ExistingPcrs = new();
        public readonly HashSet<(byte Head, byte Site, ushort Bin)> ExistingHbrs = new();
        public readonly HashSet<(byte Head, byte Site, ushort Bin)> ExistingSbrs = new();
        public readonly HashSet<(byte Head, byte Site, uint TestNumber)> ExistingTsrs = new();
    }

    private sealed class PartCountAccumulator
    {
        public uint PartCount;
        public uint AbortCount;
        public uint GoodCount;
    }

    private sealed class BinAccumulator
    {
        public uint BinCount;
        public bool AnyFailed;
        public bool AllPassed = true;
    }

    // TestAccumulator is reused from TestSummaryGenerator.
}
