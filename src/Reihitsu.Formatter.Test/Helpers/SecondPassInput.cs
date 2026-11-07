using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// Parses a fixture's text the way a formatter pass sees it: with the fixture's parse options, and with the formatting
/// context of the fixture's line ending and of the language version those options resolve to. The formatter passes of
/// <see cref="FormatterTestsBase"/>, the phases the phase-level stability check runs alone, and the fixed-point
/// assertion of the check's own tests take their input from here, so the check sees exactly what a second pass of those
/// helpers sees
/// </summary>
internal static class SecondPassInput
{
    #region Methods

    /// <summary>
    /// Parses the text and creates its formatting context
    /// </summary>
    /// <param name="text">The text to parse</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <param name="parseOptions">The parse options, or <see langword="null"/> for the defaults</param>
    /// <returns>The syntax root and the formatting context</returns>
    public static (SyntaxNode Root, FormattingContext Context) Create(string text, string endOfLine, CSharpParseOptions parseOptions)
    {
        var tree = CSharpSyntaxTree.ParseText(text, parseOptions);
        var context = new FormattingContext(endOfLine, languageVersion: LanguageVersionResolver.Resolve(tree.Options));

        return (tree.GetRoot(), context);
    }

    #endregion // Methods
}