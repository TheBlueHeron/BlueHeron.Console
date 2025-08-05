using System.Text;
using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

[TestClass]
public sealed class Tests
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
    }

    [TestMethod]
    public void BasicParsing()
    {
        var options = new BasicOptions();
        using var parser = new CommandLineParser(options);

        parser.Parse(["/A:ValueA", "/B:ValueB", "-C:ValueC", "/Bl1", "/Bl2:false", "-N1:12", "/N2:3.1415"]);

        Assert.IsTrue(
            options.BooleanOption0 == true &&
            options.BooleanOption1 == false &&
            options.NumberOption0 == 12 &&
            options.NumberOption1 == 3.1415 &&
            options.StringOptionA == "ValueA" &&
            options.StringOptionB == "ValueB" &&
            options.StringOptionC == "ValueC"
            );
    }

    //public void BasicParsingErrors()
    //{
    //    var options = new BasicOptions();
    //    var parser = new CommandLineParser(options);

    //    parser.Parse(["/A:ValueA", "/B:ValueB", "-C:"]);
    //}
}

/// <summary>
/// Public properties and fields should be recognized.
/// Private, protected, internal and static fields and properties should be ignored.
/// </summary>
public class BasicOptions
{
    [Name("A"), Description("StringOptionA")]
    public string StringOptionA { get; set; } = null!;

    [Name("B"), Description("StringOptionB")]
    public string StringOptionB = null!;

    [Name("C"), Description("StringOptionC"), Required]
    public string StringOptionC = null!;

    [Name("Bl1"), Description("BooleanOption0 -> true")]
    public bool BooleanOption0 { get; set; }

    [Name("Bl2"), Description("BooleanOption1 -> false")]
    public bool BooleanOption1;

    [Name("N1"), Description("NumberOption0 -> int"), Required]
    public int NumberOption0 { get; set; }

    [Name("N2"), Description("NumberOption1 -> double")]
    public double NumberOption1;
}