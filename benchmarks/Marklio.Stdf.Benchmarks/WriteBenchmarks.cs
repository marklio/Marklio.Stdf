using BenchmarkDotNet.Attributes;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Benchmarks;

[MemoryDiagnoser]

public class WriteBenchmarks
{
    private List<StdfRecord> _leRecords = null!;

    [GlobalSetup]
    public void Setup()
    {
        _leRecords = StdfFile.Read(SyntheticData.MediumLe).ToList();
    }

    [Benchmark(Description = "Write medium LE records")]
    public async Task<long> WriteLe()
    {
        using var ms = new MemoryStream();
        await using var writer = StdfFile.OpenWrite(ms, new StdfWriterOptions
        {
            Endianness = Endianness.LittleEndian,
        });
        foreach (var rec in _leRecords)
            await writer.WriteAsync(rec);
        return ms.Length;
    }
}
