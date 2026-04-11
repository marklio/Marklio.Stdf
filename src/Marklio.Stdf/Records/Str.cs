using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Marklio.Stdf.Records;

/// <summary>
/// STR — Scan Test Record (15, 30).
/// V4-2007. Contains detailed scan test results. Hand-implemented because it uses
/// variable-width fields (U*f) whose byte sizes are determined by earlier fields
/// (CYC_SIZE, PMR_SIZE, CHN_SIZE, PAT_SIZE, BIT_SIZE, U1_SIZE, U2_SIZE, U3_SIZE, UTX_SIZE).
/// </summary>
public readonly record struct Str : IStdfRecord, ITestRecord
{
    static byte IStdfRecord.RecordType => 15;
    static byte IStdfRecord.RecordSubType => 30;

    public byte ContinuationFlag { get; init; }
    public uint TestNumber { get; init; }
    public byte HeadNumber { get; init; }
    public byte SiteNumber { get; init; }
    public ushort PsrReference { get; init; }
    public byte TestFlags { get; init; }
    public string? LogType { get; init; }
    public string? TestText { get; init; }
    public string? AlarmId { get; init; }
    public string? ProgramText { get; init; }
    public string? ResultText { get; init; }
    public byte? ZVal { get; init; }
    public byte? FmuFlags { get; init; }
    public System.Collections.BitArray? MaskMap { get; init; }
    public System.Collections.BitArray? FailMap { get; init; }
    public ulong? CycleCount { get; init; }
    public uint? TotalFailCount { get; init; }
    public uint? TotalLogCount { get; init; }
    public ulong? CycleBase { get; init; }
    public uint? BitBase { get; init; }
    public ushort? ConditionCount { get; init; }
    public ushort? LimitCount { get; init; }
    public byte? CycleSize { get; init; }
    public byte? PmrSize { get; init; }
    public byte? ChainSize { get; init; }
    public byte? PatternSize { get; init; }
    public byte? BitSize { get; init; }
    public byte? U1Size { get; init; }
    public byte? U2Size { get; init; }
    public byte? U3Size { get; init; }
    public byte? UtxSize { get; init; }
    public ushort? CapBegin { get; init; }
    public ushort[]? LimitIndexes { get; init; }
    public uint[]? LimitSpecs { get; init; }
    public string[]? ConditionList { get; init; }
    // Variable-width arrays (stored as ulong[] regardless of wire width)
    public ushort? CycCnt { get; init; }
    public ulong[]? CycleOffsets { get; init; }
    public ushort? PmrCnt { get; init; }
    public ulong[]? PmrIndexes { get; init; }
    public ushort? ChnCnt { get; init; }
    public ulong[]? ChainNumbers { get; init; }
    public ushort? ExpCnt { get; init; }
    public byte[]? ExpectedData { get; init; }
    public ushort? CapCnt { get; init; }
    public byte[]? CaptureData { get; init; }
    public ushort? NewCnt { get; init; }
    public byte[]? NewData { get; init; }
    public ushort? PatCnt { get; init; }
    public ulong[]? PatternNumbers { get; init; }
    public ushort? BposCnt { get; init; }
    public ulong[]? BitPositions { get; init; }
    public ushort? Usr1Cnt { get; init; }
    public ulong[]? User1 { get; init; }
    public ushort? Usr2Cnt { get; init; }
    public ulong[]? User2 { get; init; }
    public ushort? Usr3Cnt { get; init; }
    public ulong[]? User3 { get; init; }
    public ushort? TxtCnt { get; init; }
    public string[]? UserText { get; init; }

    public Str()
    {
        LogType = string.Empty;
        TestText = string.Empty;
        AlarmId = string.Empty;
        ProgramText = string.Empty;
        ResultText = string.Empty;
    }

    public static Str Deserialize(ref SequenceReader<byte> reader, Endianness endianness)
    {
        var r = new Str();
        if (!reader.TryRead(out byte contFlg)) return r;
        r = r with { ContinuationFlag = contFlg };

        if (reader.Remaining < 4) return r;
        r = r with { TestNumber = ReadU4(ref reader, endianness) };

        if (!reader.TryRead(out byte head)) return r;
        r = r with { HeadNumber = head };

        if (!reader.TryRead(out byte site)) return r;
        r = r with { SiteNumber = site };

        if (reader.Remaining < 2) return r;
        r = r with { PsrReference = ReadU2(ref reader, endianness) };

        if (!reader.TryRead(out byte testFlg)) return r;
        r = r with { TestFlags = testFlg };

        if (reader.Remaining < 1) return r;
        r = r with { LogType = ReadCn(ref reader) };

        if (reader.Remaining < 1) return r;
        r = r with { TestText = ReadCn(ref reader) };

        if (reader.Remaining < 1) return r;
        r = r with { AlarmId = ReadCn(ref reader) };

        if (reader.Remaining < 1) return r;
        r = r with { ProgramText = ReadCn(ref reader) };

        if (reader.Remaining < 1) return r;
        r = r with { ResultText = ReadCn(ref reader) };

        if (!reader.TryRead(out byte zVal)) return r;
        r = r with { ZVal = zVal };

        if (!reader.TryRead(out byte fmuFlg)) return r;
        r = r with { FmuFlags = fmuFlg };

        if (reader.Remaining < 2) return r;
        r = r with { MaskMap = ReadDn(ref reader, endianness) };

        if (reader.Remaining < 2) return r;
        r = r with { FailMap = ReadDn(ref reader, endianness) };

        if (reader.Remaining < 8) return r;
        r = r with { CycleCount = ReadU8(ref reader, endianness) };

        if (reader.Remaining < 4) return r;
        r = r with { TotalFailCount = ReadU4(ref reader, endianness) };

        if (reader.Remaining < 4) return r;
        r = r with { TotalLogCount = ReadU4(ref reader, endianness) };

        if (reader.Remaining < 8) return r;
        r = r with { CycleBase = ReadU8(ref reader, endianness) };

        if (reader.Remaining < 4) return r;
        r = r with { BitBase = ReadU4(ref reader, endianness) };

        if (reader.Remaining < 2) return r;
        ushort condCnt = ReadU2(ref reader, endianness);
        r = r with { ConditionCount = condCnt };

        if (reader.Remaining < 2) return r;
        ushort limCnt = ReadU2(ref reader, endianness);
        r = r with { LimitCount = limCnt };

        if (!reader.TryRead(out byte cycSize)) return r;
        r = r with { CycleSize = cycSize };
        if (!reader.TryRead(out byte pmrSize)) return r;
        r = r with { PmrSize = pmrSize };
        if (!reader.TryRead(out byte chnSize)) return r;
        r = r with { ChainSize = chnSize };
        if (!reader.TryRead(out byte patSize)) return r;
        r = r with { PatternSize = patSize };
        if (!reader.TryRead(out byte bitSize)) return r;
        r = r with { BitSize = bitSize };
        if (!reader.TryRead(out byte u1Size)) return r;
        r = r with { U1Size = u1Size };
        if (!reader.TryRead(out byte u2Size)) return r;
        r = r with { U2Size = u2Size };
        if (!reader.TryRead(out byte u3Size)) return r;
        r = r with { U3Size = u3Size };
        if (!reader.TryRead(out byte utxSize)) return r;
        r = r with { UtxSize = utxSize };

        if (reader.Remaining < 2) return r;
        r = r with { CapBegin = ReadU2(ref reader, endianness) };

        // LIM_INDX array counted by LIM_CNT
        if (limCnt > 0 && reader.Remaining >= limCnt * 2)
        {
            var limIdx = new ushort[limCnt];
            for (int i = 0; i < limCnt; i++) limIdx[i] = ReadU2(ref reader, endianness);
            r = r with { LimitIndexes = limIdx };
        }

        // LIM_SPEC array counted by LIM_CNT
        if (limCnt > 0 && reader.Remaining >= limCnt * 4)
        {
            var limSpec = new uint[limCnt];
            for (int i = 0; i < limCnt; i++) limSpec[i] = ReadU4(ref reader, endianness);
            r = r with { LimitSpecs = limSpec };
        }

        // COND_LST array counted by COND_CNT
        if (condCnt > 0 && reader.Remaining > 0)
        {
            var condLst = new string[condCnt];
            for (int i = 0; i < condCnt; i++) condLst[i] = ReadCn(ref reader);
            r = r with { ConditionList = condLst };
        }

        // Variable-width counted arrays
        r = ReadVarArray(ref reader, r, endianness, cycSize, s => s with { CycCnt = null }, (s, cnt) => s with { CycCnt = cnt }, (s, arr) => s with { CycleOffsets = arr });
        r = ReadVarArray(ref reader, r, endianness, pmrSize, s => s with { PmrCnt = null }, (s, cnt) => s with { PmrCnt = cnt }, (s, arr) => s with { PmrIndexes = arr });
        r = ReadVarArray(ref reader, r, endianness, chnSize, s => s with { ChnCnt = null }, (s, cnt) => s with { ChnCnt = cnt }, (s, arr) => s with { ChainNumbers = arr });

        // EXP_DATA: byte arrays
        r = ReadByteCountedArray(ref reader, r, endianness, s => s with { ExpCnt = null }, (s, cnt) => s with { ExpCnt = cnt }, (s, arr) => s with { ExpectedData = arr });
        r = ReadByteCountedArray(ref reader, r, endianness, s => s with { CapCnt = null }, (s, cnt) => s with { CapCnt = cnt }, (s, arr) => s with { CaptureData = arr });
        r = ReadByteCountedArray(ref reader, r, endianness, s => s with { NewCnt = null }, (s, cnt) => s with { NewCnt = cnt }, (s, arr) => s with { NewData = arr });

        r = ReadVarArray(ref reader, r, endianness, patSize, s => s with { PatCnt = null }, (s, cnt) => s with { PatCnt = cnt }, (s, arr) => s with { PatternNumbers = arr });
        r = ReadVarArray(ref reader, r, endianness, bitSize, s => s with { BposCnt = null }, (s, cnt) => s with { BposCnt = cnt }, (s, arr) => s with { BitPositions = arr });
        r = ReadVarArray(ref reader, r, endianness, u1Size, s => s with { Usr1Cnt = null }, (s, cnt) => s with { Usr1Cnt = cnt }, (s, arr) => s with { User1 = arr });
        r = ReadVarArray(ref reader, r, endianness, u2Size, s => s with { Usr2Cnt = null }, (s, cnt) => s with { Usr2Cnt = cnt }, (s, arr) => s with { User2 = arr });
        r = ReadVarArray(ref reader, r, endianness, u3Size, s => s with { Usr3Cnt = null }, (s, cnt) => s with { Usr3Cnt = cnt }, (s, arr) => s with { User3 = arr });

        // TXT_CNT + USER_TXT (C*f with UTX_SIZE determining fixed string length)
        if (reader.Remaining >= 2)
        {
            ushort txtCnt = ReadU2(ref reader, endianness);
            r = r with { TxtCnt = txtCnt };
            if (txtCnt > 0 && utxSize > 0 && reader.Remaining >= txtCnt * utxSize)
            {
                var txt = new string[txtCnt];
                for (int i = 0; i < txtCnt; i++) txt[i] = ReadCf(ref reader, utxSize);
                r = r with { UserText = txt };
            }
        }

        return r;
    }

    public void Serialize(IBufferWriter<byte> writer, Endianness endianness)
    {
        WriteByte(writer, ContinuationFlag);
        WriteU4(writer, TestNumber, endianness);
        WriteByte(writer, HeadNumber);
        WriteByte(writer, SiteNumber);
        WriteU2(writer, PsrReference, endianness);
        WriteByte(writer, TestFlags);
        WriteCn(writer, LogType);
        WriteCn(writer, TestText);
        WriteCn(writer, AlarmId);
        WriteCn(writer, ProgramText);
        WriteCn(writer, ResultText);

        if (!ZVal.HasValue) return;
        WriteByte(writer, ZVal.Value);

        if (!FmuFlags.HasValue) return;
        WriteByte(writer, FmuFlags.Value);

        WriteDn(writer, MaskMap, endianness);
        WriteDn(writer, FailMap, endianness);

        if (!CycleCount.HasValue) return;
        WriteU8(writer, CycleCount.Value, endianness);

        if (!TotalFailCount.HasValue) return;
        WriteU4(writer, TotalFailCount.Value, endianness);

        if (!TotalLogCount.HasValue) return;
        WriteU4(writer, TotalLogCount.Value, endianness);

        if (!CycleBase.HasValue) return;
        WriteU8(writer, CycleBase.Value, endianness);

        if (!BitBase.HasValue) return;
        WriteU4(writer, BitBase.Value, endianness);

        ushort condCnt = (ushort)(ConditionList?.Length ?? 0);
        ushort limCnt = (ushort)(LimitIndexes?.Length ?? 0);
        WriteU2(writer, condCnt, endianness);
        WriteU2(writer, limCnt, endianness);

        byte cycSz = CycleSize ?? 0, pmrSz = PmrSize ?? 0, chnSz = ChainSize ?? 0;
        byte patSz = PatternSize ?? 0, bitSz = BitSize ?? 0;
        byte u1Sz = U1Size ?? 0, u2Sz = U2Size ?? 0, u3Sz = U3Size ?? 0, utxSz = UtxSize ?? 0;
        WriteByte(writer, cycSz);
        WriteByte(writer, pmrSz);
        WriteByte(writer, chnSz);
        WriteByte(writer, patSz);
        WriteByte(writer, bitSz);
        WriteByte(writer, u1Sz);
        WriteByte(writer, u2Sz);
        WriteByte(writer, u3Sz);
        WriteByte(writer, utxSz);

        if (!CapBegin.HasValue) return;
        WriteU2(writer, CapBegin.Value, endianness);

        if (limCnt > 0 && LimitIndexes != null)
            foreach (var v in LimitIndexes) WriteU2(writer, v, endianness);
        if (limCnt > 0 && LimitSpecs != null)
            foreach (var v in LimitSpecs) WriteU4(writer, v, endianness);
        if (condCnt > 0 && ConditionList != null)
            foreach (var v in ConditionList) WriteCn(writer, v);

        WriteVarArray(writer, CycCnt, CycleOffsets, cycSz, endianness);
        WriteVarArray(writer, PmrCnt, PmrIndexes, pmrSz, endianness);
        WriteVarArray(writer, ChnCnt, ChainNumbers, chnSz, endianness);

        WriteByteCountedArray(writer, ExpCnt, ExpectedData, endianness);
        WriteByteCountedArray(writer, CapCnt, CaptureData, endianness);
        WriteByteCountedArray(writer, NewCnt, NewData, endianness);

        WriteVarArray(writer, PatCnt, PatternNumbers, patSz, endianness);
        WriteVarArray(writer, BposCnt, BitPositions, bitSz, endianness);
        WriteVarArray(writer, Usr1Cnt, User1, u1Sz, endianness);
        WriteVarArray(writer, Usr2Cnt, User2, u2Sz, endianness);
        WriteVarArray(writer, Usr3Cnt, User3, u3Sz, endianness);

        if (!TxtCnt.HasValue) return;
        ushort txtCnt = TxtCnt.Value;
        WriteU2(writer, txtCnt, endianness);
        if (txtCnt > 0 && UserText != null && utxSz > 0)
            foreach (var v in UserText) WriteCf(writer, v, utxSz);
    }

    // --- Helper: read variable-width integer array ---
    private static Str ReadVarArray(
        ref SequenceReader<byte> reader, Str r, Endianness e, byte elemSize,
        Func<Str, Str> clearCnt, Func<Str, ushort, Str> setCnt, Func<Str, ulong[], Str> setArr)
    {
        if (reader.Remaining < 2) return r;
        ushort cnt = ReadU2(ref reader, e);
        r = setCnt(r, cnt);
        if (cnt > 0 && elemSize > 0 && reader.Remaining >= cnt * elemSize)
        {
            var arr = new ulong[cnt];
            for (int i = 0; i < cnt; i++) arr[i] = ReadUf(ref reader, e, elemSize);
            r = setArr(r, arr);
        }
        return r;
    }

    private static Str ReadByteCountedArray(
        ref SequenceReader<byte> reader, Str r, Endianness e,
        Func<Str, Str> clearCnt, Func<Str, ushort, Str> setCnt, Func<Str, byte[], Str> setArr)
    {
        if (reader.Remaining < 2) return r;
        ushort cnt = ReadU2(ref reader, e);
        r = setCnt(r, cnt);
        if (cnt > 0 && reader.Remaining >= cnt)
        {
            var arr = new byte[cnt];
            for (int i = 0; i < cnt; i++) reader.TryRead(out arr[i]);
            r = setArr(r, arr);
        }
        return r;
    }

    // --- Helper: write variable-width integer array ---
    private static void WriteVarArray(IBufferWriter<byte> writer, ushort? cnt, ulong[]? arr, byte elemSize, Endianness e)
    {
        if (!cnt.HasValue) return;
        WriteU2(writer, cnt.Value, e);
        if (arr != null && elemSize > 0)
            foreach (var v in arr) WriteUf(writer, v, e, elemSize);
    }

    private static void WriteByteCountedArray(IBufferWriter<byte> writer, ushort? cnt, byte[]? arr, Endianness e)
    {
        if (!cnt.HasValue) return;
        WriteU2(writer, cnt.Value, e);
        if (arr != null)
        {
            var s = writer.GetSpan(arr.Length);
            arr.AsSpan().CopyTo(s);
            writer.Advance(arr.Length);
        }
    }

    // --- Variable-width unsigned integer read/write ---
    private static ulong ReadUf(ref SequenceReader<byte> reader, Endianness e, byte size)
    {
        return size switch
        {
            1 => ReadByte(ref reader),
            2 => ReadU2(ref reader, e),
            4 => ReadU4(ref reader, e),
            8 => ReadU8(ref reader, e),
            _ => 0,
        };
    }

    private static void WriteUf(IBufferWriter<byte> writer, ulong value, Endianness e, byte size)
    {
        switch (size)
        {
            case 1: WriteByte(writer, (byte)value); break;
            case 2: WriteU2(writer, (ushort)value, e); break;
            case 4: WriteU4(writer, (uint)value, e); break;
            case 8: WriteU8(writer, value, e); break;
        }
    }

    // --- Primitive read/write helpers ---
    private static byte ReadByte(ref SequenceReader<byte> r) { r.TryRead(out byte v); return v; }
    private static ushort ReadU2(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> b = stackalloc byte[2]; r.TryCopyTo(b); r.Advance(2);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(b) : BinaryPrimitives.ReadUInt16BigEndian(b);
    }
    private static uint ReadU4(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> b = stackalloc byte[4]; r.TryCopyTo(b); r.Advance(4);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt32LittleEndian(b) : BinaryPrimitives.ReadUInt32BigEndian(b);
    }
    private static ulong ReadU8(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> b = stackalloc byte[8]; r.TryCopyTo(b); r.Advance(8);
        return e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt64LittleEndian(b) : BinaryPrimitives.ReadUInt64BigEndian(b);
    }
    private static string ReadCn(ref SequenceReader<byte> r)
    {
        r.TryRead(out byte len);
        if (len == 0) return string.Empty;
        Span<byte> b = stackalloc byte[len]; r.TryCopyTo(b); r.Advance(len);
        return Encoding.ASCII.GetString(b);
    }
    private static string ReadCf(ref SequenceReader<byte> r, int length)
    {
        Span<byte> b = length <= 512 ? stackalloc byte[length] : new byte[length];
        r.TryCopyTo(b); r.Advance(length);
        return Encoding.ASCII.GetString(b).TrimEnd();
    }
    private static System.Collections.BitArray? ReadDn(ref SequenceReader<byte> r, Endianness e)
    {
        Span<byte> lb = stackalloc byte[2]; r.TryCopyTo(lb); r.Advance(2);
        ushort bitCount = e == Endianness.LittleEndian ? BinaryPrimitives.ReadUInt16LittleEndian(lb) : BinaryPrimitives.ReadUInt16BigEndian(lb);
        if (bitCount == 0) return null;
        int byteCount = (bitCount + 7) / 8;
        var buf = new byte[byteCount]; r.TryCopyTo(buf); r.Advance(byteCount);
        return new System.Collections.BitArray(buf) { Length = bitCount };
    }

    private static void WriteByte(IBufferWriter<byte> w, byte v) { var s = w.GetSpan(1); s[0] = v; w.Advance(1); }
    private static void WriteU2(IBufferWriter<byte> w, ushort v, Endianness e)
    {
        var s = w.GetSpan(2);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt16BigEndian(s, v);
        w.Advance(2);
    }
    private static void WriteU4(IBufferWriter<byte> w, uint v, Endianness e)
    {
        var s = w.GetSpan(4);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt32LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt32BigEndian(s, v);
        w.Advance(4);
    }
    private static void WriteU8(IBufferWriter<byte> w, ulong v, Endianness e)
    {
        var s = w.GetSpan(8);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt64LittleEndian(s, v);
        else BinaryPrimitives.WriteUInt64BigEndian(s, v);
        w.Advance(8);
    }
    private static void WriteCn(IBufferWriter<byte> w, string? v)
    {
        byte len = (byte)(v?.Length ?? 0); var s = w.GetSpan(1 + len); s[0] = len;
        if (len > 0) Encoding.ASCII.GetBytes(v!, s.Slice(1, len));
        w.Advance(1 + len);
    }
    private static void WriteCf(IBufferWriter<byte> w, string? v, int length)
    {
        var s = w.GetSpan(length); s.Slice(0, length).Fill((byte)' ');
        if (v != null) { int cl = Math.Min(v.Length, length); Encoding.ASCII.GetBytes(v.AsSpan(0, cl), s); }
        w.Advance(length);
    }
    private static void WriteDn(IBufferWriter<byte> w, System.Collections.BitArray? bits, Endianness e)
    {
        if (bits == null || bits.Length == 0)
        {
            var s = w.GetSpan(2); s[0] = 0; s[1] = 0; w.Advance(2);
            return;
        }
        ushort bitCount = (ushort)bits.Length;
        int byteCount = (bitCount + 7) / 8;
        var span = w.GetSpan(2 + byteCount);
        if (e == Endianness.LittleEndian) BinaryPrimitives.WriteUInt16LittleEndian(span, bitCount);
        else BinaryPrimitives.WriteUInt16BigEndian(span, bitCount);
        var bytes = new byte[byteCount]; bits.CopyTo(bytes, 0); bytes.AsSpan().CopyTo(span.Slice(2));
        w.Advance(2 + byteCount);
    }
}
