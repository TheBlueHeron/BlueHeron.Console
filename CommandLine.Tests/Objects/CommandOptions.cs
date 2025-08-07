using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Nested options objects, i.e. the commands should all be parsed like the BasicOptions object. Usage info should handle the commands as well.
/// </summary>
internal class CommandOptions
{
    private readonly CopyCommand mCopyCommand = new();
    private readonly DeleteCommand mDeleteCommand = new();

    [Command("Copy"), Description("Copy a file to a destination.")]
    public CopyCommand Copy => mCopyCommand;

    [Command("Delete"), Description("Delete a file.")]
    public DeleteCommand Delete => mDeleteCommand;

    [Name("FailSilently"), Description("If true, the command will not throw an error on failure. Default: false.")]
    public bool FailSilently = false;
}