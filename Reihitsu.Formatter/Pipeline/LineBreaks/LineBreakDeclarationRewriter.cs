using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Formatter.Pipeline.LineBreaks;

/// <summary>
/// Applies line-break rules for declarations and members
/// </summary>
internal sealed class LineBreakDeclarationRewriter : CSharpSyntaxRewriter
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="gapNormalizer">The token gap normalizer</param>
    /// <param name="bracePlacer">The brace placer</param>
    public LineBreakDeclarationRewriter(FormattingContext context,
                                        CancellationToken cancellationToken,
                                        TokenGapNormalizer gapNormalizer,
                                        BracePlacer bracePlacer)
    {
        Context = context;
        CancellationToken = cancellationToken;
        GapNormalizer = gapNormalizer;
        BracePlacer = bracePlacer;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Gets the formatting context
    /// </summary>
    private FormattingContext Context { get; }

    /// <summary>
    /// Gets the cancellation token
    /// </summary>
    private CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets the token gap normalizer
    /// </summary>
    private TokenGapNormalizer GapNormalizer { get; }

    /// <summary>
    /// Gets the brace placer
    /// </summary>
    private BracePlacer BracePlacer { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Collapses a multi-line expression-bodied property to a single line
    /// </summary>
    /// <param name="node">The property declaration with an expression body</param>
    /// <returns>The property declaration collapsed to a single line</returns>
    private static PropertyDeclarationSyntax CollapseExpressionBodiedProperty(PropertyDeclarationSyntax node)
    {
        if (node?.ExpressionBody == null)
        {
            return null;
        }

        var updatedNode = node;
        var arrowToken = updatedNode.ExpressionBody.ArrowToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(arrowToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(arrowToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, arrowToken);
            arrowToken = updatedNode.ExpressionBody.ArrowToken;
        }

        if (arrowToken.LeadingTrivia.Any(SyntaxKind.WhitespaceTrivia) == false)
        {
            updatedNode = updatedNode.ReplaceToken(arrowToken, arrowToken.WithLeadingTrivia(arrowToken.LeadingTrivia.Add(SyntaxFactory.Space)));
        }

        var firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(firstExpressionToken) || LineBreakTriviaUtilities.HasTrailingEndOfLine(firstExpressionToken.GetPreviousToken()))
        {
            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, firstExpressionToken);
        }

        arrowToken = updatedNode.ExpressionBody.ArrowToken;
        firstExpressionToken = updatedNode.ExpressionBody.Expression.GetFirstToken();

        var previousToken = arrowToken.GetPreviousToken();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>
                             {
                                 [arrowToken] = arrowToken.WithLeadingTrivia(SyntaxFactory.Space)
                                                          .WithTrailingTrivia(SyntaxFactory.Space),
                                 [firstExpressionToken] = firstExpressionToken.WithLeadingTrivia(SyntaxFactory.TriviaList()),
                             };

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            replacementMap[previousToken] = previousToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(previousToken.TrailingTrivia)));
        }

        return updatedNode.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Determines whether the given node contains comments or directives
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns><see langword="true"/> if comments or directives are present; otherwise, <see langword="false"/></returns>
    private static bool HasCommentsOrDirectives(SyntaxNode node)
    {
        foreach (var trivia in node.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given text span occupies a single line
    /// </summary>
    /// <param name="syntaxTree">Syntax tree</param>
    /// <param name="span">Text span</param>
    /// <returns><see langword="true"/> if the span occupies a single line; otherwise, <see langword="false"/></returns>
    private static bool IsSingleLineSpan(SyntaxTree syntaxTree, TextSpan span)
    {
        var lineSpan = syntaxTree.GetLineSpan(span);

        return lineSpan.StartLinePosition.Line == lineSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Gets the first token of the property signature while skipping property-level attributes
    /// </summary>
    /// <param name="node">The property declaration to inspect</param>
    /// <returns>The first signature token</returns>
    private static SyntaxToken GetSingleLineSignatureStartToken(PropertyDeclarationSyntax node)
    {
        if (node.Modifiers.Count > 0)
        {
            return node.Modifiers[0];
        }

        return node.Type.GetFirstToken();
    }

    /// <summary>
    /// Determines whether the given auto-property can be collapsed to a single line
    /// </summary>
    /// <param name="node">The property declaration to inspect</param>
    /// <returns><see langword="true"/> if the auto-property can be collapsed; otherwise, <see langword="false"/></returns>
    private static bool CanCollapseAutoPropertyToSingleLine(PropertyDeclarationSyntax node)
    {
        if (node?.AccessorList == null || HasCommentsOrDirectives(node.AccessorList))
        {
            return false;
        }

        var tokenBeforeOpenBrace = node.AccessorList.OpenBraceToken.GetPreviousToken();
        var signatureStartToken = GetSingleLineSignatureStartToken(node);

        if (signatureStartToken == default
            || signatureStartToken.IsKind(SyntaxKind.None)
            || tokenBeforeOpenBrace == default
            || tokenBeforeOpenBrace.IsKind(SyntaxKind.None))
        {
            return false;
        }

        if (IsSingleLineSpan(node.SyntaxTree, TextSpan.FromBounds(signatureStartToken.SpanStart, tokenBeforeOpenBrace.Span.End)) == false)
        {
            return false;
        }

        if (node.Initializer != null)
        {
            if (HasCommentsOrDirectives(node.Initializer))
            {
                return false;
            }

            if (IsSingleLineSpan(node.SyntaxTree, node.Initializer.Value.Span) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Collapses a multi-line auto-property accessor list to a single line
    /// </summary>
    /// <param name="node">The property declaration with an auto-property accessor list</param>
    /// <returns>The property declaration with a single-line accessor list</returns>
    private static PropertyDeclarationSyntax CollapseAutoPropertyAccessorList(PropertyDeclarationSyntax node)
    {
        if (node?.AccessorList == null || LineBreakDetection.IsAutoPropertyAccessorList(node.AccessorList) == false)
        {
            return node;
        }

        var updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(node, node.AccessorList.OpenBraceToken);

        for (var accessorIndex = 0; accessorIndex < updatedNode.AccessorList.Accessors.Count; accessorIndex++)
        {
            var accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            for (var attributeListIndex = 0; attributeListIndex < accessor.AttributeLists.Count; attributeListIndex++)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.AttributeLists[attributeListIndex].OpenBracketToken);
                accessor = updatedNode.AccessorList.Accessors[accessorIndex];
            }

            updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.Keyword);
            accessor = updatedNode.AccessorList.Accessors[accessorIndex];

            if (accessor.SemicolonToken.IsMissing == false)
            {
                updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, accessor.SemicolonToken);
            }
        }

        updatedNode = LineBreakTriviaUtilities.CollapseTokenToSameLine(updatedNode, updatedNode.AccessorList.CloseBraceToken);

        var accessorList = updatedNode.AccessorList;
        var previousToken = accessorList.OpenBraceToken.GetPreviousToken();
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>
                             {
                                 [accessorList.OpenBraceToken] = accessorList.OpenBraceToken.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                                                            .WithTrailingTrivia(SyntaxFactory.Space),
                                 [accessorList.CloseBraceToken] = accessorList.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.TriviaList()),
                             };

        if (previousToken != default && previousToken.IsKind(SyntaxKind.None) == false)
        {
            replacementMap[previousToken] = previousToken.WithTrailingTrivia(SyntaxFactory.Space);
        }

        foreach (var accessor in accessorList.Accessors)
        {
            foreach (var attributeList in accessor.AttributeLists)
            {
                replacementMap[attributeList.OpenBracketToken] = attributeList.OpenBracketToken.WithLeadingTrivia(SyntaxFactory.TriviaList());
            }

            var tokenBeforeKeyword = accessor.Keyword.GetPreviousToken();

            if (tokenBeforeKeyword != default && tokenBeforeKeyword.IsKind(SyntaxKind.None) == false)
            {
                replacementMap[tokenBeforeKeyword] = tokenBeforeKeyword.WithTrailingTrivia(SyntaxFactory.Space);
            }

            replacementMap[accessor.Keyword] = accessor.Keyword.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                               .WithTrailingTrivia(SyntaxFactory.TriviaList());

            if (accessor.SemicolonToken.IsMissing == false)
            {
                replacementMap[accessor.SemicolonToken] = accessor.SemicolonToken.WithLeadingTrivia(SyntaxFactory.TriviaList())
                                                                                 .WithTrailingTrivia(SyntaxFactory.Space);
            }
        }

        return updatedNode.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Gets the constraint clauses from a syntax node if it supports them
    /// </summary>
    /// <param name="node">The syntax node to inspect</param>
    /// <returns>The list of constraint clauses, or <see langword="null"/> if not applicable</returns>
    private static SyntaxList<TypeParameterConstraintClauseSyntax>? GetConstraintClauses(SyntaxNode node)
    {
        switch (node)
        {
            case ClassDeclarationSyntax classDeclaration:
                return classDeclaration.ConstraintClauses;

            case StructDeclarationSyntax structDeclaration:
                return structDeclaration.ConstraintClauses;

            case InterfaceDeclarationSyntax interfaceDeclaration:
                return interfaceDeclaration.ConstraintClauses;

            case RecordDeclarationSyntax recordDeclaration:
                return recordDeclaration.ConstraintClauses;

            case MethodDeclarationSyntax methodDeclaration:
                return methodDeclaration.ConstraintClauses;

            case DelegateDeclarationSyntax delegateDeclaration:
                return delegateDeclaration.ConstraintClauses;

            case LocalFunctionStatementSyntax localFunction:
                return localFunction.ConstraintClauses;

            default:
                return null;
        }
    }

    /// <summary>
    /// Sets the constraint clauses on a syntax node
    /// </summary>
    /// <typeparam name="TNode">The syntax node type</typeparam>
    /// <param name="node">The syntax node to modify</param>
    /// <param name="constraintClauses">The new constraint clauses</param>
    /// <returns>The node with updated constraint clauses</returns>
    private static TNode SetConstraintClauses<TNode>(TNode node,
                                                     SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        where TNode : SyntaxNode
    {
        switch (node)
        {
            case ClassDeclarationSyntax classDeclaration:
                return (TNode)(SyntaxNode)classDeclaration.WithConstraintClauses(constraintClauses);

            case StructDeclarationSyntax structDeclaration:
                return (TNode)(SyntaxNode)structDeclaration.WithConstraintClauses(constraintClauses);

            case InterfaceDeclarationSyntax interfaceDeclaration:
                return (TNode)(SyntaxNode)interfaceDeclaration.WithConstraintClauses(constraintClauses);

            case RecordDeclarationSyntax recordDeclaration:
                return (TNode)(SyntaxNode)recordDeclaration.WithConstraintClauses(constraintClauses);

            case MethodDeclarationSyntax methodDeclaration:
                return (TNode)(SyntaxNode)methodDeclaration.WithConstraintClauses(constraintClauses);

            case DelegateDeclarationSyntax delegateDeclaration:
                return (TNode)(SyntaxNode)delegateDeclaration.WithConstraintClauses(constraintClauses);

            case LocalFunctionStatementSyntax localFunction:
                return (TNode)(SyntaxNode)localFunction.WithConstraintClauses(constraintClauses);

            default:
                return node;
        }
    }

    /// <summary>
    /// Collapses a parameter list opener onto the same line as its declaration token
    /// </summary>
    /// <typeparam name="TNode">The syntax node type that owns the declaration</typeparam>
    /// <param name="node">The syntax node that contains the declaration token and parameter list</param>
    /// <param name="declarationToken">The declaration token that should share a line with the opening parenthesis</param>
    /// <param name="parameterList">The parameter list to normalize</param>
    /// <returns>The updated syntax node</returns>
    private static TNode CollapseParameterListToDeclarationLine<TNode>(TNode node,
                                                                       SyntaxToken declarationToken,
                                                                       ParameterListSyntax parameterList)
        where TNode : SyntaxNode
    {
        if (TokenGapUtilities.HasLineBreakBetween(declarationToken, parameterList.OpenParenToken) == false)
        {
            return node;
        }

        var newDeclarationToken = declarationToken.WithTrailingTrivia(LineBreakTriviaUtilities.RemoveTrailingWhitespace(LineBreakTriviaUtilities.RemoveTrailingEndOfLineTrivia(declarationToken.TrailingTrivia)));
        var newOpenParen = LineBreakTriviaUtilities.RemoveLeadingEndOfLineAndWhitespace(parameterList.OpenParenToken);

        return node.ReplaceTokens([declarationToken, parameterList.OpenParenToken],
                                  (original, _) => original == declarationToken
                                                       ? newDeclarationToken
                                                       : newOpenParen);
    }

    /// <summary>
    /// Ensures the constructor initializer (<c>: base()</c> or <c>: this()</c>) starts on a new line
    /// </summary>
    /// <param name="node">The constructor declaration node</param>
    /// <returns>The constructor declaration with the initializer on a new line</returns>
    private ConstructorDeclarationSyntax EnsureConstructorInitializerOnNewLine(ConstructorDeclarationSyntax node)
    {
        if (node.Initializer == null)
        {
            return node;
        }

        var colonToken = node.Initializer.ColonToken;

        if (LineBreakTriviaUtilities.HasLeadingEndOfLine(colonToken))
        {
            return node;
        }

        var newColonToken = LineBreakTriviaUtilities.PrependEndOfLine(colonToken, Context.EndOfLine);

        return node.WithInitializer(node.Initializer.WithColonToken(newColonToken));
    }

    /// <summary>
    /// Ensures all <c>where</c> constraint clauses in a type declaration start on new lines
    /// </summary>
    /// <typeparam name="TNode">The type declaration syntax type</typeparam>
    /// <param name="node">The type declaration node</param>
    /// <returns>The node with <c>where</c> clauses on new lines</returns>
    private TNode EnsureGenericConstraintsOnNewLines<TNode>(TNode node)
        where TNode : SyntaxNode
    {
        var constraintClauses = GetConstraintClauses(node);

        if (constraintClauses == null || constraintClauses.Value.Count == 0)
        {
            return node;
        }

        var modified = false;
        var newClauses = new List<TypeParameterConstraintClauseSyntax>();

        foreach (var clause in constraintClauses.Value)
        {
            var whereKeyword = clause.WhereKeyword;

            if (LineBreakTriviaUtilities.HasLeadingEndOfLine(whereKeyword) == false)
            {
                var newWhereKeyword = LineBreakTriviaUtilities.PrependEndOfLine(whereKeyword, Context.EndOfLine);

                newClauses.Add(clause.WithWhereKeyword(newWhereKeyword));
                modified = true;
            }
            else
            {
                newClauses.Add(clause);
            }
        }

        if (modified == false)
        {
            return node;
        }

        return SetConstraintClauses(node, SyntaxFactory.List(newClauses));
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.OpenBraceToken.IsMissing == false)
        {
            node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
            node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.OpenBraceToken.IsMissing == false)
        {
            node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
            node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.OpenBraceToken.IsMissing == false)
        {
            node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
            node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.OpenBraceToken.IsMissing == false)
        {
            node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
            node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (EnumDeclarationSyntax)base.VisitEnumDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (NamespaceDeclarationSyntax)base.VisitNamespaceDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = BracePlacer.EnsureBraceOnOwnLine(node, node.OpenBraceToken, (n, t) => n.WithOpenBraceToken(t), node.CloseBraceToken, (n, t) => n.WithCloseBraceToken(t));
        node = BracePlacer.EnsureFirstContentOnNewLine(node, node.OpenBraceToken);
        node = BracePlacer.EnsureCloseBraceContinuation(node, node.CloseBraceToken);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);

        if (node.Initializer != null)
        {
            node = EnsureConstructorInitializerOnNewLine(node);
        }

        if (node.Body != null)
        {
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.OpenBraceToken, blankLineCount: 0);
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.Body.OpenBraceToken);
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.CloseBraceToken, blankLineCount: 0);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);
        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.Body != null)
        {
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.OpenBraceToken, blankLineCount: 0);
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.Body.OpenBraceToken);
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.CloseBraceToken, blankLineCount: 0);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);
        node = EnsureGenericConstraintsOnNewLines(node);

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (OperatorDeclarationSyntax)base.VisitOperatorDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.Body != null)
        {
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.OpenBraceToken, blankLineCount: 0);
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.Body.OpenBraceToken);
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.CloseBraceToken, blankLineCount: 0);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node);

        if (node == null)
        {
            return null;
        }

        node = CollapseParameterListToDeclarationLine(node, node.Identifier, node.ParameterList);
        node = EnsureGenericConstraintsOnNewLines(node);

        if (node.Body != null)
        {
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.OpenBraceToken, blankLineCount: 0);
            node = BracePlacer.EnsureFirstContentOnNewLine(node, node.Body.OpenBraceToken);
            node = GapNormalizer.NormalizeGapBeforeToken(node, node.Body.CloseBraceToken, blankLineCount: 0);
        }

        return node;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        CancellationToken.ThrowIfCancellationRequested();

        node = (PropertyDeclarationSyntax)base.VisitPropertyDeclaration(node);

        if (node == null)
        {
            return null;
        }

        if (node.ExpressionBody != null)
        {
            node = CollapseExpressionBodiedProperty(node);
        }

        if (node.AccessorList != null)
        {
            if (LineBreakDetection.IsAutoPropertyAccessorList(node.AccessorList))
            {
                if (CanCollapseAutoPropertyToSingleLine(node))
                {
                    node = CollapseAutoPropertyAccessorList(node);
                }
                else
                {
                    node = GapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.OpenBraceToken, blankLineCount: 0);
                    node = BracePlacer.EnsureFirstContentOnNewLine(node, node.AccessorList.OpenBraceToken);
                    node = GapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.CloseBraceToken, blankLineCount: 0);
                }
            }
            else
            {
                node = GapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.OpenBraceToken, blankLineCount: 0);
                node = BracePlacer.EnsureFirstContentOnNewLine(node, node.AccessorList.OpenBraceToken);
                node = GapNormalizer.NormalizeGapBeforeToken(node, node.AccessorList.CloseBraceToken, blankLineCount: 0);
            }
        }

        return node;
    }

    #endregion // CSharpSyntaxVisitor
}