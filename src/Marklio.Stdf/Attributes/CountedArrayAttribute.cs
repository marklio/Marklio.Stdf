namespace Marklio.Stdf.Attributes;

/// <summary>
/// Marks an array property as part of a counted array group.
/// The array's element count on the wire is determined by the
/// <see cref="WireCountAttribute"/> property with the matching group name.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CountedArrayAttribute : Attribute
{
    /// <summary>
    /// Group name linking this array to its <see cref="WireCountAttribute"/> count field.
    /// </summary>
    public string GroupName { get; }

    public CountedArrayAttribute(string groupName)
    {
        GroupName = groupName;
    }
}
