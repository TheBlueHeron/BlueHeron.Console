namespace BlueHeron.CommandLine;

/// <summary>
/// Container for constant values.
/// </summary>
internal class Constants
{
    #region Errors

    internal const string errInvalid = "Invalid value '{0}' for option '{1}'";
    internal const string errMissing = "Missing required argument '{0}'";
    internal const string errResFile = "Error reading response file '{0}'";
    internal const string errTooMany = "Too many arguments";
    internal const string errUnknown = "Unknown option '{0}'";

    #endregion

    #region Formats

    internal const string fmtDescription = " ({0})";
    internal const string fmtOptional = "/{0}:value{1}";
    internal const string fmtOptionalBool = "/{0}{1}";
    internal const string fmtOptions = "    {0}";
    internal const string fmtRequired = "-{0}:value{1}"; // no bool; only needs to be set if true
    internal const string fmtUsage = "Usage: {0} {1}";

    #endregion

    #region Char

    internal const char Apostrophe = '\'';
    internal const char At = '@';
    internal const char Colon = ':';
    internal const char Equal = '=';
    internal const char Minus = '-';
    internal const char Quote = '\"';
    internal const char Slash = '/';
    internal const char Space = ' ';

    #endregion

    internal const string HELP = "help";
    internal const string OPTIONS = "Options:";
    internal const string QUESTION = "?";
    internal const string TRUE = "true";
}