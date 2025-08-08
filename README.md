# BlueHeron.CommandLine.CommandLineParser

## Introduction
CommandLineParser is a 15 Kb library for .NET 9 that allows developers to define and handle command-line arguments passed to a script or application. It simplifies the process of parsing arguments, validating inputs, and providing help messages to users.

## Features
- Easy definition of command-line arguments
- Automatic generation of help and usage messages
- Support for commands
- Support for optional and required arguments
- Support for list arguments
- Type checking and default values

## Limitations
For simplicity's sake the following limitations apply:
- Arguments and their values only follow this pattern: `/Argument:value`.
- Required fields start with a minus sign, e.g. `-RequiredArgument:"Argument value"`.
- Optional fields start with a forward slash, e.g. `/OptionalArgument:12345`.
- Commands also start with a forward slash, e.g. `/Copy -Source:"Sourcepath" -Target:"Destinationpath"`.
- Lists are populated by repeating the argument, e.g. `/Path:"Path1" /Path:"Path2" /Path:"Path3"`.
- Booleans only need to be set when the value is `true` and don't need a value, e.g. `/FailSilently`. `/FailSilenty:true` or `/FailSilenty:false` is accepted, but unnecessary.
 
## Installation
To use a command-line parser in C#, you can use the following options:

1. Install the `BlueHeron.Console` NuGet package via the .NET CLI:

```bash
dotnet add package BlueHeron.Console
```

