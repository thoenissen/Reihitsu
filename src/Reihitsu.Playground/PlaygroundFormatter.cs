using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter;

namespace Reihitsu.Playground;

/// <summary>
/// Formats C# source text for the playground UI by parsing it in memory and delegating to <see cref="ReihitsuFormatter"/>,
/// without requiring a Workspace or file-system access
/// </summary>
public static class PlaygroundFormatter
{
    #region Methods

    /// <summary>
    /// Parses and formats the given C# source
    /// </summary>
    /// <param name="source">The C# source to format</param>
    /// <returns>The formatting result</returns>
    public static PlaygroundFormatResult Format(string source)
    {
        var syntaxTree = Parse(source);

        var errors = GetSyntaxErrors(syntaxTree);

        if (errors.Count > 0)
        {
            return new PlaygroundFormatResult(source, false, Errors: errors);
        }

        // The formatter leaves such input as written; it is named by the formatter's own definition (PLAY-009)
        if (ReihitsuFormatter.HoldsHidingCarrier(syntaxTree))
        {
            return new PlaygroundFormatResult(source, false, HoldsHiddenCode: true);
        }

        var result = ReihitsuFormatter.FormatSyntaxTree(syntaxTree);

        return result.Unsupported != null
                   ? new PlaygroundFormatResult(source, false, result.Unsupported)
                   : new PlaygroundFormatResult(result.Output.ToString(), true);
    }

    /// <summary>
    /// Parses C# source with the options of PLAY-005; formatting, the error list, and the highlighting of the input all read
    /// the tree this returns
    /// </summary>
    /// <param name="source">The C# source to parse</param>
    /// <returns>The syntax tree</returns>
    internal static SyntaxTree Parse(string source)
    {
        return CSharpSyntaxTree.ParseText(source);
    }

    /// <summary>
    /// Gets the error diagnostics of the syntax tree, ordered by the start of their span; errors at one start keep the order
    /// the parser reports them in, because the sort is stable (PLAY-006)
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to inspect</param>
    /// <returns>The errors; empty when the tree has none</returns>
    private static List<Diagnostic> GetSyntaxErrors(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetDiagnostics()
                         .Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
                         .OrderBy(diagnostic => diagnostic.Location.SourceSpan.Start)
                         .ToList();
    }

    #endregion // Methods
}