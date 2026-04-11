using Microsoft.CodeAnalysis;

namespace Marklio.Stdf.Generator;

/// <summary>
/// Diagnostic descriptors reported by the STDF source generator.
/// </summary>
internal static class Diagnostics
{
    public static readonly DiagnosticDescriptor UnsupportedPropertyType = new(
        id: "STDF001",
        title: "Unsupported property type",
        messageFormat: "Property '{0}' has unsupported type '{1}' for STDF serialization",
        category: "Marklio.Stdf.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CountedArrayOnNonArray = new(
        id: "STDF002",
        title: "[CountedArray] applied to non-array property",
        messageFormat: "Property '{0}' has [CountedArray] but its type '{1}' is not an array",
        category: "Marklio.Stdf.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor WireCountGroupUnmatched = new(
        id: "STDF003",
        title: "[WireCount] group has no matching [CountedArray]",
        messageFormat: "[WireCount] on property '{0}' references group '{1}' but no [CountedArray] property uses that group",
        category: "Marklio.Stdf.Generator",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FixedStringOnNonString = new(
        id: "STDF004",
        title: "[FixedString] applied to non-string property",
        messageFormat: "Property '{0}' has [FixedString] but its type '{1}' is not string",
        category: "Marklio.Stdf.Generator",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
