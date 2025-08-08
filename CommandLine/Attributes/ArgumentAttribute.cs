namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to define the corresponding commandline option.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class ArgumentAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="ArgumentAttribute"/>.
    /// </summary>
    /// <param name="name">The name of the switch</param>
    public ArgumentAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        Value = name;
    }

    /// <summary>
    /// Gets the name of the commandline switch.
    /// </summary>
    public string Value { get; }
}