using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

/// <summary>
/// Generates synthetic STDF data for tests, eliminating the need for
/// externally-sourced test files with unclear licensing.
/// </summary>
internal static class SyntheticStdf
{
    /// <summary>
    /// Small LE file (~100 bytes): FAR + MIR + 1 PIR/PTR/PRR + MRR.
    /// </summary>
    public static readonly Lazy<byte[]> SmallLe = new(GenerateSmallLe);

    /// <summary>
    /// Medium LE file with many record types and varied data.
    /// FAR + MIR + WIR + WCR + PIR/PTR/FTR/PRR cycles + HBR + SBR + PCR + TSR + BPS/EPS + DTR + MRR.
    /// </summary>
    public static readonly Lazy<byte[]> MediumLe = new(() => GenerateMedium(Endianness.LittleEndian));

    /// <summary>
    /// Medium BE file — same content as MediumLe but big-endian.
    /// </summary>
    public static readonly Lazy<byte[]> MediumBe = new(() => GenerateMedium(Endianness.BigEndian));

    /// <summary>
    /// File with a fully-populated MIR (all 38 fields) to cover the >32-field presence bit path.
    /// </summary>
    public static readonly Lazy<byte[]> FullMir = new(GenerateFullMir);

    private static byte[] GenerateSmallLe()
    {
        using var ms = new MemoryStream();
        var pipe = System.IO.Pipelines.PipeWriter.Create(ms);
        var writer = new IO.StdfRecordWriter(pipe, Endianness.LittleEndian);

        WriteSmallStdf(writer, Endianness.LittleEndian).GetAwaiter().GetResult();
        writer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        return ms.ToArray();
    }

    private static byte[] GenerateMedium(Endianness endianness)
    {
        using var ms = new MemoryStream();
        var pipe = System.IO.Pipelines.PipeWriter.Create(ms);
        var writer = new IO.StdfRecordWriter(pipe, endianness);

        WriteMediumStdf(writer, endianness).GetAwaiter().GetResult();
        writer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        return ms.ToArray();
    }

    private static byte[] GenerateFullMir()
    {
        using var ms = new MemoryStream();
        var pipe = System.IO.Pipelines.PipeWriter.Create(ms);
        var writer = new IO.StdfRecordWriter(pipe, Endianness.LittleEndian);

        WriteFullMirStdf(writer).GetAwaiter().GetResult();
        writer.DisposeAsync().AsTask().GetAwaiter().GetResult();
        return ms.ToArray();
    }