2. Download the latest release here: [Releases](https://github.com/TheBlueHeron/BlueHeron.Console/releases)

## Usage
The parser takes any object as its options container in its constructor.  
Through reflection the object is analyzed for the presence of ```Command```, ```Argument```, ```Usage``` and ```Required``` attributes.  
These attributes control the (case-insensitive) validation of the arguments array in the parser's ```Parse(string[] args)``` function.  
All attributes can be set on public instance properties and fields of the object. Shared and non-public fields and properties are ignored.  
The value of the ```Command``` and ```Argument``` attributes don't need a prefix like '-' or '/'. These will be prepended automatically, depending on presence of a ```Required``` attribute.  
The ```Command``` attribute can only be set on fields and properties that represent classes, and that class will be treated as an options object for that particular command.  
The parser has a single settable property ```SkipUnrecognizedFields``` that causes the parser to not fail when it encounters a switch that is not defined on any of the properties and fields of the options object.  
The ```Commands```, ```OptionalArguments``` and ```RequiredArguments``` properties enumerate the recognized switches with their usage information.  

## Examples
##### See also: [Tests.cs](https://github.com/TheBlueHeron/BlueHeron.Console/blob/master/CommandLine.Tests/Tests.cs)

### Basic parsing
```csharp
[TestMethod]
public void BasicParsing()
{
    var options = new BasicOptions();
    using var parser = new CommandLineParser(options);

    var parsingOk = parser.Parse(["/A:ValueA", "/B:ValueB", "-C:ValueC", "/Bl1", "/Bl2:false", "-N1:12", "/N2:3.1415", "/E1:Value1"]); // set all fields and assert

    Assert.IsTrue(
        parsingOk &&
        options.EnumOption1 == MyEnum.Value1 &&
        options.BooleanOption0 == true &&
        options.BooleanOption1 == false &&
        options.NumberOption0 == 12 &&
        options.NumberOption1 == 3.1415 &&
        options.StringOptionA == "ValueA" &&
        options.StringOptionB == "ValueB" &&
        options.StringOptionC == "ValueC"
        );

    options = new BasicOptions();
    using var parser2 = new CommandLineParser(options); // recreate is necessary
    parsingOk = parser2.Parse(["-C:ValueC", "-N1:12"]); // set required fields only and assert
    Assert.IsTrue(
        parsingOk &&
        options.EnumOption1 == MyEnum.Value0 &&
        options.BooleanOption0 == false &&
        options.BooleanOption1 == false &&
        options.NumberOption0 == 12 &&
        options.NumberOption1 == 0 &&
        options.StringOptionA == null &&
        options.StringOptionB == null &&
        options.StringOptionC == "ValueC"
        );
}

/// <summary>
/// Public properties and fields should be recognized when at least the <see cref="ArgumentAttribute"/> is set.
/// Private, protected, internal and static fields and properties should be ignored in the constructor.
/// </summary>
internal class BasicOptions
{
    [Argument("A"), Usage("StringOptionA")]
    public string StringOptionA { get; set; } = null!;

    [Argument("B"), Usage("StringOptionB")]
    public string StringOptionB = null!;

    [Argument("C"), Usage("StringOptionC -> required"), Required]
    public string StringOptionC = null!;

    [Argument("Bl1"), Usage("BooleanOption0")]
    public bool BooleanOption0 { get; set; }

    [Argument("Bl2"), Usage("BooleanOption1")]
    public bool BooleanOption1 = false;

    [Argument("N1"), Usage("NumberOption0 -> required int"), Required]
    public int NumberOption0 { get; set; }

    [Argument("N2"), Usage("NumberOption1 -> double")]
    public double NumberOption1 = 0;

    [Argument("E1"), Usage("EnumOption1 -> MyEnum")] // both value name ('Value1') and value ('1') are accepted
    public MyEnum EnumOption1 { get; set; }

    // These fields will be ignored, no matter if Name attribute is set. The Required attribute asserts this

    [Argument("Faulty1"), Required] 
    public static string StaticField = "MyStaticField";

    public static int StaticProperty { get; set; } = 2;

    protected string ProtectedField = "MyProtectedField";

    [Argument("Faulty2"), Required]
    protected static int ProtectedProperty { get; set; } = 2;

    [Argument("Faulty3"), Required]
    internal string InternalField = "MyInternalField";
}

/// <summary>
/// Enum for asserting enum parsing. Both value by name (e.g. 'Value1') and value (e.g. '1') are accepted when parsing argument values.
/// </summary>
internal enum MyEnum
{
    Value0 = 0,
    Value1 = 1,
    Value2 = 2,
}

```

### List parsing

```csharp
[TestMethod]
public void ListParsing()
{
    var options = new ListOptions();
    using var parser = new CommandLineParser(options);

    var parsingOk = parser.Parse(["/Path:\"C:\\My Folder\\Some File.txt\"", "/Path:\'C:\\My Folder\\Another File.txt\'", "/Path:Foo.html"]);

    Assert.IsTrue(
        parsingOk &&
        options.Paths.Count == 3
        );

    var requiredOptions = new RequiredListOptions();
    using var requiredParser = new CommandLineParser(requiredOptions);

    parsingOk = requiredParser.Parse(["-Path:\"C:\\My Folder\\Some File.txt\"", "-Path:\'C:\\My Folder\\Another File.txt\'", "-Path:Foo.html"]);

    Assert.IsTrue(
        parsingOk &&
        requiredOptions.Paths.Count == 3
        );
}

/// <summary>
/// Should accept multiple '/Path' arguments. String field should accept all variants:
/// /Path:Foo.html, /Path:"C:\\My Folder\\Some File.txt", /Path:'C:\\My Folder\\Another File.txt'
/// </summary>
internal class ListOptions
{
    [Argument("Path"), Usage("Add path to the Paths collection")]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}

/// <summary>
/// Should pass only when at least one list item is set.
/// </summary>
internal class RequiredListOptions
{
    [Argument("Path"), Usage("Add path to the Paths collection"), Required]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}
```

### Command parsing

```csharp
[TestMethod]
public void CommandParsing()
{
    var options = new CommandOptions();
    using var parser = new CommandLineParser(options);
    bool parsingOk;

    parsingOk = parser.Parse(["/Copy", "-Source:\"C:\\My Folder\\Source File.txt\"", "-Target:\"C:\\My Folder\\Target File.txt\"", "/FailSilently:true"]);
    Assert.IsTrue(
        parsingOk &&
        options.FailSilently == true &&
        options.Copy.SourcePath == "C:\\My Folder\\Source File.txt" &&
        options.Copy.DestinationPath == "C:\\My Folder\\Target File.txt"
        );
}

/// <summary>
/// Nested options objects, i.e. the commands should all be parsed like the BasicOptions object. Usage info should handle the commands as well.
/// </summary>
internal class CommandOptions
{
    private CopyCommand mCopyCommand = null!;
    private DeleteCommand mDeleteCommand = null!;

    [Command("Copy"), Usage("Copy a file to a destination.")]
    public CopyCommand Copy
    {
        get
        {
            mCopyCommand ??= new CopyCommand();
            return mCopyCommand;
        }
    }

    [Command("Delete"), Usage("Delete a file.")]
    public DeleteCommand Delete
    {
        get
        {
            mDeleteCommand ??= new DeleteCommand();
            return mDeleteCommand;
        }
    }

    [Argument("FailSilently"), Usage("If true, the command will not throw an error on failure. Default: false.")]
    public bool FailSilently = false;
}

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

/// <summary>
/// Example of a delete command.
/// </summary>
internal class DeleteCommand
{
    [Argument("Source"), Usage("The full path to the source file."), Required]
    public string SourcePath { get; set; } = null!;
}
```

### Usage information output example:

```
Usage: C:\<Source>\BlueHeron.Console\CommandLine.Tests\bin\Release\net9.0\CommandLine.Tests.exe 

Options:
    /FailSilently (If true, the command will not throw an error on failure. Default: false.)

Commands:
/Copy (Copy a file to a destination.)

Usage: C:\<Source>\BlueHeron.Console\CommandLine.Tests\bin\Release\net9.0\CommandLine.Tests.exe /Copy -Source:value (The full path to the source file.) -Target:value (The full path to the destination file.)

Options:
    /Overwrite (If true, an existing file on the target will be overwritten. Default: true.)

/Delete (Delete a file.)

Usage: C:\<Source>\BlueHeron.Console\CommandLine.Tests\bin\Release\net9.0\CommandLine.Tests.exe /Delete -Source:value (The full path to the source file.)

```