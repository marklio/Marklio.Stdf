namespace Marklio.Stdf.Attributes;

/// <summary>
/// Marks a <see cref="string"/> property as STDF S*n type (2-byte length-prefixed string).
/// S*n is a V4-2007 string type allowing strings up to 65535 bytes.
/// Without this attribute, string properties default to C*n (1-byte length prefix, max 255 bytes).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SnAttribute : Attribute;
