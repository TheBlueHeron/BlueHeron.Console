namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to provide a description to be displayed when retrieving usage info.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class DescriptionAttribute(string description) : Attribute
{
    /// <summary>
    /// Gets the description of the commandline option.
    /// </summary>
    public string Description { get; private set; } = description;
}