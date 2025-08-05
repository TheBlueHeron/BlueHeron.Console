namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to rename the corresponding commandline option.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class NameAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the name of the commandline option.
    /// </summary>
    public string Name { get; private set; } = name;
}