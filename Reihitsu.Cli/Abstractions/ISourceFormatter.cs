using Microsoft.CodeAnalysis;

namespace Reihitsu.Cli.Abstractions;

/// <summary>
/// Abstracts the formatting engine for testability
/// </summary>
internal interface ISourceFormatter
{
    #region Members

    /// <summary>
    /// Formats a syntax tree and returns the formatted tree
    /// </summary>
    /// <param name="syntaxTree">The syntax tree to format</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests</param>
    /// <returns>The formatted syntax tree</returns>
    SyntaxTree FormatSyntaxTree(SyntaxTree syntaxTree, CancellationToken cancellationToken);

    #endregion // Members
}