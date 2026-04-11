using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Marklio.Stdf.Generator;

namespace Marklio.Stdf.Generator.Tests;

public class DiagnosticTests
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
            public interface IStdfRecord { }
        }
        """;

    private static (ImmutableArray<Diagnostic> GeneratorDiagnostics, Compilation OutputCompilation) RunGenerator(string source)
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

        // Ensure netstandard/System.Runtime is referenced
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

        // Diagnostics reported via context.ReportDiagnostic appear in per-generator results
        var runResult = driver.GetRunResult();
        var allDiags = runResult.Results.SelectMany(r => r.Diagnostics).ToImmutableArray();

        return (allDiags, outputCompilation);
    }

    [Fact]
    public void STDF001_UnsupportedPropertyType_ReportsError()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 10)]
            public partial record struct BadRecord
            {
                public System.TimeSpan Elapsed { get; init; }
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        var stdf001 = diagnostics.Where(d => d.Id == "STDF001").ToList();
        Assert.Single(stdf001);
        Assert.Contains("Elapsed", stdf001[0].GetMessage());
        Assert.Equal(DiagnosticSeverity.Error, stdf001[0].Severity);
    }

    [Fact]
    public void STDF002_CountedArrayOnNonArray_ReportsError()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 20)]
            public partial record struct BadCountedArray
            {
                [WireCount("grp")] private ushort Count => throw new System.NotSupportedException();
                [CountedArray("grp")] public int NotAnArray { get; init; }
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        var stdf002 = diagnostics.Where(d => d.Id == "STDF002").ToList();
        Assert.Single(stdf002);
        Assert.Contains("NotAnArray", stdf002[0].GetMessage());
        Assert.Equal(DiagnosticSeverity.Error, stdf002[0].Severity);
    }

    [Fact]
    public void STDF003_WireCountGroupUnmatched_ReportsWarning()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 30)]
            public partial record struct OrphanWireCount
            {
                [WireCount("orphan")] private ushort Count => throw new System.NotSupportedException();
                public byte SomeField { get; init; }
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        var stdf003 = diagnostics.Where(d => d.Id == "STDF003").ToList();
        Assert.Single(stdf003);
        Assert.Contains("orphan", stdf003[0].GetMessage());
        Assert.Equal(DiagnosticSeverity.Warning, stdf003[0].Severity);
    }

    [Fact]
    public void STDF004_FixedStringOnNonString_ReportsError()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 40)]
            public partial record struct BadFixedString
            {
                [FixedString(10)] public int NotAString { get; init; }
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        var stdf004 = diagnostics.Where(d => d.Id == "STDF004").ToList();
        Assert.Single(stdf004);
        Assert.Contains("NotAString", stdf004[0].GetMessage());
        Assert.Equal(DiagnosticSeverity.Error, stdf004[0].Severity);
    }

    [Fact]
    public void ValidRecord_ProducesNoDiagnostics()
    {
        var source = """
            using Marklio.Stdf.Attributes;

            namespace TestRecords;

            [StdfRecord(1, 50)]
            public partial record struct GoodRecord
            {
                [WireCount("items")] private ushort ItemCount => throw new System.NotSupportedException();
                [CountedArray("items")] public ushort[] Items { get; init; }
                public byte SimpleField { get; init; }
                public string Name { get; init; }
                [FixedString(8)] public string FixedName { get; init; }
            }
            """;

        var (diagnostics, _) = RunGenerator(source);

        var stdfDiags = diagnostics.Where(d => d.Id.StartsWith("STDF")).ToList();
        Assert.Empty(stdfDiags);
    }
}
