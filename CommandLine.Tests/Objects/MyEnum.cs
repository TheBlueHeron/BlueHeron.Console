namespace BlueHeron.CommandLine.Tests;

/// <summary>
/// Enum for asserting enum parsing. Both value by name (e.g. 'Value1') and value (e.g. '1') are accepted when parsing argument values.
/// </summary>
internal enum MyEnum
{
    Value0 = 0,
    Value1 = 1,
    Value2 = 2,
}