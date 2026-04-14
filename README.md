# Marklio.Stdf

A high-performance .NET 10 library for reading and writing [STDF](https://en.wikipedia.org/wiki/Standard_Test_Data_Format) (Standard Test Data Format) V4 and V4-2007 files used in semiconductor test data.

## Features

- **Byte-exact round-tripping** — Read and re-write STDF files with identical output, preserving trailing data, unknown records, and vendor extensions.
- **All 32 V4/V4-2007 record types** — FAR, MIR, MRR, PCR, HBR, SBR, PMR, PGR, PLR, RDR, SDR, WIR, WRR, WCR, PIR, PRR, TSR, PTR, MPR, FTR, BPS, EPS, GDR, DTR, ATR, VUR, PSR, NMR, CNR, SSR, CDR, STR.
- **Streaming API** — `IAsyncEnumerable<StdfRecord>` with `System.IO.Pipelines` for low-allocation, high-throughput I/O.
- **Pattern matching** — Shared interfaces (`IHeadRecord`, `IHeadSiteRecord`, `IBinRecord`, `ITestRecord`) for ergonomic `is`/`switch` pattern matching.
- **Transparent compression** — Auto-detects and decompresses gzip/bzip2 on read. Configurable compression on write.
- **Both endiannesses** — Reads big-endian and little-endian files (auto-detected from FAR). Writes either.
- **Source-generated serialization** — Record structs are declared with attributes; a Roslyn incremental generator produces all serialization code.

## Quick start

### Reading files

Use pattern matching to match specific record types — this is the canonical pattern:

```csharp
using Marklio.Stdf;
using Marklio.Stdf.Records;

await foreach (var record in StdfFile.ReadAsync("data.stdf"))
{
    if (record is Mir mir)
        Console.WriteLine($"Lot: {mir.LotId}");
    else if (record is Ptr ptr)
        Console.WriteLine($"Test {ptr.TestNumber}: {ptr.Result} {ptr.Units}");
    else if (record is Prr prr)
        Console.WriteLine($"Part {prr.PartId} — bin {prr.HardwareBin}");
}
```

Use `is` to match across record families via shared interfaces:

```csharp
await foreach (var rec in StdfFile.ReadAsync("wafer1.stdf"))
{
    if (rec is ITestRecord test)
        Console.WriteLine($"  Head {test.HeadNumber} Site {test.SiteNumber}");

    if (rec is IBinRecord bin)
        Console.WriteLine($"Bin {bin.BinNumber}: {bin.BinCount} parts");
}
```

### Writing files

```csharp
await using var writer = await StdfFile.OpenWriteAsync("output.stdf");
await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });
await writer.WriteAsync(new Mir
{
    SetupTime = DateTime.UtcNow,
    StartTime = DateTime.UtcNow,
    StationNumber = 1,
    LotId = "LOT001",
    PartType = "SENSOR-A",
    NodeName = "TESTER01",
});
await writer.WriteAsync(new Mrr { FinishTime = DateTime.UtcNow });
```

Round-trip with modifications (byte-exact for unmodified records):

```csharp
await using var w = await StdfFile.OpenWriteAsync("modified.stdf");
await foreach (var rec in StdfFile.ReadAsync("input.stdf"))
{
    await w.WriteAsync(rec);
}
```

Write compressed output:

```csharp
await using var wGz = await StdfFile.OpenWriteAsync("output.stdf.gz", new StdfWriterOptions
{
    Compression = StdfCompression.Gzip,
});
```

### Options and recovery mode

By default, corrupt or truncated records throw `StdfParseException`. Enable
recovery mode to skip over bad data and continue reading:

```csharp
var options = new StdfReaderOptions { RecoveryMode = true };
await foreach (var record in StdfFile.ReadAsync("data.stdf", options))
{
    // Corrupt records are skipped instead of throwing
}
```

You can also receive a callback for each skipped region:

```csharp
var options = new StdfReaderOptions
{
    RecoveryMode = true,
    OnRecovery = e => Console.WriteLine($"Skipped {e}"),
};
```

### Synchronous API

Read from a byte array or `ReadOnlyMemory<byte>` — compression is auto-detected:

```csharp
var data = File.ReadAllBytes("wafer1.stdf.gz");
foreach (var rec in StdfFile.Read(data))
{
    if (rec is Ptr ptr)
        Console.WriteLine($"{ptr.TestNumber}: {ptr.Result}");
}
```

### Error handling

Without recovery mode, corrupt data throws `StdfParseException`:

```csharp
try
{
    await foreach (var rec in StdfFile.ReadAsync("bad.stdf"))
    { /* ... */ }
}
catch (StdfParseException ex)
{
    Console.WriteLine($"Parse error at offset {ex.Message}");
}
```

## API overview

| Method | Description |
|---|---|
| `StdfFile.ReadAsync(path)` | Read from file path → `IAsyncEnumerable<StdfRecord>` |
| `StdfFile.ReadAsync(stream)` | Read from any `Stream` |
| `StdfFile.Read(ReadOnlyMemory<byte>)` | Synchronous read from bytes |
| `StdfFile.OpenWriteAsync(path, options?)` | Open file writer → `StdfWriter` |
| `StdfFile.OpenWrite(stream, options?)` | Open stream writer |

All read methods auto-detect endianness and compression.

## Record interfaces

```
IHeadRecord              — HeadNumber
  └─ IHeadSiteRecord     — + SiteNumber
       ├─ IBinRecord      — + BinNumber, BinCount, PassFail, BinName
       └─ ITestRecord     — + TestNumber
```

## Continuation merging

STDF V4-2007 introduced PSR (Pattern Sequence Record) and STR (Scan Test Record) types
that can span multiple physical STDF records via a continuation flag. The
`MergeContinuations()` extension method reassembles these multi-segment sequences into
single logical records, making downstream analysis simpler.

```csharp
// Async — merged records are yielded as single PSR/STR instances
await foreach (var rec in StdfFile.ReadAsync("data.stdf").MergeContinuations())
{
    if (rec is Psr psr)
        Console.WriteLine($"PSR {psr.PsrIndex}: {psr.PatternLabels?.Length} patterns");
}

// Synchronous equivalent
foreach (var rec in StdfFile.Read(bytes).MergeContinuations())
{
    // ...
}
```

> **⚠️ TrailingData is not preserved.**
> Merged records do **not** retain `TrailingData` from individual continuation segments.
> Files written after merging will **not** be byte-exact copies of the original.
> If byte-exact round-tripping is required, iterate the raw record stream directly
> without calling `MergeContinuations()`.

## Requirements

- .NET 10.0 or later

## License

MIT
