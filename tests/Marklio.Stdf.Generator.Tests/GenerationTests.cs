using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Marklio.Stdf.Generator;

namespace Marklio.Stdf.Generator.Tests;

/// <summary>
/// Tests that verify the generator produces correct, compilable output for various record shapes.
/// </summary>
public class GenerationTests
{
    /// <summary>
    /// Minimal attribute stubs so the test compilation can resolve attribute types
    /// without referencing the full Marklio.Stdf assembly.
    /// </summary>
    private const string AttributeStubs = """
        namespace Marklio.Stdf.Attributes
        {
            [System.AttributeUsage(System.AttributeTargets.Struct)]
            public sealed class StdfRecordAttribute : System.Attribute
            {
                public byte RecordType { get; }
                public byte RecordSubType { get; }
                public StdfRecordAttribute(byte recordType, byte recordSubType)
                {
                    RecordType = recordType;
                    RecordSubType = recordSubType;
                }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class CountedArrayAttribute : System.Attribute
            {
                public string GroupName { get; }
                public CountedArrayAttribute(string groupName) { GroupName = groupName; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class WireCountAttribute : System.Attribute
            {
                public string GroupName { get; }
                public WireCountAttribute(string groupName) { GroupName = groupName; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class FixedStringAttribute : System.Attribute
            {
                public int Length { get; }
                public FixedStringAttribute(int length) { Length = length; }
            }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class NibbleAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class BitArrayAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class BitFieldAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class BitEncodedAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class StdfDateTimeAttribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class C1Attribute : System.Attribute { }

            [System.AttributeUsage(System.AttributeTargets.Property)]
            public sealed class SnAttribute : System.Attribute { }
        }

        namespace Marklio.Stdf
        {
            public interface IStdfRecord
            {
                static abstract byte RecordType { get; }
                static abstract byte RecordSubType { get; }
                void Serialize(System.Buffers.IBufferWriter<byte> writer, Endianness endianness);
            }

            public enum Endianness { Little, Big }

            public readonly struct StdfRecord
            {
                public IStdfRecord Record { get; }
                public byte RecordType { get; }
                public byte RecordSubType { get; }
                internal StdfRecord(IStdfRecord record, byte recType, byte recSub)
                {
                    Record = record;
                    RecordType = recType;
                    RecordSubType = recSub;
                }
            }
        }

        namespace Marklio.Stdf.Records
        {
            public partial record struct Gdr : Marklio.Stdf.IStdfRecord
            {
                static byte Marklio.Stdf.IStdfRecord.RecordType => 50;
                static byte Marklio.Stdf.IStdfRecord.RecordSubType => 10;
                public void Serialize(System.Buffers.IBufferWriter<byte> writer, Marklio.Stdf.Endianness endianness) { }
                public static Gdr Deserialize(ref System.Buffers.SequenceReader<byte> reader, Marklio.Stdf.Endianness endianness) => default;
            }

            public partial record struct Str : Marklio.Stdf.IStdfRecord
            {
                static byte Marklio.Stdf.IStdfRecord.RecordType => 15;
                static byte Marklio.Stdf.IStdfRecord.RecordSubType => 30;
                public void Serialize(System.Buffers.IBufferWriter<byte> writer, Marklio.Stdf.Endianness endianness) { }
                public static Str Deserialize(ref System.Buffers.SequenceReader<byte> reader, Marklio.Stdf.Endianness endianness) => default;
            }
        }

        namespace Marklio.Stdf.IO
        {
            public static class StdfSerializationHelpers
            {
                public static ushort ReadU2(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static short ReadI2(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static uint ReadU4(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static int ReadI4(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static ulong ReadU8(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static long ReadI8(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static float ReadR4(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static double ReadR8(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => 0;
                public static string ReadCn(ref System.Buffers.SequenceReader<byte> r) => "";
                public static string ReadSn(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => "";
                public static string ReadCf(ref System.Buffers.SequenceReader<byte> r, int len) => "";
                public static byte[] ReadBn(ref System.Buffers.SequenceReader<byte> r) => System.Array.Empty<byte>();
                public static System.Collections.BitArray ReadDn(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => new(0);
                public static System.DateTime ReadDateTime(ref System.Buffers.SequenceReader<byte> r, Marklio.Stdf.Endianness e) => default;
                public static byte[] ReadU1Array(ref System.Buffers.SequenceReader<byte> r, ushort c) => System.Array.Empty<byte>();
                public static ushort[] ReadU2Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<ushort>();
                public static uint[] ReadU4Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<uint>();
                public static ulong[] ReadU8Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<ulong>();
                public static sbyte[] ReadI1Array(ref System.Buffers.SequenceReader<byte> r, ushort c) => System.Array.Empty<sbyte>();
                public static short[] ReadI2Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<short>();
                public static int[] ReadI4Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<int>();
                public static long[] ReadI8Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<long>();
                public static float[] ReadR4Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<float>();
                public static double[] ReadR8Array(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<double>();
                public static string[] ReadCnArray(ref System.Buffers.SequenceReader<byte> r, ushort c) => System.Array.Empty<string>();
                public static string[] ReadSnArray(ref System.Buffers.SequenceReader<byte> r, ushort c, Marklio.Stdf.Endianness e) => System.Array.Empty<string>();
                public static byte[] ReadNibbleArray(ref System.Buffers.SequenceReader<byte> r, ushort c) => System.Array.Empty<byte>();
                public static void WriteU1(System.Buffers.IBufferWriter<byte> w, byte v) { }
                public static void WriteI1(System.Buffers.IBufferWriter<byte> w, sbyte v) { }
                public static void WriteU2(System.Buffers.IBufferWriter<byte> w, ushort v, Marklio.Stdf.Endianness e) { }
                public static void WriteI2(System.Buffers.IBufferWriter<byte> w, short v, Marklio.Stdf.Endianness e) { }
                public static void WriteU4(System.Buffers.IBufferWriter<byte> w, uint v, Marklio.Stdf.Endianness e) { }
                public static void WriteI4(System.Buffers.IBufferWriter<byte> w, int v, Marklio.Stdf.Endianness e) { }
                public static void WriteU8(System.Buffers.IBufferWriter<byte> w, ulong v, Marklio.Stdf.Endianness e) { }
                public static void WriteI8(System.Buffers.IBufferWriter<byte> w, long v, Marklio.Stdf.Endianness e) { }
                public static void WriteR4(System.Buffers.IBufferWriter<byte> w, float v, Marklio.Stdf.Endianness e) { }
                public static void WriteR8(System.Buffers.IBufferWriter<byte> w, double v, Marklio.Stdf.Endianness e) { }
                public static void WriteC1(System.Buffers.IBufferWriter<byte> w, char v) { }
                public static void WriteCn(System.Buffers.IBufferWriter<byte> w, string v) { }
                public static void WriteSn(System.Buffers.IBufferWriter<byte> w, string v, Marklio.Stdf.Endianness e) { }
                public static void WriteCf(System.Buffers.IBufferWriter<byte> w, string v, int len) { }
                public static void WriteBn(System.Buffers.IBufferWriter<byte> w, byte[] v) { }
                public static void WriteDn(System.Buffers.IBufferWriter<byte> w, System.Collections.BitArray v, Marklio.Stdf.Endianness e) { }
                public static void WriteN1(System.Buffers.IBufferWriter<byte> w, byte v) { }
                public static void WriteDateTime(System.Buffers.IBufferWriter<byte> w, System.DateTime v, Marklio.Stdf.Endianness e) { }
                public static void WriteU1Array(System.Buffers.IBufferWriter<byte> w, byte[]? v) { }
                public static void WriteU2Array(System.Buffers.IBufferWriter<byte> w, ushort[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteU4Array(System.Buffers.IBufferWriter<byte> w, uint[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteU8Array(System.Buffers.IBufferWriter<byte> w, ulong[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteI1Array(System.Buffers.IBufferWriter<byte> w, sbyte[]? v) { }
                public static void WriteI2Array(System.Buffers.IBufferWriter<byte> w, short[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteI4Array(System.Buffers.IBufferWriter<byte> w, int[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteI8Array(System.Buffers.IBufferWriter<byte> w, long[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteR4Array(System.Buffers.IBufferWriter<byte> w, float[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteR8Array(System.Buffers.IBufferWriter<byte> w, double[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteCnArray(System.Buffers.IBufferWriter<byte> w, string[]? v) { }
                public static void WriteSnArray(System.Buffers.IBufferWriter<byte> w, string[]? v, Marklio.Stdf.Endianness e) { }
                public static void WriteNibbleArray(System.Buffers.IBufferWriter<byte> w, byte[]? v) { }
            }
        }
        """;

