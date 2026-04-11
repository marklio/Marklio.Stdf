using BenchmarkDotNet.Attributes;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Benchmarks;

[MemoryDiagnoser]

public class ReadBenchmarks
{
    private byte[] _leData = null!;
    private byte[] _beData = null!;

    [GlobalSetup]
    public void Setup()
    {
        _leData = SyntheticData.MediumLe;
        _beData = SyntheticData.MediumBe;
    }

    [Benchmark(Description = "Read medium LE (sync)")]
    public int ReadLe_Sync()
    {
        int count = 0;
        foreach (var rec in StdfFile.Read(_leData))
            count++;
        return count;
    }

    [Benchmark(Description = "Read medium BE (sync)")]
    public int ReadBe_Sync()
    {
        int count = 0;
        foreach (var rec in StdfFile.Read(_beData))
            count++;
        return count;
    }

    [Benchmark(Description = "Read medium LE (async)")]
    public async Task<int> ReadLe_Async()
    {
        int count = 0;
        using var ms = new MemoryStream(_leData, writable: false);
        await foreach (var rec in StdfFile.ReadAsync(ms))
            count++;
        return count;
    }

    [Benchmark(Description = "Read medium BE (async)")]
    public async Task<int> ReadBe_Async()
    {
        int count = 0;
        using var ms = new MemoryStream(_beData, writable: false);
        await foreach (var rec in StdfFile.ReadAsync(ms))
            count++;
        return count;
    }
}
