using Marklio.Stdf;
using Marklio.Stdf.Records;

const string FileName = "sample.stdf";

// ── Step 1: Write a minimal STDF file ────────────────────────────
// Every STDF file starts with a FAR (File Attributes Record) followed
// by a MIR (Master Information Record). We then insert part results
// for two devices and close with a MRR (Master Results Record).

await using (var writer = await StdfFile.OpenWriteAsync(FileName))
{
    await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });

    await writer.WriteAsync(new Mir
    {
        SetupTime = DateTime.UtcNow,
        StartTime = DateTime.UtcNow,
        StationNumber = 1,
        // BurnInTime and CommandModeCode must be set so the record isn't
        // truncated before the string fields that follow them.
        BurnInTime = 0,
        CommandModeCode = ' ',
        LotId = "LOT-2025A",
        PartType = "CHIP-XY",
        NodeName = "TESTER01",
        TesterType = "Marklio",
        JobName = "QA_FINAL",
    });

    // Two devices, each with a PIR → PTR(s) → PRR sequence
    for (byte site = 0; site < 2; site++)
    {
        await writer.WriteAsync(new Pir { HeadNumber = 1, SiteNumber = site });

        await writer.WriteAsync(new Ptr
        {
            TestNumber = 1001,
            HeadNumber = 1,
            SiteNumber = site,
            Result = 3.30f + site * 0.01f,
            TestText = "Vcc",
            AlarmId = "",
            OptionalFlags = 0,
            ResultExponent = 0,
            LowLimitExponent = 0,
            HighLimitExponent = 0,
            LowLimit = 3.0f,
            HighLimit = 3.6f,
            Units = "V",
        });

        await writer.WriteAsync(new Ptr
        {
            TestNumber = 1002,
            HeadNumber = 1,
            SiteNumber = site,
            Result = 25.0f + site * 0.5f,
            TestText = "Idd",
            AlarmId = "",
            OptionalFlags = 0,
            ResultExponent = 0,
            LowLimitExponent = 0,
            HighLimitExponent = 0,
            LowLimit = 10.0f,
            HighLimit = 50.0f,
            Units = "mA",
        });

        await writer.WriteAsync(new Prr
        {
            HeadNumber = 1,
            SiteNumber = site,
            PartFlag = 0,
            NumTestsExecuted = 2,
            HardwareBin = 1,
            SoftwareBin = 1,
            XCoordinate = (short)site,
            YCoordinate = 0,
            TestTime = 120,
            PartId = $"SN-{site + 1:D4}",
        });
    }

    await writer.WriteAsync(new Mrr
    {
        FinishTime = DateTime.UtcNow,
        DispositionCode = 'P',
    });
}

Console.WriteLine($"Wrote {FileName}");
Console.WriteLine();

// ── Step 2: Read the file back and print a summary ───────────────
// TryGetRecord<T>() is the canonical way to match a specific record type.

string? lotId = null;
int partCount = 0;
int testCount = 0;

await foreach (var rec in StdfFile.ReadAsync(FileName))
{
    if (rec.TryGetRecord<Mir>(out var mir))
    {
        lotId = mir.LotId;
        Console.WriteLine($"Lot : {mir.LotId}");
        Console.WriteLine($"Part type : {mir.PartType}");
        Console.WriteLine($"Tester : {mir.NodeName}");
    }
    else if (rec.TryGetRecord<Ptr>(out var ptr))
    {
        testCount++;
        Console.WriteLine($"  Test {ptr.TestNumber} ({ptr.TestText}): {ptr.Result} {ptr.Units}");
    }
    else if (rec.TryGetRecord<Prr>(out var prr))
    {
        partCount++;
        Console.WriteLine($"  Part {prr.PartId} — bin {prr.HardwareBin}, {prr.NumTestsExecuted} tests");
    }
    else if (rec.TryGetRecord<Mrr>(out var mrr))
    {
        Console.WriteLine($"Finished : {mrr.FinishTime:u}");
    }
}

Console.WriteLine();
Console.WriteLine($"Summary: lot={lotId}, parts={partCount}, test results={testCount}");

// Clean up the sample file
File.Delete(FileName);
