using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace BlueHeron.CommandLine;

/// <summary>
/// Reflection based commandline options parser.
/// The value of a (1st and only) argument starting with '@' is treated as a text file where each line contains an argument and its value, if needed.
/// Arguments starting with a forward slash are treated as optional arguments.
/// Arguments starting with a minus sign are treated as required arguments.
/// Argument '?' or 'help' will cause output of usage information.
/// </summary>
public sealed partial class CommandLineParser : IDisposable
{
    #region Objects and variables

    private bool mDisposed;
    private readonly object mOptions;
    private readonly TextWriter mOutput;
    private readonly Dictionary<string, MemberInfo> mOptionalOptions = [];
	private readonly List<string> mOptionalUsageInfo = [];
	private readonly Dictionary<string, Tuple<MemberInfo, bool>> mRequiredOptions = [];
	private readonly List<string> mRequiredUsageInfo = [];

    private readonly char[] mQuotes = [Constants.Quote, Constants.Apostrophe];
    private readonly char[] mSeparators = [Constants.Colon, Constants.Equal];

    #endregion

    #region Construction and destruction

    /// <summary>
    /// Creates a new <see cref="CommandLineParser"/> based on fields of the given options object.
    /// </summary>
    /// <param name="options">The object, defining command line options</param>
    /// <param name="output">The <see cref="TextWriter"/> to use as output. If <see cref="null"/>, the <see cref="Console.Out"/> will be used</param>
    public CommandLineParser(object options, TextWriter? output = null)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        mOptions = options;
        mOutput = output ?? Console.Out;

