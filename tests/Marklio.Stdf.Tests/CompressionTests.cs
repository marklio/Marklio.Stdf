using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

public class CompressionTests
{
    [Theory]
    [InlineData(StdfCompression.Gzip)]
    [InlineData(StdfCompression.Bzip2)]
    public async Task AsyncRead_CompressedMatchesPlain(StdfCompression compression)
    {
        var plainData = SyntheticStdf.MediumLe.Value;
        var compressed = Compress(plainData, compression);

        using var plainStream = new MemoryStream(plainData);
        var plainRecords = await StdfFile.ReadAsync(plainStream).ToListAsync();

        using var compStream = new MemoryStream(compressed);
        var compRecords = await StdfFile.ReadAsync(compStream).ToListAsync();

        Assert.Equal(plainRecords.Count, compRecords.Count);
        for (int i = 0; i < plainRecords.Count; i++)
        {
            Assert.Equal(plainRecords[i].RecordType, compRecords[i].RecordType);
            Assert.Equal(plainRecords[i].RecordSubType, compRecords[i].RecordSubType);
        }
    }

    [Theory]
    [InlineData(StdfCompression.Gzip)]
    [InlineData(StdfCompression.Bzip2)]
    public void SyncRead_CompressedMatchesPlain(StdfCompression compression)
    {
        var plainData = SyntheticStdf.MediumLe.Value;
        var compressed = Compress(plainData, compression);

        var plainRecords = StdfFile.Read(plainData).ToList();
        var compRecords = StdfFile.Read(compressed).ToList();

        Assert.Equal(plainRecords.Count, compRecords.Count);
    }

    [Theory]
    [InlineData(StdfCompression.Gzip)]
    [InlineData(StdfCompression.Bzip2)]
    public async Task WriteCompressed_ThenRead_RoundTrips(StdfCompression compression)
    {
        var originalRecords = StdfFile.Read(SyntheticStdf.MediumLe.Value).ToList();

        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var writer = await StdfFile.OpenWriteAsync(tempPath, new StdfWriterOptions
            {
                Endianness = Endianness.LittleEndian,
                Compression = compression,
            }))
            {
                foreach (var rec in originalRecords)
                    await writer.WriteAsync(rec);
            }

            // Verify the temp file starts with compression magic bytes
            var header = new byte[2];
            using (var fs = File.OpenRead(tempPath))
                fs.Read(header);

            if (compression == StdfCompression.Gzip)
            {
                Assert.Equal(0x1F, header[0]);
                Assert.Equal(0x8B, header[1]);
            }
            else
            {
                Assert.Equal((byte)'B', header[0]);
                Assert.Equal((byte)'Z', header[1]);
            }

            // Read back and verify record count matches
            var reReadRecords = await StdfFile.ReadAsync(tempPath).ToListAsync();
            Assert.Equal(originalRecords.Count, reReadRecords.Count);

            Assert.Equal(originalRecords[0].RecordType, reReadRecords[0].RecordType);
            Assert.Equal(originalRecords[^1].RecordType, reReadRecords[^1].RecordType);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task BigEndian_CompressedRoundTrips()
    {
        var originalRecords = StdfFile.Read(SyntheticStdf.MediumBe.Value).ToList();

        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var writer = await StdfFile.OpenWriteAsync(tempPath, new StdfWriterOptions
            {
                Endianness = Endianness.BigEndian,
                Compression = StdfCompression.Gzip,
            }))
            {
                foreach (var rec in originalRecords)
                    await writer.WriteAsync(rec);
            }

            var reRead = await StdfFile.ReadAsync(tempPath).ToListAsync();
            Assert.Equal(originalRecords.Count, reRead.Count);
            Assert.True(reRead[0].Is<Far>(out var far));
            Assert.Equal((byte)1, far.CpuType); // big-endian preserved
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    private static byte[] Compress(byte[] data, StdfCompression compression)
    {
        using var ms = new MemoryStream();
        using (var compStream = IO.CompressionHelper.WrapForWriting(ms, compression))
        {
            compStream.Write(data, 0, data.Length);
        }
        return ms.ToArray();
    }
}

internal static class AsyncEnumerableExtensions
{
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
            list.Add(item);
        return list;
    }
}
