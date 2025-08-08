namespace BlueHeron.CommandLine.Tests;

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