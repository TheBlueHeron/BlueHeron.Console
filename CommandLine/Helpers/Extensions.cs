using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace BlueHeron.CommandLine;

/// <summary>
/// Extension functions for <see cref="FieldInfo"/>.
/// </summary>
internal static class Extensions
{
    #region MemberInfo

    /// <summary>
    /// Returns the <see cref="Attribute"/> of type <typeparamref name="T"/> on the given <see cref="MemberInfo"/> if present; else <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Attribute"/></typeparam>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static T? GetAttribute<T>(this MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttributes(typeof(T), false).OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Returns either the <see cref="FieldInfo.FieldType"/> or the <see cref="PropertyInfo.PropertyType"/>, depending on the <see cref="MemberInfo.MemberType"/>.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    /// <returns>The <see cref="Type"/></returns>
    [DebuggerStepThrough]
    internal static Type GetFieldType(this MemberInfo member)
    {
        return member.MemberType == MemberTypes.Field ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType ;
    }

    /// <summary>
    /// Returns the <see cref="IList"/> represented by this <see cref="MemberInfo"/> on the given options object.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static IList? GetList(this MemberInfo member, object options)
    {
        return (IList?)member.GetValue(options);
    }

    /// <summary>
    /// Gets the type of the element in the <see cref="IList"/>, represented by this <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static Type GetListElementType(this MemberInfo member)
    {
        return GetFieldType(member).GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)).First().GetGenericArguments()[0];
    }

    /// <summary>
    /// Returns the value of the <see cref="DescriptionAttribute"/> if present; else <see langword="null"/>.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static string? GetOptionDescription(this MemberInfo member)
    {
        return member.GetAttribute<DescriptionAttribute>()?.Description;
    }

    /// <summary>
    /// Returns the value of the <see cref="NameAttribute"/> if present; else the <see cref="MemberInfo.Name"/> value.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static string GetOptionName(this MemberInfo member)
    {
        return member.GetAttribute<NameAttribute>()?.Name ?? member.Name;
    }

    /// <summary>
    /// Teturns the value of this <see cref="MemberInfo"/>.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    /// <param name="options">The options object</param>
    [DebuggerStepThrough]
    internal static object? GetValue(this MemberInfo member, object options)
    {
    return member.MemberType == MemberTypes.Field ? ((FieldInfo)member).GetValue(options) : ((PropertyInfo)member).GetValue(options);
    }

    /// <summary>
    /// Returns a <see langword="bool"/> determining whether this <see cref="MemberInfo"/> represents an <see cref="IList"/> implementation.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"</param>
    [DebuggerStepThrough]
    internal static bool IsList(this MemberInfo member)
    {
        return typeof(IList).IsAssignableFrom(GetFieldType(member));
    }

    /// <summary>
    /// Returns a <see langword="bool"/>, determining whether the option represented by this <see cref="MemberInfo"/> is required (i.e. has a <see cref="RequiredAttribute"/> defined on it).
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    [DebuggerStepThrough]
    internal static bool IsRequired(this MemberInfo member)
    {
        return GetAttribute<RequiredAttribute>(member) != null;
    }

    /// <summary>
    /// Sets the given value of this <see cref="MemberInfo"/> on the given options object.
    /// </summary>
    /// <param name="member">This <see cref="MemberInfo"/></param>
    /// <param name="options">The options object</param>
    /// <param name="value">The value to set</param>
    [DebuggerStepThrough]
    internal static void SetValue(this MemberInfo member, object options, object? value)
    {
        if (member.MemberType == MemberTypes.Field)
        {
            ((FieldInfo)member).SetValue(options, value);
        }
        else
        {
            ((PropertyInfo)member).SetValue(options, value);
        }
    }

    #endregion
}