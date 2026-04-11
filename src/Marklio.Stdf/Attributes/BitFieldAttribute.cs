namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <c>byte</c> property maps to STDF B*1 (bit-encoded flags)
/// rather than the default U*1.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BitFieldAttribute : Attribute;
