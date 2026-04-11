namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <see cref="System.Collections.BitArray"/> property maps to
/// STDF D*n (bit-count-prefixed bit-encoded data).
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BitArrayAttribute : Attribute;
