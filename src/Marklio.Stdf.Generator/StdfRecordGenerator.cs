using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marklio.Stdf.Generator;

[Generator]
public class StdfRecordGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var analysisResults = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Marklio.Stdf.Attributes.StdfRecordAttribute",
                predicate: (node, _) => node is TypeDeclarationSyntax,
                transform: (ctx, _) => RecordAnalyzer.Analyze(ctx))
            .Where(r => r is not null)
            .Select((r, _) => r!)
            .Collect();

        context.RegisterSourceOutput(analysisResults, static (ctx, results) => EmitGeneratedCode(ctx, results));
    }

    private static void EmitGeneratedCode(SourceProductionContext context, ImmutableArray<AnalysisResult> results)
    {
        // Report all diagnostics first
        foreach (var result in results)
        {
            foreach (var diag in result.Diagnostics)
            {
                context.ReportDiagnostic(diag.ToDiagnostic());
            }
        }

        // Emit source for records that have metadata
        var records = results
            .Where(r => r.Metadata is not null)
            .Select(r => r.Metadata!)
            .ToImmutableArray();

        foreach (var record in records)
        {
            var source = RecordEmitter.EmitRecordPartial(record);
            context.AddSource($"{record.TypeName}.g.cs", source);
        }

        if (records.Length > 0)
        {
            var registry = RecordEmitter.EmitRegistry(records);
            context.AddSource("RecordRegistry.g.cs", registry);
        }
    }
}