    private static (ImmutableArray<Diagnostic> GeneratorDiagnostics, Compilation OutputCompilation, string[] GeneratedSources) RunGenerator(string source)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(AttributeStubs, path: "Attributes.cs"),
            CSharpSyntaxTree.ParseText(source, path: "Test.cs"),
        };

        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
            .Select(a => MetadataReference.CreateFromFile(a.Location))
            .Cast<MetadataReference>()
            .ToList();

        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new StdfRecordGenerator();
        var driver = CSharpGeneratorDriver.Create(generator);
        driver = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(
            compilation, out var outputCompilation, out _);

        var runResult = driver.GetRunResult();
        var allDiags = runResult.Results.SelectMany(r => r.Diagnostics).ToImmutableArray();

        var generatedSources = runResult.Results
            .SelectMany(r => r.GeneratedSources)
            .Select(s => s.SourceText.ToString())
            .ToArray();

        return (allDiags, outputCompilation, generatedSources);
    }

    private static void AssertNoGeneratorErrors(ImmutableArray<Diagnostic> diagnostics)
    {
        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        Assert.Empty(errors);
    }

    private static void AssertOutputCompiles(Compilation outputCompilation)
    {
        var compileDiags = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        Assert.Empty(compileDiags);
    }

    [Fact]
    public void SimpleRecord_GeneratesCorrectly()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 10)]
            public partial record struct SimpleRec
            {
                public byte FieldA { get; set; }
                public byte FieldB { get; set; }
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct SimpleRec"));
        Assert.Contains("public SimpleRec()", recordSource);
        Assert.Contains("Deserialize", recordSource);
        Assert.Contains("Serialize", recordSource);
        Assert.Contains("_fieldPresence", recordSource);
    }

    [Fact]
    public void RecordWithOptionalFields_GeneratesPresenceBitField()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(2, 10)]
            public partial record struct OptionalRec
            {
                public byte? FieldA { get; set; }
                public ushort? FieldB { get; set; }
                public uint? FieldC { get; set; }
                public float? FieldD { get; set; }
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct OptionalRec"));
        Assert.Contains("_fieldPresence", recordSource);
        // With <=32 fields, presence should be uint
        Assert.Contains("uint _fieldPresence", recordSource);
        // Nullable fields should have .HasValue checks in serialization
        Assert.Contains("HasValue", recordSource);
    }

    [Fact]
    public void RecordWithCountedArrays_GeneratesWireCount()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(3, 10)]
            public partial record struct ArrayRec
            {
                [WireCount("items")] private ushort ItemCount => throw new System.NotSupportedException();
                [CountedArray("items")] public ushort[] Items { get; set; }
                public byte TrailingField { get; set; }
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct ArrayRec"));
        // Wire count local should be generated in deserialization
        Assert.Contains("_cnt_items", recordSource);
        // Wire count should be computed from array length in serialization
        Assert.Contains("_wireCount_items", recordSource);
        Assert.Contains("_rawCount_items", recordSource);
    }

    [Fact]
    public void RecordWithFixedString_GeneratesFixedLength()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(4, 10)]
            public partial record struct FixedStringRec
            {
                [FixedString(8)] public string FixedName { get; set; }
                public byte OtherField { get; set; }
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct FixedStringRec"));
        // ReadCf with the fixed length should appear in deserialization
        Assert.Contains("ReadCf(ref reader, 8)", recordSource);
        // WriteCf with the fixed length should appear in serialization
        Assert.Contains("WriteCf(writer, FixedName, 8)", recordSource);
    }

    [Fact]
    public void EmptyRecord_GeneratesMinimalCode()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(5, 10)]
            public partial record struct EmptyRec
            {
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct EmptyRec"));
        Assert.Contains("Deserialize", recordSource);
        Assert.Contains("Serialize", recordSource);
        Assert.Contains("IStdfRecord", recordSource);
    }

    [Fact]
    public void RecordWithMoreThan32Fields_UsesUlongPresence()
    {
        // Generate a record with 33 fields to cross the uint -> ulong threshold
        var fields = string.Join("\n    ",
            Enumerable.Range(1, 33).Select(i => $"public byte Field{i} {{ get; set; }}"));

        var source = "using Marklio.Stdf.Attributes;\n\n" +
            "namespace TestRecords;\n\n" +
            "[StdfRecord(6, 10)]\n" +
            "public partial record struct BigRec\n{\n    " +
            fields + "\n}\n";

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct BigRec"));
        // With >32 fields, presence must use ulong
        Assert.Contains("ulong _fieldPresence", recordSource);
        // Literal suffix should be UL instead of u
        Assert.Contains("UL", recordSource);
    }

    [Fact]
    public void RecordWithSharedCountGroup_ValidatesLengths()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(7, 10)]
            public partial record struct SharedGroupRec
            {
                [WireCount("grp")] private ushort GrpCount => throw new System.NotSupportedException();
                [CountedArray("grp")] public ushort[] ArrayA { get; set; }
                [CountedArray("grp")] public float[] ArrayB { get; set; }
            }
            """;

        var (diagnostics, outputCompilation, generatedSources) = RunGenerator(source);

        AssertNoGeneratorErrors(diagnostics);
        AssertOutputCompiles(outputCompilation);

        var recordSource = generatedSources.First(s => s.Contains("partial record struct SharedGroupRec"));
        // Both arrays should use the same wire count local during deserialization
        Assert.Contains("_cnt_grp", recordSource);
        // The wire count should be derived from the first array's length
        Assert.Contains("ArrayA?.Length ?? 0", recordSource);
        // Both arrays should appear in the generated code
        Assert.Contains("ArrayA", recordSource);
        Assert.Contains("ArrayB", recordSource);
    }
}
