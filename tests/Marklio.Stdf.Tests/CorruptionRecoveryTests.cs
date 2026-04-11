using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class CorruptionRecoveryTests
{
    /// <summary>
    /// Builds minimal valid STDF bytes: FAR + a few PIR/PRR records.
    /// </summary>
    private static byte[] BuildValidStdf()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR: recLen=2, type=0, sub=10, cpuType=2 (LE), version=4
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // PIR: recLen=2, type=5, sub=10, head=1, site=1
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)1);

        // PRR: recLen=2+, minimal truncated is fine
        // Actually PRR has required fields. Let's just add another PIR.
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)2);

        return ms.ToArray();
    }

    [Fact]
    public void NormalReading_NoRecoveryNeeded()
    {
        var data = BuildValidStdf();
        var records = StdfFile.Read(data).ToList();
        Assert.Equal(3, records.Count);
        Assert.True(records[0].Is<Far>(out _));
        Assert.True(records[1].Is<Pir>(out _));
        Assert.True(records[2].Is<Pir>(out _));
    }

    [Fact]
    public void RecoveryMode_SkipsCorruptedBytes()
    {
        // Build data: FAR + garbage bytes + PIR
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR: recLen=2, type=0, sub=10
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // Inject 10 bytes of garbage
        bw.Write(new byte[] { 0xFF, 0xFE, 0xFD, 0xFC, 0xFB, 0xFA, 0xF9, 0xF8, 0xF7, 0xF6 });

        // PIR: recLen=2, type=5, sub=10
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)3);

        // Another PIR to enable double-validation of the first PIR
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)4);

        var data = ms.ToArray();

        // Without recovery: reads FAR, then hits garbage, throws
        Assert.Throws<StdfParseException>(() => StdfFile.Read(data).ToList());

        // With recovery: should find the PIR after garbage
        var recoveryEvents = new List<StdfRecoveryEvent>();
        var opts = new StdfReaderOptions
        {
            RecoveryMode = true,
            OnRecovery = e => recoveryEvents.Add(e)
        };
        var recoveredRecords = StdfFile.Read(data, opts).ToList();
        Assert.True(recoveredRecords.Count >= 3, $"Expected at least 3 records, got {recoveredRecords.Count}");
        Assert.True(recoveredRecords[0].Is<Far>(out _));

        // Verify PIRs were recovered
        var pirs = recoveredRecords.Where(r => r.Is<Pir>(out _)).ToList();
        Assert.True(pirs.Count >= 1, "Should recover at least one PIR after corruption");

        // Recovery event should have been reported
        Assert.True(recoveryEvents.Count > 0, "Expected recovery events to be reported");
    }

    [Fact]
    public void RecoveryMode_CorruptedRecLen()
    {
        // Build data: FAR + PIR with corrupted recLen + another PIR
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // PIR with massive recLen that extends past file
        bw.Write((ushort)9999); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)1);

        // Valid PIR after the corrupt one (at a known offset)
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)2);

        // Another PIR for double-validation
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)3);

        var data = ms.ToArray();

        var opts = new StdfReaderOptions { RecoveryMode = true };
        var records = StdfFile.Read(data, opts).ToList();

        Assert.True(records[0].Is<Far>(out _));
        // Should recover at least one PIR
        var pirs = records.Where(r => r.Is<Pir>(out _)).ToList();
        Assert.True(pirs.Count >= 1, $"Expected PIRs after recovery, got {pirs.Count}");
    }

    [Fact]
    public async Task AsyncRecoveryMode_SkipsCorruptedBytes()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // Garbage
        bw.Write(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0xCA, 0xFE });

        // PIR
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)5);

        // Another PIR for double-validation
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)6);

        var data = ms.ToArray();
        var stream = new MemoryStream(data);

        var opts = new StdfReaderOptions { RecoveryMode = true };
        var records = new List<StdfRecord>();
        await foreach (var rec in StdfFile.ReadAsync(stream, opts))
            records.Add(rec);

        Assert.True(records[0].Is<Far>(out _));
        var pirs = records.Where(r => r.Is<Pir>(out _)).ToList();
        Assert.True(pirs.Count >= 1, "Should recover PIRs after corruption in async mode");
    }

    [Fact]
    public void ValidFile_WithRecoveryMode_NoRecoveryTriggered()
    {
        // Verify recovery mode doesn't break valid files
        var data = SyntheticStdf.MediumLe.Value;
        var events = new List<StdfRecoveryEvent>();

        var opts = new StdfReaderOptions
        {
            RecoveryMode = true,
            OnRecovery = e => events.Add(e)
        };
        var records = StdfFile.Read(data, opts).ToList();

        Assert.NotEmpty(records);
        Assert.Empty(events); // valid file should trigger no recovery
    }
}
