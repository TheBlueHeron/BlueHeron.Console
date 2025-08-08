namespace BlueHeron.CommandLine.Tests;

/// <summary>
/// Should pass only when at least one list item is set.
/// </summary>
internal class RequiredListOptions
{
    [Argument("Path"), Usage("Add path to the Paths collection"), Required]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}