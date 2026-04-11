using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Benchmarks;

/// <summary>
/// Generates synthetic STDF data for benchmarks.
/// </summary>
internal static class SyntheticData
{
    private static byte[]? _mediumLe;
    private static byte[]? _mediumBe;

    public static byte[] MediumLe => _mediumLe ??= Generate(Endianness.LittleEndian);
    public static byte[] MediumBe => _mediumBe ??= Generate(Endianness.BigEndian);

    private static byte[] Generate(Endianness endianness)
    {
        byte cpuType = endianness == Endianness.BigEndian ? (byte)1 : (byte)2;

        using var ms = new MemoryStream();
        var task = Task.Run(async () =>
        {
            await using var writer = StdfFile.OpenWrite(ms, new StdfWriterOptions { Endianness = endianness });

            await writer.WriteAsync(new Far { CpuType = cpuType, StdfVersion = 4 });
            await writer.WriteAsync(new Mir
            {
                SetupTime = new DateTime(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc),
                StartTime = new DateTime(2025, 3, 1, 8, 5, 0, DateTimeKind.Utc),
                StationNumber = 3,
                ModeCode = 'P',
                LotId = "BENCH-LOT",
                PartType = "BENCH-CHIP",
                NodeName = "BENCH-TESTER",
                JobName = "BENCH_JOB",
            });

            var rng = new Random(42);
            for (int part = 0; part < 500; part++)
            {
                byte site = (byte)((part % 4) + 1);
                await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = site });

                for (int t = 0; t < 5; t++)
                {
                    await writer.WriteAsync(new Ptr
                    {
                        TestNumber = (uint)(1000 + t),
                        HeadNumber = 1,
                        SiteNumber = site,
                        TestFlags = 0,
                        ParametricFlags = 0,
                        Result = (float)(rng.NextDouble() * 5.0),
                        TestText = $"Test_{t}",
                        Units = "V",
                        LowLimit = 0.5f,
                        HighLimit = 4.5f,
                    });
                }

                await writer.WriteAsync(new Ftr
                {
                    TestNumber = 2000,
                    HeadNumber = 1,
                    SiteNumber = site,
                    TestFlags = 0,
                });

                await writer.WriteAsync(new Prr
                {
                    HeadNumber = 1,
                    SiteNumber = site,
                    PartFlag = 0,
                    NumTestsExecuted = 6,
                    HardwareBin = 1,
                    SoftwareBin = 1,
                    XCoordinate = (short)(part % 20),
                    YCoordinate = (short)(part / 20),
                    TestTime = (uint)(50 + rng.Next(100)),
                    PartId = $"P{part:D4}",
                });
            }

            await writer.WriteAsync(new Mrr
            {
                FinishTime = new DateTime(2025, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            });
        });
        task.GetAwaiter().GetResult();
        return ms.ToArray();
    }
}
