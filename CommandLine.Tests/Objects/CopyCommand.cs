using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Example of a copy command.
/// </summary>
internal class CopyCommand
{
    [Name("Source"), Description("The full path to the source file."), Required]
    public string SourcePath { get; set; } = null!;

    [Name("Target"), Description("The full path to the destination file."), Required]
    public string DestinationPath { get; set; } = null!;

    [Name("Overwrite"), Description("If true, an existing file on the target will be overwritten. Default: true.")]
    public bool Overwrite { get; set; } = true;
}