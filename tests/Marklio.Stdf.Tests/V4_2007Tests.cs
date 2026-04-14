using System.Buffers;
using System.Collections;
using Marklio.Stdf;
using Marklio.Stdf.Records;
using Xunit;

namespace Marklio.Stdf.Tests;

/// <summary>
/// Tests for STDF V4-2007 records: VUR, PSR, NMR, CNR, SSR, CDR, STR, and GDR registry.
/// </summary>
public class V4_2007Tests
{
    private static T RoundTrip<T>(T record, Endianness endianness = Endianness.LittleEndian) where T : StdfRecord
    {
        var buffer = new ArrayBufferWriter<byte>();
        record.Serialize(buffer, endianness);
        var bytes = buffer.WrittenSpan.ToArray();
        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        // SequenceReader is ref struct — call Deserialize directly per type
        return DeserializeFromBytes<T>(bytes, endianness);
    }

    private static T DeserializeFromBytes<T>(byte[] bytes, Endianness endianness = Endianness.LittleEndian) where T : StdfRecord
    {
        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        // Use dynamic dispatch via the static Deserialize method on each type
        // Since SequenceReader<byte> is a ref struct, we can't use reflection.
        // Instead, use a type switch for known types.
        return DeserializeSwitch<T>(ref reader, endianness);
    }

    private static T DeserializeSwitch<T>(ref SequenceReader<byte> reader, Endianness endianness) where T : StdfRecord
    {
        object result = typeof(T).Name switch
        {
            nameof(Vur) => Vur.Deserialize(ref reader, endianness),
            nameof(Psr) => Psr.Deserialize(ref reader, endianness),
            nameof(Nmr) => Nmr.Deserialize(ref reader, endianness),
            nameof(Cnr) => Cnr.Deserialize(ref reader, endianness),
            nameof(Ssr) => Ssr.Deserialize(ref reader, endianness),
            nameof(Cdr) => Cdr.Deserialize(ref reader, endianness),
            nameof(Str) => Str.Deserialize(ref reader, endianness),
            nameof(Gdr) => Gdr.Deserialize(ref reader, endianness),
            _ => throw new NotSupportedException($"No deserialization for {typeof(T).Name}"),
        };
        return (T)result;
    }

    [Fact]
    public void Vur_RoundTrip()
    {
        var vur = new Vur
        {
            UpdateNames = ["V4-2007", "SCAN-2007"],
        };

        var buffer = new ArrayBufferWriter<byte>();
        vur.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        // VUR wire format: count (U*1=2), then two C*n strings
        Assert.Equal(2, bytes[0]); // count
        Assert.Equal(7, bytes[1]); // "V4-2007" length
        Assert.Equal((byte)'V', bytes[2]);

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Vur.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.NotNull(result.UpdateNames);
        Assert.Equal(2, result.UpdateNames!.Length);
        Assert.Equal("V4-2007", result.UpdateNames[0]);
        Assert.Equal("SCAN-2007", result.UpdateNames[1]);
    }

    [Fact]
    public void Psr_RoundTrip_WithU8Arrays()
    {
        var psr = new Psr
        {
            ContinuationFlag = 0,
            PsrIndex = 1,
            PsrName = "Scan1",
            OptionalFlags = 0xFF,
            TotalPatternCount = 3,
            PatternBegin = [100UL, 200UL, 300UL],
            PatternEnd = [199UL, 299UL, 399UL],
            PatternFiles = ["file1.pat", "file2.pat", "file3.pat"],
            PatternLabels = ["label1", "label2", "label3"],
            FileUids = ["uid1", "uid2", "uid3"],
            AtpgDescriptions = ["desc1", "desc2", "desc3"],
            SourceIds = ["src1", "src2", "src3"],
        };

        var buffer = new ArrayBufferWriter<byte>();
        psr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Psr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(0, result.ContinuationFlag);
        Assert.Equal((ushort)1, result.PsrIndex);
        Assert.Equal("Scan1", result.PsrName);
        Assert.Equal((byte)0xFF, result.OptionalFlags);
        Assert.Equal((ushort)3, result.TotalPatternCount);
        Assert.NotNull(result.PatternBegin);
        Assert.Equal(3, result.PatternBegin!.Length);
        Assert.Equal(100UL, result.PatternBegin[0]);
        Assert.Equal(200UL, result.PatternBegin[1]);
        Assert.Equal(300UL, result.PatternBegin[2]);
        Assert.Equal([199UL, 299UL, 399UL], result.PatternEnd!);
        Assert.Equal(["file1.pat", "file2.pat", "file3.pat"], result.PatternFiles!);
    }

