using System.Linq;
using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline;

namespace Reihitsu.Formatter.Test.Unit.Pipeline;

/// <summary>
/// Test phase that removes the indentation of every line, so the indentation phase, run after it, writes the indentation
/// back: a reversal of two phases that the check must catch without depending on a production phase
/// </summary>
internal sealed class IndentationStrippingPhase : IFormattingPhase
{
    #region Methods

    /// <summary>
    /// Returns the text the phase returns for the given input
    /// </summary>
    /// <param name="input">The input text</param>
    /// <param name="endOfLine">The end-of-line sequence of the formatting context</param>
    /// <returns>The output text</returns>
    public static string GetOutput(string input, string endOfLine)
    {
        return string.Join(endOfLine, input.Split(endOfLine).Select(line => line.TrimStart(' ')));
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