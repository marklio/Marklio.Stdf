namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <c>string</c> property maps to STDF C*f (fixed-length character string)
/// rather than the default C*n (length-prefixed).
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class FixedStringAttribute : Attribute
{
    /// <summary>The fixed length in bytes.</summary>
    public int Length { get; }

    public FixedStringAttribute(int length)
    {
        Length = length;
    }
}
