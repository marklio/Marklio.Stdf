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
        var recordProvider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Marklio.Stdf.Attributes.StdfRecordAttribute",
                predicate: (node, _) => node is TypeDeclarationSyntax,
                transform: (ctx, _) => RecordAnalyzer.Analyze(ctx))
            .Where(r => r is not null)
            .Select((r, _) => r!);

        // Per-record output: only re-runs when that specific record changes
        context.RegisterSourceOutput(recordProvider, static (spc, result) =>
        {
            foreach (var diag in result.Diagnostics)
                spc.ReportDiagnostic(diag.ToDiagnostic());

            if (result.Metadata is not null)
            {
                var source = RecordEmitter.EmitRecordPartial(result.Metadata);
                spc.AddSource($"{result.Metadata.TypeName}.g.cs", source);
            }
        });

        // Registry output: re-runs when ANY record changes, but only emits the registry
        context.RegisterSourceOutput(recordProvider.Collect(), static (spc, results) =>
        {
            var metadata = results
                .Where(r => r.Metadata is not null)
                .Select(r => r.Metadata!);

            if (metadata.Any())
            {
                var registry = RecordEmitter.EmitRegistry(metadata);
                spc.AddSource("RecordRegistry.g.cs", registry);
            }
        });
    }
}
