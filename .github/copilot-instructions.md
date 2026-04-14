# Copilot Instructions — Marklio.Stdf

## Build and test

```powershell
# Build (must use --no-incremental after generator changes)
dotnet build --no-incremental

# Run all tests
dotnet test

# Run a single test by name
dotnet test --filter "FullyQualifiedName~RealFileTests.RoundTrip_ByteExact"

# Run tests in a single class
dotnet test --filter "FullyQualifiedName~MirTests"
```

The `--no-incremental` flag on build is **required** after changes to the source generator (`Marklio.Stdf.Generator`). Roslyn incremental generators cache aggressively, and stale generated code causes confusing errors. The flag is only needed on `dotnet build`; `dotnet test` rebuilds automatically.

## Design goals

1. **Byte-exact round-tripping** — Reading and re-writing an unmodified STDF file must produce identical bytes. This is the primary correctness invariant and is verified by integration tests against real-world files.
2. **Idiomatic CLR types** — STDF wire types map to natural .NET types (`U*4` timestamps → `DateTime`, `C*n` → `string`, `B*1` → `byte` with `[BitField]`, etc.). Consumers shouldn't need to think in STDF type codes.
3. **Low-allocation, high-throughput I/O** — Uses `System.IO.Pipelines` (`PipeReader`/`PipeWriter`) internally. Record structs avoid heap allocation. Supports files, network streams, byte arrays, and memory-mapped files through standard .NET I/O primitives.
4. **Source-generator-driven correctness** — Record structure is declared once via attributes; serialization/deserialization code is generated. This eliminates hand-rolled serialize/deserialize drift and ensures wire order matches declaration order.
5. **Pattern matching ergonomics** — `IAsyncEnumerable<StdfRecord>` API composes with LINQ. Shared interfaces (`IHeadRecord`, `IBinRecord`, `ITestRecord`) enable `is`/`switch` pattern matching across related record types.

## Key design decisions

| Decision | Choice | Rationale |
|---|---|---|
| STDF versions | V4 + V4-2007 addendum | Covers all modern STDF files; extensible for custom records |
| Record representation | `partial record class` inheriting `StdfRecord` base + marker attributes | Clean type hierarchy, pattern matching via `is`, source generator produces the other partial |
| Reading API | `IAsyncEnumerable<StdfRecord>` | Composable with LINQ, `await foreach`, pattern matching |
| I/O engine | `System.IO.Pipelines` | Native .NET high-perf I/O; avoids custom buffering abstractions |
| Round-trip strategy | Full re-serialize; presence bitmask + trailing data preservation | Byte-identical for untouched records; optimal write for modified records |
| Field presence | Internal bitmask (`uint`/`ulong`) per class | Invisible to consumers; tracks which optional fields existed on wire |
| Endianness | Both LE and BE read/write; auto-detect from FAR | STDF spec requires FAR first; peek at CPU_TYP before interpreting REC_LEN |
| Unknown records | `UnknownRecord` with raw bytes | Enables round-tripping files with vendor-specific or future record types |
| Count fields | Private throwing property position markers | Zero storage, correct wire position, group-linked via attribute — generator never calls them |
| Hand-implemented records | GDR and STR bypass generator | V*n typed fields and variable-width U*f fields are too complex for the generator pattern |

## STDF type mapping

| STDF Type | CLR Type | Attribute needed | Notes |
|---|---|---|---|
| `U*1` | `byte` | — | Default for `byte` |
| `U*2` | `ushort` | — | |
| `U*4` | `uint` | — | Or `DateTime` with `[StdfDateTime]` |
| `U*8` | `ulong` | — | V4-2007 |
| `I*1` | `sbyte` | — | |
| `I*2` | `short` | — | |
| `I*4` | `int` | — | |
| `I*8` | `long` | — | V4-2007 |
| `R*4` | `float` | — | |
| `R*8` | `double` | — | |
| `C*1` | `char` | `[C1]` | Disambiguates from U\*1 |
| `C*n` | `string` | — | 1-byte length prefix |
| `C*f` | `string` | `[FixedString(n)]` | Fixed-length |
| `S*n` | `string` | `[Sn]` | V4-2007, 2-byte length prefix |
| `B*1` | `byte` | `[BitField]` | Disambiguates from U\*1 |
| `B*n` | `byte[]` | `[BitEncoded]` | Variable-length raw bytes |
| `D*n` | `BitArray` | `[BitArray]` | Bit-count-prefixed |
| `N*1` | `byte` | `[Nibble]` | 4-bit; packed 2-per-byte in arrays |
| `kxTYPE` | `T[]` | `[CountedArray("group")]` | Linked to `[WireCount("group")]` count |

