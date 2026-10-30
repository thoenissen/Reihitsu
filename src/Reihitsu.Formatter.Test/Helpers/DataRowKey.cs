using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Formats the arguments of a data-driven test execution into the text that keys the execution in the phase-level
/// stability check. The text is unique: two different argument lists never give the same text, because every argument
/// is written as a self-delimiting token — a quoted and escaped string or character, a bracketed array, or a scalar
/// whose text holds no separator, quote, or bracket — and scalars of different types carry different spellings. An
/// argument the text cannot represent fails at test start with a message that names it, instead of colliding with
/// another row. Type names are written as full names, so a type whose full name another loaded assembly also defines
/// is refused
/// </summary>
public static class DataRowKey
{
    #region Constants

    /// <summary>
    /// The separator between the arguments of a row and between the elements of an array
    /// </summary>
    private const string Separator = ", ";

    /// <summary>
    /// The characters a type name written without quotes must not contain, so the token it starts stays self-delimiting
    /// </summary>
    private const string ReservedTypeNameCharacters = "\"',[]()";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Whether a type's full name names it uniquely among the loaded assemblies, per type
    /// </summary>
    private static readonly ConcurrentDictionary<Type, bool> _uniquelyNamedTypes = new();

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Formats the arguments of a data row
    /// </summary>
    /// <param name="data">The arguments, as <see cref="TestContext.TestData"/> or <see cref="DataRowAttribute.Data"/> hold them, or <see langword="null"/> for a test that is not data-driven</param>
    /// <returns>The formatted data row, or <see langword="null"/> when there are no arguments</returns>
    public static string Format(object[] data)
    {
        if (data == null
            || data.Length == 0)
        {
            return null;
        }

        var tokens = new List<string>();

        for (var index = 0; index < data.Length; index++)
        {
            tokens.Add(FormatArgument(data[index], $"argument {index + 1}"));
        }

        return string.Join(Separator, tokens);
    }

    /// <summary>
    /// Formats one argument, or fails when the key cannot represent it
    /// </summary>
    /// <param name="argument">The argument</param>
    /// <param name="position">Where the argument sits in the row, for the failure message</param>
    /// <returns>The token</returns>
    private static string FormatArgument(object argument, string position)
    {
        switch (argument)
        {
            case null:
                return "null";

            case string text:
                return Quote(text, '"');

            case char character:
                return Quote(character.ToString(), '\'');

            case bool flag:
                return flag ? "true" : "false";

            case Type type:
                return type.FullName != null
                       && IsUniquelyNamed(type)
                           ? $"typeof({Quote(type.FullName, '"')})"
                           : Refuse(typeof(Type), position);

            case Enum value:
                return FormatEnum(value, position);

            case Array array:
                return FormatArray(array, position);
        }

        var number = FormatNumber(argument);

        if (number != null)
        {
            return number;
        }

        return Refuse(argument.GetType(), position);
    }

    /// <summary>
    /// Formats a number with a suffix that names its type, so numbers of different types never share a token
    /// </summary>
    /// <param name="argument">The argument</param>
    /// <returns>The token, or <see langword="null"/> when the argument is not a number</returns>
    private static string FormatNumber(object argument)
    {
        return argument switch
               {
                   int value => value.ToString(CultureInfo.InvariantCulture),
                   uint value => $"{value.ToString(CultureInfo.InvariantCulture)}u",
                   long value => $"{value.ToString(CultureInfo.InvariantCulture)}L",
                   ulong value => $"{value.ToString(CultureInfo.InvariantCulture)}UL",
                   short value => $"(short){value.ToString(CultureInfo.InvariantCulture)}",
                   ushort value => $"(ushort){value.ToString(CultureInfo.InvariantCulture)}",
                   byte value => $"(byte){value.ToString(CultureInfo.InvariantCulture)}",
                   sbyte value => $"(sbyte){value.ToString(CultureInfo.InvariantCulture)}",
                   float value => $"{value.ToString("R", CultureInfo.InvariantCulture)}f",
                   double value => $"{value.ToString("R", CultureInfo.InvariantCulture)}d",
                   decimal value => $"{value.ToString(CultureInfo.InvariantCulture)}m",
                   _ => null
               };
    }

