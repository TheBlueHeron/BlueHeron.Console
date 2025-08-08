using System.Text;
using BlueHeron.CommandLine;

namespace BlueHeron.CommandLine.Tests;

[TestClass]
public sealed class Tests(TestContext ctx)
{
    [TestMethod]
    public void TestBasicUsageInfo()
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
    public void TestCommandUsageInfo()
    {
        var options = new CommandOptions();
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