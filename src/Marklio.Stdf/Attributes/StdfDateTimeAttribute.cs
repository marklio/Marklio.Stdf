namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates that a <c>uint</c> property represents an STDF U*4 timestamp
/// and should be mapped to/from <see cref="DateTime"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class StdfDateTimeAttribute : Attribute;
