using System.Runtime.CompilerServices;
using Marklio.Stdf.Records;

namespace Marklio.Stdf;

/// <summary>
/// Extension methods for merging V4-2007 continuation records (PSR, STR)
/// into single logical records.
/// </summary>
public static class ContinuationMerger
{
    private const byte ContinuationBit = 0x01;

    /// <summary>
    /// Merges V4-2007 continuation records (PSR, STR) into single logical records.
    /// Non-continuation records pass through unchanged. Merged records lose their
    /// <see cref="StdfRecord.TrailingData"/> and byte-exact round-trip fidelity.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PSR and STR records in STDF V4-2007 may span multiple physical records using a
    /// continuation flag. This method reassembles those multi-segment sequences into
    /// single logical records so callers can iterate without tracking continuation state.
    /// </para>
    /// <para>
    /// <strong>Warning:</strong> Merged records do <em>not</em> preserve
    /// <see cref="StdfRecord.TrailingData"/> from individual continuation segments.
    /// Any trailing bytes present in the original segments are silently dropped during
    /// the merge. This means that writing merged records back to a file will
    /// <em>not</em> produce a byte-exact copy of the original file.
    /// </para>
    /// <para>
    /// If byte-exact round-trip fidelity is required, do <em>not</em> use this method.
    /// Instead, iterate the raw <see cref="StdfRecord"/> stream directly.
    /// </para>
    /// </remarks>
    public static async IAsyncEnumerable<StdfRecord> MergeContinuations(
        this IAsyncEnumerable<StdfRecord> source,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        List<Psr>? psrBuffer = null;
        List<Str>? strBuffer = null;

        await foreach (var rec in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (rec.Record is Psr psr)
            {
                psrBuffer ??= new List<Psr>();
                psrBuffer.Add(psr);

                if ((psr.ContinuationFlag & ContinuationBit) == 0)
                {
                    // Final segment — merge and emit
                    var merged = MergePsr(psrBuffer);
                    yield return new StdfRecord(merged, 1, 90);
                    psrBuffer.Clear();
                }
            }
            else if (rec.Record is Str str)
            {
                strBuffer ??= new List<Str>();
                strBuffer.Add(str);

                if ((str.ContinuationFlag & ContinuationBit) == 0)
                {
                    var merged = MergeStr(strBuffer);
                    yield return new StdfRecord(merged, 15, 30);
                    strBuffer.Clear();
                }
            }
            else
            {
                yield return rec;
            }
        }

        // Flush any trailing incomplete continuation sequences
        if (psrBuffer is { Count: > 0 })
        {
            var merged = MergePsr(psrBuffer);
            yield return new StdfRecord(merged, 1, 90);
        }
        if (strBuffer is { Count: > 0 })
        {
            var merged = MergeStr(strBuffer);
            yield return new StdfRecord(merged, 15, 30);
        }
    }

    /// <summary>
    /// Synchronous version of <see cref="MergeContinuations(IAsyncEnumerable{StdfRecord}, CancellationToken)"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PSR and STR records in STDF V4-2007 may span multiple physical records using a
    /// continuation flag. This method reassembles those multi-segment sequences into
    /// single logical records so callers can iterate without tracking continuation state.
    /// </para>
    /// <para>
    /// <strong>Warning:</strong> Merged records do <em>not</em> preserve
    /// <see cref="StdfRecord.TrailingData"/> from individual continuation segments.
    /// Any trailing bytes present in the original segments are silently dropped during
    /// the merge. This means that writing merged records back to a file will
    /// <em>not</em> produce a byte-exact copy of the original file.
    /// </para>
    /// <para>
    /// If byte-exact round-trip fidelity is required, do <em>not</em> use this method.
    /// Instead, iterate the raw <see cref="StdfRecord"/> stream directly.
    /// </para>
    /// </remarks>
    public static IEnumerable<StdfRecord> MergeContinuations(this IEnumerable<StdfRecord> source)
    {
        List<Psr>? psrBuffer = null;
        List<Str>? strBuffer = null;

        foreach (var rec in source)
        {
            if (rec.Record is Psr psr)
            {
                psrBuffer ??= new List<Psr>();
                psrBuffer.Add(psr);

                if ((psr.ContinuationFlag & ContinuationBit) == 0)
                {
                    var merged = MergePsr(psrBuffer);
                    yield return new StdfRecord(merged, 1, 90);
                    psrBuffer.Clear();
                }
            }
            else if (rec.Record is Str str)
            {
                strBuffer ??= new List<Str>();
                strBuffer.Add(str);

                if ((str.ContinuationFlag & ContinuationBit) == 0)
                {
                    var merged = MergeStr(strBuffer);
                    yield return new StdfRecord(merged, 15, 30);
                    strBuffer.Clear();
                }
            }
            else
            {
                yield return rec;
            }
        }

        if (psrBuffer is { Count: > 0 })
        {
            var merged = MergePsr(psrBuffer);
            yield return new StdfRecord(merged, 1, 90);
        }
        if (strBuffer is { Count: > 0 })
        {
            var merged = MergeStr(strBuffer);
            yield return new StdfRecord(merged, 15, 30);
        }
    }