        foreach (var field in mOptions.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)) // loop over defined public instance fields and properties
        {
            var fieldName = field.GetOptionName();
            var fieldDescription = field.GetOptionDescription();

            fieldDescription = fieldDescription == null ? string.Empty : string.Format(Constants.fmtDescription, fieldDescription);
            if (field.IsRequired())
            {
                mRequiredOptions.Add(fieldName.ToLowerInvariant(), new(field, false));
                mRequiredUsageInfo.Add(string.Format(Constants.fmtRequired, fieldName, fieldDescription));
            }
            else
            {
                mOptionalOptions.Add(fieldName.ToLowerInvariant(), field);

                var fieldType = field.MemberType == MemberTypes.Field ? ((FieldInfo)field).FieldType : ((PropertyInfo)field).PropertyType;
                if (fieldType == typeof(bool))
                {
                    mOptionalUsageInfo.Add(string.Format(Constants.fmtOptionalBool, fieldName, fieldDescription));
                }
                else
                {
                    mOptionalUsageInfo.Add(string.Format(Constants.fmtOptional, fieldName, fieldDescription));
                }
            }
        }
        mOptionalUsageInfo.Sort();
        mRequiredUsageInfo.Sort();
    }

    /// <summary>
    /// Releases managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    #region Properties

    /// <summary>
    /// If <see langword="false"/> parsing will fail if an unrecognized argument is encountered.
    /// </summary>
    public bool IgnoreUnrecognizedArgument { get; set; } = false;

    #endregion

    #region Public methods and functions

    /// <summary>
    /// Outputs an error message to the configured <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="message">The message</param>
    /// <param name="args">The arguments</param>
    [DebuggerStepThrough]
    public void Error(string message, params object[] args)
    {
        var name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

        mOutput.WriteLine(message, args);
        mOutput.WriteLine();
        mOutput.WriteLine(Constants.fmtUsage, name, string.Join(Constants.Space, mRequiredUsageInfo));
        mOutput.WriteLine();
        mOutput.WriteLine(Constants.OPTIONS);

        foreach (var optional in mOptionalUsageInfo)
        {
            mOutput.WriteLine(Constants.fmtOptions, optional);
        }
    }

    /// <summary>
    /// Parses the commandline and returns <see langword="true"/> if the operation was successful.
    /// </summary>
    /// <param name="args">The commandline arguments</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/>.</returns>
    public bool Parse(string[] args)
    {
        if (args.Contains(Constants.QUESTION) || args.Contains(Constants.HELP))
        {
            Usage();
            return false;
        }
        else
        {
            foreach (var arg in args)
            {
                if (!ParseArgument(arg.Trim()))
                {
                    return false;
                }
            }

            var missingRequiredOption = mRequiredOptions.Values.FirstOrDefault(field => !field.Item2)?.Item1; // ensure all required options are present

            if (missingRequiredOption != null)
            {
                Error(string.Format(Constants.errMissing, missingRequiredOption.GetOptionName()));
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Outputs usage information to the configured <see cref="TextWriter"/>.
    /// </summary>
    [DebuggerStepThrough]
    public void Usage()
    {
        var name = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName);

        mOutput.WriteLine(Constants.fmtUsage, name, string.Join(Constants.Space, mRequiredUsageInfo));
        mOutput.WriteLine();
        mOutput.WriteLine(Constants.OPTIONS);

        foreach (var optional in mOptionalUsageInfo)
        {
            mOutput.WriteLine(Constants.fmtOptions, optional);
        }
    }

    #endregion

    #region Private methods and functions

    /// <summary>
    /// Converts the given string value into the given <see cref="Type"/>.
    /// </summary>
    /// <param name="value">The string representation of the value to convert</param>
    /// <param name="type">The <see cref="Type"/> to convert the value into</param>
    /// <returns>The converted value</returns>
    [DebuggerStepThrough]
    private object? ChangeType(string value, Type type)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }
        if (type == typeof(string))
        {
            return value.Trim(mQuotes);
        }
        return TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
    }

    /// <summary>
    /// Disposes the <see cref="TextWriter"/>.
    /// </summary>
    /// <param name="disposing">Flag to prevent spurious calls</param>
    private void Dispose(bool disposing)
    {
        if (!mDisposed)
        {
            if (disposing)
            {
                mOutput?.Dispose();
            }
            mDisposed = true;
        }
    }

    /// <summary>
    /// Parses the given argument.
    /// </summary>
    /// <param name="arg">The argument</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseArgument(string arg)
    {
        if (arg.StartsWith(Constants.At))
        {
            return ParseFile(arg[1..]); // parse argument file
        }
        else if (arg.StartsWith(Constants.Slash)) // parse optional argument
        {
            return ParseOptionalArgument(arg);
        }
        else if (arg.StartsWith(Constants.Minus)) // parse required argument
        {
            return ParseRequiredArgument(arg);
        }
        else
        {
            return IgnoreUnrecognizedArgument;
        }
    }

    /// <summary>
    /// Parses the file with the given name.
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseFile(string fileName)
    {
        string[] lines;

        try
        {
            lines = File.ReadAllLines(fileName);
        }
        catch
        {
            Error(Constants.errResFile, fileName);
            return false;
        }

        return lines.Select(line => line.Trim())
                    .Where(line => !string.IsNullOrEmpty(line))
                    .All(ParseArgument);
    }

    /// <summary>
    /// Parses the given optional argument.
    /// </summary>
    /// <param name="arg">The argument</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseOptionalArgument(string arg)
    {
        var split = arg[1..].Split(mSeparators, 2, StringSplitOptions.None);
        var name = split[0];
        MemberInfo? field;

        if (!mOptionalOptions.TryGetValue(name.ToLowerInvariant(), out field))
        {
            Error(Constants.errUnknown, name);
            return false;
        }
        var fieldType = field.GetFieldType();
        var value = (split.Length > 1) ? split[1] : fieldType == typeof(bool) ? Constants.TRUE : string.Empty;
        return SetOption(field, value);
    }

    /// <summary>
    /// Parses the given required argument.
    /// </summary>
    /// <param name="arg">The argument</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseRequiredArgument(string arg)
    {
        var split = arg[1..].Split(mSeparators, 2, StringSplitOptions.None);
        var name = split[0];
        Tuple<MemberInfo, bool>? field;

        if (!mRequiredOptions.TryGetValue(name.ToLowerInvariant(), out field))
        {
            Error(Constants.errUnknown, name);
            return false;
        }
        var fieldType = field.Item1.GetFieldType();
        var value = (split.Length > 1) ? split[1] : fieldType == typeof(bool) ? Constants.TRUE : string.Empty;
        if (string.IsNullOrEmpty(value))
        {
            Error(Constants.errMissing, name);
            return false;
        }
        if (SetOption(field.Item1, value))
        {
            mRequiredOptions[name.ToLowerInvariant()] = new(field.Item1, true);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the given value.
    /// </summary>
    /// <param name="member">The <see cref="MemberInfo"/> to set</param>
    /// <param name="value">The value</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if the value was properly set</returns>
    private bool SetOption(MemberInfo member, string value)
    {
        try
        {
            if (member.IsList())
            {
                member.GetList(mOptions)?.Add(ChangeType(value, member.GetListElementType())); // append the value to the list of options
            }
            else
            {
                member.SetValue(mOptions, ChangeType(value, member.GetFieldType())); // set the value of a single option
            }
            return true;
        }
        catch
        {
            Error(Constants.errInvalid, value, member.GetOptionName());
            return false;
        }
    }

    #endregion
}