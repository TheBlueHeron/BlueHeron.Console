using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Nested options objects, i.e. the commands should all be parsed like the BasicOptions object. Usage info should handle the commands as well.
/// </summary>
internal class CommandOptions
{
    private CopyCommand mCopyCommand = null!;
    private DeleteCommand mDeleteCommand = null!;

    [Command("Copy"), Description("Copy a file to a destination.")]
    public CopyCommand Copy
    {
        get
        {
            mCopyCommand ??= new CopyCommand();
            return mCopyCommand;
        }
    }

    [Command("Delete"), Description("Delete a file.")]
    public DeleteCommand Delete
    {
        get
        {
            mDeleteCommand ??= new DeleteCommand();
            return mDeleteCommand;
        }
    }

    [Name("FailSilently"), Description("If true, the command will not throw an error on failure. Default: false.")]
    public bool FailSilently = false;
}