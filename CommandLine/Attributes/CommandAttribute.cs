namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to indicate it represents a command with its own options.
/// The field must be a class (i.e. is an options object itself).
/// </summary>
/// <param name="name">The commandline option</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="CommandAttribute"/>.
    /// </summary>
    /// <param name="name">The name of the switch</param>
    public CommandAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        Value = name;
    }

    /// <summary>
    /// Gets the name of the commandline switch.
    /// </summary>
    public string Value { get; }
}