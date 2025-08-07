using BlueHeron.CommandLine;
using Description = BlueHeron.CommandLine.DescriptionAttribute;

namespace CommandLine.Tests;

/// <summary>
/// Public properties and fields should be recognized when at least the Name attribute is set.
/// Private, protected, internal and static fields and properties should be ignored in the constructor.
/// </summary>
internal class BasicOptions
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
    public bool BooleanOption1 = false;

    [Name("N1"), Description("NumberOption0 -> required int"), Required]
    public int NumberOption0 { get; set; }

    [Name("N2"), Description("NumberOption1 -> double")]
    public double NumberOption1 = 0;

    [Name("E1"), Description("EnumOption1 -> MyEnum")] // both value name ('Value1') and value ('1') are accepted
    public MyEnum EnumOption1 { get; set; }

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