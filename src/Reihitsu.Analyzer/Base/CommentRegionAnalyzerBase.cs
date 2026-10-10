using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that report comments placed inside a delimited region such as a parameter list, an
/// argument list, or a statement header. The innermost region containing the comment decides; a gap inside a
/// construct nested in the region that lays out its own lines (a block, an initializer, a switch expression, …) does
/// not belong to the region, and a comment directly before a binary operator or before a later call of a fluent chain
/// stays valid
/// </summary>
public abstract class CommentRegionAnalyzerBase : CommentPositionAnalyzerBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    protected CommentRegionAnalyzerBase(string diagnosticId, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the delimiters of the region when the node is one this rule inspects
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="openToken">First token of the region</param>
    /// <param name="closeToken">Last token of the region</param>
    /// <returns><see langword="true"/> if the node is a region of this rule</returns>
    protected abstract bool TryGetRegion(SyntaxNode node, out SyntaxToken openToken, out SyntaxToken closeToken);

    /// <summary>
    /// Determines whether a comment directly before a region's first token is reported as well
    /// </summary>
    /// <param name="previousToken">Token before the comment</param>
    /// <param name="nextToken">Token after the comment</param>
    /// <returns><see langword="true"/> if the comment is misplaced</returns>
    protected virtual bool IsMisplacedBeforeRegion(SyntaxToken previousToken, SyntaxToken nextToken)
    {
        return false;
    }

    #endregion // Methods

    #region CommentPositionAnalyzerBase

    /// <inheritdoc/>
    protected sealed override bool IsMisplaced(SyntaxTrivia comment, SyntaxToken previousToken, SyntaxToken nextToken)
    {
        if (previousToken.IsKind(SyntaxKind.None))
        {
            return false;
        }

        if (IsMisplacedBeforeRegion(previousToken, nextToken))
        {
            return true;
        }

        if (CommentPositionUtilities.IsValidInsideRegion(nextToken))
        {
            return false;
        }

        for (var node = nextToken.Parent; node != null; node = node.Parent)
        {
            if (TryGetRegion(node, out var openToken, out var closeToken)
                && CommentPositionUtilities.IsGapWithin(previousToken, nextToken, openToken, closeToken))
            {
                return CommentPositionUtilities.IsOwnedByNestedConstruct(node, nextToken) == false;
            }
        }

        return false;
    }

    #endregion // CommentPositionAnalyzerBase
}