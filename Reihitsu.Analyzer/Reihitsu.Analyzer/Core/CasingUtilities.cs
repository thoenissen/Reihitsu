﻿using System.Collections.Generic;
using System.Text;

namespace Reihitsu.Analyzer.Core;

/// <summary>
/// Casing utilities
/// </summary>
public static class CasingUtilities
{
    #region Public methods

    /// <summary>
    /// Checks if a given string is in PascalCase.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is in PascalCase, false otherwise.</returns>
    public static bool IsPascalCase(string input)
    {
        bool isPascalCase = string.IsNullOrEmpty(input) == false

                         // The first character must be a letter and uppercase
                         && char.IsUpper(input[0])

                         // All other characters must be letters or digits
                         && input.Skip(1).Any(character => char.IsLetterOrDigit(character) == false) == false;

        return isPascalCase;
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

        return ConvertToPascalCase(input).ToString();
    }

    /// <summary>
    /// Checks if a given string is in camelCase.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string is in camelCase, false otherwise.</returns>
    public static bool IsCamelCase(string input)
    {
        bool isCamelCase = string.IsNullOrEmpty(input) == false

                        // The first character must be a letter and lowercase
                        && char.IsLower(input[0])

                        // All other characters must be letters or digits
                        && input.Skip(1).Any(character => char.IsLetterOrDigit(character) == false) == false;

        return isCamelCase;
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

        builder.Replace(builder[0], char.ToLowerInvariant(builder[0]), 0, 1);

        return builder.ToString();
    }

    #endregion // Public methods

    #region Private methods

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
            var isUpperString = buffer.All(char.IsUpper);

            foreach (var character in buffer)
            {
                builder.Append(isUpperString
                                   ? char.ToLowerInvariant(character)
                                   : character);
            }

            buffer = new();
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