    [Fact]
    public void Psr_ByteExact_RoundTrip()
    {
        var psr = new Psr
        {
            ContinuationFlag = 0,
            PsrIndex = 42,
            PsrName = "Test",
            OptionalFlags = 0,
            TotalPatternCount = 1,
            PatternBegin = [0UL],
            PatternEnd = [99UL],
            PatternFiles = ["test.pat"],
            PatternLabels = ["lbl"],
            FileUids = ["u1"],
            AtpgDescriptions = ["d1"],
            SourceIds = ["s1"],
        };

        var buffer1 = new ArrayBufferWriter<byte>();
        psr.Serialize(buffer1, Endianness.LittleEndian);
        var bytes1 = buffer1.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes1);
        var reader = new SequenceReader<byte>(seq);
        var deserialized = Psr.Deserialize(ref reader, Endianness.LittleEndian);

        var buffer2 = new ArrayBufferWriter<byte>();
        deserialized.Serialize(buffer2, Endianness.LittleEndian);
        var bytes2 = buffer2.WrittenSpan.ToArray();

        Assert.Equal(bytes1, bytes2);
    }

    [Fact]
    public void Nmr_RoundTrip()
    {
        var nmr = new Nmr
        {
            ContinuationFlag = 0,
            TotalMapCount = 2,
            PmrIndexes = [10, 20],
            AtpgNames = ["chain_a", "chain_b"],
        };

        var buffer = new ArrayBufferWriter<byte>();
        nmr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Nmr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(0, result.ContinuationFlag);
        Assert.Equal((ushort)2, result.TotalMapCount);
        Assert.Equal([10, 20], result.PmrIndexes);
        Assert.Equal(["chain_a", "chain_b"], result.AtpgNames!);
    }

    [Fact]
    public void Cnr_RoundTrip_WithSnString()
    {
        var cnr = new Cnr
        {
            ChainNumber = 5,
            BitPosition = 1024,
            CellName = "scan_cell_ff_q",
        };

        var buffer = new ArrayBufferWriter<byte>();
        cnr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        // Verify S*n encoding: 2-byte length prefix
        // First 2 bytes: ChainNumber (LE: 0x05, 0x00)
        // Next 4 bytes: BitPosition (LE: 0x00, 0x04, 0x00, 0x00)
        // Then S*n: 2-byte length (14, 0x00) + "scan_cell_ff_q"
        int snOffset = 2 + 4; // after U*2 + U*4
        Assert.Equal(14, bytes[snOffset]); // low byte of length
        Assert.Equal(0, bytes[snOffset + 1]); // high byte of length

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Cnr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal((ushort)5, result.ChainNumber);
        Assert.Equal(1024u, result.BitPosition);
        Assert.Equal("scan_cell_ff_q", result.CellName);
    }

    [Fact]
    public void Ssr_RoundTrip()
    {
        var ssr = new Ssr
        {
            SsrName = "MainStructure",
            ChainList = [1, 2, 3, 4],
        };

        var buffer = new ArrayBufferWriter<byte>();
        ssr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Ssr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal("MainStructure", result.SsrName);
        Assert.Equal([1, 2, 3, 4], result.ChainList);
    }

    [Fact]
    public void Cdr_RoundTrip_MultipleCountGroups()
    {
        var cdr = new Cdr
        {
            ContinuationFlag = 0,
            CdrIndex = 7,
            ChainName = "chain0",
            ChainLength = 512,
            ScanInPin = 100,
            ScanOutPin = 200,
            MasterClocks = [10, 11],
            SlaveClocks = [20],
            InversionValue = 1,
            CellList = ["cell_a", "cell_b", "cell_c"],
        };

        var buffer = new ArrayBufferWriter<byte>();
        cdr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Cdr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(0, result.ContinuationFlag);
        Assert.Equal((ushort)7, result.CdrIndex);
        Assert.Equal("chain0", result.ChainName);
        Assert.Equal(512u, result.ChainLength);
        Assert.Equal((ushort)100, result.ScanInPin);
        Assert.Equal((ushort)200, result.ScanOutPin);
        Assert.Equal([10, 11], result.MasterClocks);
        Assert.Equal([20], result.SlaveClocks);
        Assert.Equal((byte)1, result.InversionValue);
        Assert.Equal(["cell_a", "cell_b", "cell_c"], result.CellList!);
    }

    [Fact]
    public void Cdr_ByteExact_RoundTrip()
    {
        var cdr = new Cdr
        {
            ContinuationFlag = 0,
            CdrIndex = 1,
            ChainName = "ch",
            ChainLength = 64,
            ScanInPin = 5,
            ScanOutPin = 6,
            MasterClocks = [1],
            SlaveClocks = [2],
            InversionValue = 0,
            CellList = ["x"],
        };

        var buffer1 = new ArrayBufferWriter<byte>();
        cdr.Serialize(buffer1, Endianness.LittleEndian);
        var bytes1 = buffer1.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes1);
        var reader = new SequenceReader<byte>(seq);
        var result = Cdr.Deserialize(ref reader, Endianness.LittleEndian);

        var buffer2 = new ArrayBufferWriter<byte>();
        result.Serialize(buffer2, Endianness.LittleEndian);
        var bytes2 = buffer2.WrittenSpan.ToArray();

        Assert.Equal(bytes1, bytes2);
    }

    [Fact]
    public void Str_BasicRoundTrip()
    {
        var str = new Str
        {
            ContinuationFlag = 0,
            TestNumber = 1001,
            HeadNumber = 1,
            SiteNumber = 0,
            PsrReference = 42,
            TestFlags = 0x80,
            LogType = "SCAN",
            TestText = "Test1",
            AlarmId = "",
            ProgramText = "prog",
            ResultText = "pass",
            ZVal = 0,
            FmuFlags = 0,
            MaskMap = null,
            FailMap = null,
            CycleCount = 1000UL,
            TotalFailCount = 5,
            TotalLogCount = 10,
            CycleBase = 0UL,
            BitBase = 0,
            ConditionCount = 0,
            LimitCount = 0,
            CycleSize = 4,
            PmrSize = 2,
            ChainSize = 2,
            PatternSize = 4,
            BitSize = 4,
            U1Size = 1,
            U2Size = 1,
            U3Size = 1,
            UtxSize = 0,
            CapBegin = 0,
        };

        var buffer = new ArrayBufferWriter<byte>();
        str.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Str.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(1001u, result.TestNumber);
        Assert.Equal((byte)1, result.HeadNumber);
        Assert.Equal((ushort)42, result.PsrReference);
        Assert.Equal("SCAN", result.LogType);
        Assert.Equal(1000UL, result.CycleCount);
        Assert.Equal(5u, result.TotalFailCount);
    }

    [Fact]
    public void Gdr_RoundTrip_ViaRecordRegistry()
    {
        // Build GDR binary: count=2, field1=U1(42), field2=Cn("hello")
        var buffer = new ArrayBufferWriter<byte>();
        var gdr = new Gdr
        {
            Fields =
            [
                new GdrField { Type = GdrFieldType.U1, Value = (byte)42 },
                new GdrField { Type = GdrFieldType.Cn, Value = "hello" },
            ],
        };
        gdr.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Gdr.Deserialize(ref reader, Endianness.LittleEndian);

        Assert.Equal(2, result.Fields.Length);
        Assert.Equal(GdrFieldType.U1, result.Fields[0].Type);
        Assert.Equal((byte)42, result.Fields[0].Value);
        Assert.Equal(GdrFieldType.Cn, result.Fields[1].Type);
        Assert.Equal("hello", result.Fields[1].Value);
    }

    [Fact]
    public async Task Gdr_Discovered_ByReader()
    {
        // Build a minimal STDF stream: FAR + GDR
        var buffer = new ArrayBufferWriter<byte>();
        var endianness = Endianness.LittleEndian;

        // FAR header: REC_LEN=2, REC_TYP=0, REC_SUB=10
        var hdr = buffer.GetSpan(4);
        hdr[0] = 2; hdr[1] = 0; hdr[2] = 0; hdr[3] = 10;
        buffer.Advance(4);
        var far = new Far { CpuType = 2, StdfVersion = 4 };
        far.Serialize(buffer, endianness);

        // GDR header: REC_TYP=50, REC_SUB=10
        var gdr = new Gdr
        {
            Fields = [new GdrField { Type = GdrFieldType.U1, Value = (byte)99 }],
        };
        var gdrBuf = new ArrayBufferWriter<byte>();
        gdr.Serialize(gdrBuf, endianness);
        ushort gdrLen = (ushort)gdrBuf.WrittenCount;

        var gdrHdr = buffer.GetSpan(4);
        gdrHdr[0] = (byte)(gdrLen & 0xFF); gdrHdr[1] = (byte)(gdrLen >> 8);
        gdrHdr[2] = 50; gdrHdr[3] = 10;
        buffer.Advance(4);
        gdrBuf.WrittenSpan.CopyTo(buffer.GetSpan(gdrLen));
        buffer.Advance(gdrLen);

        var records = new List<StdfRecord>();
        await foreach (var rec in StdfFile.ReadAsync(new MemoryStream(buffer.WrittenSpan.ToArray())))
            records.Add(rec);

        Assert.Equal(2, records.Count);
        Assert.IsType<Far>(records[0]);
        Assert.IsType<Gdr>(records[1]);
        var resultGdr = (Gdr)records[1];
        Assert.Single(resultGdr.Fields);
        Assert.Equal((byte)99, resultGdr.Fields[0].Value);
    }

    [Fact]
    public void Vur_Empty_RoundTrip()
    {
        var vur = new Vur { UpdateNames = [] };

        var buffer = new ArrayBufferWriter<byte>();
        vur.Serialize(buffer, Endianness.LittleEndian);
        var bytes = buffer.WrittenSpan.ToArray();

        Assert.Single(bytes); // just the count byte = 0

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Vur.Deserialize(ref reader, Endianness.LittleEndian);

        // Empty array: count is 0, so array stays null (count > 0 check fails)
        Assert.Null(result.UpdateNames);
    }

    [Fact]
    public void BigEndian_Psr_RoundTrip()
    {
        var psr = new Psr
        {
            ContinuationFlag = 0,
            PsrIndex = 0x1234,
            PsrName = "BE",
            OptionalFlags = 0,
            TotalPatternCount = 1,
            PatternBegin = [0x0102030405060708UL],
            PatternEnd = [0x0807060504030201UL],
            PatternFiles = ["f"],
            PatternLabels = ["l"],
            FileUids = ["u"],
            AtpgDescriptions = ["d"],
            SourceIds = ["s"],
        };

        var buf1 = new ArrayBufferWriter<byte>();
        psr.Serialize(buf1, Endianness.BigEndian);
        var bytes = buf1.WrittenSpan.ToArray();

        var seq = new ReadOnlySequence<byte>(bytes);
        var reader = new SequenceReader<byte>(seq);
        var result = Psr.Deserialize(ref reader, Endianness.BigEndian);

        Assert.Equal((ushort)0x1234, result.PsrIndex);
        Assert.Equal(0x0102030405060708UL, result.PatternBegin![0]);
        Assert.Equal(0x0807060504030201UL, result.PatternEnd![0]);
    }
}