## Architecture

This is an STDF (Standard Test Data Format) V4/V4-2007 parser and writer for .NET 10. The core design is: **declare record classes inheriting `StdfRecord` with marker attributes → source generator produces serialization code → I/O layer streams records via System.IO.Pipelines**.

### Projects

- **`Marklio.Stdf`** — Runtime library. Record types, I/O, public API.
- **`Marklio.Stdf.Generator`** — Roslyn incremental source generator (targets `netstandard2.0`). Referenced as an `Analyzer` by the runtime library.
- **`Marklio.Stdf.Tests`** — xUnit tests for the runtime library.
- **`Marklio.Stdf.Generator.Tests`** — Tests for generator output verification.

### Source generator pipeline

1. **`StdfRecordGenerator`** — Entry point. Hooks `ForAttributeWithMetadataName` on `[StdfRecord]` types. Collects metadata, emits one `*.g.cs` per record plus `RecordRegistry.g.cs`.
2. **`RecordAnalyzer`** — Walks the **syntax-tree declaration order** of properties in the `[StdfRecord]`-attributed partial declaration. Maps CLR types + attributes to `StdfFieldType`. Skips explicit interface implementations.
3. **`FieldMetadata`** — Data model passed from analyzer to emitter.
4. **`RecordEmitter`** — Generates the other `partial` half of each record class: `_fieldPresence` bitmask, default constructor, `RecordType`/`RecordSubType` overrides, `Deserialize`, `Serialize`, and shared helper methods. Also emits the `RecordRegistry` dispatch table.

### I/O layer

- **`StdfFile`** — Public façade. `ReadAsync(path|stream)` → `IAsyncEnumerable<StdfRecord>`. `Read(ReadOnlyMemory<byte>)` → `IEnumerable<StdfRecord>`. `OpenWriteAsync` / `OpenWrite` → `StdfWriter`.
- **`StdfRecordReader`** — Reads from `PipeReader`. Auto-detects endianness by peeking at the FAR record's `CPU_TYP` byte before interpreting `REC_LEN`.
- **`StdfRecordWriter`** — Serializes payload first (to compute `REC_LEN`), then writes header + payload + trailing data.
- **`StdfRecord`** — Abstract base `record class`. All records inherit from this. Defines abstract `RecordType`, `RecordSubType`, `Serialize`, and holds `TrailingData` (for byte-exact round-tripping).
- **`UnknownRecord`** — Preserves raw payload bytes for unrecognized record types.

### Record interfaces (for pattern matching)

```
IHeadRecord              — HeadNumber
  └─ IHeadSiteRecord     — + SiteNumber
       ├─ IBinRecord      — + BinNumber, BinCount, PassFail, BinName
       └─ ITestRecord     — + TestNumber
```

Records like HBR/SBR use **explicit interface implementations** to map their specific property names (e.g., `HardwareBin`) to the generic interface (`BinNumber`).

## Record definition conventions

Every generated record is a `public partial record class` inheriting from `StdfRecord`, with `[StdfRecord(type, subtype)]` in `src/Marklio.Stdf/Records/`. The source generator produces the other partial.

### Property order is wire order

The generator reads properties in **syntax declaration order**. This order must exactly match the STDF specification's field order for the record.

### Key attribute patterns

| Attribute | Property type | Purpose |
|---|---|---|
| *(none)* | `byte`, `ushort`, `uint`, `float`, etc. | Standard scalar field |
| `[C1]` | `char` or `char?` | Single-byte character field |
| `[BitField]` | `byte` | Flag byte (raw bits, no bool expansion) |
| `[BitArray]` | `BitArray` | Variable-length bit array (B*n) |
| `[BitEncoded]` | `byte[]` | Variable-length raw byte data (B*n) |
| `[Nibble]` | `byte` | 4-bit nibble field (N*1) |
| `[StdfDateTime]` | `DateTime` | U*4 Unix timestamp |
| `[Sn]` | `string` | V4-2007 2-byte length-prefixed string |
| `[FixedString(n)]` | `string` | Fixed-length character field (C*f) |
| `[WireCount("group")]` | *(see below)* | Count field for counted arrays |
| `[CountedArray("group")]` | `T[]?` | Array whose length is given by the matching wire count |

