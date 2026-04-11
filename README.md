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

```csharp
using Marklio.Stdf;
using Marklio.Stdf.Records;

// Read from a file (or .stdf.gz / .stdf.bz2)
await foreach (var rec in StdfFile.ReadAsync("wafer1.stdf"))
{
    if (rec.Record is Ptr ptr)
        Console.WriteLine($"Test {ptr.TestNumber}: {ptr.Result} {ptr.Units}");

    // Pattern match across record families
    if (rec.Record is ITestRecord test)
        Console.WriteLine($"  Head {test.HeadNumber} Site {test.SiteNumber}");

    if (rec.Record is IBinRecord bin)
        Console.WriteLine($"Bin {bin.BinNumber}: {bin.BinCount} parts");
}

// Synchronous read from a byte array
var data = File.ReadAllBytes("wafer1.stdf.gz"); // auto-decompresses
foreach (var rec in StdfFile.Read(data))
{
    // ...
}
```

### Writing

```csharp
// Write a new file
await using var writer = await StdfFile.OpenWriteAsync("output.stdf");
await writer.WriteAsync(new Far { CpuType = 2, StdfVersion = 4 });
await writer.WriteAsync(new Mir { SetupTime = DateTime.UtcNow, StartTime = DateTime.UtcNow, StationNumber = 1 });
// ...

// Round-trip with modifications
await using var w = await StdfFile.OpenWriteAsync("modified.stdf");
await foreach (var rec in StdfFile.ReadAsync("input.stdf"))
{
    await w.WriteAsync(rec); // byte-exact for unmodified records
}

// Write compressed
await using var wGz = await StdfFile.OpenWriteAsync("output.stdf.gz", new StdfWriterOptions
{
    Compression = StdfCompression.Gzip,
});
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

## Requirements

- .NET 10.0 or later

## License

MIT
