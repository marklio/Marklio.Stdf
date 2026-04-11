using System.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace Marklio.Stdf.Records;

/// <summary>
/// STR — Scan Test Record (15, 30).
/// V4-2007. Contains detailed scan test results. Hand-implemented because it uses
/// variable-width fields (U*f) whose byte sizes are determined by earlier fields
/// (CYC_SIZE, PMR_SIZE, CHN_SIZE, PAT_SIZE, BIT_SIZE, U1_SIZE, U2_SIZE, U3_SIZE, UTX_SIZE).
/// Supports continuation. Implements <see cref="ITestRecord"/>.
/// </summary>
public readonly record struct Str : IStdfRecord, ITestRecord
{
    static byte IStdfRecord.RecordType => 15;
    static byte IStdfRecord.RecordSubType => 30;

    /// <summary>
    /// Continuation flag. Bit 0: if set, this record continues in the next STR record.
    /// [STDF: CONT_FLG, B*1]
    /// </summary>
    public byte ContinuationFlag { get; init; }

    /// <summary>
    /// Test number.
    /// [STDF: TEST_NUM, U*4]
    /// </summary>
    public uint TestNumber { get; init; }

    /// <summary>
    /// Test head number.
    /// [STDF: HEAD_NUM, U*1]
    /// </summary>
    public byte HeadNumber { get; init; }

    /// <summary>
    /// Test site number.
    /// [STDF: SITE_NUM, U*1]
    /// </summary>
    public byte SiteNumber { get; init; }

    /// <summary>
    /// Index of the PSR record describing the pattern sequence.
    /// [STDF: PSR_REF, U*2]
    /// </summary>
    public ushort PsrReference { get; init; }

    /// <summary>
    /// Test flags (same encoding as PTR).
    /// [STDF: TEST_FLG, B*1]
    /// </summary>
    public byte TestFlags { get; init; }

    /// <summary>
    /// Type of data log.
    /// [STDF: LOG_TYP, C*n]
    /// </summary>
    public string? LogType { get; init; }

    /// <summary>
    /// Descriptive test text.
    /// [STDF: TEST_TXT, C*n]
    /// </summary>
    public string? TestText { get; init; }

    /// <summary>
    /// Alarm name or ID.
    /// [STDF: ALARM_ID, C*n]
    /// </summary>
    public string? AlarmId { get; init; }

    /// <summary>
    /// Program text.
    /// [STDF: PROG_TXT, C*n]
    /// </summary>
    public string? ProgramText { get; init; }

    /// <summary>
    /// Result text.
    /// [STDF: RSLT_TXT, C*n]
    /// </summary>
    public string? ResultText { get; init; }

    /// <summary>
    /// Z value.
    /// [STDF: Z_VAL, U*1]
    /// </summary>
    public byte? ZVal { get; init; }

    /// <summary>
    /// Fail/mask/unused flags.
    /// [STDF: FMU_FLG, B*1]
    /// </summary>
    public byte? FmuFlags { get; init; }

    /// <summary>
    /// Mask bitmap.
    /// [STDF: MASK_MAP, D*n]
    /// </summary>
    /// <remarks>
    /// Wire format is bit-count-prefixed (D*n), deserialized into a <see cref="System.Collections.BitArray"/>.
    /// </remarks>
    public System.Collections.BitArray? MaskMap { get; init; }

    /// <summary>
    /// Fail bitmap.
    /// [STDF: FAL_MAP, D*n]
    /// </summary>
    /// <remarks>
    /// Wire format is bit-count-prefixed (D*n), deserialized into a <see cref="System.Collections.BitArray"/>.
    /// </remarks>
    public System.Collections.BitArray? FailMap { get; init; }

    /// <summary>
    /// Total cycles logged.
    /// [STDF: CYC_CNT, U*8]
    /// </summary>
    public ulong? CycleCount { get; init; }

    /// <summary>
    /// Total fails logged.
    /// [STDF: TOTF_CNT, U*4]
    /// </summary>
    public uint? TotalFailCount { get; init; }

    /// <summary>
    /// Total entries logged.
    /// [STDF: TOTL_CNT, U*4]
    /// </summary>
    public uint? TotalLogCount { get; init; }

    /// <summary>
    /// Base cycle number offset.
    /// [STDF: CYC_BASE, U*8]
    /// </summary>
    public ulong? CycleBase { get; init; }

    /// <summary>
    /// Base bit number offset.
    /// [STDF: BIT_BASE, U*4]
    /// </summary>
    public uint? BitBase { get; init; }

    /// <summary>
    /// Number of conditions.
    /// [STDF: COND_CNT, U*2]
    /// </summary>
    public ushort? ConditionCount { get; init; }

    /// <summary>
    /// Number of limits.
    /// [STDF: LIM_CNT, U*2]
    /// </summary>
    public ushort? LimitCount { get; init; }

    /// <summary>
    /// Byte width for cycle offset fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: CYC_SIZE, U*1]
    /// </summary>
    public byte? CycleSize { get; init; }

    /// <summary>
    /// Byte width for PMR index fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: PMR_SIZE, U*1]
    /// </summary>
    public byte? PmrSize { get; init; }

    /// <summary>
    /// Byte width for chain number fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: CHN_SIZE, U*1]
    /// </summary>
    public byte? ChainSize { get; init; }

    /// <summary>
    /// Byte width for pattern number fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: PAT_SIZE, U*1]
    /// </summary>
    public byte? PatternSize { get; init; }

    /// <summary>
    /// Byte width for bit position fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: BIT_SIZE, U*1]
    /// </summary>
    public byte? BitSize { get; init; }

    /// <summary>
    /// Byte width for User1 fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: U1_SIZE, U*1]
    /// </summary>
    public byte? U1Size { get; init; }

    /// <summary>
    /// Byte width for User2 fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: U2_SIZE, U*1]
    /// </summary>
    public byte? U2Size { get; init; }

    /// <summary>
    /// Byte width for User3 fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: U3_SIZE, U*1]
    /// </summary>
    public byte? U3Size { get; init; }

    /// <summary>
    /// Byte width for UserText index fields (1/2/4/8).
    /// Set this when creating records from scratch to match the required encoding width.
    /// Preserved from deserialization for round-trip fidelity.
    /// [STDF: UTX_SIZE, U*1]
    /// </summary>
    public byte? UtxSize { get; init; }

    /// <summary>
    /// Starting capture index for this record.
    /// [STDF: CAP_BGN, U*2]
    /// </summary>
    public ushort? CapBegin { get; init; }

    /// <summary>
    /// Limit indexes (counted by LIM_CNT).
    /// [STDF: LIM_INDX, kxU*2]
    /// </summary>
    public ushort[]? LimitIndexes { get; init; }

    /// <summary>
    /// Limit spec values (counted by LIM_CNT).
    /// [STDF: LIM_SPEC, kxU*4]
    /// </summary>
    public uint[]? LimitSpecs { get; init; }

    /// <summary>
    /// Condition names (counted by COND_CNT).
    /// [STDF: COND_LST, kxC*n]
    /// </summary>
    public string[]? ConditionList { get; init; }

    /// <summary>
    /// Number of cycle offset entries in this segment. Derived from <see cref="CycleOffsets"/>.
    /// [STDF: CYC_CNT, U*2]
    /// </summary>
    public ushort? CycCnt => (ushort?)CycleOffsets?.Length;

    /// <summary>
    /// Cycle offset values.
    /// [STDF: CYC_OFST, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="CycleSize"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? CycleOffsets { get; init; }

    /// <summary>
    /// Number of PMR index entries. Derived from <see cref="PmrIndexes"/>.
    /// [STDF: PMR_CNT, U*2]
    /// </summary>
    public ushort? PmrCnt => (ushort?)PmrIndexes?.Length;

    /// <summary>
    /// PMR indexes.
    /// [STDF: PMR_INDX, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="PmrSize"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? PmrIndexes { get; init; }

    /// <summary>
    /// Number of chain number entries. Derived from <see cref="ChainNumbers"/>.
    /// [STDF: CHN_CNT, U*2]
    /// </summary>
    public ushort? ChnCnt => (ushort?)ChainNumbers?.Length;

    /// <summary>
    /// Chain numbers.
    /// [STDF: CHN_NUM, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="ChainSize"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? ChainNumbers { get; init; }

    /// <summary>
    /// Number of expected data bytes. Derived from <see cref="ExpectedData"/>.
    /// [STDF: EXP_CNT, U*2]
    /// </summary>
    public ushort? ExpCnt => (ushort?)ExpectedData?.Length;

    /// <summary>
    /// Expected compare data.
    /// [STDF: EXP_DATA, kxU*1]
    /// </summary>
    public byte[]? ExpectedData { get; init; }

    /// <summary>
    /// Number of captured data bytes. Derived from <see cref="CaptureData"/>.
    /// [STDF: CAP_CNT, U*2]
    /// </summary>
    public ushort? CapCnt => (ushort?)CaptureData?.Length;

    /// <summary>
    /// Captured data.
    /// [STDF: CAP_DATA, kxU*1]
    /// </summary>
    public byte[]? CaptureData { get; init; }

    /// <summary>
    /// Number of new data bytes. Derived from <see cref="NewData"/>.
    /// [STDF: NEW_CNT, U*2]
    /// </summary>
    public ushort? NewCnt => (ushort?)NewData?.Length;

    /// <summary>
    /// New (repaired) data.
    /// [STDF: NEW_DATA, kxU*1]
    /// </summary>
    public byte[]? NewData { get; init; }

    /// <summary>
    /// Number of pattern number entries. Derived from <see cref="PatternNumbers"/>.
    /// [STDF: PAT_CNT, U*2]
    /// </summary>
    public ushort? PatCnt => (ushort?)PatternNumbers?.Length;

    /// <summary>
    /// Pattern numbers.
    /// [STDF: PAT_NUM, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="PatternSize"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? PatternNumbers { get; init; }

    /// <summary>
    /// Number of bit position entries. Derived from <see cref="BitPositions"/>.
    /// [STDF: BPOS_CNT, U*2]
    /// </summary>
    public ushort? BposCnt => (ushort?)BitPositions?.Length;

    /// <summary>
    /// Bit positions.
    /// [STDF: BIT_POS, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="BitSize"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? BitPositions { get; init; }

    /// <summary>
    /// Number of User1 entries. Derived from <see cref="User1"/>.
    /// [STDF: USR1_CNT, U*2]
    /// </summary>
    public ushort? Usr1Cnt => (ushort?)User1?.Length;

    /// <summary>
    /// User-defined data 1.
    /// [STDF: USR1, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="U1Size"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? User1 { get; init; }

    /// <summary>
    /// Number of User2 entries. Derived from <see cref="User2"/>.
    /// [STDF: USR2_CNT, U*2]
    /// </summary>
    public ushort? Usr2Cnt => (ushort?)User2?.Length;

    /// <summary>
    /// User-defined data 2.
    /// [STDF: USR2, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="U2Size"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? User2 { get; init; }

    /// <summary>
    /// Number of User3 entries. Derived from <see cref="User3"/>.
    /// [STDF: USR3_CNT, U*2]
    /// </summary>
    public ushort? Usr3Cnt => (ushort?)User3?.Length;

    /// <summary>
    /// User-defined data 3.
    /// [STDF: USR3, kxU*f]
    /// </summary>
    /// <remarks>
    /// Variable-width field. Wire byte width per element is determined by <see cref="U3Size"/>. Always stored as <see langword="ulong"/>[] regardless of wire width.
    /// </remarks>
    public ulong[]? User3 { get; init; }

    /// <summary>
    /// Number of user text entries. Derived from <see cref="UserText"/>.
    /// [STDF: TXT_CNT, U*2]
    /// </summary>
    public ushort? TxtCnt => (ushort?)UserText?.Length;

    /// <summary>
    /// User text entries.
    /// [STDF: USER_TXT, kxC*n]
    /// </summary>
    public string[]? UserText { get; init; }

    /// <summary>Initializes a new instance of the <see cref="Str"/> record.</summary>
    public Str()
    {
        LogType = string.Empty;
        TestText = string.Empty;
        AlarmId = string.Empty;
        ProgramText = string.Empty;
        ResultText = string.Empty;
    }

    /// <summary>Deserializes a <see cref="Str"/> from the specified reader.</summary>
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
        r = ReadVarArray(ref reader, r, endianness, cycSize, (s, arr) => s with { CycleOffsets = arr });
        r = ReadVarArray(ref reader, r, endianness, pmrSize, (s, arr) => s with { PmrIndexes = arr });
        r = ReadVarArray(ref reader, r, endianness, chnSize, (s, arr) => s with { ChainNumbers = arr });

        // EXP_DATA: byte arrays
        r = ReadByteCountedArray(ref reader, r, endianness, (s, arr) => s with { ExpectedData = arr });
        r = ReadByteCountedArray(ref reader, r, endianness, (s, arr) => s with { CaptureData = arr });
        r = ReadByteCountedArray(ref reader, r, endianness, (s, arr) => s with { NewData = arr });

        r = ReadVarArray(ref reader, r, endianness, patSize, (s, arr) => s with { PatternNumbers = arr });
        r = ReadVarArray(ref reader, r, endianness, bitSize, (s, arr) => s with { BitPositions = arr });
        r = ReadVarArray(ref reader, r, endianness, u1Size, (s, arr) => s with { User1 = arr });
        r = ReadVarArray(ref reader, r, endianness, u2Size, (s, arr) => s with { User2 = arr });
        r = ReadVarArray(ref reader, r, endianness, u3Size, (s, arr) => s with { User3 = arr });

        // TXT_CNT + USER_TXT (C*f with UTX_SIZE determining fixed string length)
        if (reader.Remaining >= 2)
        {
            ushort txtCnt = ReadU2(ref reader, endianness);
            if (txtCnt > 0 && utxSize > 0 && reader.Remaining >= txtCnt * utxSize)
            {
                var txt = new string[txtCnt];
                for (int i = 0; i < txtCnt; i++) txt[i] = ReadCf(ref reader, utxSize);
                r = r with { UserText = txt };
            }
            else
            {
                r = r with { UserText = Array.Empty<string>() };
            }
        }

        return r;
    }

    /// <summary>Serializes this <see cref="Str"/> to the specified writer.</summary>
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

        WriteVarArray(writer, CycleOffsets, cycSz, endianness);
        WriteVarArray(writer, PmrIndexes, pmrSz, endianness);
        WriteVarArray(writer, ChainNumbers, chnSz, endianness);

        WriteByteCountedArray(writer, ExpectedData, endianness);
        WriteByteCountedArray(writer, CaptureData, endianness);
        WriteByteCountedArray(writer, NewData, endianness);

        WriteVarArray(writer, PatternNumbers, patSz, endianness);
        WriteVarArray(writer, BitPositions, bitSz, endianness);
        WriteVarArray(writer, User1, u1Sz, endianness);
        WriteVarArray(writer, User2, u2Sz, endianness);
        WriteVarArray(writer, User3, u3Sz, endianness);

        if (UserText == null) return;
        ushort txtCnt = (ushort)UserText.Length;
        if (txtCnt > 0 && utxSz == 0)
            throw new InvalidOperationException("UtxSize must be set when UserText is non-empty.");
        WriteU2(writer, txtCnt, endianness);
        if (txtCnt > 0)
            foreach (var v in UserText) WriteCf(writer, v, utxSz);
    }

    // --- Helper: read variable-width integer array ---
    private static Str ReadVarArray(
        ref SequenceReader<byte> reader, Str r, Endianness e, byte elemSize,
        Func<Str, ulong[], Str> setArr)
    {
        if (reader.Remaining < 2) return r;
        ushort cnt = ReadU2(ref reader, e);
        if (cnt > 0 && elemSize > 0 && reader.Remaining >= cnt * elemSize)
        {
            var arr = new ulong[cnt];
            for (int i = 0; i < cnt; i++) arr[i] = ReadUf(ref reader, e, elemSize);
            r = setArr(r, arr);
        }
        else
        {
            r = setArr(r, Array.Empty<ulong>());
        }
        return r;
    }

    private static Str ReadByteCountedArray(
        ref SequenceReader<byte> reader, Str r, Endianness e,
        Func<Str, byte[], Str> setArr)
    {
        if (reader.Remaining < 2) return r;
        ushort cnt = ReadU2(ref reader, e);
        if (cnt > 0 && reader.Remaining >= cnt)
        {
            var arr = new byte[cnt];
            for (int i = 0; i < cnt; i++) reader.TryRead(out arr[i]);
            r = setArr(r, arr);
        }
        else
        {
            r = setArr(r, Array.Empty<byte>());
        }
        return r;
    }

    // --- Helper: write variable-width integer array ---
    private static void WriteVarArray(IBufferWriter<byte> writer, ulong[]? arr, byte elemSize, Endianness e)
    {
        if (arr == null) return;
        if (arr.Length > 0 && elemSize == 0)
            throw new InvalidOperationException("Element size must be set when array is non-empty.");
        WriteU2(writer, (ushort)arr.Length, e);
        if (arr.Length > 0)
            foreach (var v in arr) WriteUf(writer, v, e, elemSize);
    }

    private static void WriteByteCountedArray(IBufferWriter<byte> writer, byte[]? arr, Endianness e)
    {
        if (arr == null) return;
        WriteU2(writer, (ushort)arr.Length, e);
        if (arr.Length > 0)
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
