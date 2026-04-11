namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <c>byte</c> or <c>byte[]</c> property maps to STDF N*1 nibble type.
/// In arrays, nibbles are packed two-per-byte on the wire.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NibbleAttribute : Attribute;
