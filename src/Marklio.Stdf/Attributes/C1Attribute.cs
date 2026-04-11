namespace Marklio.Stdf.Attributes;

/// <summary>
/// Indicates a <c>char</c> property maps to STDF C*1 (single character)
/// rather than being inferred as U*1.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class C1Attribute : Attribute;
