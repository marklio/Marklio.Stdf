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
        var recordTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Marklio.Stdf.Attributes.StdfRecordAttribute",
                predicate: (node, _) => node is TypeDeclarationSyntax,
                transform: (ctx, _) => RecordAnalyzer.Analyze(ctx))
            .Where(m => m is not null)
            .Select((m, _) => m!)
            .Collect();

        context.RegisterSourceOutput(recordTypes, static (ctx, records) => EmitGeneratedCode(ctx, records));
    }

    private static void EmitGeneratedCode(SourceProductionContext context, ImmutableArray<RecordMetadata> records)
    {
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
