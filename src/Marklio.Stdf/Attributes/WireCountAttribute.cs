namespace Marklio.Stdf.Attributes;

/// <summary>
/// Private wire-position marker for an STDF counted-array count field.
/// Place this on a private, expression-bodied throwing property to define
/// where the count field appears in the serialization order.
/// The return type determines the STDF count type (e.g., <c>ushort</c> → U*2, <c>byte</c> → U*1).
/// The property body is never called by generated code.
/// </summary>
/// <example>
/// <code>
/// [WireCount("rtn")] private ushort ReturnCount => throw new NotSupportedException();
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class WireCountAttribute : Attribute
{
    /// <summary>Group name linking this count to its <see cref="CountedArrayAttribute"/> arrays.</summary>
    public string GroupName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WireCountAttribute"/> class
    /// with the specified group name.
    /// </summary>
    /// <param name="groupName">The group name linking this count field to its corresponding <see cref="CountedArrayAttribute"/> arrays.</param>
    public WireCountAttribute(string groupName)
    {
        GroupName = groupName;
    }
}
