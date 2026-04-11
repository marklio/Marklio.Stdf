namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <c>byte[]</c> property maps to STDF B*n (variable-length bit-encoded data,
/// prefixed by a byte count).
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BitEncodedAttribute : Attribute;
