namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to provide a description to be displayed when retrieving usage info.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class UsageAttribute : Attribute
{
    /// <summary>
    /// Creates a new <see cref="UsageAttribute"/>.
    /// </summary>
    /// <param name="usage">The usage information for the switch</param>
    public UsageAttribute(string usage)
    {
        ArgumentException.ThrowIfNullOrEmpty(usage, nameof(usage));
        Value = usage;
    }

    /// <summary>
    /// Gets the description of the commandline option.
    /// </summary>
    public string Value { get; }
}