    /// <summary>
    /// Formats an enum value as its type and its underlying number, so flag combinations and undefined values stay
    /// unique and hold no separator
    /// </summary>
    /// <param name="value">The value</param>
    /// <param name="position">Where the argument sits in the row, for the failure message</param>
    /// <returns>The token</returns>
    private static string FormatEnum(Enum value, string position)
    {
        var type = value.GetType();

        if (IsPlainTypeName(type) == false)
        {
            return Refuse(type, position);
        }

        var underlying = Convert.ChangeType(value, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);

        return $"{type.FullName}({FormatNumber(underlying)})";
    }

    /// <summary>
    /// Formats a one-dimensional array as its element type followed by its elements in brackets
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="position">Where the argument sits in the row, for the failure message</param>
    /// <returns>The token</returns>
    private static string FormatArray(Array array, string position)
    {
        var elementType = array.GetType().GetElementType();

        var elementTypeName = FormatElementTypeName(elementType);

        if (array.Rank != 1
            || elementTypeName == null)
        {
            return Refuse(array.GetType(), position);
        }

        var elements = new List<string>();

        for (var index = 0; index < array.Length; index++)
        {
            elements.Add(FormatArgument(array.GetValue(index), $"{position}, element {index + 1}"));
        }

        return $"{elementTypeName}[{string.Join(Separator, elements)}]";
    }

    /// <summary>
    /// Formats the element type of an array: a plain full name, followed by <c>[]</c> once per level of a jagged array.
    /// The last bracket pair of an array token always holds its elements, so the suffixes stay unambiguous
    /// </summary>
    /// <param name="elementType">The element type</param>
    /// <returns>The name, or <see langword="null"/> when the type cannot be written without quotes</returns>
    private static string FormatElementTypeName(Type elementType)
    {
        if (elementType == null)
        {
            return null;
        }

        if (elementType.IsArray)
        {
            var innerName = elementType.GetArrayRank() == 1
                                ? FormatElementTypeName(elementType.GetElementType())
                                : null;

            return innerName == null ? null : $"{innerName}[]";
        }

        return IsPlainTypeName(elementType) ? elementType.FullName : null;
    }

    /// <summary>
    /// Determines whether a type's full name can be written without quotes inside a token and names the type uniquely
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns><see langword="true"/> when the full name holds none of the reserved characters and names no other loaded type</returns>
    private static bool IsPlainTypeName(Type type)
    {
        return type.FullName != null
               && type.FullName.IndexOfAny(ReservedTypeNameCharacters.ToCharArray()) < 0
               && IsUniquelyNamed(type);
    }

    /// <summary>
    /// Determines whether no other loaded assembly defines a different type with the same full name, so the full name
    /// identifies the type in a key. A type forwarded to the type itself is the same type
    /// </summary>
    /// <param name="type">The type</param>
    /// <returns><see langword="true"/> when the full name is unique among the loaded assemblies</returns>
    private static bool IsUniquelyNamed(Type type)
    {
        return _uniquelyNamedTypes.GetOrAdd(type,
                                            candidate => AppDomain.CurrentDomain
                                                                  .GetAssemblies()
                                                                  .Where(assembly => assembly != candidate.Assembly)
                                                                  .Select(assembly => assembly.GetType(candidate.FullName, throwOnError: false))
                                                                  .All(found => found == null || found == candidate));
    }

    /// <summary>
    /// Quotes a text, escaping the backslash and the quote character, so the token ends at its closing quote
    /// </summary>
    /// <param name="text">The text</param>
    /// <param name="quote">The quote character</param>
    /// <returns>The quoted text</returns>
    private static string Quote(string text, char quote)
    {
        var builder = new StringBuilder(text.Length + 2);

        builder.Append(quote);

        foreach (var character in text)
        {
            if (character == '\\'
                || character == quote)
            {
                builder.Append('\\');
            }

            builder.Append(character);
        }

        builder.Append(quote);

        return builder.ToString();
    }

    /// <summary>
    /// Fails for an argument the key cannot represent
    /// </summary>
    /// <param name="type">The argument's type</param>
    /// <param name="position">Where the argument sits in the row</param>
    /// <returns>Never returns</returns>
    private static string Refuse(Type type, string position)
    {
        Assert.Fail($"The phase-level stability check cannot key this data row: {position} is a {type.FullName ?? type.Name}. A data row may hold only null, strings, characters, booleans, numbers, enums, types, and one-dimensional arrays of these, because the check keys each data-driven execution by the text of its arguments and that text must be unique.");

        return null;
    }

    #endregion // Methods
}