### Counted arrays

Count fields are **private, throw-only positional markers** — they exist only to define wire position and are not stored at runtime:

```csharp
[WireCount("sites")] private byte SiteCount => throw new NotSupportedException();
[CountedArray("sites")] public byte[]? SiteNumbers { get; set; }
```

Multiple arrays can share one count by using the same group name. The wire count type (`byte` → U\*1, `ushort` → U\*2, `uint` → U\*4) controls the on-wire width.

### Optional trailing fields

Use nullable types (`ushort?`, `string?`, `char?`, etc.) for optional trailing fields. Serialization uses an `else return;` pattern — it stops writing at the first absent field. This means optional fields must remain at the end and in spec order.

### Field presence bitmask

The generator creates an internal `_fieldPresence` field (`uint` for ≤32 fields, `ulong` for >32). The default constructor sets all bits (so user-created records serialize all set fields). `Deserialize` resets to 0 and sets bits only for fields actually read from the wire. This asymmetry is intentional: user-created records should write everything the user set; deserialized records should only write what was on the wire.

**Truncated count edge case**: If a record is truncated after a count field but before its arrays (count = 5, arrays absent), re-serialization writes count = 0 (from null array). This semantic repair is intentional — the original record was already malformed.

### Non-nullable string defaults

Non-nullable `string` properties get `= string.Empty` in the generated constructor.

### Properties must use `{ get; set; }`

Not `{ get; init; }` — the generated `Deserialize` method assigns to properties on the result instance.

### Hand-implemented records

**GDR** (50,10) and **STR** (15,30) are too complex for the generator (V\*n typed fields, variable-width `U*f` fields). They are `record class` types inheriting from `StdfRecord` directly with hand-written `Deserialize`/`Serialize`. Both are hardcoded in `RecordEmitter.EmitRegistry`.

If adding a new hand-implemented record, you must also register it in `EmitRegistry` in `RecordEmitter.cs`.

## Byte-exact round-tripping

This is a core design goal. Key mechanisms:

1. **`TrailingData`** on `StdfRecord` preserves any bytes after the last recognized field (padding, vendor extensions).
2. **`UnknownRecord`** preserves the complete raw payload of unrecognized record types.
3. **Field presence bitmask** ensures only fields that were actually on the wire get re-serialized.
4. **Big-endian bootstrapping** — The reader peeks at `CPU_TYP` (byte offset 4) before interpreting `REC_LEN`, since `REC_LEN` itself is endian-dependent.

Round-trip correctness is verified by `RealFileTests.RoundTrip_ByteExact` against real-world STDF files in `tests/Marklio.Stdf.Tests/TestData/`.

## Testing patterns

- **Unit tests** construct raw byte payloads, deserialize with `SequenceReader<byte>`, verify fields, re-serialize to `ArrayBufferWriter<byte>`, and compare bytes.
- **Integration tests** read real `.stdf` files from `TestData/`, verify structure, and check byte-exact round-trips.
- **Interface tests** verify pattern matching across `IHeadRecord`, `IHeadSiteRecord`, `IBinRecord`, `ITestRecord`.

## Known pitfalls

- Changing the source generator requires `dotnet build --no-incremental` to see the effect.
- When a record has >32 fields (e.g., MIR has 38), the presence bitmask is `ulong`. All bit shift literals in generated code must use the `UL` suffix — `(1UL << bit)` not `(1u << bit)`.
- Explicit interface implementations on record classes (like `ushort IBinRecord.BinNumber => HardwareBin;`) must be in the same partial declaration but are **skipped** by the analyzer via `ExplicitInterfaceSpecifier` check.
- Empty counted arrays (count=0) must still set the presence bit for the array field, otherwise the `else return;` pattern halts serialization of all subsequent fields.
