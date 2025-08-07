using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Should pass only when at least one list item is set.
/// </summary>
internal class RequiredListOptions
{
    [Name("Path"), Description("Add path to the Paths collection"), Required]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}