    private static async Task WriteSmallStdf(IO.StdfRecordWriter writer, Endianness endianness)
    {
        byte cpuType = endianness == Endianness.BigEndian ? (byte)1 : (byte)2;
        await writer.WriteAsync(new Far { CpuType = cpuType, StdfVersion = 4 });
        await writer.WriteAsync(new Mir
        {
            SetupTime = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            StartTime = new DateTime(2025, 1, 15, 10, 5, 0, DateTimeKind.Utc),
            StationNumber = 1,
            ModeCode = 'P',
            LotId = "LOT001",
            PartType = "CHIP-A",
            NodeName = "TESTER-1",
            JobName = "TEST_JOB",
        });
        await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = 1 });
        await writer.WriteAsync(new Ptr
        {
            TestNumber = 1000,
            HeadNumber = 1,
            SiteNumber = 1,
            TestFlags = 0,
            ParametricFlags = 0,
            Result = 3.14f,
            TestText = "Vcc_test",
            Units = "V",
        });
        await writer.WriteAsync(new Prr
        {
            HeadNumber = 1,
            SiteNumber = 1,
            PartFlag = 0,
            NumTestsExecuted = 1,
            HardwareBin = 1,
            SoftwareBin = 1,
            XCoordinate = 5,
            YCoordinate = 10,
            TestTime = 100,
            PartId = "P001",
        });
        await writer.WriteAsync(new Mrr
        {
            FinishTime = new DateTime(2025, 1, 15, 11, 0, 0, DateTimeKind.Utc),
        });
    }

    private static async Task WriteMediumStdf(IO.StdfRecordWriter writer, Endianness endianness)
    {
        byte cpuType = endianness == Endianness.BigEndian ? (byte)1 : (byte)2;
        await writer.WriteAsync(new Far { CpuType = cpuType, StdfVersion = 4 });
        await writer.WriteAsync(new Mir
        {
            SetupTime = new DateTime(2025, 3, 1, 8, 0, 0, DateTimeKind.Utc),
            StartTime = new DateTime(2025, 3, 1, 8, 5, 0, DateTimeKind.Utc),
            StationNumber = 3,
            ModeCode = 'P',
            RetestCode = ' ',
            ProtectionCode = ' ',
            BurnInTime = 0,
            CommandModeCode = ' ',
            LotId = "LOT-MEDIUM-001",
            PartType = "SYN-CHIP-X",
            NodeName = "SYNTH-TESTER",
            TesterType = "Synthetic",
            JobName = "MEDIUM_TEST",
            JobRevision = "1.0",
            FacilityId = "FAC01",
            FloorId = "FL3",
        });

        // WIR + WCR
        await writer.WriteAsync(new Wir
        {
            HeadNumber = 1,
            StartTime = new DateTime(2025, 3, 1, 8, 10, 0, DateTimeKind.Utc),
            WaferId = "W001",
        });
        await writer.WriteAsync(new Wcr
        {
            WaferSize = 300_000.0f,
            DieHeight = 5000.0f,
            DieWidth = 5000.0f,
            WaferUnits = 2, // millimeters
            WaferFlat = 'U',
        });

        // PMR
        await writer.WriteAsync(new Pmr
        {
            PinIndex = 1,
            ChannelType = 0,
            ChannelName = "VCC",
            PhysicalName = "PIN_1",
            LogicalName = "VCC_SUPPLY",
            HeadNumber = 1,
            SiteNumber = 1,
        });

        // SDR
        await writer.WriteAsync(new Sdr
        {
            HeadNumber = 1,
            SiteGroup = 0,
            SiteNumbers = [1, 2, 3, 4],
            HandlerType = "HANDLER-A",
        });

        // BPS - begin program section
        await writer.WriteAsync(new Bps { SequenceName = "DC_TESTS" });

        // DTR - data text
        await writer.WriteAsync(new Dtr { TextData = "Starting DC parametric tests..." });

        // 50 PIR/PTR/FTR/PRR cycles with varied data
        var rng = new Random(42); // deterministic seed
        for (int part = 0; part < 50; part++)
        {
            byte site = (byte)((part % 4) + 1);
            await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = site });

            // 3 parametric tests per part
            for (int t = 0; t < 3; t++)
            {
                await writer.WriteAsync(new Ptr
                {
                    TestNumber = (uint)(1000 + t),
                    HeadNumber = 1,
                    SiteNumber = site,
                    TestFlags = 0,
                    ParametricFlags = 0,
                    Result = (float)(rng.NextDouble() * 5.0),
                    TestText = $"Param_Test_{t}",
                    Units = t == 0 ? "V" : t == 1 ? "A" : "ohm",
                    LowLimit = 0.5f,
                    HighLimit = 4.5f,
                    LowSpecLimit = 0.1f,
                    HighSpecLimit = 4.9f,
                });
            }

            // 1 functional test per part
            await writer.WriteAsync(new Ftr
            {
                TestNumber = 2000,
                HeadNumber = 1,
                SiteNumber = site,
                TestFlags = (byte)(part % 10 == 7 ? 1 : 0), // occasional fail
            });

            bool pass = part % 10 != 7;
            await writer.WriteAsync(new Prr
            {
                HeadNumber = 1,
                SiteNumber = site,
                PartFlag = (byte)(pass ? 0 : 0x08),
                NumTestsExecuted = 4,
                HardwareBin = (ushort)(pass ? 1 : 5),
                SoftwareBin = (ushort)(pass ? 1 : 10),
                XCoordinate = (short)(part % 10),
                YCoordinate = (short)(part / 10),
                TestTime = (uint)(50 + rng.Next(100)),
                PartId = $"P{part:D4}",
            });
        }

        // EPS - end program section
        await writer.WriteAsync(new Eps());

        // HBR / SBR
        await writer.WriteAsync(new Hbr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            HardwareBin = 1,
            BinCount = 45,
            BinPassFail = 'P',
            BinName = "GOOD",
        });
        await writer.WriteAsync(new Hbr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            HardwareBin = 5,
            BinCount = 5,
            BinPassFail = 'F',
            BinName = "FUNC_FAIL",
        });
        await writer.WriteAsync(new Sbr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            SoftwareBin = 1,
            BinCount = 45,
            BinPassFail = 'P',
            BinName = "PASS",
        });
        await writer.WriteAsync(new Sbr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            SoftwareBin = 10,
            BinCount = 5,
            BinPassFail = 'F',
            BinName = "FAIL",
        });

        // PCR
        await writer.WriteAsync(new Pcr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            PartCount = 50,
            RetestCount = 0,
            AbortCount = 0,
            GoodCount = 45,
            FunctionalCount = 50,
        });

        // TSR
        await writer.WriteAsync(new Tsr
        {
            HeadNumber = 255,
            SiteNumber = 0,
            TestType = 'P',
            TestNumber = 1000,
            ExecutedCount = 50,
            FailedCount = 5,
            TestName = "Param_Test_0",
        });

        // WRR
        await writer.WriteAsync(new Wrr
        {
            HeadNumber = 1,
            FinishTime = new DateTime(2025, 3, 1, 9, 0, 0, DateTimeKind.Utc),
            PartCount = 50,
            RetestCount = 0,
            AbortCount = 0,
            GoodCount = 45,
            FunctionalCount = 50,
            WaferId = "W001",
        });

        // MRR
        await writer.WriteAsync(new Mrr
        {
            FinishTime = new DateTime(2025, 3, 1, 9, 5, 0, DateTimeKind.Utc),
            DispositionCode = ' ',
        });
    }

    private static async Task WriteFullMirStdf(IO.StdfRecordWriter writer)
    {
        await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });

        // MIR with all 38 fields populated — tests >32-field presence bit logic
        await writer.WriteAsync(new Mir
        {
            SetupTime = new DateTime(2025, 6, 15, 10, 0, 0, DateTimeKind.Utc),
            StartTime = new DateTime(2025, 6, 15, 10, 5, 0, DateTimeKind.Utc),
            StationNumber = 42,
            ModeCode = 'P',
            RetestCode = 'Y',
            ProtectionCode = '0',
            BurnInTime = 120,
            CommandModeCode = 'A',
            LotId = "FULL-MIR-LOT",
            PartType = "FULL-MIR-CHIP",
            NodeName = "NODE-42",
            TesterType = "SYNTH-ATE",
            JobName = "FULL_MIR_JOB",
            JobRevision = "2.1",
            SublotId = "SUB-001",
            OperatorName = "OP-SYNTH",
            ExecType = "EXEC-A",
            ExecVersion = "3.0.0",
            TestCode = "TC-FULL",
            TestTemperature = "25.0",
            UserText = "Synthetic full MIR test",
            AuxiliaryFile = "aux.dat",
            PackageType = "BGA-256",
            FamilyId = "FAM-X",
            DateCode = "2506",
            FacilityId = "FAC-FULL",
            FloorId = "FL-7",
            ProcessId = "PROC-42",
            OperationFrequency = "1.2GHz",
            SpecificationName = "SPEC-001",
            SpecificationVersion = "1.0",
            FlowId = "FLOW-MAIN",
            SetupId = "SETUP-A",
            DesignRevision = "REV-C",
            EngineeringId = "ENG-001",
            RomCode = "ROM-42",
            SerialNumber = "SN-12345",
            SupervisorName = "SUPER-A",
        });

        // Minimal part cycle to make a valid file
        await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = 1 });
        await writer.WriteAsync(new Prr
        {
            HeadNumber = 1,
            SiteNumber = 1,
            PartFlag = 0,
            NumTestsExecuted = 0,
            HardwareBin = 1,
            SoftwareBin = 1,
        });
        await writer.WriteAsync(new Mrr
        {
            FinishTime = new DateTime(2025, 6, 15, 11, 0, 0, DateTimeKind.Utc),
        });
    }
}
