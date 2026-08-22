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
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        if (HasSyntaxErrors(syntaxTree))
        {
            return new PlaygroundFormatResult(source, false);
        }

        var formattedTree = ReihitsuFormatter.FormatSyntaxTree(syntaxTree);

        return new PlaygroundFormatResult(formattedTree.ToString(), true);
    }

    /// <summary>
    /// Checks whether the syntax tree contains error diagnostics
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to inspect</param>
    /// <returns><see langword="true"/> if the tree has at least one error diagnostic; otherwise, <see langword="false"/></returns>
    private static bool HasSyntaxErrors(SyntaxTree syntaxTree)
    {
        return syntaxTree.GetDiagnostics()
                         .Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
    }

    #endregion // Methods
}