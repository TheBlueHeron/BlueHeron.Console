using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Example of a delete command.
/// </summary>
internal class DeleteCommand
{
    [Name("Source"), Description("The full path to the source file."), Required]
    public string SourcePath { get; set; } = null!;
}