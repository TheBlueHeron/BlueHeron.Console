namespace BlueHeron.CommandLine;

/// <summary>
/// <see cref="Attribute"/> that can be used on the field of an options Object to indicate that the corresponding commandline option is mandatory.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RequiredAttribute : Attribute { }