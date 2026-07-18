using System;
using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Core;

/// <summary>
/// Casing utilities
/// </summary>
public static class CasingUtilities
{
    #region Public methods

    /// <summary>
    /// Checks if a given string is in PascalCase
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is in PascalCase, false otherwise</returns>
    public static bool IsPascalCase(string input)
    {
        if (input is null)
        {
            return false;
        }

        return IsPascalCase(input.AsSpan());
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Converted string</returns>
    public static string ToPascalCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)
            || IsPascalCase(input))
        {
            return input;
        }

        var builder = ConvertToPascalCase(input);

        // Identifiers without letters or digits (for example "_", "__") produce an empty result, in which case the
        // input is returned unchanged
        return builder.Length == 0
                   ? input
                   : builder.ToString();
    }

    /// <summary>
    /// Checks if a given string is a valid type parameter name (a single uppercase 'T', an uppercase 'T' followed by
    /// a PascalCase name, or an uppercase 'T' followed by digits, matching the BCL convention for numbered type
    /// parameters such as 'T1'/'T2')
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is a valid type parameter name, false otherwise</returns>
    public static bool IsTypeParameterName(string input)
    {
        if (string.IsNullOrEmpty(input)
            || input[0] != 'T')
        {
            return false;
        }

        return IsTypeParameterNameSuffix(input.AsSpan().Slice(1));
    }

    /// <summary>
    /// Converts a string to a type parameter name (an uppercase 'T' followed by a PascalCase name or by digits)
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Converted string</returns>
    public static string ToTypeParameterName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)
            || IsTypeParameterName(input))
        {
            return input;
        }

        // A leading 't' or 'T' followed by an uppercase letter or a digit is treated as an existing prefix attempt
        // (for example "tKey" or "t1"), so only the casing of the prefix is corrected instead of prepending a
        // second 'T'
        var hasPrefix = (input[0] == 'T'
                         || input[0] == 't')
                        && (input.Length == 1
                            || char.IsUpper(input[1])
                            || char.IsDigit(input[1]));

        if (hasPrefix
            && input.Length == 1)
        {
            return "T";
        }

        var body = hasPrefix
                       ? ToPascalCase(input.Substring(1))
                       : ToPascalCase(input);

        // Identifiers without a PascalCase or digit representation (for example "_", "__") cannot be converted, in
        // which case the input is returned unchanged
        return IsTypeParameterNameSuffix(body.AsSpan())
                   ? "T" + body
                   : input;
    }

    /// <summary>
    /// Checks if a given string is in camelCase
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is in camelCase, false otherwise</returns>
    public static bool IsCamelCase(string input)
    {
        if (input is null)
        {
            return false;
        }

        return IsCamelCase(input.AsSpan());
    }

    /// <summary>
    /// Checks if a given string is in _camelCase
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is in _camelCase format, false otherwise</returns>
    public static bool IsUnderlineCamelCase(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        if (input[0] != '_')
        {
            return false;
        }

        if (input.Length < 2)
        {
            return true;
        }

        return IsCamelCase(input.AsSpan().Slice(1));
    }

    /// <summary>
    /// Converts a string to camelCase
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Converted string</returns>
    public static string ToCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)
            || IsCamelCase(input))
        {
            return input;
        }

        var builder = ConvertToPascalCase(input);

        // Identifiers without letters or digits (for example "_", "__") produce an empty result, in which case the
        // input is returned unchanged
        if (builder.Length == 0)
        {
            return input;
        }

        builder.Replace(builder[0], char.ToLowerInvariant(builder[0]), 0, 1);

        return builder.ToString();
    }

    /// <summary>
    /// Converts a string to _camelCase
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Converted string</returns>
    public static string ToUnderlineCamelCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input)
            || IsUnderlineCamelCase(input))
        {
            return input;
        }

        var builder = ConvertToPascalCase(input);

        // Identifiers without letters or digits (for example "_", "__") produce an empty result, in which case the
        // input is returned unchanged
        if (builder.Length == 0)
        {
            return input;
        }

        builder.Replace(builder[0], char.ToLowerInvariant(builder[0]), 0, 1);
        builder.Insert(0, "_");

        return builder.ToString();
    }

    #endregion // Public methods

    #region Private methods

    /// <summary>
    /// Checks if a given string is in PascalCase
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is in PascalCase, false otherwise</returns>
    private static bool IsPascalCase(ReadOnlySpan<char> input)
    {
        var isPascalCase = input.IsEmpty == false

                           // The first character must be a letter and uppercase
                           && char.IsUpper(input[0]);

        if (isPascalCase)
        {
            // All other characters must be letters or digits
            foreach (var character in input.Slice(1))
            {
                if (char.IsLetterOrDigit(character) == false)
                {
                    isPascalCase = false;

                    break;
                }
            }
        }

        return isPascalCase;
    }

    /// <summary>
    /// Checks if a given span is a valid suffix for a type parameter name: empty (a bare 'T'), a PascalCase name, or
    /// one or more digits (matching the BCL convention for numbered type parameters such as 'T1'/'T2')
    /// </summary>
    /// <param name="suffix">The suffix to check, without the leading 'T'</param>
    /// <returns>True if the suffix is valid, false otherwise</returns>
    private static bool IsTypeParameterNameSuffix(ReadOnlySpan<char> suffix)
    {
        if (suffix.IsEmpty
            || IsPascalCase(suffix))
        {
            return true;
        }

        foreach (var character in suffix)
        {
            if (char.IsDigit(character) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks if a given string is in camelCase
    /// </summary>
    /// <param name="input">The string to check</param>
    /// <returns>True if the string is in camelCase, false otherwise</returns>
    private static bool IsCamelCase(ReadOnlySpan<char> input)
    {
        var isCamelCase = input.IsEmpty == false

                          // The first character must be a letter and lowercase
                          && char.IsLower(input[0]);

        if (isCamelCase)
        {
            // All other characters must be letters or digits
            foreach (var character in input.Slice(1))
            {
                if (char.IsLetterOrDigit(character) == false)
                {
                    isCamelCase = false;

                    break;
                }
            }
        }

        return isCamelCase;
    }

    /// <summary>
    /// Converts a string to PascalCase
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>Converted string</returns>
    private static StringBuilder ConvertToPascalCase(string input)
    {
        var builder = new StringBuilder(input.Length);
        var buffer = new List<char>();
        var upperCharacter = true;

        void AddBuffer()
        {
            var isUpperString = buffer.TrueForAll(char.IsUpper);

            foreach (var character in buffer)
            {
                builder.Append(isUpperString
                                   ? char.ToLowerInvariant(character)
                                   : character);
            }

            buffer = [];
        }

        foreach (var character in input)
        {
            if (char.IsLetterOrDigit(character) == false)
            {
                upperCharacter = true;

                AddBuffer();
            }
            else if (upperCharacter)
            {
                builder.Append(char.ToUpperInvariant(character));

                upperCharacter = false;
            }
            else
            {
                buffer.Add(character);
            }
        }

        AddBuffer();

        return builder;
    }

    #endregion // Private methods
}