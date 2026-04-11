using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Marklio.Stdf.Generator;

/// <summary>
/// Serializable snapshot of a diagnostic to carry through the incremental pipeline.
/// </summary>
internal sealed class DiagnosticInfo : IEquatable<DiagnosticInfo>
{
    public DiagnosticDescriptor Descriptor { get; }
    public string FilePath { get; }
    public TextSpan Span { get; }
    public LinePositionSpan LineSpan { get; }
    public object[] MessageArgs { get; }

    public DiagnosticInfo(DiagnosticDescriptor descriptor, Location location, params object[] messageArgs)
    {
        Descriptor = descriptor;
        MessageArgs = messageArgs;

        if (location.SourceTree is not null)
        {
            FilePath = location.SourceTree.FilePath;
            Span = location.SourceSpan;
            LineSpan = location.SourceTree.GetLineSpan(location.SourceSpan).Span;
        }
        else
        {
            FilePath = "";
            Span = default;
            LineSpan = default;
        }
    }

    public Diagnostic ToDiagnostic()
    {
        var location = Location.Create(FilePath, Span, LineSpan);
        return Diagnostic.Create(Descriptor, location, MessageArgs);
    }

    public bool Equals(DiagnosticInfo? other)
    {
        if (other is null) return false;
        return Descriptor.Id == other.Descriptor.Id
            && FilePath == other.FilePath
            && Span == other.Span;
    }

    public override bool Equals(object? obj) => Equals(obj as DiagnosticInfo);

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (Descriptor.Id?.GetHashCode() ?? 0);
            hash = hash * 31 + (FilePath?.GetHashCode() ?? 0);
            hash = hash * 31 + Span.GetHashCode();
            return hash;
        }
    }
}
