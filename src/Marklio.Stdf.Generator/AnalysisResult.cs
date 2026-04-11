using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Marklio.Stdf.Generator;

/// <summary>
/// Carries record metadata and any diagnostics through the incremental pipeline.
/// </summary>
internal sealed class AnalysisResult : IEquatable<AnalysisResult>
{
    public RecordMetadata? Metadata { get; }
    public ImmutableArray<DiagnosticInfo> Diagnostics { get; }

    public AnalysisResult(RecordMetadata? metadata, IEnumerable<DiagnosticInfo> diagnostics)
    {
        Metadata = metadata;
        Diagnostics = diagnostics.ToImmutableArray();
    }

    public bool Equals(AnalysisResult? other)
    {
        if (other is null) return false;
        if (!Equals(Metadata, other.Metadata)) return false;
        if (Diagnostics.Length != other.Diagnostics.Length) return false;
        for (int i = 0; i < Diagnostics.Length; i++)
        {
            if (!Diagnostics[i].Equals(other.Diagnostics[i])) return false;
        }
        return true;
    }

    public override bool Equals(object? obj) => Equals(obj as AnalysisResult);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = Metadata?.GetHashCode() ?? 0;
            hash = hash * 31 + Diagnostics.Length;
            return hash;
        }
    }
}
