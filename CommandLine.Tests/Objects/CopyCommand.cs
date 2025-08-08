namespace BlueHeron.CommandLine.Tests;

/// <summary>
/// Example of a copy command.
/// </summary>
internal class CopyCommand
{
    [Argument("Source"), Usage("The full path to the source file."), Required]
    public string SourcePath { get; set; } = null!;

    [Argument("Target"), Usage("The full path to the destination file."), Required]
    public string DestinationPath { get; set; } = null!;

    [Argument("Overwrite"), Usage("If true, an existing file on the target will be overwritten. Default: true.")]
    public bool Overwrite { get; set; } = true;
}