    private static Psr MergePsr(List<Psr> segments)
    {
        if (segments.Count == 1)
            return segments[0];

        var first = segments[0];
        return new Psr
        {
            ContinuationFlag = 0, // merged record is not a continuation
            PsrIndex = first.PsrIndex,
            PsrName = first.PsrName,
            OptionalFlags = first.OptionalFlags,
            TotalPatternCount = first.TotalPatternCount,
            PatternBegin = ConcatArrays(segments, s => s.PatternBegin),
            PatternEnd = ConcatArrays(segments, s => s.PatternEnd),
            PatternFiles = ConcatArrays(segments, s => s.PatternFiles),
            PatternLabels = ConcatArrays(segments, s => s.PatternLabels),
            FileUids = ConcatArrays(segments, s => s.FileUids),
            AtpgDescriptions = ConcatArrays(segments, s => s.AtpgDescriptions),
            SourceIds = ConcatArrays(segments, s => s.SourceIds),
        };
    }

    private static Str MergeStr(List<Str> segments)
    {
        if (segments.Count == 1)
            return segments[0];

        var first = segments[0];
        return new Str
        {
            ContinuationFlag = 0,
            TestNumber = first.TestNumber,
            HeadNumber = first.HeadNumber,
            SiteNumber = first.SiteNumber,
            PsrReference = first.PsrReference,
            TestFlags = first.TestFlags,
            LogType = first.LogType,
            TestText = first.TestText,
            AlarmId = first.AlarmId,
            ProgramText = first.ProgramText,
            ResultText = first.ResultText,
            ZVal = first.ZVal,
            FmuFlags = first.FmuFlags,
            MaskMap = first.MaskMap,
            FailMap = first.FailMap,
            CycleCount = first.CycleCount,
            TotalFailCount = first.TotalFailCount,
            TotalLogCount = first.TotalLogCount,
            CycleBase = first.CycleBase,
            BitBase = first.BitBase,
            ConditionCount = first.ConditionCount,
            LimitCount = first.LimitCount,
            CycleSize = first.CycleSize,
            PmrSize = first.PmrSize,
            ChainSize = first.ChainSize,
            PatternSize = first.PatternSize,
            BitSize = first.BitSize,
            U1Size = first.U1Size,
            U2Size = first.U2Size,
            U3Size = first.U3Size,
            UtxSize = first.UtxSize,
            CapBegin = first.CapBegin,
            LimitIndexes = first.LimitIndexes,
            LimitSpecs = first.LimitSpecs,
            ConditionList = ConcatArrays(segments, s => s.ConditionList),
            CycleOffsets = ConcatArrays(segments, s => s.CycleOffsets),
            PmrIndexes = ConcatArrays(segments, s => s.PmrIndexes),
            ChainNumbers = ConcatArrays(segments, s => s.ChainNumbers),
            ExpectedData = ConcatByteArrays(segments, s => s.ExpectedData),
            CaptureData = ConcatByteArrays(segments, s => s.CaptureData),
            NewData = ConcatByteArrays(segments, s => s.NewData),
            PatternNumbers = ConcatArrays(segments, s => s.PatternNumbers),
            BitPositions = ConcatArrays(segments, s => s.BitPositions),
            User1 = ConcatArrays(segments, s => s.User1),
            User2 = ConcatArrays(segments, s => s.User2),
            User3 = ConcatArrays(segments, s => s.User3),
            UserText = ConcatArrays(segments, s => s.UserText),
        };
    }

    private static T[]? ConcatArrays<TRecord, T>(List<TRecord> segments, Func<TRecord, T[]?> selector)
    {
        int total = 0;
        foreach (var seg in segments)
        {
            var arr = selector(seg);
            if (arr != null) total += arr.Length;
        }
        if (total == 0) return null;

        var result = new T[total];
        int offset = 0;
        foreach (var seg in segments)
        {
            var arr = selector(seg);
            if (arr != null)
            {
                arr.CopyTo(result, offset);
                offset += arr.Length;
            }
        }
        return result;
    }

    private static byte[]? ConcatByteArrays<TRecord>(List<TRecord> segments, Func<TRecord, byte[]?> selector)
        => ConcatArrays(segments, selector);

    private static ushort? SumCounts<TRecord>(List<TRecord> segments, Func<TRecord, ushort?> selector)
    {
        ushort total = 0;
        bool any = false;
        foreach (var seg in segments)
        {
            var val = selector(seg);
            if (val.HasValue)
            {
                total += val.Value;
                any = true;
            }
        }
        return any ? total : null;
    }
}

