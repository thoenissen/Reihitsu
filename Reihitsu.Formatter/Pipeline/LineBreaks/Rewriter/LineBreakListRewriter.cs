using System;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.Core.Utilities;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for argument and parameter lists
/// </summary>
internal sealed class LineBreakListRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    /// <summary>
    /// Attribute formatting that runs later in the same line-break pass; used to read a list element as that pass emits it
    /// </summary>
    private readonly AttributeTargetFormattingRewriter _attributeFormatter;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public LineBreakListRewriter(FormattingContext context,
                                 CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
        _attributeFormatter = new AttributeTargetFormattingRewriter(context, cancellationToken);
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Collapses a parameter-list opener when the owning declaration contains the previous token whose trailing trivia
    /// owns the line break. This is the single collapse policy for every parameter-list owner covered by
    /// <c>RH5105OpeningParenthesisMustBeOnDeclarationLineAnalyzer</c>'s <c>ParameterListParentPolicy</c>: unlike a
    /// policy scoped to the parameter list alone, resolving the previous token from the owning declaration reaches
    /// it even when a type-parameter list separates the declaration token from the opening parenthesis
    /// </summary>
    /// <typeparam name="TNode">The parameter-list owner type</typeparam>
    /// <param name="node">The parameter-list owner</param>
    /// <param name="parameterList">The parameter list to normalize</param>
    /// <returns>The updated owner</returns>
    internal static TNode CollapseOwnedOpenParenToDeclarationLine<TNode>(TNode node,
                                                                         ParameterListSyntax parameterList)
        where TNode : SyntaxNode
    {
        var openParenToken = parameterList.OpenParenToken;
        var previousToken = openParenToken.GetPreviousToken();

        if (previousToken == default
            || previousToken.IsKind(SyntaxKind.None)
            || TokenGapUtilities.HasLineBreakBetween(previousToken, openParenToken) == false
            || LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, openParenToken))
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(openParenToken);

        return node.ReplaceTokens([previousToken, openParenToken],
                                  (original, _) => original == previousToken
                                                       ? newPreviousToken
                                                       : newOpenParen);
    }

    /// <summary>
    /// Collapses the first element of a list to the same line as the opening delimiter
    /// when it currently starts on a new line
    /// </summary>
    /// <typeparam name="TNode">The list syntax node type</typeparam>
    /// <param name="node">The list node</param>
    /// <param name="firstElementToken">The first token of the first element, or <see langword="default"/> when the list is empty</param>
    /// <returns>The list with the first element collapsed</returns>
    private static TNode CollapseFirstElementToSameLine<TNode>(TNode node,
                                                               SyntaxToken firstElementToken)
        where TNode : SyntaxNode
    {
        if (firstElementToken == default
            || firstElementToken.IsKind(SyntaxKind.None)
            || LineBreakTriviaUtilities.HasLeadingEndOfLine(firstElementToken) == false)
        {
            return node;
        }

        return LineBreakTriviaUtilities.CollapseTokenToSameLine(node, firstElementToken);
    }

    /// <summary>
    /// Collapses the opening parenthesis of a parameter list onto the declaration line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseOpenParenToDeclarationLine(ParameterListSyntax node)
    {
        if (TokenLocator.TryGetPreviousToken(node, node.OpenParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.OpenParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, node.OpenParenToken))
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.OpenParenToken);

        if (TokenLocator.ContainsToken(node, previousToken) == false)
        {
            return node.WithOpenParenToken(newOpenParen);
        }

        return node.ReplaceTokens([previousToken, node.OpenParenToken],
                                  (original, _) => original == previousToken
                                                       ? newPreviousToken
                                                       : newOpenParen);
    }

    /// <summary>
    /// Collapses misplaced commas onto the previous parameter line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <param name="endOfLine">The end-of-line sequence to keep after a moved separator</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseSeparatorsToPreviousParameterLine(ParameterListSyntax node,
                                                                                 string endOfLine)
    {
        for (var separatorIndex = 0; separatorIndex < node.Parameters.SeparatorCount; separatorIndex++)
        {
            var previousToken = node.Parameters[separatorIndex].GetLastToken();
            var separator = node.Parameters.GetSeparator(separatorIndex);
            var nextParameter = node.Parameters[separatorIndex + 1].GetFirstToken();

            if (TokenGapUtilities.HasLineBreakBetween(previousToken, separator) == false)
            {
                continue;
            }

            if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, separator))
            {
                continue;
            }

            var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
            var newSeparator = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(separator);

            if (LineBreakTriviaUtilities.HasTrailingEndOfLine(newSeparator) == false && LineBreakTriviaUtilities.HasLeadingEndOfLine(nextParameter) == false)
            {
                var newTrailing = newSeparator.TrailingTrivia
                                              .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                              .ToList();

                newTrailing.Add(SyntaxFactory.EndOfLine(endOfLine));
                newSeparator = newSeparator.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailing));
            }

            node = node.ReplaceTokens([previousToken, separator],
                                      (original, _) => original == previousToken
                                                           ? newPreviousToken
                                                           : newSeparator);
        }

        return node;
    }

    /// <summary>
    /// Collapses the closing parenthesis onto the final parameter line
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <returns>The updated parameter list</returns>
    private static ParameterListSyntax CollapseCloseParenToParameterLine(ParameterListSyntax node)
    {
        if (TokenLocator.TryGetPreviousToken(node, node.CloseParenToken, out var previousToken) == false
            || TokenGapUtilities.HasLineBreakBetween(previousToken, node.CloseParenToken) == false)
        {
            return node;
        }

        if (LineBreakTriviaUtilities.WouldJoinAcrossUnjoinableTrivia(previousToken, node.CloseParenToken))
        {
            return node;
        }

        var newPreviousToken = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        var newCloseParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(node.CloseParenToken);

        return node.ReplaceTokens([previousToken, node.CloseParenToken],
                                  (original, _) => original == previousToken
                                                       ? newPreviousToken
                                                       : newCloseParen);
    }

    /// <summary>
    /// Ensures that all arguments start on their own line once the argument list is multi-line or an argument signals a split
    /// </summary>
    /// <param name="node">The argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The argument list with arguments on separate lines</returns>
    private ArgumentListSyntax EnsureArgumentsOnSeparateLines(ArgumentListSyntax node,
                                                              string endOfLine)
    {
        if (node.Arguments.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Arguments, endOfLine);
    }

    /// <summary>
    /// Determines whether a bracketed argument list can be safely collapsed to one line
    /// </summary>
    /// <param name="originalNode">The bracketed argument list as it entered this visit, still attached to its owner</param>
    /// <param name="node">The bracketed argument list node after its elements were visited</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The interior-scoped check is deliberate. The collapse only rewrites the gaps between the
    /// brackets - opening bracket to first argument, the separators, and last argument to closing
    /// bracket - so a comment trailing the closing bracket is never crossed and must not force the
    /// list apart. RH5307 applies the same interior-scoped guard in its analyzer and its code fix, so
    /// all three surfaces agree on the decision, including for the equivalent auto-property collapse.
    /// The owner is read from the original node, because a list rebuilt by visiting its elements no
    /// longer has a parent
    /// </remarks>
    private bool CanSafelyCollapseBracketedArguments(BracketedArgumentListSyntax originalNode,
                                                     BracketedArgumentListSyntax node)
    {
        if (originalNode.Parent is not ElementAccessExpressionSyntax and not ImplicitElementAccessSyntax)
        {
            return false;
        }

        return CanSafelyCollapseInteriorList(node, node.Arguments, originalNode.Arguments);
    }

    /// <summary>
    /// Collapses bracketed indexer arguments onto a single line when safe
    /// </summary>
    /// <param name="originalNode">The bracketed argument list as it entered this visit</param>
    /// <param name="node">The bracketed argument list node after its elements were visited</param>
    /// <returns>The updated bracketed argument list</returns>
    private BracketedArgumentListSyntax CollapseBracketedArgumentsToSingleLine(BracketedArgumentListSyntax originalNode,
                                                                               BracketedArgumentListSyntax node)
    {
        if (LineBreakDetection.IsMultiLine(node) == false || CanSafelyCollapseBracketedArguments(originalNode, node) == false)
        {
            return node;
        }

        if (node.Arguments.Count > 0)
        {
            node = CollapseFirstElementToSameLine(node, node.Arguments[0].GetFirstToken());
        }

        for (var argumentIndex = 1; argumentIndex < node.Arguments.Count; argumentIndex++)
        {
            var firstToken = node.Arguments[argumentIndex].GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, firstToken);
            }
        }

        for (var separatorIndex = 0; separatorIndex < node.Arguments.SeparatorCount; separatorIndex++)
        {
            var separator = node.Arguments.GetSeparator(separatorIndex);

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(separator) || LineBreakTriviaUtilities.HasTrailingEndOfLine(separator))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, separator);
            }
        }

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(node.CloseBracketToken))
        {
            node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, node.CloseBracketToken);
        }

        return node;
    }

    /// <summary>
    /// Determines whether a delimited list can be safely collapsed to one line by rewriting only its
    /// interior gaps
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the list</typeparam>
    /// <param name="node">The list node</param>
    /// <param name="elements">The list's elements</param>
    /// <param name="writtenElements">The list's elements as they entered this visit, before their own lists were rewritten</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// Shared by every interior-scoped collapse in this rewriter: bracketed indexer arguments and
    /// every angle-bracket list (type argument, type parameter, function-pointer parameter). Callers
    /// that also restrict which owning construct is eligible check that separately before calling this.
    /// An element blocks the collapse only when it spans lines both as written and once attribute formatting
    /// has run, so attribute-list gaps that the same pass closes later do not keep the list wrapped. Lines that
    /// this pass opens inside an element written on one line - by attribute formatting or by splitting a list
    /// nested in the element - do not block the collapse, so the decision matches the one taken on the
    /// formatted output
    /// </remarks>
    private bool CanSafelyCollapseInteriorList<TElement>(SyntaxNode node,
                                                         SeparatedSyntaxList<TElement> elements,
                                                         SeparatedSyntaxList<TElement> writtenElements)
        where TElement : SyntaxNode
    {
        if (SyntaxNodeUtilities.InteriorContainsCommentOrDirective(node))
        {
            return false;
        }

        for (var elementIndex = 0; elementIndex < elements.Count; elementIndex++)
        {
            if (LineBreakDetection.IsMultiLine(writtenElements[elementIndex])
                && IsMultiLineAfterAttributeFormatting(elements[elementIndex]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether an element spans multiple lines once the attribute lists inside it are formatted.
    /// <see cref="AttributeTargetFormattingRewriter"/> runs after this rewriter in the same line-break pass and
    /// can close or open lines inside an element, so a layout decided from the element as written would only
    /// settle on the next formatter pass
    /// </summary>
    /// <param name="element">The list element</param>
    /// <returns><see langword="true"/> if the element spans multiple lines after attribute formatting; otherwise, <see langword="false"/></returns>
    private bool IsMultiLineAfterAttributeFormatting(SyntaxNode element)
    {
        if (element.DescendantNodes().Any(descendant => descendant is AttributeListSyntax) == false)
        {
            return LineBreakDetection.IsMultiLine(element);
        }

        return LineBreakDetection.IsMultiLine(_attributeFormatter.Visit(element));
    }

    /// <summary>
    /// Collapses a wrapped angle-bracket delimited list onto a single line when safe
    /// </summary>
    /// <typeparam name="TNode">The list syntax node type</typeparam>
    /// <typeparam name="TElement">The type of the elements in the list</typeparam>
    /// <param name="originalNode">The list as it entered this visit</param>
    /// <param name="node">The list node after its elements were visited</param>
    /// <param name="getElements">Reads the current elements from the (possibly already updated) node</param>
    /// <param name="getCloseToken">Reads the current closing angle bracket from the (possibly already updated) node</param>
    /// <returns>The updated list</returns>
    private TNode CollapseAngleBracketListToSingleLine<TNode, TElement>(TNode originalNode,
                                                                        TNode node,
                                                                        Func<TNode, SeparatedSyntaxList<TElement>> getElements,
                                                                        Func<TNode, SyntaxToken> getCloseToken)
        where TNode : SyntaxNode
        where TElement : SyntaxNode
    {
        var elements = getElements(node);

        if (LineBreakDetection.IsMultiLine(node) == false || CanSafelyCollapseInteriorList(node, elements, getElements(originalNode)) == false)
        {
            return node;
        }

        if (elements.Count > 0)
        {
            node = CollapseFirstElementToSameLine(node, elements[0].GetFirstToken());
            elements = getElements(node);
        }

        for (var elementIndex = 1; elementIndex < elements.Count; elementIndex++)
        {
            var firstToken = elements[elementIndex].GetFirstToken();

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstToken))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, firstToken);
                elements = getElements(node);
            }
        }

        for (var separatorIndex = 0; separatorIndex < elements.SeparatorCount; separatorIndex++)
        {
            var separator = elements.GetSeparator(separatorIndex);

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(separator) || LineBreakTriviaUtilities.HasTrailingEndOfLine(separator))
            {
                node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, separator);
                elements = getElements(node);
            }
        }

        var closeToken = getCloseToken(node);

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(closeToken))
        {
            node = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, closeToken);
        }

        return node;
    }

    /// <summary>
    /// Ensures that all arguments start on their own line once the attribute argument list is multi-line or an argument signals a split
    /// </summary>
    /// <param name="node">The attribute argument list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting arguments</param>
    /// <returns>The attribute argument list with arguments on separate lines</returns>
    private AttributeArgumentListSyntax EnsureAttributeArgumentsOnSeparateLines(AttributeArgumentListSyntax node,
                                                                                string endOfLine)
    {
        if (node.Arguments.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Arguments, endOfLine);
    }

    /// <summary>
    /// Ensures that all parameters start on their own line once the parameter list is multi-line or a parameter signals a split
    /// </summary>
    /// <param name="node">The parameter list node</param>
    /// <param name="endOfLine">The end-of-line sequence to insert when splitting parameters</param>
    /// <returns>The parameter list with parameters on separate lines</returns>
    private ParameterListSyntax EnsureParametersOnSeparateLines(ParameterListSyntax node,
                                                                string endOfLine)
    {
        if (node.Parameters.Count <= 1)
        {
            return node;
        }

        return EnsureSeparatorsHaveEndOfLine(node, node.Parameters, endOfLine);
    }

    /// <summary>
    /// Ensures that each separator in a separated syntax list has a trailing end-of-line trivia
    /// once the list is already multi-line or an element signals a split, including an element that
    /// attribute formatting later in this pass spreads over several lines
    /// </summary>
    /// <typeparam name="TNode">The type of the containing syntax node</typeparam>
    /// <typeparam name="TElement">The type of the elements in the separated list</typeparam>
    /// <param name="node">The containing syntax node</param>
    /// <param name="list">The separated syntax list to process</param>
    /// <param name="endOfLine">The end-of-line sequence to add after separators that need splitting</param>
    /// <returns>The node with updated separators</returns>
    private TNode EnsureSeparatorsHaveEndOfLine<TNode, TElement>(TNode node,
                                                                 SeparatedSyntaxList<TElement> list,
                                                                 string endOfLine)
        where TNode : SyntaxNode
        where TElement : SyntaxNode
    {
        var hasElementSplitSignal = HasElementSplitSignal(list);

        if (LineBreakDetection.IsMultiLine(node) == false && hasElementSplitSignal == false)
        {
            return node;
        }

        var hasExistingLineBreak = hasElementSplitSignal;
        var tokensToReplace = new List<SyntaxToken>();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        for (var separatorIndex = 0; separatorIndex < list.SeparatorCount; separatorIndex++)
        {
            var separator = list.GetSeparator(separatorIndex);
            var nextElement = list[separatorIndex + 1];
            var nextFirstToken = nextElement.GetFirstToken();

            if (LineBreakTriviaUtilities.HasTrailingEndOfLine(separator) || LineBreakTriviaUtilities.HasLeadingEndOfLine(nextFirstToken))
            {
                hasExistingLineBreak = true;

                continue;
            }

            var newTrailing = separator.TrailingTrivia
                                       .Where(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false)
                                       .ToList();

            newTrailing.Add(SyntaxFactory.EndOfLine(endOfLine));

            tokensToReplace.Add(separator);
            replacementMap[separator] = separator.WithTrailingTrivia(SyntaxFactory.TriviaList(newTrailing));
        }

        if (hasExistingLineBreak == false || tokensToReplace.Count == 0)
        {
            return node;
        }

        return node.ReplaceTokens(tokensToReplace, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Determines whether any element in a separated list already signals that the outer list should split
    /// </summary>
    /// <typeparam name="TElement">The type of the elements in the separated list</typeparam>
    /// <param name="list">The separated syntax list to inspect</param>
    /// <returns><see langword="true"/> if any element already signals an outer split; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// An element that attribute formatting will spread over several lines later in the same pass signals the
    /// split as well. An element that spans lines as written keeps signaling it even when attribute formatting
    /// would join it, so a list that is already split stays split
    /// </remarks>
    private bool HasElementSplitSignal<TElement>(SeparatedSyntaxList<TElement> list)
        where TElement : SyntaxNode
    {
        return list.Any(element => LineBreakTriviaUtilities.HasLeadingEndOfLine(element.GetFirstToken())
                                   || LineBreakDetection.IsMultiLine(element)
                                   || IsMultiLineAfterAttributeFormatting(element));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
    {
        node = (AnonymousMethodExpressionSyntax)base.VisitAnonymousMethodExpression(node);

        return node?.ParameterList == null
                   ? node
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        return node?.ParameterList == null
                   ? node
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitExtensionBlockDeclaration(ExtensionBlockDeclarationSyntax node)
    {
        node = (ExtensionBlockDeclarationSyntax)base.VisitExtensionBlockDeclaration(node);

        return node == null
                   ? null
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        return node?.ParameterList == null
                   ? node
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        return node?.ParameterList == null
                   ? node
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        return node?.ParameterList == null
                   ? node
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        node = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);

        return node == null
                   ? null
                   : CollapseOwnedOpenParenToDeclarationLine(node, node.ParameterList);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ArgumentListSyntax)base.VisitArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstElementToSameLine(node, node.Arguments.Count > 0 ? node.Arguments[0].GetFirstToken() : default);

        return EnsureArgumentsOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var originalNode = node;

        node = (BracketedArgumentListSyntax)base.VisitBracketedArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseBracketedArgumentsToSingleLine(originalNode, node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (AttributeArgumentListSyntax)base.VisitAttributeArgumentList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseFirstElementToSameLine(node, node.Arguments.Count > 0 ? node.Arguments[0].GetFirstToken() : default);

        return EnsureAttributeArgumentsOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (ParameterListSyntax)base.VisitParameterList(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseOpenParenToDeclarationLine(node);
        node = CollapseFirstElementToSameLine(node, node.Parameters.Count > 0 ? node.Parameters[0].GetFirstToken() : default);
        node = CollapseSeparatorsToPreviousParameterLine(node, _context.EndOfLine);
        node = CollapseCloseParenToParameterLine(node);

        return EnsureParametersOnSeparateLines(node, _context.EndOfLine);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var originalNode = node;

        node = (TypeArgumentListSyntax)base.VisitTypeArgumentList(node);

        return node == null
                   ? null
                   : CollapseAngleBracketListToSingleLine(originalNode,
                                                          node,
                                                          static typeArgumentList => typeArgumentList.Arguments,
                                                          static typeArgumentList => typeArgumentList.GreaterThanToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitTypeParameterList(TypeParameterListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var originalNode = node;

        node = (TypeParameterListSyntax)base.VisitTypeParameterList(node);

        return node == null
                   ? null
                   : CollapseAngleBracketListToSingleLine(originalNode,
                                                          node,
                                                          static typeParameterList => typeParameterList.Parameters,
                                                          static typeParameterList => typeParameterList.GreaterThanToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitFunctionPointerParameterList(FunctionPointerParameterListSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        var originalNode = node;

        node = (FunctionPointerParameterListSyntax)base.VisitFunctionPointerParameterList(node);

        return node == null
                   ? null
                   : CollapseAngleBracketListToSingleLine(originalNode,
                                                          node,
                                                          static functionPointerParameterList => functionPointerParameterList.Parameters,
                                                          static functionPointerParameterList => functionPointerParameterList.GreaterThanToken);
    }

    #endregion // CSharpSyntaxVisitor
}