using System.Text;
using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

[TestClass]
public sealed class Tests(TestContext ctx)
{
    [TestMethod]
    public void TestUsageInfo()
    {
        var options = new BasicOptions();
        var sb = new StringBuilder();
        using var parser = new CommandLineParser(options, new StringWriter(sb));
        
        parser.Usage();

        var usage = sb.ToString();

        Assert.IsNotNull(usage);
        ctx.WriteLine(usage);
    }

    [TestMethod]
    public void BasicParsing()
    {
        var options = new BasicOptions();
        using var parser = new CommandLineParser(options);

        var parsingOk = parser.Parse(["/A:ValueA", "/B:ValueB", "-C:ValueC", "/Bl1", "/Bl2:false", "-N1:12", "/N2:3.1415"]); // set all fields and assert

        Assert.IsTrue(
            parsingOk &&
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
            options.BooleanOption0 == false &&
            options.BooleanOption1 == false &&
            options.NumberOption0 == 12 &&
            options.NumberOption1 == 0 &&
            options.StringOptionA == null &&
            options.StringOptionB == null &&
            options.StringOptionC == "ValueC"
            );
    }

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

    [TestMethod]
    public void BasicParsingErrors()
    {
        var options = new BasicOptions();
        var sb = new StringBuilder();
        var parser = new CommandLineParser(options, new StringWriter(sb));
        bool parsingOk;
        
        parsingOk = parser.Parse(["-N1:12", "-C:"]); // empty required value C
        Assert.IsFalse(parsingOk);
        ctx.WriteLine(sb.ToString());
        sb.Clear();

        parsingOk = parser.Parse(["-N1:12", "-C"]); // empty required value C
        Assert.IsFalse(parsingOk);
        ctx.WriteLine(sb.ToString());
        sb.Clear();

        parsingOk = parser.Parse(["-N1:12"]); // missing required value C
        Assert.IsFalse(parsingOk);
        ctx.WriteLine(sb.ToString());
        sb.Clear();

        parsingOk = parser.Parse(["-N1:Boo", "-C:ValueC"]); // invalid value N1
        Assert.IsFalse(parsingOk);
        ctx.WriteLine(sb.ToString());
        sb.Clear();
    }
}

/// <summary>
/// Public properties and fields should be recognized when at least the Name attribute is set.
/// Private, protected, internal and static fields and properties should be ignored in the constructor.
/// </summary>
public class BasicOptions
{
    [Name("A"), Description("StringOptionA")]
    public string StringOptionA { get; set; } = null!;

    [Name("B"), Description("StringOptionB")]
    public string StringOptionB = null!;

    [Name("C"), Description("StringOptionC -> required"), Required]
    public string StringOptionC = null!;

    [Name("Bl1"), Description("BooleanOption0")]
    public bool BooleanOption0 { get; set; }

    [Name("Bl2"), Description("BooleanOption1")]
    public bool BooleanOption1;

    [Name("N1"), Description("NumberOption0 -> required int"), Required]
    public int NumberOption0 { get; set; }

    [Name("N2"), Description("NumberOption1 -> double")]
    public double NumberOption1;

    // These fields will be ignored, no matter if Name attribute is set. The Required attribute asserts this

    [Name("Faulty1"), Required] 
    public static string StaticField = "MyStaticField";

    public static int StaticProperty { get; set; } = 2;

    protected string ProtectedField = "MyProtectedField";

    [Name("Faulty2"), Required]
    protected static int ProtectedProperty { get; set; } = 2;

    [Name("Faulty3"), Required]
    internal string InternalField = "MyInternalField";
}

/// <summary>
/// Should accept multiple '/Path' arguments. String field should accept all variants:
/// /Path:Foo.html, /Path:"C:\\My Folder\\Some File.txt", /Path:'C:\\My Folder\\Another File.txt'
/// </summary>
public class ListOptions
{
    [Name("Path"), Description("Add path to the Paths collection")]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}

/// <summary>
/// Should pass only when at least one list item is set.
/// </summary>
public class RequiredListOptions
{
    [Name("Path"), Description("Add path to the Paths collection"), Required]
    public List<string> Paths { get; set; } = []; // shouldn't be null
}