using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class StreamOwnershipTests
{
    /// <summary>
    /// Builds a minimal valid STDF byte array (FAR + PIR + PRR) using BinaryWriter.
    /// </summary>
    private static byte[] BuildMinimalStdf()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR record: CpuType=2 (LE), StdfVersion=4
        WriteRecord(bw, 0, 10, [0x02, 0x04]);

        // PIR record: Head=1, Site=1
        WriteRecord(bw, 5, 10, [0x01, 0x01]);

        // PRR record
        WriteRecord(bw, 5, 20, [
            0x01,           // HeadNumber
            0x01,           // SiteNumber
            0x00,           // PartFlag
            0x01, 0x00,     // NumTestsExecuted = 1
            0x01, 0x00,     // HardwareBin = 1
        ]);

        return ms.ToArray();
    }

    private static void WriteRecord(BinaryWriter bw, byte recType, byte recSub, byte[] payload)
    {
        bw.Write((ushort)payload.Length);
        bw.Write(recType);
        bw.Write(recSub);
        bw.Write(payload);
    }

    [Fact]
    public async Task CallerStreamRemainsOpenAfterRead()
    {
        var data = BuildMinimalStdf();
        var ms = new MemoryStream(data);

        var records = new List<StdfRecord>();
        await foreach (var rec in StdfFile.ReadAsync(ms))
            records.Add(rec);

        Assert.True(records.Count > 0);
        Assert.True(ms.CanRead, "Caller's stream should remain open after full enumeration");
    }

    [Fact]
    public async Task CallerStreamRemainsOpenAfterEarlyExit()
    {
        var data = BuildMinimalStdf();
        var ms = new MemoryStream(data);

        await foreach (var rec in StdfFile.ReadAsync(ms))
        {
            break; // abandon after first record
        }

        Assert.True(ms.CanRead, "Caller's stream should remain open after early exit");
    }

    [Fact]
    public async Task CallerStreamRemainsOpenAfterWrite()
    {
        var ms = new MemoryStream();

        await using (var writer = StdfFile.OpenWrite(ms))
        {
            await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });
            await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = 1 });
            await writer.WriteAsync(new Prr
            {
                HeadNumber = 1,
                SiteNumber = 1,
                PartFlag = 0,
                NumTestsExecuted = 1,
                HardwareBin = 1,
            });
        }

        Assert.True(ms.CanRead, "Caller's stream should remain open after writer is disposed");
        Assert.True(ms.Length > 0, "Writer should have produced data");
    }

    [Fact]
    public async Task PathBasedReadDisposesCleanly()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            // Write a valid STDF file
            await File.WriteAllBytesAsync(tempPath, BuildMinimalStdf());

            // Read via path — exercises leaveOpen:false and owned-stream cleanup.
            // Should not throw (verifies no double-dispose).
            var records = new List<StdfRecord>();
            await foreach (var rec in StdfFile.ReadAsync(tempPath))
                records.Add(rec);

            Assert.True(records.Count > 0);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
