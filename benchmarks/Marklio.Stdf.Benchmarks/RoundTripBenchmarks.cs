using BenchmarkDotNet.Attributes;
using Marklio.Stdf;

namespace Marklio.Stdf.Benchmarks;

[MemoryDiagnoser]

public class RoundTripBenchmarks
{
    private byte[] _leData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _leData = SyntheticData.MediumLe;
    }

    [Benchmark(Description = "Round-trip medium LE (read + write)")]
    public async Task<long> RoundTripLe()
    {
        var records = StdfFile.Read(_leData);

        using var ms = new MemoryStream(_leData.Length);
        await using var writer = StdfFile.OpenWrite(ms, new StdfWriterOptions
        {
            Endianness = Endianness.LittleEndian,
        });
        foreach (var rec in records)
            await writer.WriteAsync(rec);
        return ms.Length;
    }
}
