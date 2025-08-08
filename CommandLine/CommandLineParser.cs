using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace BlueHeron.CommandLine;

/// <summary>
/// Reflection based commandline options parser.
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
    private readonly Dictionary<string, CommandLineParser> mCommands = [];
    private readonly Dictionary<string, string> mCommandUsageInfo = [];
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
            var isCommand = field.IsCommand();
            var fieldName = isCommand ? field.GetCommandName() : field.GetOptionName();
            var fieldDescription = field.GetOptionDescription();

            fieldDescription = fieldDescription == null ? string.Empty : string.Format(Constants.fmtDescription, fieldDescription);

            if (isCommand)
            {
                var commandOptions = field.GetValue(options);
                if (commandOptions != null)
                {
                    var commandParser = new CommandLineParser(commandOptions, mOutput) { IgnoreUnrecognizedArgument = true };

                    mCommands.Add(fieldName.ToLowerInvariant(), commandParser);
                    mCommandUsageInfo.Add($"{fieldName}{fieldDescription}", commandParser.GetUsage(fieldName));
                }
            }
            else if (field.IsRequired())
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
    /// Returns the command switches available on the options object.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> Commands => mCommands.Keys.Select((c, i) => new KeyValuePair<string, string>(c, mCommandUsageInfo.ElementAt(i).Value));

    /// <summary>
    /// If <see langword="false"/> parsing will fail if an unrecognized argument is encountered.
    /// </summary>
    public bool IgnoreUnrecognizedArgument { get; set; } = false;

    /// <summary>
    /// Returns the optional switches available on the options object.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> OptionalArguments => mOptionalOptions.Keys.Select((o, i) => new KeyValuePair<string, string>(o, mOptionalUsageInfo[i]));

    /// <summary>
    /// Returns the required switches available on the options object.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> RequiredArguments => mRequiredOptions.Keys.Select((r, i) => new KeyValuePair<string, string>(r, mRequiredUsageInfo[i]));

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
    /// Returns the usage info as a string.
    /// </summary>
    /// <param name="commandName">Optional name of the command represented by this <see cref="CommandLineParser"/></param>
    [DebuggerStepThrough]
    public string GetUsage()
    {
        return GetUsage(null);
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
                if (!ParseArgument(arg.Trim(), args))
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
        WriteUsage(mOutput);
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
    /// Returns the usage info as a string.
    /// </summary>
    /// <param name="commandName">Optional name of the command represented by this <see cref="CommandLineParser"/></param>
    [DebuggerStepThrough]
    private string GetUsage(string? commandName = null)
    {
        var sb = new StringBuilder();
        using var writer = new StringWriter(sb);

        WriteUsage(writer, commandName);
        return sb.ToString();
    }

    /// <summary>
    /// Returns a boolean determining whether the required field that represents the given argument was successfully handled by this parser or any of the command parsers it contains.
    /// </summary>
    /// <param name="name">The argument to assert</param>
    /// <returns><see langword="true"/> if the required field was handled; else <see langword="false"/></returns>
    private bool IsRequiredFieldHandled(string name)
    {
        if (mRequiredOptions.TryGetValue(name, out var value) && value.Item2 == true)
        {
            return true;
        }
        return mCommands.Count > 0 && mCommands.Values.Where(p => p.IsRequiredFieldHandled(name)).Any();
    }

    /// <summary>
    /// Parses the given argument.
    /// </summary>
    /// <param name="arg">The current argument</param>
    /// <param name="args">All commandline arguments</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseArgument(string arg, string[] args)
    {
        if (arg.StartsWith(Constants.Slash)) // parse optional argument
        {
            if (mCommands.ContainsKey(arg[1..].ToLowerInvariant()))
            {
                return ParseCommandArgument(arg[1..].ToLowerInvariant(), args);
            }
            else
            {
                return ParseOptionalArgument(arg);
            }
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
    /// Parses the command with the given argument
    /// </summary>
    /// <param name="arg">The command argument</param>
    /// <param name="args">All commandline parameters</param>
    /// <returns>A <see langword="bool"/>, <see langword="true"/> if parsing was successful; else <see langword="false"/></returns>
    private bool ParseCommandArgument(string arg, string[] args)
    {
        return mCommands[arg].Parse(args);
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
            if (IgnoreUnrecognizedArgument)
            {
                return true;
            }
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
            if (IgnoreUnrecognizedArgument || IsRequiredFieldHandled(name.ToLowerInvariant()))
            {
                return true;
            }
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

    /// <summary>
    /// Writes usage info to the given <see cref="TextWriter"/>
    /// </summary>
    /// <param name="writer">The <see cref="TextWriter"/> to use</param>
    /// <param name="commandName">Optional name of the command represented by this <see cref="CommandLineParser"/></param>
    private void WriteUsage(TextWriter writer, string? commandName = null)
    {
        var name = Process.GetCurrentProcess().MainModule?.FileName ?? Constants.QUESTION;

        writer.WriteLine(Constants.fmtUsage, string.IsNullOrEmpty(commandName) ? name : $"{name} /{commandName}", string.Join(Constants.Space, mRequiredUsageInfo));
        if (mOptionalUsageInfo.Count != 0)
        {
            writer.WriteLine();
            writer.WriteLine(Constants.OPTIONS);
            foreach (var optional in mOptionalUsageInfo)
            {
                writer.WriteLine(Constants.fmtOptions, optional);
            }
        }
        if (mCommands.Count != 0)
        {
            writer.WriteLine();
            writer.WriteLine(Constants.COMMANDS);
            foreach (var command in mCommandUsageInfo)
            {
                writer.WriteLine(Constants.fmtCommand, command.Key, command.Value);
            }
        }
    }

    #endregion
}