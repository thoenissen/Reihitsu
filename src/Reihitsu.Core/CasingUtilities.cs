using System;
using System.Collections.Generic;
using System.Globalization;
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

        var words = SplitIntoPascalCaseWords(input);

        // Identifiers without letters or digits (for example "_", "__") produce no word, in which case the input is
        // returned unchanged
        return words.Count == 0
                   ? input
                   : string.Concat(words);
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

        // A leading 't' or 'T' followed by an uppercase letter, a digit, or '_' is treated as an existing prefix attempt
        // (for example "tKey", "t1", or "T_Key"), so only the casing of the prefix is corrected instead of prepending a
        // second 'T'. The underscores directly behind the prefix separate it from the body and are dropped with it
        var hasPrefix = (input[0] == 'T'
                         || input[0] == 't')
                        && (input.Length == 1
                            || char.IsUpper(input[1])
                            || char.IsDigit(input[1])
                            || input[1] == '_');

        var bodyStart = hasPrefix
                            ? 1
                            : 0;

        while (hasPrefix
               && bodyStart < input.Length
               && input[bodyStart] == '_')
        {
            bodyStart++;
        }

        if (bodyStart == input.Length)
        {
            return "T";
        }

        var body = ToPascalCase(input.Substring(bodyStart));

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

        var words = SplitIntoPascalCaseWords(input);

        // Identifiers without letters or digits (for example "_", "__") produce no word, in which case the input is
        // returned unchanged
        if (words.Count == 0)
        {
            return input;
        }

        words[0] = LowercaseLeadingUppercaseRun(words[0]);

        return string.Concat(words);
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

        var words = SplitIntoPascalCaseWords(input);

        // Identifiers without letters or digits (for example "_", "__") produce no word, in which case the input is
        // returned unchanged
        if (words.Count == 0)
        {
            return input;
        }

        words[0] = LowercaseLeadingUppercaseRun(words[0]);

        return "_" + string.Concat(words);
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
            // All other characters must be word characters: letters, digits, letter numbers, combining marks, or
            // connector punctuation other than '_'
            foreach (var character in input.Slice(1))
            {
                if (IsWordCharacter(character) == false)
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
            // All other characters must be word characters: letters, digits, letter numbers, combining marks, or
            // connector punctuation other than '_'
            foreach (var character in input.Slice(1))
            {
                if (IsWordCharacter(character) == false)
                {
                    isCamelCase = false;

                    break;
                }
            }
        }

        return isCamelCase;
    }

    /// <summary>
    /// Determines whether a character belongs to a word of a name: a letter, a decimal digit, a letter number, a
    /// combining mark, or connector punctuation other than '_'
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <returns>True if the character belongs to a word, false otherwise</returns>
    private static bool IsWordCharacter(char character)
    {
        if (character == '_')
        {
            return false;
        }

        if (char.IsLetterOrDigit(character))
        {
            return true;
        }

        return char.GetUnicodeCategory(character) is UnicodeCategory.LetterNumber
                                                  or UnicodeCategory.NonSpacingMark
                                                  or UnicodeCategory.SpacingCombiningMark
                                                  or UnicodeCategory.ConnectorPunctuation;
    }

    /// <summary>
    /// Determines whether a character is a combining mark, which counts as part of the character in front of it
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <returns>True if the character is a combining mark, false otherwise</returns>
    private static bool IsCombiningMark(char character)
    {
        return char.GetUnicodeCategory(character) is UnicodeCategory.NonSpacingMark
                                                  or UnicodeCategory.SpacingCombiningMark;
    }

    /// <summary>
    /// Splits a name into the words of its PascalCase form: the name is split at every character that is no word
    /// character, which drops it, the first character of each word is written in uppercase, and the other characters of
    /// a word are written in lowercase when they are all uppercase letters, not counting combining marks, and kept
    /// otherwise
    /// </summary>
    /// <param name="input">Input string</param>
    /// <returns>The words in PascalCase, in their order; empty when the name has no word character</returns>
    private static List<string> SplitIntoPascalCaseWords(string input)
    {
        var words = new List<string>();
        var start = 0;

        while (start < input.Length)
        {
            if (IsWordCharacter(input[start]) == false)
            {
                start++;

                continue;
            }

            var end = start;

            while (end < input.Length
                   && IsWordCharacter(input[end]))
            {
                end++;
            }

            words.Add(ToPascalCaseWord(input, start, end));

            start = end;
        }

        return words;
    }

    /// <summary>
    /// Writes one word in PascalCase: its first character in uppercase, and the other characters in lowercase when they
    /// are all uppercase letters, not counting combining marks, otherwise as written
    /// </summary>
    /// <param name="input">Input string</param>
    /// <param name="start">Index of the word's first character</param>
    /// <param name="end">Index behind the word's last character</param>
    /// <returns>The word in PascalCase</returns>
    private static string ToPascalCaseWord(string input, int start, int end)
    {
        var isUpperWord = true;

        for (var index = start + 1; index < end; index++)
        {
            if (IsCombiningMark(input[index]) == false
                && char.IsUpper(input[index]) == false)
            {
                isUpperWord = false;

                break;
            }
        }

        var builder = new StringBuilder(end - start);

        builder.Append(char.ToUpperInvariant(input[start]));

        for (var index = start + 1; index < end; index++)
        {
            builder.Append(isUpperWord
                               ? char.ToLowerInvariant(input[index])
                               : input[index]);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Writes the leading run of uppercase letters of a word in lowercase, for the first word of a camelCase name. A
    /// combining mark counts as part of the letter in front of it. When the run has two or more letters and a lowercase
    /// letter follows it directly, the run's last letter is the first letter of the next word and keeps its case
    /// </summary>
    /// <param name="word">The first word, in PascalCase</param>
    /// <returns>The word with its leading run of uppercase letters in lowercase</returns>
    private static string LowercaseLeadingUppercaseRun(string word)
    {
        var runEnd = 0;
        var letterCount = 0;
        var lastLetterIndex = -1;

        while (runEnd < word.Length)
        {
            if (char.IsUpper(word[runEnd]))
            {
                lastLetterIndex = runEnd;
                letterCount++;
            }
            else if (letterCount == 0
                     || IsCombiningMark(word[runEnd]) == false)
            {
                break;
            }

            runEnd++;
        }

        var lowercaseEnd = letterCount >= 2
                           && runEnd < word.Length
                           && char.IsLower(word[runEnd])
                               ? lastLetterIndex
                               : runEnd;

        var builder = new StringBuilder(word.Length);

        for (var index = 0; index < word.Length; index++)
        {
            builder.Append(index < lowercaseEnd
                               ? char.ToLowerInvariant(word[index])
                               : word[index]);
        }

        return builder.ToString();
    }

    #endregion // Private methods
}