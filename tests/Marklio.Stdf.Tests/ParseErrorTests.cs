using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class ParseErrorTests
{
    /// <summary>
    /// Builds a valid STDF (FAR + PIR) then truncates it mid-record.
    /// </summary>
    private static byte[] BuildTruncatedStdf()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR: recLen=2, type=0, sub=10, cpuType=2 (LE), version=4
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // PIR header says 2 payload bytes, but we only write 1
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); // missing second byte

        return ms.ToArray();
    }

    /// <summary>
    /// Builds STDF data where a corrupt header appears after valid records.
    /// The corrupt header claims a huge payload length that exceeds the remaining data.
    /// </summary>
    private static byte[] BuildCorruptHeaderStdf()
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        // FAR
        bw.Write((ushort)2); bw.Write((byte)0); bw.Write((byte)10);
        bw.Write((byte)2); bw.Write((byte)4);

        // Valid PIR
        bw.Write((ushort)2); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)1);

        // Corrupt record: known type (PIR) but recLen=5000 — way past end of data
        bw.Write((ushort)5000); bw.Write((byte)5); bw.Write((byte)10);
        bw.Write((byte)1); bw.Write((byte)2);

        return ms.ToArray();
    }

    [Fact]
    public void TruncatedFile_WithoutRecovery_ThrowsStdfParseException()
    {
        var data = BuildTruncatedStdf();

        var ex = Assert.Throws<StdfParseException>(() => StdfFile.Read(data).ToList());
        Assert.Contains("Truncated", ex.Message);
        Assert.True(ex.Offset >= 6, $"Expected offset >= 6 (past FAR), got {ex.Offset}");
    }

    [Fact]
    public void TruncatedFile_WithRecovery_DoesNotThrow()
    {
        var data = BuildTruncatedStdf();
        var opts = new StdfReaderOptions { RecoveryMode = true };

        // Should not throw — recovery mode tolerates truncation
        var records = StdfFile.Read(data, opts).ToList();
        Assert.NotEmpty(records);
        Assert.IsType<Far>(records[0]);
    }

    [Fact]
    public void CorruptHeader_WithoutRecovery_ThrowsStdfParseException()
    {
        var data = BuildCorruptHeaderStdf();

        var ex = Assert.Throws<StdfParseException>(() => StdfFile.Read(data).ToList());
        Assert.Contains("Truncated", ex.Message);
        Assert.True(ex.Offset > 0, $"Expected positive offset, got {ex.Offset}");
    }

    [Fact]
    public async Task TruncatedFile_Async_WithoutRecovery_ThrowsStdfParseException()
    {
        var data = BuildTruncatedStdf();
        var stream = new MemoryStream(data);

        var ex = await Assert.ThrowsAsync<StdfParseException>(async () =>
        {
            await foreach (var _ in StdfFile.ReadAsync(stream)) { }
        });
        Assert.Contains("Truncated", ex.Message);
    }

    [Fact]
    public async Task TruncatedFile_Async_WithRecovery_DoesNotThrow()
    {
        var data = BuildTruncatedStdf();
        var stream = new MemoryStream(data);
        var opts = new StdfReaderOptions { RecoveryMode = true };

        var records = new List<StdfRecord>();
        await foreach (var rec in StdfFile.ReadAsync(stream, opts))
            records.Add(rec);

        Assert.NotEmpty(records);
        Assert.IsType<Far>(records[0]);
    }

    [Fact]
    public void ValidFile_DoesNotThrow()
    {
        // Sanity check: valid files should not be affected
        var data = SyntheticStdf.SmallLe.Value;
        var records = StdfFile.Read(data).ToList();
        Assert.True(records.Count >= 3, "Valid file should parse normally");
    }
}
