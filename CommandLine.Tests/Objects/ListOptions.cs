using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Should accept multiple '/Path' arguments. String field should accept all variants:
/// /Path:Foo.html, /Path:"C:\\My Folder\\Some File.txt", /Path:'C:\\My Folder\\Another File.txt'
/// </summary>
internal class ListOptions
{
    [Name("Path"), Description("Add path to the Paths collection")]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}