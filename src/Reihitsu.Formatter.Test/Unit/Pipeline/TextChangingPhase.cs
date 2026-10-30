using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Test phase that appends a comment line to every text it receives, so every final output changes when it runs alone
/// </summary>
internal sealed class TextChangingPhase : IFormattingPhase
{
    #region Constants

    /// <summary>
    /// The comment the phase appends
    /// </summary>
    public const string AppendedComment = "// changed";

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Returns the text the phase returns for the given input
    /// </summary>
    /// <param name="input">The input text</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The output text</returns>
    public static string GetOutput(string input, string endOfLine)
    {
        return $"{input}{endOfLine}{AppendedComment}";
    }

    #endregion // Methods

    #region IFormattingPhase

    /// <inheritdoc/>
    public SyntaxNode Execute(SyntaxNode root, FormattingContext context, CancellationToken cancellationToken)
    {
        return CSharpSyntaxTree.ParseText(GetOutput(root.ToFullString(), context.EndOfLine), (CSharpParseOptions)root.SyntaxTree.Options, cancellationToken: cancellationToken).GetRoot(cancellationToken);
    }

    #endregion // IFormattingPhase
}