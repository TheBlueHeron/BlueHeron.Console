namespace BlueHeron.CommandLine.Tests;

/// <summary>
/// Example of a delete command.
/// </summary>
internal class DeleteCommand
{
    [Argument("Source"), Usage("The full path to the source file."), Required]
    public string SourcePath { get; set; } = null!;
}