using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class RoundTripTests
{
    [Fact]
    public void SyncRead_RoundTrip_ByteExact()
    {
        // Build a minimal STDF file in memory: FAR + PIR + PRR
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        // FAR record: header(2+2+1+1=4) but STDF header = REC_LEN(2) + REC_TYP(1) + REC_SUB(1) = 4 total
        WriteRecord(bw, 0, 10, [0x02, 0x04]);  // FAR: CPU=2(LE), Version=4

        // PIR record
        WriteRecord(bw, 5, 10, [0x01, 0x01]);  // Head=1, Site=1

        // PRR record
        WriteRecord(bw, 5, 20, [
            0x01,                   // HeadNumber
            0x01,                   // SiteNumber
            0x00,                   // PartFlag
            0x0A, 0x00,             // NumTestsExecuted = 10
            0x01, 0x00,             // HardwareBin = 1
        ]);

        var original = ms.ToArray();

        // Read synchronously from memory
        var records = StdfFile.Read(original).ToList();

        Assert.Equal(3, records.Count);
        var far = Assert.IsType<Far>(records[0]);
        Assert.Equal(2, far.CpuType);
        Assert.Equal(4, far.StdfVersion);

        var pir = Assert.IsType<Pir>(records[1]);
        Assert.Equal(1, pir.HeadNumber);
        Assert.Equal(1, pir.SiteNumber);

        var prr = Assert.IsType<Prr>(records[2]);
        Assert.Equal(1, prr.HeadNumber);
        Assert.Equal(10, prr.NumTestsExecuted);
        Assert.Equal(1, prr.HardwareBin);

        // Write back and verify byte-exact round-trip
        var output = new MemoryStream();
        var obw = new BinaryWriter(output);
        foreach (var record in records)
        {
            var payload = new System.Buffers.ArrayBufferWriter<byte>();
            record.Serialize(payload, Endianness.LittleEndian);

            // Write header
            obw.Write((ushort)payload.WrittenCount);  // REC_LEN
            obw.Write(record.RecordType);              // REC_TYP
            obw.Write(record.RecordSubType);           // REC_SUB
            obw.Write(payload.WrittenSpan);            // payload
        }

        Assert.Equal(original, output.ToArray());
    }

    [Fact]
    public async Task AsyncRead_RoundTrip_ViaFile()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            // Build a minimal STDF file
            await using (var fs = File.Create(tempPath))
            await using (var bw = new BinaryWriter(fs))
            {
                WriteRecord(bw, 0, 10, [0x02, 0x04]);  // FAR
                WriteRecord(bw, 50, 30, [0x05, (byte)'H', (byte)'e', (byte)'l', (byte)'l', (byte)'o']); // DTR "Hello"
            }

            // Read back using async API
            var records = new List<StdfRecord>();
            await foreach (var rec in StdfFile.ReadAsync(tempPath))
                records.Add(rec);

            Assert.Equal(2, records.Count);
            Assert.IsType<Far>(records[0]);
            var dtr = Assert.IsType<Dtr>(records[1]);
            Assert.Equal("Hello", dtr.TextData);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task WriteAsync_ProducesValidStdf()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            // Write using StdfFile API
            await using (var writer = await StdfFile.OpenWriteAsync(tempPath))
            {
                await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });
                await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = 1 });
                await writer.WriteAsync(new Prr
                {
                    HeadNumber = 1,
                    SiteNumber = 1,
                    PartFlag = 0,
                    NumTestsExecuted = 5,
                    HardwareBin = 1,
                });
            }

            // Read back and verify
            var records = new List<StdfRecord>();
            await foreach (var rec in StdfFile.ReadAsync(tempPath))
                records.Add(rec);

            Assert.Equal(3, records.Count);
            var far = Assert.IsType<Far>(records[0]);
            Assert.Equal(2, far.CpuType);
            var prr = Assert.IsType<Prr>(records[2]);
            Assert.Equal(5, prr.NumTestsExecuted);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public void UnknownRecord_PreservedInRoundTrip()
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        WriteRecord(bw, 0, 10, [0x02, 0x04]);       // FAR
        WriteRecord(bw, 99, 99, [0xDE, 0xAD, 0xBE]); // Unknown record

        var original = ms.ToArray();
        var records = StdfFile.Read(original).ToList();

        Assert.Equal(2, records.Count);
        var unknown = Assert.IsType<UnknownRecord>(records[1]);
        Assert.Equal(99, unknown.RecordType);
        Assert.Equal(99, unknown.RecordSubType);
        Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE }, unknown.RawData.ToArray());
    }

    private static void WriteRecord(BinaryWriter bw, byte recType, byte recSub, byte[] payload)
    {
        bw.Write((ushort)payload.Length); // REC_LEN
        bw.Write(recType);               // REC_TYP
        bw.Write(recSub);                // REC_SUB
        bw.Write(payload);               // payload
    }
}
