using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Formatter.Rules.Indentation;

/// <summary>
/// Combined indentation and alignment rule that handles both block indentation
/// and continuation-line alignment patterns in a single pass.
/// <c>VisitToken</c> handles block indentation for all first-on-line tokens.
/// Node-level visitors override the block indentation for continuation lines.
/// </summary>
internal sealed class IndentationAndAlignmentRule : FormattingRuleBase
{
    #region Fields

    /// <summary>
    /// Tracks the assignment whitespace length of the parent initializer.
    /// Used to compute correct column positions for nested initializers.
    /// </summary>
    private readonly Stack<int> _parentAssignmentWhitespaceStack = new();

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public IndentationAndAlignmentRule(FormattingContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    #endregion // Constructor

    #region Block Indentation

    /// <inheritdoc/>
    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        token = base.VisitToken(token);

        if (token.IsKind(SyntaxKind.None))
        {
            return token;
        }

        if (IsFirstTokenOnLine(token) == false)
        {
            return token;
        }

        if (token.Parent != null
            && IsInsideExpressionLambdaArgumentBody(token.Parent)
            && IsContinuationToken(token))
        {
            return token;
        }

        var indentLevel = ComputeIndentLevel(token);
        var expectedIndentWidth = indentLevel * FormattingContext.IndentSize;

        if (IsSwitchSectionBlockBraceToken(token))
        {
            var currentIndentWidth = GetLeadingWhitespaceLength(token);

            if (currentIndentWidth > expectedIndentWidth)
            {
                expectedIndentWidth = currentIndentWidth;
            }
        }

        var expectedWhitespace = new string(' ', expectedIndentWidth);

        var leading = token.LeadingTrivia;
        var newLeading = ReplaceLeadingWhitespace(leading, expectedWhitespace);

        return token.WithLeadingTrivia(newLeading);
    }

    /// <summary>
    /// Determines whether the given token is the first token on its line
    /// (i.e., its leading trivia contains an end-of-line, or it is the very first token in the file).
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if the token is the first on its line; otherwise, <c>false</c>.</returns>
    private static bool IsFirstTokenOnLine(SyntaxToken token)
    {
        if (token.GetPreviousToken().IsKind(SyntaxKind.None))
        {
            return true;
        }

        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        var previousToken = token.GetPreviousToken();

        foreach (var trivia in previousToken.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the token is an opening or closing brace of a block that is a direct
    /// statement within a switch section.
    /// </summary>
    /// <param name="token">The token to inspect.</param>
    /// <returns><c>true</c> if the token is a switch-section block brace; otherwise, <c>false</c>.</returns>
    private static bool IsSwitchSectionBlockBraceToken(SyntaxToken token)
    {
        if (token.Parent is not BlockSyntax block || block.Parent is not SwitchSectionSyntax)
        {
            return false;
        }

        return token == block.OpenBraceToken || token == block.CloseBraceToken;
    }

    /// <summary>
    /// Replaces leading whitespace trivia (that appears after the last end-of-line in the leading trivia)
    /// with the expected indentation whitespace.
    /// </summary>
    /// <param name="leading">The leading trivia list.</param>
    /// <param name="expectedWhitespace">The expected whitespace string.</param>
    /// <returns>The updated trivia list.</returns>
    private static SyntaxTriviaList ReplaceLeadingWhitespace(SyntaxTriviaList leading, string expectedWhitespace)
    {
        var result = new List<SyntaxTrivia>(leading.Count);
        var lastLineBoundaryIndex = -1;

        // Detect BOM at the start of the trivia list
        var hasBom = leading.Count > 0
                     && leading[0].IsKind(SyntaxKind.WhitespaceTrivia)
                     && leading[0].ToString() == "\uFEFF";

        for (var i = 0; i < leading.Count; i++)
        {
            if (leading[i].IsKind(SyntaxKind.EndOfLineTrivia) || leading[i].HasStructure)
            {
                lastLineBoundaryIndex = i;
            }
        }

        if (lastLineBoundaryIndex == -1)
        {
            // No end-of-line in leading trivia — this is the very first token.
            // Remove any existing whitespace and prepend expected indentation.
            var startIndex = hasBom ? 1 : 0;

            if (hasBom)
            {
                result.Add(leading[0]);
            }

            if (startIndex < leading.Count && leading[startIndex].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                if (expectedWhitespace.Length > 0)
                {
                    result.Add(SyntaxFactory.Whitespace(expectedWhitespace));
                }

                for (var i = startIndex + 1; i < leading.Count; i++)
                {
                    result.Add(leading[i]);
                }
            }
            else
            {
                if (expectedWhitespace.Length > 0)
                {
                    result.Add(SyntaxFactory.Whitespace(expectedWhitespace));
                }

                for (var i = startIndex; i < leading.Count; i++)
                {
                    result.Add(leading[i]);
                }
            }

            return SyntaxFactory.TriviaList(result);
        }

        // Copy everything up to and including the last line boundary
        for (var i = 0; i <= lastLineBoundaryIndex; i++)
        {
            result.Add(leading[i]);
        }

        // Skip any existing whitespace after the last line boundary
        var afterBoundary = lastLineBoundaryIndex + 1;

        if (afterBoundary < leading.Count && leading[afterBoundary].IsKind(SyntaxKind.WhitespaceTrivia))
        {
            afterBoundary++;
        }

        // Insert expected indentation
        if (expectedWhitespace.Length > 0)
        {
            result.Add(SyntaxFactory.Whitespace(expectedWhitespace));
        }

        // Copy remaining non-whitespace trivia (e.g., comments, preprocessor directives)
        for (var i = afterBoundary; i < leading.Count; i++)
        {
            result.Add(leading[i]);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether the given syntax node is a construct that increases
    /// indentation for the specified token.
    /// </summary>
    /// <param name="node">The syntax node to check.</param>
    /// <param name="token">The token being indented.</param>
    /// <returns><c>true</c> if the node increases indentation for the token; otherwise, <c>false</c>.</returns>
    private static bool IsIndentingConstruct(SyntaxNode node, SyntaxToken token)
    {
        return node switch
               {
                   BlockSyntax block => IsInsideBraces(token, block.OpenBraceToken, block.CloseBraceToken),
                   TypeDeclarationSyntax typeDecl => IsInsideBraces(token, typeDecl.OpenBraceToken, typeDecl.CloseBraceToken),
                   NamespaceDeclarationSyntax nsDecl => IsInsideBraces(token, nsDecl.OpenBraceToken, nsDecl.CloseBraceToken),
                   EnumDeclarationSyntax enumDecl => IsInsideBraces(token, enumDecl.OpenBraceToken, enumDecl.CloseBraceToken),
                   SwitchStatementSyntax switchStmt => IsInsideBraces(token, switchStmt.OpenBraceToken, switchStmt.CloseBraceToken),
                   SwitchSectionSyntax switchSection => IsInsideSwitchSectionStatements(token, switchSection),
                   AccessorListSyntax accessorList => IsInsideBraces(token, accessorList.OpenBraceToken, accessorList.CloseBraceToken),
                   InitializerExpressionSyntax initializer => IsInsideBraces(token, initializer.OpenBraceToken, initializer.CloseBraceToken),
                   AnonymousObjectCreationExpressionSyntax anon => IsInsideBraces(token, anon.OpenBraceToken, anon.CloseBraceToken),
                   CollectionExpressionSyntax collection => IsInsideBraces(token, collection.OpenBracketToken, collection.CloseBracketToken),
                   _ => false
               };
    }

    /// <summary>
    /// Determines whether a token is positionally between the open and close braces
    /// (exclusive of the braces themselves).
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <param name="openBrace">The open brace token.</param>
    /// <param name="closeBrace">The close brace token.</param>
    /// <returns><c>true</c> if the token is inside the braces; otherwise, <c>false</c>.</returns>
    private static bool IsInsideBraces(SyntaxToken token, SyntaxToken openBrace, SyntaxToken closeBrace)
    {
        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return false;
        }

        return token.SpanStart > openBrace.SpanStart && token.SpanStart < closeBrace.SpanStart;
    }

    /// <summary>
    /// Determines whether the token is inside the statements of a switch section
    /// (i.e., not part of the case/default labels).
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <param name="switchSection">The switch section.</param>
    /// <returns><c>true</c> if the token is inside the section's statements; otherwise, <c>false</c>.</returns>
    private static bool IsInsideSwitchSectionStatements(SyntaxToken token, SwitchSectionSyntax switchSection)
    {
        var ancestor = token.Parent;

        while (ancestor != null && ancestor != switchSection)
        {
            if (ancestor is StatementSyntax statement && switchSection.Statements.Contains(statement))
            {
                if (statement is BlockSyntax block
                    && (token == block.OpenBraceToken || token == block.CloseBraceToken))
                {
                    return false;
                }

                return true;
            }

            ancestor = ancestor.Parent;
        }

        return false;
    }

    /// <summary>
    /// Computes the expected indentation level for the given token
    /// based on its ancestor nodes in the syntax tree.
    /// When the token's ancestor chain does not reach a <see cref="CompilationUnitSyntax"/>,
    /// the base indent level from the formatting context is added to account for
    /// parent context lost after structural transforms.
    /// </summary>
    /// <param name="token">The token to compute indentation for.</param>
    /// <returns>The indentation level (number of indent units).</returns>
    private int ComputeIndentLevel(SyntaxToken token)
    {
        var level = 0;
        var node = token.Parent;
        var reachedCompilationUnit = false;

        while (node != null)
        {
            if (node is CompilationUnitSyntax)
            {
                reachedCompilationUnit = true;
            }

            if (IsIndentingConstruct(node, token))
            {
                level++;
            }

            node = node.Parent;
        }

        if (reachedCompilationUnit == false)
        {
            level += Context.BaseIndentLevel;
        }

        return level;
    }

    #endregion // Block Indentation

    #region Argument Alignment

    /// <inheritdoc/>
    public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
    {
        var visited = (ArgumentListSyntax)base.VisitArgumentList(node);
        var adjustedColumn = AdjustColumnForNormalization(node.OpenParenToken, node.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Character);

        return AlignArgumentList(node, visited, adjustedColumn);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBracketedArgumentList(BracketedArgumentListSyntax node)
    {
        var visited = (BracketedArgumentListSyntax)base.VisitBracketedArgumentList(node);

        if (visited == null)
        {
            return null;
        }

        if (visited.Arguments.Count <= 1)
        {
            return visited;
        }

        var openBracketColumn = AdjustColumnForNormalization(node.OpenBracketToken, node.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var alignColumn = openBracketColumn + 1;
        var firstArgLine = node.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        // Align each argument that starts on a line after the first argument
        var newArguments = AlignNodesToColumn(node.Arguments, visited.Arguments, firstArgLine, alignColumn);

        if (newArguments == null)
        {
            return visited;
        }

        return visited.WithArguments(SyntaxFactory.SeparatedList(newArguments, visited.Arguments.GetSeparators()));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAttributeArgumentList(AttributeArgumentListSyntax node)
    {
        var visited = (AttributeArgumentListSyntax)base.VisitAttributeArgumentList(node);

        if (visited == null)
        {
            return null;
        }

        if (visited.Arguments.Count <= 1)
        {
            return visited;
        }

        var openParenColumn = AdjustColumnForNormalization(node.OpenParenToken, node.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var alignColumn = openParenColumn + 1;
        var firstArgLine = node.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        var newArguments = AlignNodesToColumn(node.Arguments, visited.Arguments, firstArgLine, alignColumn);

        if (newArguments == null)
        {
            return visited;
        }

        return visited.WithArguments(SyntaxFactory.SeparatedList(newArguments, visited.Arguments.GetSeparators()));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitParameterList(ParameterListSyntax node)
    {
        var visited = (ParameterListSyntax)base.VisitParameterList(node);

        if (visited == null)
        {
            return null;
        }

        if (visited.Parameters.Count <= 1)
        {
            return visited;
        }

        var openParenColumn = AdjustColumnForNormalization(node.OpenParenToken, node.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var alignColumn = openParenColumn + 1;
        var firstParamLine = node.Parameters[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        var newParams = AlignNodesToColumn(node.Parameters, visited.Parameters, firstParamLine, alignColumn);

        if (newParams == null)
        {
            return visited;
        }

        return visited.WithParameters(SyntaxFactory.SeparatedList(newParams, visited.Parameters.GetSeparators()));
    }

    /// <summary>
    /// Aligns nodes in a separated list to a target column. Nodes that start on a line
    /// after the first node's line are aligned to the specified column.
    /// </summary>
    /// <typeparam name="T">The type of syntax node.</typeparam>
    /// <param name="originalNodes">The original nodes (for position information).</param>
    /// <param name="visitedNodes">The visited nodes to align.</param>
    /// <param name="firstNodeLine">The line of the first node.</param>
    /// <param name="alignColumn">The column to align to.</param>
    /// <returns>A list of aligned nodes, or <c>null</c> if no changes were made.</returns>
    private static List<T> AlignNodesToColumn<T>(SeparatedSyntaxList<T> originalNodes, SeparatedSyntaxList<T> visitedNodes, int firstNodeLine, int alignColumn)
        where T : SyntaxNode
    {
        var hasChanges = false;
        var result = new List<T>(visitedNodes.Count);

        for (var nodeIndex = 0; nodeIndex < originalNodes.Count && nodeIndex < visitedNodes.Count; nodeIndex++)
        {
            var originalNode = originalNodes[nodeIndex];
            var visitedNode = visitedNodes[nodeIndex];
            var nodeLine = originalNode.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (nodeLine > firstNodeLine)
            {
                var aligned = AlignNodeToColumn(visitedNode, alignColumn);

                result.Add(aligned);

                if (aligned != visitedNode)
                {
                    hasChanges = true;
                }
            }
            else
            {
                result.Add(visitedNode);
            }
        }

        if (hasChanges == false)
        {
            return null;
        }

        return result;
    }

    /// <summary>
    /// Aligns a single syntax node to the specified column by replacing the leading
    /// whitespace of its first token.
    /// </summary>
    /// <typeparam name="T">The type of syntax node.</typeparam>
    /// <param name="node">The node to align.</param>
    /// <param name="column">The target column.</param>
    /// <returns>The aligned node.</returns>
    private static T AlignNodeToColumn<T>(T node, int column)
        where T : SyntaxNode
    {
        var firstToken = node.GetFirstToken();
        var leading = firstToken.LeadingTrivia;
        var newLeading = ReplaceLeadingWhitespace(leading, new string(' ', column));
        var newToken = firstToken.WithLeadingTrivia(newLeading);

        return node.ReplaceToken(firstToken, newToken);
    }

    /// <summary>
    /// Aligns arguments in an argument list so that continuation arguments
    /// are aligned to the column after the opening parenthesis.
    /// </summary>
    /// <param name="originalNode">The original argument list node (for position information).</param>
    /// <param name="visited">The visited argument list node.</param>
    /// <param name="adjustedOpenParenColumn">The column of the opening parenthesis, adjusted for block indent normalization.</param>
    /// <returns>The argument list with aligned arguments.</returns>
    private ArgumentListSyntax AlignArgumentList(ArgumentListSyntax originalNode, ArgumentListSyntax visited, int adjustedOpenParenColumn)
    {
        if (visited.Arguments.Count == 0)
        {
            return visited;
        }

        var normalizedVisited = CollapseFirstArgumentToOpenParenLine(originalNode, visited);
        var firstArgumentCollapsed = normalizedVisited != visited;

        visited = normalizedVisited;

        var alignColumn = adjustedOpenParenColumn + 1;
        var firstArgLine = originalNode.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;
        var openParenLine = originalNode.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (visited.Arguments.Count == 1)
        {
            var alignedArgument = visited.Arguments[0];
            var hasChanges = firstArgumentCollapsed;

            if (firstArgLine > openParenLine && firstArgumentCollapsed == false)
            {
                alignedArgument = AlignNodeToColumn(alignedArgument, alignColumn);
                hasChanges = alignedArgument != visited.Arguments[0];
            }

            var singleArgumentList = new List<ArgumentSyntax>(1);

            singleArgumentList.Add(alignedArgument);
            var singleArgumentLambdaShifted = ShiftLambdaBlockBodies(originalNode.Arguments, singleArgumentList, firstArgLine, alignColumn);

            if (hasChanges == false && singleArgumentLambdaShifted == false)
            {
                return visited;
            }

            return visited.WithArguments(SyntaxFactory.SeparatedList(singleArgumentList, visited.Arguments.GetSeparators()));
        }

        List<ArgumentSyntax> alignedArguments = null;

        if (visited.Arguments.Count > 1)
        {
            alignedArguments = AlignNodesToColumn(originalNode.Arguments, visited.Arguments, firstArgLine, alignColumn);
        }

        var newArguments = alignedArguments ?? visited.Arguments.ToList();
        var lambdaShifted = ShiftLambdaBlockBodies(originalNode.Arguments, newArguments, firstArgLine, alignColumn);

        if (alignedArguments == null && lambdaShifted == false && firstArgumentCollapsed == false)
        {
            return visited;
        }

        return visited.WithArguments(SyntaxFactory.SeparatedList(newArguments, visited.Arguments.GetSeparators()));
    }

    /// <summary>
    /// Moves the first argument to the same line as the opening parenthesis when it was originally
    /// placed on a following line.
    /// </summary>
    /// <param name="originalNode">The original argument list node.</param>
    /// <param name="visited">The visited argument list node.</param>
    /// <returns>The updated argument list.</returns>
    private ArgumentListSyntax CollapseFirstArgumentToOpenParenLine(ArgumentListSyntax originalNode, ArgumentListSyntax visited)
    {
        if (visited.Arguments.Count == 0)
        {
            return visited;
        }

        var firstArgLine = originalNode.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;
        var openParenLine = originalNode.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (firstArgLine <= openParenLine)
        {
            return visited;
        }

        var hasChanges = false;
        var updatedOpenParen = visited.OpenParenToken;
        var strippedOpenParenTrailing = StripTrailingEndOfLine(updatedOpenParen.TrailingTrivia);

        if (strippedOpenParenTrailing.Count != updatedOpenParen.TrailingTrivia.Count)
        {
            updatedOpenParen = updatedOpenParen.WithTrailingTrivia(strippedOpenParenTrailing);
            hasChanges = true;
        }

        var updatedFirstArgument = visited.Arguments[0];
        var firstToken = updatedFirstArgument.GetFirstToken();
        var strippedFirstLeading = StripLeadingEndOfLineAndWhitespace(firstToken.LeadingTrivia);

        if (strippedFirstLeading.Count != firstToken.LeadingTrivia.Count)
        {
            var updatedFirstToken = firstToken.WithLeadingTrivia(strippedFirstLeading);

            updatedFirstArgument = updatedFirstArgument.ReplaceToken(firstToken, updatedFirstToken);
            hasChanges = true;
        }

        if (hasChanges == false)
        {
            return visited;
        }

        var updatedArguments = visited.Arguments.ToList();

        updatedArguments[0] = updatedFirstArgument;

        return visited.WithOpenParenToken(updatedOpenParen)
                      .WithArguments(SyntaxFactory.SeparatedList(updatedArguments, visited.Arguments.GetSeparators()));
    }

    /// <summary>
    /// Shifts the block bodies of lambda arguments so that their indentation
    /// matches the argument alignment column instead of tree-based indentation.
    /// </summary>
    /// <param name="originalArguments">The original arguments from the unmodified tree.</param>
    /// <param name="arguments">The list of aligned arguments to process.</param>
    /// <param name="firstArgumentLine">The line of the first argument in the argument list.</param>
    /// <param name="alignedArgumentColumn">The target alignment column for arguments on subsequent lines.</param>
    /// <returns><c>true</c> if any lambda block body indentation changed; otherwise, <c>false</c>.</returns>
    private bool ShiftLambdaBlockBodies(SeparatedSyntaxList<ArgumentSyntax> originalArguments, List<ArgumentSyntax> arguments, int firstArgumentLine, int alignedArgumentColumn)
    {
        var hasChanges = false;

        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];

            if (i >= originalArguments.Count)
            {
                continue;
            }

            var originalArgument = originalArguments[i];
            var originalArgumentFirstToken = originalArgument.GetFirstToken();
            var originalArgumentLine = GetLine(originalArgumentFirstToken);

            int alignColumn;

            if (originalArgumentLine > firstArgumentLine)
            {
                alignColumn = alignedArgumentColumn;

                if (originalArgument.NameColon != null)
                {
                    alignColumn = AdjustColumnForNormalization(originalArgument.Expression.GetFirstToken(),
                                                               GetColumn(originalArgument.Expression.GetFirstToken()));
                }
            }
            else
            {
                if (arguments.Count == 1
                    && (originalArgument.Expression is ParenthesizedLambdaExpressionSyntax || originalArgument.Expression is SimpleLambdaExpressionSyntax)
                    && originalArgument.Parent is ArgumentListSyntax { Parent: InvocationExpressionSyntax invocation }
                    && invocation.Expression is not MemberAccessExpressionSyntax
                    && invocation.Expression is not ConditionalAccessExpressionSyntax)
                {
                    continue;
                }

                alignColumn = AdjustColumnForNormalization(originalArgumentFirstToken, GetColumn(originalArgumentFirstToken));
            }

            BlockSyntax block;

            if (argument.Expression is ParenthesizedLambdaExpressionSyntax parenLambda)
            {
                block = parenLambda.Body as BlockSyntax;
            }
            else if (argument.Expression is SimpleLambdaExpressionSyntax simpleLambda)
            {
                block = simpleLambda.Body as BlockSyntax;
            }
            else
            {
                continue;
            }

            if (block == null)
            {
                continue;
            }

            var blockOpenBrace = block.OpenBraceToken;

            if (IsFirstTokenOnLine(blockOpenBrace) == false)
            {
                continue;
            }

            var currentIndent = GetLeadingWhitespaceLength(blockOpenBrace);
            var shift = alignColumn - currentIndent;

            if (shift == 0)
            {
                continue;
            }

            var tokensToReplace = new Dictionary<SyntaxToken, SyntaxToken>();

            foreach (var token in block.DescendantTokens())
            {
                if (IsFirstTokenOnLine(token) == false)
                {
                    continue;
                }

                var tokenIndent = GetLeadingWhitespaceLength(token);
                var newIndent = tokenIndent + shift;

                if (newIndent < 0)
                {
                    newIndent = 0;
                }

                var newLeading = ReplaceLeadingWhitespace(token.LeadingTrivia, new string(' ', newIndent));

                tokensToReplace.Add(token, token.WithLeadingTrivia(newLeading));
            }

            if (tokensToReplace.Count > 0)
            {
                arguments[i] = argument.ReplaceTokens(tokensToReplace.Keys, (original, _) => tokensToReplace[original]);
                hasChanges = true;
            }
        }

        return hasChanges;
    }

    #endregion // Argument Alignment

    #region Constructor Initializer Alignment

    /// <inheritdoc/>
    public override SyntaxNode VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        var visited = (ConstructorDeclarationSyntax)base.VisitConstructorDeclaration(node);

        if (visited?.Initializer == null)
        {
            return visited;
        }

        var constructorFirstToken = node.GetFirstToken();
        var constructorIndent = AdjustColumnForNormalization(constructorFirstToken, GetColumn(constructorFirstToken));
        var initializerIndent = constructorIndent + FormattingContext.IndentSize;
        var indentString = new string(' ', initializerIndent);

        // The colon token of the initializer should be on a new line with the computed indent
        var colonToken = visited.Initializer.ColonToken;
        var newColonToken = colonToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                         SyntaxFactory.Whitespace(indentString));

        visited = visited.WithInitializer(visited.Initializer.WithColonToken(newColonToken));

        return StripTrailingEndOfLineFromPreviousToken(visited, visited.Initializer!.ColonToken);
    }

    #endregion // Constructor Initializer Alignment

    #region Generic Constraint Alignment

    /// <inheritdoc/>
    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var visited = (ClassDeclarationSyntax)base.VisitClassDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitStructDeclaration(StructDeclarationSyntax node)
    {
        var visited = (StructDeclarationSyntax)base.VisitStructDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        var visited = (InterfaceDeclarationSyntax)base.VisitInterfaceDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        var visited = (RecordDeclarationSyntax)base.VisitRecordDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        var visited = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        var visited = (DelegateDeclarationSyntax)base.VisitDelegateDeclaration(node);

        if (visited == null)
        {
            return null;
        }

        return AlignConstraintClauses(node, visited, node.ConstraintClauses, visited.ConstraintClauses, (v, c) => v.WithConstraintClauses(c));
    }

    /// <summary>
    /// Aligns <c>where</c> constraint clauses so they are indented by one indent level
    /// relative to the declaration.
    /// </summary>
    /// <typeparam name="T">The type of the declaration syntax node.</typeparam>
    /// <param name="original">The original declaration node (for position information).</param>
    /// <param name="visited">The visited declaration node.</param>
    /// <param name="originalClauses">The original constraint clauses.</param>
    /// <param name="visitedClauses">The visited constraint clauses.</param>
    /// <param name="withClauses">A function that creates a new declaration with the given constraint clauses.</param>
    /// <returns>The declaration with aligned constraint clauses.</returns>
    private T AlignConstraintClauses<T>(T original, T visited, SyntaxList<TypeParameterConstraintClauseSyntax> originalClauses, SyntaxList<TypeParameterConstraintClauseSyntax> visitedClauses, Func<T, SyntaxList<TypeParameterConstraintClauseSyntax>, T> withClauses)
        where T : SyntaxNode
    {
        if (originalClauses.Count == 0)
        {
            return visited;
        }

        var declarationFirstToken = original.GetFirstToken();
        var declarationIndent = AdjustColumnForNormalization(declarationFirstToken, GetColumn(declarationFirstToken));
        var constraintIndent = declarationIndent + FormattingContext.IndentSize;
        var indentString = new string(' ', constraintIndent);

        var newClauses = new List<TypeParameterConstraintClauseSyntax>();
        var hasChanges = false;

        foreach (var clause in visitedClauses)
        {
            var whereKeyword = clause.WhereKeyword;
            var newWhereKeyword = whereKeyword.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                 SyntaxFactory.Whitespace(indentString));

            if (whereKeyword != newWhereKeyword)
            {
                hasChanges = true;
            }

            newClauses.Add(clause.WithWhereKeyword(newWhereKeyword));
        }

        if (hasChanges == false)
        {
            return visited;
        }

        var result = withClauses(visited, SyntaxFactory.List(newClauses));

        // Strip trailing EOL from the token before each where keyword to prevent blank lines.
        // Process in reverse order so that earlier clause tokens remain valid after each replacement.
        for (var i = newClauses.Count - 1; i >= 0; i--)
        {
            var resultClauses = GetConstraintClauses(result);

            if (resultClauses.Count > i)
            {
                result = StripTrailingEndOfLineFromPreviousToken(result, resultClauses[i].WhereKeyword);
            }
        }

        return result;
    }

    #endregion // Generic Constraint Alignment

    #region Switch Expression Layout

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchExpression(SwitchExpressionSyntax node)
    {
        var visited = (SwitchExpressionSyntax)base.VisitSwitchExpression(node);

        if (visited == null)
        {
            return null;
        }

        // Only format multi-line switch expressions
        var switchLine = node.SwitchKeyword.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeBraceLine = node.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (switchLine == closeBraceLine)
        {
            return visited;
        }

        // Align braces to the governing expression's start column
        var governingExpressionToken = node.GoverningExpression.GetFirstToken();
        var governingExpressionColumn = AdjustColumnForNormalization(governingExpressionToken, governingExpressionToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var braceIndent = new string(' ', governingExpressionColumn);
        var armIndent = governingExpressionColumn + FormattingContext.IndentSize;

        // Align opening brace
        var openBrace = visited.OpenBraceToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                 SyntaxFactory.Whitespace(braceIndent));

        // Align each arm
        var newArms = new List<SwitchExpressionArmSyntax>();

        for (var armIndex = 0; armIndex < visited.Arms.Count && armIndex < node.Arms.Count; armIndex++)
        {
            var originalArm = node.Arms[armIndex];
            var originalArmFirstToken = originalArm.GetFirstToken();
            var originalArmColumn = AdjustColumnForNormalization(originalArmFirstToken, GetColumn(originalArmFirstToken));
            var armShift = armIndent - originalArmColumn;
            var alignedArm = AlignNodeToColumn(visited.Arms[armIndex], armIndent);

            if (armShift != 0)
            {
                alignedArm = ShiftArmContinuationLines(alignedArm, originalArm.GetLocation().GetLineSpan().StartLinePosition.Line, armShift);
            }

            var originalArmFirstLine = originalArm.GetLocation().GetLineSpan().StartLinePosition.Line;
            var originalLineLeadingOrCount = 0;

            foreach (var token in originalArm.DescendantTokens())
            {
                if (token.IsKind(SyntaxKind.OrKeyword)
                    && IsFirstTokenOnLine(token)
                    && GetLine(token) > originalArmFirstLine)
                {
                    originalLineLeadingOrCount++;
                }
            }

            if (originalLineLeadingOrCount > 0)
            {
                var visitedLineLeadingOrTokens = new List<SyntaxToken>();

                foreach (var token in alignedArm.DescendantTokens())
                {
                    if (token.IsKind(SyntaxKind.OrKeyword) && IsFirstTokenOnLine(token))
                    {
                        visitedLineLeadingOrTokens.Add(token);
                    }
                }

                var replacements = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

                for (var i = 0; i < originalLineLeadingOrCount && i < visitedLineLeadingOrTokens.Count; i++)
                {
                    var visitedOrToken = visitedLineLeadingOrTokens[i];
                    var newLeading = ReplaceLeadingWhitespace(visitedOrToken.LeadingTrivia, new string(' ', armIndent));

                    replacements.Add((visitedOrToken, visitedOrToken.WithLeadingTrivia(newLeading)));
                }

                if (replacements.Count > 0)
                {
                    alignedArm = alignedArm.ReplaceTokens(replacements.ConvertAll(pair => pair.OldToken),
                                                          (original, _) =>
                                                          {
                                                              var pair = replacements.FirstOrDefault(pair => pair.OldToken == original);

                                                              if (pair != default)
                                                              {
                                                                  return pair.NewToken;
                                                              }

                                                              return original;
                                                          });
                }
            }

            newArms.Add(alignedArm);
        }

        // Align closing brace
        var closeBrace = visited.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                   SyntaxFactory.Whitespace(braceIndent));

        visited = visited.WithOpenBraceToken(openBrace)
                         .WithArms(SyntaxFactory.SeparatedList(newArms, visited.Arms.GetSeparators()))
                         .WithCloseBraceToken(closeBrace);

        // Strip trailing EOL from tokens before the braces to prevent blank lines
        visited = StripTrailingEndOfLineFromPreviousToken(visited, visited.OpenBraceToken);

        return StripTrailingEndOfLineFromPreviousToken(visited, visited.CloseBraceToken);
    }

    /// <summary>
    /// Shifts continuation lines inside a switch expression arm by the provided column delta,
    /// preserving relative layout when the arm head itself is realigned.
    /// </summary>
    /// <param name="arm">The visited arm to shift.</param>
    /// <param name="firstLine">The first line of the arm head.</param>
    /// <param name="columnShift">The number of columns to shift continuation lines.</param>
    /// <returns>The shifted arm.</returns>
    private static SwitchExpressionArmSyntax ShiftArmContinuationLines(SwitchExpressionArmSyntax arm, int firstLine, int columnShift)
    {
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in arm.DescendantTokens())
        {
            if (IsFirstTokenOnLine(token) == false || GetLine(token) <= firstLine)
            {
                continue;
            }

            var currentIndent = GetLeadingWhitespaceLength(token);
            var shiftedIndent = currentIndent + columnShift;

            if (shiftedIndent < 0)
            {
                shiftedIndent = 0;
            }

            var newLeading = ReplaceLeadingWhitespace(token.LeadingTrivia, new string(' ', shiftedIndent));

            replacements[token] = token.WithLeadingTrivia(newLeading);
        }

        if (replacements.Count == 0)
        {
            return arm;
        }

        return arm.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    #endregion // Switch Expression Layout

    #region Conditional Expression Layout

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalExpression(ConditionalExpressionSyntax node)
    {
        var visited = (ConditionalExpressionSyntax)base.VisitConditionalExpression(node);

        if (visited == null)
        {
            return null;
        }

        var conditionEndLine = node.Condition.GetLocation().GetLineSpan().EndLinePosition.Line;
        var questionLine = GetLine(node.QuestionToken);
        var whenTrueLine = GetLine(node.WhenTrue.GetFirstToken());

        if (ShouldFormatConditionalExpression(conditionEndLine, questionLine, whenTrueLine) == false)
        {
            return visited;
        }

        // Align ? and : relative to the computed conditional alignment anchor.
        var alignColumn = GetConditionalQuestionAlignmentColumn(node);

        var colonLine = node.ColonToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        var newQuestion = visited.QuestionToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                  SyntaxFactory.Whitespace(new string(' ', alignColumn)));

        if (questionLine == conditionEndLine && whenTrueLine > questionLine)
        {
            newQuestion = newQuestion.WithTrailingTrivia(SyntaxFactory.Whitespace(" "));

            var whenTrueFirstToken = visited.WhenTrue.GetFirstToken();
            var strippedLeadingTrivia = StripLeadingEndOfLineAndWhitespace(whenTrueFirstToken.LeadingTrivia);

            if (strippedLeadingTrivia.Count != whenTrueFirstToken.LeadingTrivia.Count)
            {
                visited = visited.ReplaceToken(whenTrueFirstToken, whenTrueFirstToken.WithLeadingTrivia(strippedLeadingTrivia));
            }
        }

        SyntaxToken newColon;

        if (colonLine > questionLine)
        {
            newColon = visited.ColonToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                            SyntaxFactory.Whitespace(new string(' ', alignColumn)));
        }
        else
        {
            newColon = visited.ColonToken;
        }

        visited = visited.WithQuestionToken(newQuestion)
                         .WithColonToken(newColon);

        // Strip trailing EOL from the token before ? to prevent blank line
        visited = StripTrailingEndOfLineFromPreviousToken(visited, visited.QuestionToken);

        // Strip trailing EOL from the token before : if it was reformatted
        if (colonLine > questionLine)
        {
            visited = StripTrailingEndOfLineFromPreviousToken(visited, visited.ColonToken);
        }

        return visited;
    }

    /// <summary>
    /// Determines whether a conditional expression should be aligned based on the line layout
    /// of the condition, <c>?</c> token, and true branch.
    /// </summary>
    /// <param name="conditionEndLine">The line where the condition ends.</param>
    /// <param name="questionLine">The line where the <c>?</c> token appears.</param>
    /// <param name="whenTrueLine">The line where the true branch begins.</param>
    /// <returns><c>true</c> if the conditional expression should be formatted; otherwise, <c>false</c>.</returns>
    private static bool ShouldFormatConditionalExpression(int conditionEndLine, int questionLine, int whenTrueLine)
    {
        return questionLine != conditionEndLine || whenTrueLine > questionLine;
    }

    /// <summary>
    /// Computes the target alignment column for the <c>?</c> token of a conditional expression.
    /// For nested conditional expressions in the parent's <c>whenTrue</c> branch, where the nested
    /// condition starts on the same line as the parent's <c>?</c>, alignment is based on the parent
    /// <c>?</c> column plus one indent level.
    /// </summary>
    /// <param name="node">The conditional expression node.</param>
    /// <returns>The target column for the <c>?</c> token.</returns>
    private int GetConditionalQuestionAlignmentColumn(ConditionalExpressionSyntax node)
    {
        if (node.Parent is ConditionalExpressionSyntax parentConditional
            && parentConditional.WhenTrue == node
            && GetLine(parentConditional.QuestionToken) == GetLine(node.Condition.GetFirstToken()))
        {
            return GetConditionalQuestionAlignmentColumn(parentConditional) + FormattingContext.IndentSize;
        }

        var conditionFirstToken = node.Condition.GetFirstToken();
        var conditionStartColumn = conditionFirstToken.GetLocation().GetLineSpan().StartLinePosition.Character;

        return AdjustColumnForNormalization(conditionFirstToken, conditionStartColumn) + FormattingContext.IndentSize;
    }

    #endregion // Conditional Expression Layout

    #region Object Initializer Layout

    /// <inheritdoc/>
    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        var newKeywordColumn = ComputeObjectCreationNewKeywordColumn(node.NewKeyword, node.Parent, node.SpanStart);

        _parentAssignmentWhitespaceStack.Push(newKeywordColumn + FormattingContext.IndentSize);

        var visited = (ObjectCreationExpressionSyntax)base.VisitObjectCreationExpression(node);

        _parentAssignmentWhitespaceStack.Pop();

        if (visited == null)
        {
            return null;
        }

        if (visited.Initializer == null
            || (visited.Initializer.Kind() != SyntaxKind.ObjectInitializerExpression
                && visited.Initializer.Kind() != SyntaxKind.CollectionInitializerExpression))
        {
            return visited;
        }

        if ((node.Parent is MemberAccessExpressionSyntax || node.Parent is ConditionalAccessExpressionSyntax)
            && visited.Initializer.Kind() == SyntaxKind.ObjectInitializerExpression)
        {
            return visited;
        }

        var correctedInitializer = RebuildInitializer(visited, newKeywordColumn);
        var correctedObjectCreation = visited.WithInitializer(correctedInitializer)
                                             .WithLeadingTrivia(visited.GetLeadingTrivia())
                                             .WithTrailingTrivia(visited.GetTrailingTrivia());

        return StripTrailingEndOfLineFromPreviousToken(correctedObjectCreation, correctedObjectCreation.Initializer!.OpenBraceToken);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitImplicitObjectCreationExpression(ImplicitObjectCreationExpressionSyntax node)
    {
        var newKeywordColumn = ComputeObjectCreationNewKeywordColumn(node.NewKeyword, node.Parent, node.SpanStart);

        _parentAssignmentWhitespaceStack.Push(newKeywordColumn + FormattingContext.IndentSize);

        var visited = (ImplicitObjectCreationExpressionSyntax)base.VisitImplicitObjectCreationExpression(node);

        _parentAssignmentWhitespaceStack.Pop();

        if (visited == null)
        {
            return null;
        }

        if (visited.Initializer == null
            || (visited.Initializer.Kind() != SyntaxKind.ObjectInitializerExpression
                && visited.Initializer.Kind() != SyntaxKind.CollectionInitializerExpression))
        {
            return visited;
        }

        if ((node.Parent is MemberAccessExpressionSyntax || node.Parent is ConditionalAccessExpressionSyntax)
            && visited.Initializer.Kind() == SyntaxKind.ObjectInitializerExpression)
        {
            return visited;
        }

        var correctedInitializer = RebuildInitializer(visited.Initializer, newKeywordColumn);
        var correctedObjectCreation = visited.WithInitializer(correctedInitializer)
                                             .WithLeadingTrivia(visited.GetLeadingTrivia())
                                             .WithTrailingTrivia(visited.GetTrailingTrivia());

        return StripTrailingEndOfLineFromPreviousToken(correctedObjectCreation, correctedObjectCreation.Initializer!.OpenBraceToken);
    }

    /// <summary>
    /// Computes the alignment column for an object creation <c>new</c> keyword,
    /// accounting for nested initializer assignments and switch-expression arm behavior.
    /// </summary>
    /// <param name="newKeyword">The <c>new</c> keyword token.</param>
    /// <param name="parentNode">The parent node of the object creation expression.</param>
    /// <param name="nodeSpanStart">The span start of the object creation expression.</param>
    /// <returns>The target column for the <c>new</c> keyword.</returns>
    private int ComputeObjectCreationNewKeywordColumn(SyntaxToken newKeyword, SyntaxNode parentNode, int nodeSpanStart)
    {
        if (_parentAssignmentWhitespaceStack.Count > 0
            && parentNode is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment)
        {
            var parentAssignmentWhitespace = _parentAssignmentWhitespaceStack.Peek();
            var offsetToNew = nodeSpanStart - assignment.SpanStart;

            return parentAssignmentWhitespace + offsetToNew;
        }

        if (IsInsideSwitchExpressionArm(parentNode))
        {
            return newKeyword.GetLocation().GetLineSpan().StartLinePosition.Character;
        }

        return AdjustColumnForNormalization(newKeyword, newKeyword.GetLocation().GetLineSpan().StartLinePosition.Character);
    }

    /// <summary>
    /// Determines whether the given node is nested within a switch expression arm.
    /// </summary>
    /// <param name="node">The node to inspect.</param>
    /// <returns><c>true</c> if the node is within a switch expression arm; otherwise, <c>false</c>.</returns>
    private bool IsInsideSwitchExpressionArm(SyntaxNode node)
    {
        var current = node;

        while (current != null)
        {
            if (current is SwitchExpressionArmSyntax)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitAnonymousObjectCreationExpression(AnonymousObjectCreationExpressionSyntax node)
    {
        var newKeywordColumn = AdjustColumnForNormalization(node.NewKeyword, node.NewKeyword.GetLocation().GetLineSpan().StartLinePosition.Character);
        var visited = (AnonymousObjectCreationExpressionSyntax)base.VisitAnonymousObjectCreationExpression(node);

        if (visited == null)
        {
            return null;
        }

        if (node.Parent is MemberAccessExpressionSyntax || node.Parent is ConditionalAccessExpressionSyntax)
        {
            return visited;
        }

        var braceWhitespace = new string(' ', newKeywordColumn);
        var elementWhitespace = new string(' ', newKeywordColumn + FormattingContext.IndentSize);
        var hasChanges = false;

        if ((visited.Initializers.Count > 0
             && GetLine(visited.OpenBraceToken) == GetLine(visited.Initializers[0].GetFirstToken()))
            || GetLine(visited.OpenBraceToken) == GetLine(visited.NewKeyword))
        {
            var newOpenBraceLeading = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(Context.EndOfLine), SyntaxFactory.Whitespace(braceWhitespace));
            var newOpenBraceTrailing = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(Context.EndOfLine));
            var newOpenBraceToken = visited.OpenBraceToken;

            newOpenBraceToken = newOpenBraceToken.WithLeadingTrivia(newOpenBraceLeading);
            newOpenBraceToken = newOpenBraceToken.WithTrailingTrivia(newOpenBraceTrailing);

            if (newOpenBraceToken != visited.OpenBraceToken)
            {
                visited = visited.WithOpenBraceToken(newOpenBraceToken);
                hasChanges = true;
            }
        }

        if (visited.Initializers.Count > 0
            && GetLine(visited.CloseBraceToken) == GetLine(visited.Initializers[visited.Initializers.Count - 1].GetLastToken()))
        {
            var newCloseBraceLeading = SyntaxFactory.TriviaList(SyntaxFactory.EndOfLine(Context.EndOfLine), SyntaxFactory.Whitespace(braceWhitespace));
            var newCloseBraceToken = visited.CloseBraceToken.WithLeadingTrivia(newCloseBraceLeading);

            if (newCloseBraceToken != visited.CloseBraceToken)
            {
                visited = visited.WithCloseBraceToken(newCloseBraceToken);
                hasChanges = true;
            }
        }

        if (IsFirstTokenOnLine(visited.OpenBraceToken))
        {
            var newTrivia = ReplaceLeadingWhitespace(visited.OpenBraceToken.LeadingTrivia, braceWhitespace);
            var newOpenBraceToken = visited.OpenBraceToken.WithLeadingTrivia(newTrivia);

            if (newOpenBraceToken != visited.OpenBraceToken)
            {
                visited = visited.WithOpenBraceToken(newOpenBraceToken);
                hasChanges = true;
            }
        }

        if (IsFirstTokenOnLine(visited.CloseBraceToken))
        {
            var newTrivia = ReplaceLeadingWhitespace(visited.CloseBraceToken.LeadingTrivia, braceWhitespace);
            var newCloseBraceToken = visited.CloseBraceToken.WithLeadingTrivia(newTrivia);

            if (newCloseBraceToken != visited.CloseBraceToken)
            {
                visited = visited.WithCloseBraceToken(newCloseBraceToken);
                hasChanges = true;
            }
        }

        var updatedInitializers = visited.Initializers.ToList();

        for (var initializerIndex = 0; initializerIndex < updatedInitializers.Count && initializerIndex < node.Initializers.Count; initializerIndex++)
        {
            var initializer = updatedInitializers[initializerIndex];
            var firstToken = initializer.GetFirstToken();
            var newTrivia = ReplaceLeadingWhitespace(firstToken.LeadingTrivia, elementWhitespace);
            var newFirstToken = firstToken.WithLeadingTrivia(newTrivia);

            if (newFirstToken != firstToken)
            {
                initializer = initializer.ReplaceToken(firstToken, newFirstToken);
                hasChanges = true;
            }

            initializer = AlignAnonymousObjectMemberChainContinuation(node.Initializers[initializerIndex], initializer, elementWhitespace.Length);

            if (initializer != updatedInitializers[initializerIndex])
            {
                hasChanges = true;
            }

            updatedInitializers[initializerIndex] = initializer;
        }

        if (hasChanges == false)
        {
            return visited;
        }

        return visited.WithInitializers(SyntaxFactory.SeparatedList(updatedInitializers, visited.Initializers.GetSeparators()));
    }

    /// <summary>
    /// Aligns continuation method-chain links inside an anonymous object member expression
    /// relative to the member declarator indentation.
    /// </summary>
    /// <param name="originalMember">The original anonymous object member.</param>
    /// <param name="visitedMember">The visited anonymous object member.</param>
    /// <param name="memberIndent">The target indentation column of the member declarator.</param>
    /// <returns>The member with aligned continuation chain links.</returns>
    private AnonymousObjectMemberDeclaratorSyntax AlignAnonymousObjectMemberChainContinuation(AnonymousObjectMemberDeclaratorSyntax originalMember, AnonymousObjectMemberDeclaratorSyntax visitedMember, int memberIndent)
    {
        var originalDotTokens = originalMember.Expression.DescendantTokens().Where(IsFluentChainDotToken).ToList();
        var visitedDotTokens = visitedMember.Expression.DescendantTokens().Where(IsFluentChainDotToken).ToList();

        if (originalDotTokens.Count < 2 || originalDotTokens.Count != visitedDotTokens.Count)
        {
            return visitedMember;
        }

        var firstDotLine = GetLine(originalDotTokens[0]);
        var firstMemberToken = originalMember.GetFirstToken();

        if (firstDotLine != GetLine(firstMemberToken))
        {
            return visitedMember;
        }

        var relativeDotColumn = GetColumn(originalDotTokens[0]) - GetColumn(firstMemberToken);

        if (relativeDotColumn < 0)
        {
            relativeDotColumn = 0;
        }

        var targetDotColumn = memberIndent + relativeDotColumn;
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        for (var tokenIndex = 1; tokenIndex < visitedDotTokens.Count && tokenIndex < originalDotTokens.Count; tokenIndex++)
        {
            if (IsFirstTokenOnLine(originalDotTokens[tokenIndex]) == false)
            {
                continue;
            }

            if (GetLine(originalDotTokens[tokenIndex]) <= firstDotLine)
            {
                continue;
            }

            var visitedDotToken = visitedDotTokens[tokenIndex];
            var targetColumn = targetDotColumn;
            var originalDotColumn = GetColumn(originalDotTokens[tokenIndex]);

            if (originalDotColumn > targetColumn)
            {
                targetColumn = originalDotColumn;
            }

            var newLeadingTrivia = ReplaceLeadingWhitespace(visitedDotToken.LeadingTrivia, new string(' ', targetColumn));

            replacementMap[visitedDotToken] = visitedDotToken.WithLeadingTrivia(newLeadingTrivia);
        }

        if (replacementMap.Count == 0)
        {
            return visitedMember;
        }

        var newExpression = visitedMember.Expression.ReplaceTokens(replacementMap.Keys, (originalToken, _) => replacementMap[originalToken]);

        return visitedMember.WithExpression(newExpression);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitArrayCreationExpression(ArrayCreationExpressionSyntax node)
    {
        var newKeywordColumn = AdjustColumnForNormalization(node.NewKeyword, node.NewKeyword.GetLocation().GetLineSpan().StartLinePosition.Character);

        var visited = (ArrayCreationExpressionSyntax)base.VisitArrayCreationExpression(node);

        if (visited?.Initializer == null || visited.Initializer.Kind() != SyntaxKind.ArrayInitializerExpression)
        {
            return visited;
        }

        if (node.Parent is MemberAccessExpressionSyntax || node.Parent is ConditionalAccessExpressionSyntax)
        {
            return visited;
        }

        var initializer = visited.Initializer;
        var braceWhitespace = new string(' ', newKeywordColumn);
        var elementWhitespace = new string(' ', newKeywordColumn + FormattingContext.IndentSize);
        var tokensToReplace = new List<SyntaxToken>();
        var replacementMap = new Dictionary<int, SyntaxTriviaList>();

        if (IsFirstTokenOnLine(initializer.OpenBraceToken))
        {
            var newTrivia = ReplaceLeadingWhitespace(initializer.OpenBraceToken.LeadingTrivia, braceWhitespace);
            tokensToReplace.Add(initializer.OpenBraceToken);
            replacementMap[initializer.OpenBraceToken.SpanStart] = newTrivia;
        }

        if (IsFirstTokenOnLine(initializer.CloseBraceToken))
        {
            var newTrivia = ReplaceLeadingWhitespace(initializer.CloseBraceToken.LeadingTrivia, braceWhitespace);
            tokensToReplace.Add(initializer.CloseBraceToken);
            replacementMap[initializer.CloseBraceToken.SpanStart] = newTrivia;
        }

        foreach (var expression in initializer.Expressions)
        {
            var firstToken = expression.GetFirstToken();

            if (IsFirstTokenOnLine(firstToken))
            {
                var newTrivia = ReplaceLeadingWhitespace(firstToken.LeadingTrivia, elementWhitespace);
                tokensToReplace.Add(firstToken);
                replacementMap[firstToken.SpanStart] = newTrivia;
            }
        }

        if (tokensToReplace.Count == 0)
        {
            return visited;
        }

        return visited.ReplaceTokens(tokensToReplace, (original, _) => original.WithLeadingTrivia(replacementMap[original.SpanStart]));
    }

    /// <summary>
    /// Rebuilds a single assignment expression with corrected leading trivia
    /// and stripped trailing end-of-line trivia on the last token.
    /// </summary>
    /// <param name="expression">The assignment expression to rebuild.</param>
    /// <param name="whitespace">The whitespace string to use as leading trivia.</param>
    /// <returns>The expression with corrected trivia.</returns>
    private ExpressionSyntax RebuildAssignment(ExpressionSyntax expression, string whitespace)
    {
        var result = expression.WithoutLeadingTrivia()
                               .WithLeadingTrivia(SyntaxFactory.Whitespace(whitespace));

        if (result is AssignmentExpressionSyntax assignmentExpression)
        {
            result = AlignMultilineAssignmentChain(assignmentExpression, whitespace);

            if (result is AssignmentExpressionSyntax chainAlignedAssignmentExpression)
            {
                result = AlignMultilineAssignmentLambdaBlock(chainAlignedAssignmentExpression, whitespace.Length);
            }
        }

        if (result is ObjectCreationExpressionSyntax objectCreationExpression
            && objectCreationExpression.Initializer != null
            && (objectCreationExpression.Initializer.Kind() == SyntaxKind.ObjectInitializerExpression
                || objectCreationExpression.Initializer.Kind() == SyntaxKind.CollectionInitializerExpression))
        {
            result = objectCreationExpression.WithInitializer(RebuildInitializer(objectCreationExpression.Initializer, whitespace.Length));
        }
        else if (result is ImplicitObjectCreationExpressionSyntax implicitObjectCreationExpression
                 && implicitObjectCreationExpression.Initializer != null
                 && (implicitObjectCreationExpression.Initializer.Kind() == SyntaxKind.ObjectInitializerExpression
                     || implicitObjectCreationExpression.Initializer.Kind() == SyntaxKind.CollectionInitializerExpression))
        {
            result = implicitObjectCreationExpression.WithInitializer(RebuildInitializer(implicitObjectCreationExpression.Initializer, whitespace.Length));
        }

        var lastToken = result.GetLastToken();
        var strippedTrailing = StripTrailingEndOfLinesFromTrivia(lastToken.TrailingTrivia);

        if (strippedTrailing.Count != lastToken.TrailingTrivia.Count)
        {
            result = result.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(SyntaxFactory.TriviaList(strippedTrailing)));
        }

        return result;
    }

    /// <summary>
    /// Aligns continuation lines of a fluent/member chain inside an assignment expression.
    /// Only continuation dots on lines after the assignment line are realigned.
    /// </summary>
    /// <param name="assignmentExpression">The assignment expression.</param>
    /// <param name="whitespace">The target assignment indentation whitespace.</param>
    /// <returns>The aligned assignment expression.</returns>
    private ExpressionSyntax AlignMultilineAssignmentChain(AssignmentExpressionSyntax assignmentExpression, string whitespace)
    {
        var assignmentFirstToken = assignmentExpression.GetFirstToken();
        var assignmentLine = GetLine(assignmentFirstToken);
        var assignmentColumn = GetColumn(assignmentFirstToken);
        var eligibleDotTokens = assignmentExpression.Right
                                                    .DescendantTokens()
                                                    .Where(IsFluentChainDotToken)
                                                    .ToList();

        if (eligibleDotTokens.Count == 0)
        {
            return assignmentExpression;
        }

        var firstDotToken = eligibleDotTokens[0];

        if (firstDotToken == default)
        {
            return assignmentExpression;
        }

        var firstDotLineSpan = firstDotToken.GetLocation().GetLineSpan();

        if (firstDotLineSpan.StartLinePosition.Line != assignmentLine)
        {
            return assignmentExpression;
        }

        var relativeDotColumn = firstDotLineSpan.StartLinePosition.Character - assignmentColumn;

        if (relativeDotColumn < 0)
        {
            relativeDotColumn = 0;
        }

        var targetDotColumn = whitespace.Length + relativeDotColumn;
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var dotToken in eligibleDotTokens)
        {
            var dotLine = dotToken.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (dotLine > assignmentLine)
            {
                var updatedDot = dotToken.WithLeadingTrivia(ReplaceLeadingWhitespace(dotToken.LeadingTrivia,
                                                                                     new string(' ', targetDotColumn)));

                replacementMap[dotToken] = updatedDot;
            }
        }

        if (replacementMap.Count == 0)
        {
            return assignmentExpression;
        }

        return assignmentExpression.ReplaceTokens(replacementMap.Keys, (original, _) => replacementMap[original]);
    }

    /// <summary>
    /// Aligns multi-line lambda block bodies in assignment right-hand expressions to the lambda expression column.
    /// </summary>
    /// <param name="assignmentExpression">The assignment expression.</param>
    /// <param name="assignmentIndent">The assignment indentation column.</param>
    /// <returns>The assignment expression with aligned lambda block body, if applicable.</returns>
    private ExpressionSyntax AlignMultilineAssignmentLambdaBlock(AssignmentExpressionSyntax assignmentExpression, int assignmentIndent)
    {
        BlockSyntax block = null;

        if (assignmentExpression.Right is SimpleLambdaExpressionSyntax { Body: BlockSyntax simpleBlock })
        {
            block = simpleBlock;
        }
        else if (assignmentExpression.Right is ParenthesizedLambdaExpressionSyntax { Body: BlockSyntax parenthesizedBlock })
        {
            block = parenthesizedBlock;
        }

        if (block == null)
        {
            return assignmentExpression;
        }

        if (IsFirstTokenOnLine(block.OpenBraceToken) == false)
        {
            return assignmentExpression;
        }

        var assignmentFirstToken = assignmentExpression.GetFirstToken();
        var lambdaFirstToken = assignmentExpression.Right.GetFirstToken();
        var relativeLambdaColumn = GetColumn(lambdaFirstToken) - GetColumn(assignmentFirstToken);

        if (relativeLambdaColumn < 0)
        {
            relativeLambdaColumn = 0;
        }

        var targetColumn = assignmentIndent + relativeLambdaColumn;
        var currentIndent = GetLeadingWhitespaceLength(block.OpenBraceToken);
        var shift = targetColumn - currentIndent;

        if (shift == 0)
        {
            return assignmentExpression;
        }

        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in block.DescendantTokens())
        {
            if (IsFirstTokenOnLine(token) == false)
            {
                continue;
            }

            var tokenIndent = GetLeadingWhitespaceLength(token);
            var newIndent = tokenIndent + shift;

            if (newIndent < 0)
            {
                newIndent = 0;
            }

            var newLeading = ReplaceLeadingWhitespace(token.LeadingTrivia, new string(' ', newIndent));

            replacements[token] = token.WithLeadingTrivia(newLeading);
        }

        if (replacements.Count == 0)
        {
            return assignmentExpression;
        }

        return assignmentExpression.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Determines whether a dot token belongs to a fluent member-access chain that should
    /// participate in assignment continuation alignment.
    /// </summary>
    /// <param name="dotToken">The dot token to evaluate.</param>
    /// <returns><c>true</c> if the token is part of a fluent chain; otherwise, <c>false</c>.</returns>
    private bool IsFluentChainDotToken(SyntaxToken dotToken)
    {
        if (dotToken.IsKind(SyntaxKind.DotToken) == false)
        {
            return false;
        }

        if (dotToken.Parent is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        return memberAccess.Parent is InvocationExpressionSyntax
               || memberAccess.Parent is MemberAccessExpressionSyntax
               || memberAccess.Parent is ConditionalAccessExpressionSyntax;
    }

    /// <summary>
    /// Removes trailing end-of-line and whitespace trivia from a trivia list.
    /// Returns the result as a <see cref="List{T}"/> to allow count comparison with the original.
    /// </summary>
    /// <param name="triviaList">The trivia list to strip.</param>
    /// <returns>The stripped trivia as a list.</returns>
    private List<SyntaxTrivia> StripTrailingEndOfLinesFromTrivia(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList);

        while (result.Count > 0
               && (result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            result.RemoveAt(result.Count - 1);
        }

        return result;
    }

    /// <summary>
    /// Rebuilds the initializer expression with correct indentation.
    /// </summary>
    /// <param name="objectCreation">The object creation expression whose initializer is to be rebuilt.</param>
    /// <param name="newKeywordColumn">The column position of the <c>new</c> keyword.</param>
    /// <returns>The rebuilt initializer expression.</returns>
    private InitializerExpressionSyntax RebuildInitializer(ObjectCreationExpressionSyntax objectCreation, int newKeywordColumn)
    {
        return RebuildInitializer(objectCreation.Initializer, newKeywordColumn);
    }

    /// <summary>
    /// Rebuilds the initializer expression with correct indentation based on the owning keyword column.
    /// </summary>
    /// <param name="initializer">The initializer expression to rebuild.</param>
    /// <param name="newKeywordColumn">The column position of the <c>new</c> keyword.</param>
    /// <returns>The rebuilt initializer expression.</returns>
    private InitializerExpressionSyntax RebuildInitializer(InitializerExpressionSyntax initializer, int newKeywordColumn)
    {
        var braceWhitespace = new string(' ', newKeywordColumn);
        var assignmentWhitespace = new string(' ', newKeywordColumn + FormattingContext.IndentSize);
        var hasTrailingComma = initializer.Expressions.Count > 0
                               && initializer.Expressions.SeparatorCount == initializer.Expressions.Count;

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine), SyntaxFactory.Whitespace(braceWhitespace))
                                     .WithTrailingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine));

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken);

        if (hasTrailingComma)
        {
            closeBrace = closeBrace.WithLeadingTrivia(SyntaxFactory.Whitespace(braceWhitespace));
        }
        else
        {
            closeBrace = closeBrace.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine), SyntaxFactory.Whitespace(braceWhitespace));
        }

        var expressions = RebuildAssignments(initializer, assignmentWhitespace);
        var rebuiltInitializer = SyntaxFactory.InitializerExpression(initializer.Kind(), openBrace, expressions, closeBrace);

        return StripTrailingEndOfLineFromPreviousToken(rebuiltInitializer, rebuiltInitializer.OpenBraceToken);
    }

    /// <summary>
    /// Rebuilds the assignment expressions list with correct leading whitespace
    /// and comma separators that include trailing end-of-line trivia.
    /// </summary>
    /// <param name="initializer">The original initializer containing the assignments.</param>
    /// <param name="whitespace">The whitespace string to prepend to each assignment.</param>
    /// <returns>The rebuilt separated syntax list of expressions.</returns>
    private SeparatedSyntaxList<ExpressionSyntax> RebuildAssignments(InitializerExpressionSyntax initializer, string whitespace)
    {
        if (initializer.Expressions.Count == 0)
        {
            return SyntaxFactory.SeparatedList<ExpressionSyntax>();
        }

        var expressions = initializer.Expressions;

        for (var index = 0; index < expressions.Count; index++)
        {
            var expression = RebuildAssignment(expressions[index], whitespace);

            if (index == expressions.Count - 1)
            {
                expression = expression.WithoutTrailingTrivia();
            }

            expressions = expressions.Replace(expressions[index], expression);
        }

        for (var index = 0; index < expressions.SeparatorCount; index++)
        {
            var commaToken = expressions.GetSeparator(index)
                                        .WithTrailingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine));

            expressions = expressions.ReplaceSeparator(expressions.GetSeparator(index), commaToken);
        }

        return expressions;
    }

    #endregion // Object Initializer Layout

    #region Collection Expression Alignment

    /// <inheritdoc/>
    public override SyntaxNode VisitCollectionExpression(CollectionExpressionSyntax node)
    {
        var visited = (CollectionExpressionSyntax)base.VisitCollectionExpression(node);

        if (visited == null || visited.Elements.Count == 0)
        {
            return visited;
        }

        var openBracketColumn = AdjustColumnForNormalization(node.OpenBracketToken, node.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var alignColumn = openBracketColumn + FormattingContext.IndentSize;
        var openBracketLine = node.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var hasObjectCreationInitializerElement = node.Elements.Any(IsObjectCreationElementWithInitializer);

        var hasElementChanges = false;
        var newElements = new List<CollectionElementSyntax>(visited.Elements.Count);

        for (var elementIndex = 0; elementIndex < visited.Elements.Count && elementIndex < node.Elements.Count; elementIndex++)
        {
            var originalElement = node.Elements[elementIndex];
            var visitedElement = visited.Elements[elementIndex];
            var originalElementFirstToken = originalElement.GetFirstToken();
            var originalElementLine = GetLine(originalElementFirstToken);

            if (originalElementLine > openBracketLine)
            {
                var elementAlignColumn = alignColumn;
                var isObjectCreationElementWithInitializer = IsObjectCreationElementWithInitializer(originalElement);

                var originalElementColumn = AdjustColumnForNormalization(originalElementFirstToken, GetColumn(originalElementFirstToken));
                var elementShift = elementAlignColumn - originalElementColumn;
                var alignedElement = AlignNodeToColumn(visitedElement, elementAlignColumn);

                if (elementShift != 0
                    && isObjectCreationElementWithInitializer == false)
                {
                    alignedElement = ShiftNodeContinuationLines(alignedElement, originalElementLine, elementShift);
                }

                alignedElement = RebuildObjectCreationInitializerInCollectionElement(alignedElement, elementAlignColumn);

                if (alignedElement != visitedElement)
                {
                    hasElementChanges = true;
                }

                newElements.Add(alignedElement);
            }
            else
            {
                newElements.Add(visitedElement);
            }
        }

        if (hasElementChanges)
        {
            visited = visited.WithElements(SyntaxFactory.SeparatedList(newElements, visited.Elements.GetSeparators()));
        }

        // Align the closing bracket to the same column as the opening bracket
        var closeBracketLine = node.CloseBracketToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (closeBracketLine > openBracketLine)
        {
            var closeBracketColumn = openBracketColumn;

            var closeBracketToken = visited.CloseBracketToken;
            var newCloseBracket = closeBracketToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                      SyntaxFactory.Whitespace(new string(' ', closeBracketColumn)));

            if (newCloseBracket != closeBracketToken)
            {
                visited = visited.WithCloseBracketToken(newCloseBracket);
                visited = StripTrailingEndOfLineFromPreviousToken(visited, visited.CloseBracketToken);
            }
        }

        return visited;
    }

    /// <summary>
    /// Shifts continuation-line indentation for all first-on-line tokens in a node
    /// that appear after the specified first line.
    /// </summary>
    /// <typeparam name="T">The syntax node type.</typeparam>
    /// <param name="node">The node to shift.</param>
    /// <param name="firstLine">The first line to keep unchanged.</param>
    /// <param name="columnShift">The number of columns to shift continuation lines.</param>
    /// <returns>The shifted node.</returns>
    private static T ShiftNodeContinuationLines<T>(T node, int firstLine, int columnShift)
        where T : SyntaxNode
    {
        var replacements = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var token in node.DescendantTokens())
        {
            if (IsFirstTokenOnLine(token) == false || GetLine(token) <= firstLine)
            {
                continue;
            }

            var currentIndent = GetLeadingWhitespaceLength(token);
            var shiftedIndent = currentIndent + columnShift;

            if (shiftedIndent < 0)
            {
                shiftedIndent = 0;
            }

            var newLeading = ReplaceLeadingWhitespace(token.LeadingTrivia, new string(' ', shiftedIndent));

            replacements[token] = token.WithLeadingTrivia(newLeading);
        }

        if (replacements.Count == 0)
        {
            return node;
        }

        return (T)node.ReplaceTokens(replacements.Keys, (original, _) => replacements[original]);
    }

    /// <summary>
    /// Determines whether the collection element is an object-creation expression with an initializer.
    /// </summary>
    /// <param name="element">The collection element.</param>
    /// <returns><c>true</c> if the element is an object creation with object/collection initializer; otherwise, <c>false</c>.</returns>
    private static bool IsObjectCreationElementWithInitializer(CollectionElementSyntax element)
    {
        if (element is not ExpressionElementSyntax expressionElement)
        {
            return false;
        }

        InitializerExpressionSyntax initializer;

        if (expressionElement.Expression is ObjectCreationExpressionSyntax objectCreation)
        {
            initializer = objectCreation.Initializer;
        }
        else if (expressionElement.Expression is ImplicitObjectCreationExpressionSyntax implicitObjectCreation)
        {
            initializer = implicitObjectCreation.Initializer;
        }
        else
        {
            return false;
        }

        if (initializer == null)
        {
            return false;
        }

        return initializer.Kind() == SyntaxKind.ObjectInitializerExpression
               || initializer.Kind() == SyntaxKind.CollectionInitializerExpression;
    }

    /// <summary>
    /// Rebuilds object-creation initializers inside a collection element to align braces and members
    /// to the collection element column.
    /// </summary>
    /// <param name="element">The collection element to process.</param>
    /// <param name="elementColumn">The aligned column of the collection element.</param>
    /// <returns>The updated collection element.</returns>
    private CollectionElementSyntax RebuildObjectCreationInitializerInCollectionElement(CollectionElementSyntax element, int elementColumn)
    {
        if (element is not ExpressionElementSyntax expressionElement)
        {
            return element;
        }

        InitializerExpressionSyntax initializer;

        if (expressionElement.Expression is ObjectCreationExpressionSyntax objectCreation)
        {
            initializer = objectCreation.Initializer;
        }
        else if (expressionElement.Expression is ImplicitObjectCreationExpressionSyntax implicitObjectCreation)
        {
            initializer = implicitObjectCreation.Initializer;
        }
        else
        {
            return element;
        }

        if (initializer == null
            || (initializer.Kind() != SyntaxKind.ObjectInitializerExpression
                && initializer.Kind() != SyntaxKind.CollectionInitializerExpression))
        {
            return element;
        }

        if (expressionElement.Expression is ObjectCreationExpressionSyntax originalObjectCreation)
        {
            var rebuiltInitializer = RebuildInitializer(originalObjectCreation.Initializer, elementColumn);
            var rebuiltObjectCreation = originalObjectCreation.WithInitializer(rebuiltInitializer);

            if (rebuiltObjectCreation == originalObjectCreation)
            {
                return element;
            }

            return expressionElement.WithExpression(rebuiltObjectCreation);
        }

        var originalImplicitObjectCreation = (ImplicitObjectCreationExpressionSyntax)expressionElement.Expression;
        var rebuiltImplicitInitializer = RebuildInitializer(originalImplicitObjectCreation.Initializer, elementColumn);
        var rebuiltImplicitObjectCreation = originalImplicitObjectCreation.WithInitializer(rebuiltImplicitInitializer);

        if (rebuiltImplicitObjectCreation == originalImplicitObjectCreation)
        {
            return element;
        }

        return expressionElement.WithExpression(rebuiltImplicitObjectCreation);
    }

    #endregion // Collection Expression Alignment

    #region Method Chain Alignment

    /// <inheritdoc/>
    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        if (IsInnerChainMember(node))
        {
            return base.VisitMemberAccessExpression(node);
        }

        return AlignChain(node);
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitConditionalAccessExpression(ConditionalAccessExpressionSyntax node)
    {
        if (IsInnerChainMember(node))
        {
            return base.VisitConditionalAccessExpression(node);
        }

        return AlignChain(node);
    }

    /// <summary>
    /// Determines whether the given node is an inner member of a larger chain.
    /// </summary>
    /// <param name="node">The node to check.</param>
    /// <returns><c>true</c> if the node is part of a larger chain; otherwise, <c>false</c>.</returns>
    private static bool IsInnerChainMember(SyntaxNode node)
    {
        var current = node.Parent;

        while (current != null)
        {
            switch (current)
            {
                case InvocationExpressionSyntax:
                case ElementAccessExpressionSyntax:
                case PostfixUnaryExpressionSyntax:
                    current = current.Parent;

                    continue;
                case MemberAccessExpressionSyntax:
                case ConditionalAccessExpressionSyntax:
                    return true;
            }

            break;
        }

        return false;
    }

    /// <summary>
    /// Collects all chain link tokens from the outermost node down to the root expression.
    /// Only invoked member accesses produce chain links; property-only accesses are skipped.
    /// Also traverses into <see cref="ConditionalAccessExpressionSyntax.WhenNotNull"/> to collect
    /// additional chain links from the conditional access continuation.
    /// </summary>
    /// <param name="node">The outermost node of the chain.</param>
    /// <returns>List of alignment tokens in chain order (first link closest to root).</returns>
    private static List<SyntaxToken> CollectChainLinks(SyntaxNode node)
    {
        var links = new List<SyntaxToken>();
        var current = node;
        var whenNotNullLinks = new List<SyntaxToken>();
        var conditionalAccessIndex = -1;

        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                current = invocation.Expression;
            }
            else if (current is MemberAccessExpressionSyntax memberAccess)
            {
                current = ProcessMemberAccess(memberAccess, links);
            }
            else if (current is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                conditionalAccessIndex = links.Count;

                links.Add(conditionalAccess.OperatorToken);
                CollectWhenNotNullLinks(conditionalAccess.WhenNotNull, whenNotNullLinks);

                current = conditionalAccess.Expression;
            }
            else if (current is ElementAccessExpressionSyntax elementAccess)
            {
                current = elementAccess.Expression;
            }
            else if (current is PostfixUnaryExpressionSyntax postfix)
            {
                current = postfix.Operand;
            }
            else
            {
                break;
            }
        }

        links.Reverse();

        if (whenNotNullLinks.Count > 0 && conditionalAccessIndex >= 0)
        {
            var reversedIndex = links.Count - 1 - conditionalAccessIndex;

            links.InsertRange(reversedIndex + 1, whenNotNullLinks);
        }

        return links;
    }

    /// <summary>
    /// Collects chain link tokens from the <c>WhenNotNull</c> part of a conditional access expression.
    /// Skips <see cref="MemberBindingExpressionSyntax"/> since its dot is part of the <c>?.</c> operator.
    /// </summary>
    /// <param name="whenNotNull">The WhenNotNull expression.</param>
    /// <param name="links">The list to add collected link tokens to.</param>
    private static void CollectWhenNotNullLinks(ExpressionSyntax whenNotNull, List<SyntaxToken> links)
    {
        var current = whenNotNull;
        var tempLinks = new List<SyntaxToken>();

        while (current != null)
        {
            if (current is InvocationExpressionSyntax invocation)
            {
                current = invocation.Expression;
            }
            else if (current is MemberAccessExpressionSyntax memberAccess)
            {
                tempLinks.Add(memberAccess.OperatorToken);

                current = memberAccess.Expression;
            }
            else
            {
                break;
            }
        }

        tempLinks.Reverse();

        links.AddRange(tempLinks);
    }

    /// <summary>
    /// Processes a member access expression within a chain, adding the operator
    /// token to the chain links list. When the member access wraps a postfix unary
    /// expression (e.g., <c>expr!.Member</c>), both the member access dot and the
    /// postfix operator are added so that the entire <c>!.</c> pair participates
    /// in chain alignment.
    /// </summary>
    /// <param name="memberAccess">The member access expression.</param>
    /// <param name="links">The list of chain link tokens to add to.</param>
    /// <returns>The next node to process in the chain.</returns>
    private static ExpressionSyntax ProcessMemberAccess(MemberAccessExpressionSyntax memberAccess, List<SyntaxToken> links)
    {
        if (memberAccess.Expression is PostfixUnaryExpressionSyntax postfixUnary)
        {
            links.Add(memberAccess.OperatorToken);
            links.Add(postfixUnary.OperatorToken);

            return postfixUnary.Operand;
        }

        links.Add(memberAccess.OperatorToken);

        return memberAccess.Expression;
    }

    /// <summary>
    /// Collapses the <c>WhenNotNull</c> part of a conditional access expression
    /// onto the same line as the operator token, if they are on different lines.
    /// </summary>
    /// <param name="cae">The conditional access expression.</param>
    /// <returns>The modified node with collapsed WhenNotNull.</returns>
    private static SyntaxNode CollapseConditionalAccessWhenNotNull(ConditionalAccessExpressionSyntax cae)
    {
        var operatorToken = cae.OperatorToken;
        var whenNotNullFirstToken = cae.WhenNotNull.GetFirstToken();

        if (GetLine(operatorToken) == GetLine(whenNotNullFirstToken))
        {
            return cae;
        }

        var newOperator = operatorToken.WithTrailingTrivia(StripTrailingEndOfLine(operatorToken.TrailingTrivia));
        var newWhenNotNullFirst = whenNotNullFirstToken.WithLeadingTrivia(StripLeadingEndOfLineAndWhitespace(whenNotNullFirstToken.LeadingTrivia));

        return cae.ReplaceTokens([operatorToken, whenNotNullFirstToken],
                                 (original, _) =>
                                 {
                                     if (original == operatorToken)
                                     {
                                         return newOperator;
                                     }

                                     if (original == whenNotNullFirstToken)
                                     {
                                         return newWhenNotNullFirst;
                                     }

                                     return original;
                                 });
    }

    /// <summary>
    /// Aligns all chain links in the outermost chain rooted at the given node.
    /// The first chain link is placed on the same line as the root expression,
    /// and subsequent links are aligned vertically beneath it on new lines.
    /// </summary>
    /// <param name="node">The outermost chain node.</param>
    /// <returns>The rewritten node with aligned chain links.</returns>
    private SyntaxNode AlignChain(SyntaxNode node)
    {
        var originalLinks = CollectChainLinks(node);

        if (originalLinks.Count == 0)
        {
            return VisitChainBase(node);
        }

        var preserveFirstLinkLine = ShouldPreserveFirstLinkForStatementLambdaChain(node, originalLinks);

        var firstLink = originalLinks[0];
        var previousToken = firstLink.GetPreviousToken();
        var previousTokenLine = GetLine(previousToken);
        var firstLinkLine = GetLine(firstLink);

        var isMultiLine = false;

        if (firstLinkLine != previousTokenLine)
        {
            isMultiLine = true;
        }
        else
        {
            foreach (var link in originalLinks)
            {
                if (GetLine(link) != previousTokenLine)
                {
                    isMultiLine = true;

                    break;
                }
            }
        }

        if (isMultiLine == false)
        {
            if (previousToken.IsKind(SyntaxKind.CloseBraceToken)
                && previousToken.Parent is InitializerExpressionSyntax { Parent: ObjectCreationExpressionSyntax }
                && originalLinks.Count > 1)
            {
                isMultiLine = true;
            }
        }

        if (isMultiLine == false)
        {
            return VisitChainBase(node);
        }

        int referenceColumn;
        var moveFirstLinkToSameLine = false;

        if (TryGetInitializerAssignmentReferenceColumn(firstLink, out var initializerReferenceColumn))
        {
            referenceColumn = initializerReferenceColumn;
        }
        else
        {
            var firstInvokedLinkIndex = FindFirstInvokedLinkIndex(originalLinks);

            if (firstInvokedLinkIndex >= 0 && GetLine(originalLinks[firstInvokedLinkIndex]) == previousTokenLine)
            {
                referenceColumn = GetChainReferenceColumn(originalLinks[firstInvokedLinkIndex]);
            }
            else if (firstInvokedLinkIndex > 0
                     && IsInsideExpressionLambdaArgumentBody(node)
                     && IsNonInvokedMemberAccessLink(originalLinks[0])
                     && GetLine(originalLinks[firstInvokedLinkIndex]) > previousTokenLine)
            {
                referenceColumn = GetChainReferenceColumn(originalLinks[firstInvokedLinkIndex]);
            }
            else if (firstLinkLine == previousTokenLine)
            {
                referenceColumn = GetChainReferenceColumn(firstLink);
            }
            else if (preserveFirstLinkLine)
            {
                referenceColumn = GetChainReferenceColumn(firstLink);
            }
            else
            {
                referenceColumn = AdjustColumnForNormalization(previousToken, GetEndColumn(previousToken));
                moveFirstLinkToSameLine = true;
            }
        }

        if (TryGetConditionalBranchAlignmentShift(node, previousToken, out var conditionalBranchShift))
        {
            referenceColumn += conditionalBranchShift;
        }

        var visited = VisitChainBase(node);

        if (visited is ConditionalAccessExpressionSyntax visitedCae)
        {
            visited = CollapseConditionalAccessWhenNotNull(visitedCae);
        }

        var visitedLinks = CollectChainLinks(visited);

        if (visitedLinks.Count != originalLinks.Count)
        {
            return visited;
        }

        var visitedLinkSpanStarts = new HashSet<int>();

        foreach (var visitedLink in visitedLinks)
        {
            visitedLinkSpanStarts.Add(visitedLink.SpanStart);
        }

        var originalPreviousToken = firstLink.GetPreviousToken();
        var visitedFirstLink = visitedLinks[0];
        var visitedPreviousToken = visitedFirstLink.GetPreviousToken();

        if (originalPreviousToken.IsKind(SyntaxKind.CloseBraceToken)
            && originalPreviousToken.Parent is InitializerExpressionSyntax { Parent: ObjectCreationExpressionSyntax })
        {
            referenceColumn = GetLeadingWhitespaceLength(visitedPreviousToken) + 1;
        }

        var replacementPairs = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

        if (preserveFirstLinkLine && visitedLinks.Count > 0)
        {
            var firstVisitedLink = visitedLinks[0];
            var firstLinkLeading = BuildChainAlignedTrivia(firstVisitedLink, referenceColumn);

            replacementPairs.Add((firstVisitedLink, firstVisitedLink.WithLeadingTrivia(firstLinkLeading)));
        }

        if (moveFirstLinkToSameLine)
        {
            var visitedPreviousTokenForMove = visitedLinks[0].GetPreviousToken();
            var strippedTrailing = StripTrailingEndOfLine(visitedPreviousTokenForMove.TrailingTrivia);

            if (strippedTrailing.Count != visitedPreviousTokenForMove.TrailingTrivia.Count)
            {
                replacementPairs.Add((visitedPreviousTokenForMove, visitedPreviousTokenForMove.WithTrailingTrivia(strippedTrailing)));
            }

            var firstVisitedLink = visitedLinks[0];
            var strippedLeading = StripLeadingEndOfLineAndWhitespace(firstVisitedLink.LeadingTrivia);

            replacementPairs.Add((firstVisitedLink, firstVisitedLink.WithLeadingTrivia(strippedLeading)));
        }

        for (var i = 1; i < visitedLinks.Count; i++)
        {
            if (GetLine(originalLinks[i]) == GetLine(originalLinks[i - 1])
                && HasNonInvokedLinkOnLine(originalLinks, GetLine(originalLinks[i])))
            {
                continue;
            }

            var link = visitedLinks[i];
            var linkPreviousToken = link.GetPreviousToken();

            if (linkPreviousToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia))
            {
                var strippedTrailing = StripTrailingEndOfLine(linkPreviousToken.TrailingTrivia);

                replacementPairs.Add((linkPreviousToken, linkPreviousToken.WithTrailingTrivia(strippedTrailing)));
            }

            var newTrivia = BuildChainAlignedTrivia(link, referenceColumn);

            replacementPairs.Add((link, link.WithLeadingTrivia(newTrivia)));

            var continuationShiftPairs = BuildInvocationContinuationShiftPairs(originalLinks[i], link, referenceColumn, visitedLinkSpanStarts);

            foreach (var continuationShiftPair in continuationShiftPairs)
            {
                var alreadyAdded = false;

                foreach (var replacementPair in replacementPairs)
                {
                    if (replacementPair.OldToken == continuationShiftPair.OldToken)
                    {
                        alreadyAdded = true;

                        break;
                    }
                }

                if (alreadyAdded == false)
                {
                    replacementPairs.Add(continuationShiftPair);
                }
            }
        }

        if (replacementPairs.Count == 0)
        {
            return visited;
        }

        return visited.ReplaceTokens(replacementPairs.ConvertAll(pair => pair.OldToken),
                                     (original, _) =>
                                     {
                                         foreach (var pair in replacementPairs)
                                         {
                                             if (pair.OldToken == original)
                                             {
                                                 return pair.NewToken;
                                             }
                                         }

                                         return original;
                                     });
    }

    /// <summary>
    /// Builds replacement pairs that shift continuation lines inside an invocation expression
    /// when its chain link token is realigned to a different column.
    /// </summary>
    /// <param name="originalLink">The original chain link token.</param>
    /// <param name="visitedLink">The visited chain link token.</param>
    /// <param name="targetColumn">The alignment target column for the link.</param>
    /// <param name="chainLinkSpanStarts">All chain-link span starts in the visited node.</param>
    /// <returns>The replacement pairs for continuation tokens.</returns>
    private List<(SyntaxToken OldToken, SyntaxToken NewToken)> BuildInvocationContinuationShiftPairs(SyntaxToken originalLink, SyntaxToken visitedLink, int targetColumn, HashSet<int> chainLinkSpanStarts)
    {
        var replacementPairs = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

        if (visitedLink.Parent is not MemberAccessExpressionSyntax memberAccess
            || memberAccess.Parent is not InvocationExpressionSyntax invocation
            || invocation.Expression != memberAccess)
        {
            return replacementPairs;
        }

        var originalColumn = AdjustColumnForNormalization(originalLink, GetColumn(originalLink));
        var shift = targetColumn - originalColumn;

        if (shift == 0)
        {
            return replacementPairs;
        }

        var originalLinkLine = GetLine(originalLink);

        foreach (var token in invocation.DescendantTokens())
        {
            if (chainLinkSpanStarts.Contains(token.SpanStart))
            {
                continue;
            }

            if (IsFirstTokenOnLine(token) == false)
            {
                continue;
            }

            if (GetLine(token) <= originalLinkLine)
            {
                continue;
            }

            var tokenIndent = GetLeadingWhitespaceLength(token);
            var newIndent = tokenIndent + shift;

            if (newIndent < 0)
            {
                newIndent = 0;
            }

            var newLeading = ReplaceLeadingWhitespace(token.LeadingTrivia, new string(' ', newIndent));

            if (newLeading != token.LeadingTrivia)
            {
                replacementPairs.Add((token, token.WithLeadingTrivia(newLeading)));
            }
        }

        return replacementPairs;
    }

    /// <summary>
    /// Gets the reference column for a chain link token. For lines that start with logical
    /// operators, the raw token column is used to avoid applying block-normalization deltas
    /// from the operator line to chain alignment.
    /// </summary>
    /// <param name="linkToken">The chain link token.</param>
    /// <returns>The reference column for chain alignment.</returns>
    private int GetChainReferenceColumn(SyntaxToken linkToken)
    {
        var column = GetColumn(linkToken);

        if (IsLogicalOperatorFirstTokenOnLine(linkToken))
        {
            return column;
        }

        return AdjustColumnForNormalization(linkToken, column);
    }

    /// <summary>
    /// Determines whether the first token on the link token's line is a logical operator.
    /// </summary>
    /// <param name="linkToken">The chain link token.</param>
    /// <returns><c>true</c> if the line starts with a logical operator; otherwise, <c>false</c>.</returns>
    private bool IsLogicalOperatorFirstTokenOnLine(SyntaxToken linkToken)
    {
        var firstTokenOnLine = linkToken;
        var line = GetLine(linkToken);

        while (true)
        {
            var previousToken = firstTokenOnLine.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None) || GetLine(previousToken) != line)
            {
                break;
            }

            firstTokenOnLine = previousToken;
        }

        return firstTokenOnLine.IsKind(SyntaxKind.AmpersandAmpersandToken)
               || firstTokenOnLine.IsKind(SyntaxKind.BarBarToken)
               || firstTokenOnLine.IsKind(SyntaxKind.BarToken)
               || firstTokenOnLine.IsKind(SyntaxKind.QuestionQuestionToken);
    }

    /// <summary>
    /// Tries to compute the additional column shift that will be applied by
    /// conditional-expression alignment when a chain anchor is on the same line as
    /// the conditional <c>?</c> or <c>:</c> token.
    /// </summary>
    /// <param name="chainNode">The chain node being aligned.</param>
    /// <param name="anchorToken">The token that anchors first-link collapsing (typically the token before the first link).</param>
    /// <param name="shift">The additional column shift that should be applied.</param>
    /// <returns><c>true</c> when a pending conditional alignment shift applies; otherwise, <c>false</c>.</returns>
    private bool TryGetConditionalBranchAlignmentShift(SyntaxNode chainNode, SyntaxToken anchorToken, out int shift)
    {
        shift = 0;

        var current = chainNode.Parent;

        while (current != null)
        {
            if (current is not ConditionalExpressionSyntax conditionalExpression)
            {
                current = current.Parent;

                continue;
            }

            var questionLine = GetLine(conditionalExpression.QuestionToken);
            var conditionEndLine = conditionalExpression.Condition.GetLocation().GetLineSpan().EndLinePosition.Line;
            var whenTrueLine = GetLine(conditionalExpression.WhenTrue.GetFirstToken());

            if (ShouldFormatConditionalExpression(conditionEndLine, questionLine, whenTrueLine) == false)
            {
                return false;
            }

            var alignColumn = GetConditionalQuestionAlignmentColumn(conditionalExpression);
            var anchorLine = GetLine(anchorToken);
            var questionColumn = AdjustColumnForNormalization(conditionalExpression.QuestionToken, GetColumn(conditionalExpression.QuestionToken));

            if (anchorLine == questionLine)
            {
                shift = alignColumn - questionColumn;

                return shift != 0;
            }

            var colonLine = GetLine(conditionalExpression.ColonToken);

            if (colonLine > questionLine && anchorLine == colonLine)
            {
                var colonColumn = AdjustColumnForNormalization(conditionalExpression.ColonToken, GetColumn(conditionalExpression.ColonToken));

                shift = alignColumn - colonColumn;

                return shift != 0;
            }

            return false;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the first chain link should remain on its own line for
    /// single-link invocation chains that contain a statement lambda argument.
    /// </summary>
    /// <param name="node">The outermost chain node.</param>
    /// <param name="originalLinks">The collected original chain links.</param>
    /// <returns><c>true</c> if the first link should remain on its own line; otherwise, <c>false</c>.</returns>
    private bool ShouldPreserveFirstLinkForStatementLambdaChain(SyntaxNode node, List<SyntaxToken> originalLinks)
    {
        if (originalLinks.Count != 1)
        {
            return false;
        }

        if (node is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        if (memberAccess.Parent is not InvocationExpressionSyntax invocation
            || invocation.Expression != memberAccess)
        {
            return false;
        }

        foreach (var argument in invocation.ArgumentList.Arguments)
        {
            if (argument.Expression is SimpleLambdaExpressionSyntax { Body: BlockSyntax simpleBlock } simpleLambda
                && GetLine(simpleBlock.OpenBraceToken) > GetLine(simpleLambda.ArrowToken))
            {
                return true;
            }

            if (argument.Expression is ParenthesizedLambdaExpressionSyntax { Body: BlockSyntax parenthesizedBlock } parenthesizedLambda
                && GetLine(parenthesizedBlock.OpenBraceToken) > GetLine(parenthesizedLambda.ArrowToken))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to compute the chain alignment reference column for chains inside
    /// object-initializer assignments.
    /// </summary>
    /// <param name="firstLink">The first chain link token in the original syntax tree.</param>
    /// <param name="referenceColumn">The computed reference column.</param>
    /// <returns><c>true</c> when an initializer-assignment context was found; otherwise, <c>false</c>.</returns>
    private bool TryGetInitializerAssignmentReferenceColumn(SyntaxToken firstLink, out int referenceColumn)
    {
        referenceColumn = 0;

        if (_parentAssignmentWhitespaceStack.Count == 0)
        {
            return false;
        }

        var currentNode = firstLink.Parent;

        while (currentNode != null)
        {
            if (currentNode is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment)
            {
                var assignmentStartLine = GetLine(assignment.GetFirstToken());

                if (GetLine(firstLink) != assignmentStartLine)
                {
                    return false;
                }

                var parentAssignmentWhitespace = _parentAssignmentWhitespaceStack.Peek();
                var offsetFromAssignmentStart = firstLink.SpanStart - assignment.SpanStart;

                referenceColumn = parentAssignmentWhitespace + offsetFromAssignmentStart;

                return true;
            }

            currentNode = currentNode.Parent;
        }

        return false;
    }

    /// <summary>
    /// Visits the base implementation for the given chain node.
    /// </summary>
    /// <param name="node">The chain node to visit.</param>
    /// <returns>The visited node.</returns>
    private SyntaxNode VisitChainBase(SyntaxNode node)
    {
        if (node is MemberAccessExpressionSyntax memberAccess)
        {
            return base.VisitMemberAccessExpression(memberAccess);
        }

        return base.VisitConditionalAccessExpression((ConditionalAccessExpressionSyntax)node);
    }

    /// <summary>
    /// Builds the corrected leading trivia for a chain link token to align it
    /// to the reference column on a new line.
    /// </summary>
    /// <param name="token">The chain link token to align.</param>
    /// <param name="referenceColumn">The target column.</param>
    /// <returns>The corrected leading trivia list.</returns>
    private SyntaxTriviaList BuildChainAlignedTrivia(SyntaxToken token, int referenceColumn)
    {
        var newLeadingTrivia = default(SyntaxTriviaList);

        foreach (var trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && trivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                newLeadingTrivia = newLeadingTrivia.Add(trivia);
            }
        }

        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.EndOfLine(Context.EndOfLine));
        newLeadingTrivia = newLeadingTrivia.Add(SyntaxFactory.Whitespace(new string(' ', referenceColumn)));

        return newLeadingTrivia;
    }

    #endregion // Method Chain Alignment

    #region Logical Expression Layout

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
    {
        var visited = (BinaryExpressionSyntax)base.VisitBinaryExpression(node);

        if (visited == null)
        {
            return null;
        }

        if (IsLogicalOrNullCoalescingExpression(node) == false)
        {
            return visited;
        }

        if (IsInsideExpressionLambdaArgumentBody(node))
        {
            return visited;
        }

        if (IsOutermostLogicalOrNullCoalescingExpression(node) == false)
        {
            return visited;
        }

        var originalOperators = new List<SyntaxToken>();

        CollectLogicalOrNullCoalescingOperators(node, originalOperators);

        if (originalOperators.Count == 0)
        {
            return visited;
        }

        var visitedOperators = new List<SyntaxToken>();

        CollectLogicalOrNullCoalescingOperators(visited, visitedOperators);

        var replacementPairs = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

        for (var i = 0; i < originalOperators.Count && i < visitedOperators.Count; i++)
        {
            var originalOperator = originalOperators[i];
            var originalParent = (BinaryExpressionSyntax)originalOperator.Parent;

            if (originalOperator.SyntaxTree != null
                && originalParent != null)
            {
                var leftLineSpan = originalParent.Left.SyntaxTree.GetLineSpan(originalParent.Left.Span);
                var leftFirstToken = originalParent.Left.GetFirstToken();
                var targetColumn = AdjustColumnForNormalization(leftFirstToken, leftLineSpan.StartLinePosition.Character);

                if (originalOperator.IsKind(SyntaxKind.QuestionQuestionToken))
                {
                    targetColumn += FormattingContext.IndentSize;
                }

                var leftEndLine = leftLineSpan.EndLinePosition.Line;

                var operatorLineSpan = originalOperator.SyntaxTree.GetLineSpan(originalOperator.Span);
                var operatorLine = operatorLineSpan.StartLinePosition.Line;

                if (operatorLine > leftEndLine)
                {
                    var newTrivia = BuildLogicalAlignedTrivia(visitedOperators[i], targetColumn);

                    replacementPairs.Add((visitedOperators[i], visitedOperators[i].WithLeadingTrivia(newTrivia)));
                }
            }
        }

        if (replacementPairs.Count == 0)
        {
            return visited;
        }

        return visited.ReplaceTokens(replacementPairs.ConvertAll(pair => pair.OldToken),
                                     (original, _) =>
                                     {
                                         var pair = replacementPairs.FirstOrDefault(pair => pair.OldToken == original);

                                         if (pair != default)
                                         {
                                             return pair.NewToken;
                                         }

                                         return original;
                                     });
    }

    /// <summary>
    /// Determines whether the given binary expression is a logical expression
    /// (<c>&amp;&amp;</c> or <c>||</c>) or a null-coalescing expression (<c>??</c>).
    /// </summary>
    /// <param name="node">The binary expression to check.</param>
    /// <returns><c>true</c> if the expression is alignable by this rule; otherwise, <c>false</c>.</returns>
    private static bool IsLogicalOrNullCoalescingExpression(BinaryExpressionSyntax node)
    {
        return node.IsKind(SyntaxKind.LogicalAndExpression)
               || node.IsKind(SyntaxKind.LogicalOrExpression)
               || node.IsKind(SyntaxKind.BitwiseOrExpression)
               || node.IsKind(SyntaxKind.CoalesceExpression);
    }

    /// <summary>
    /// Determines whether the given alignable binary expression is the outermost one
    /// (i.e., its parent is not also an alignable binary expression).
    /// </summary>
    /// <param name="node">The binary expression to check.</param>
    /// <returns><c>true</c> if it is the outermost alignable expression; otherwise, <c>false</c>.</returns>
    private static bool IsOutermostLogicalOrNullCoalescingExpression(BinaryExpressionSyntax node)
    {
        if (node.Parent is BinaryExpressionSyntax parentBinary
            && IsLogicalOrNullCoalescingExpression(parentBinary))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified node is inside an expression-bodied lambda
    /// that is passed as an argument.
    /// </summary>
    /// <param name="node">The node to inspect.</param>
    /// <returns><c>true</c> when the node is inside an expression-bodied lambda argument; otherwise, <c>false</c>.</returns>
    private static bool IsInsideExpressionLambdaArgumentBody(SyntaxNode node)
    {
        var current = node;

        while (current != null)
        {
            if (current is SimpleLambdaExpressionSyntax simpleLambda
                && simpleLambda.Body is ExpressionSyntax
                && simpleLambda.Parent is ArgumentSyntax)
            {
                return true;
            }

            if (current is ParenthesizedLambdaExpressionSyntax parenthesizedLambda
                && parenthesizedLambda.Body is ExpressionSyntax
                && parenthesizedLambda.Parent is ArgumentSyntax)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }

    /// <summary>
    /// Recursively collects all alignable operator tokens from the given binary expression
    /// and its nested alignable binary children.
    /// </summary>
    /// <param name="node">The binary expression to collect operators from.</param>
    /// <param name="operators">The list to add operator tokens to.</param>
    private static void CollectLogicalOrNullCoalescingOperators(BinaryExpressionSyntax node, List<SyntaxToken> operators)
    {
        if (IsLogicalOrNullCoalescingExpression(node) == false)
        {
            return;
        }

        if (node.Left is BinaryExpressionSyntax leftBinary)
        {
            CollectLogicalOrNullCoalescingOperators(leftBinary, operators);
        }

        operators.Add(node.OperatorToken);

        if (node.Right is BinaryExpressionSyntax rightBinary)
        {
            CollectLogicalOrNullCoalescingOperators(rightBinary, operators);
        }
    }

    /// <summary>
    /// Builds the corrected leading trivia for a logical operator token to align it
    /// to the target column.
    /// </summary>
    /// <param name="operatorToken">The operator token to align.</param>
    /// <param name="targetColumn">The target column position.</param>
    /// <returns>The corrected leading trivia list.</returns>
    private static SyntaxTriviaList BuildLogicalAlignedTrivia(SyntaxToken operatorToken, int targetColumn)
    {
        return ReplaceLeadingWhitespace(operatorToken.LeadingTrivia, new string(' ', targetColumn));
    }

    /// <inheritdoc/>
    public override SyntaxNode VisitBinaryPattern(BinaryPatternSyntax node)
    {
        var visited = (BinaryPatternSyntax)base.VisitBinaryPattern(node);

        if (visited == null)
        {
            return null;
        }

        if (node.IsKind(SyntaxKind.OrPattern) == false)
        {
            return visited;
        }

        if (IsOutermostOrPattern(node) == false)
        {
            return visited;
        }

        var originalOperators = new List<SyntaxToken>();

        CollectOrPatternOperators(node, originalOperators);

        if (originalOperators.Count == 0)
        {
            return visited;
        }

        var visitedOperators = new List<SyntaxToken>();

        CollectOrPatternOperators(visited, visitedOperators);

        var hasIsPatternColumn = false;
        var isPatternColumn = 0;
        var currentPatternParent = node.Parent;

        while (currentPatternParent != null)
        {
            if (currentPatternParent is IsPatternExpressionSyntax isPatternExpression)
            {
                var isKeywordColumn = isPatternExpression.IsKeyword.GetLocation().GetLineSpan().StartLinePosition.Character;

                isPatternColumn = AdjustColumnForNormalization(isPatternExpression.IsKeyword, isKeywordColumn);
                hasIsPatternColumn = true;

                break;
            }

            currentPatternParent = currentPatternParent.Parent;
        }

        var replacementPairs = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

        for (var i = 0; i < originalOperators.Count && i < visitedOperators.Count; i++)
        {
            var originalOperator = originalOperators[i];

            if (originalOperator.Parent is BinaryPatternSyntax originalParent == false)
            {
                continue;
            }

            if (originalOperator.SyntaxTree == null)
            {
                continue;
            }

            var leftLineSpan = originalParent.Left.SyntaxTree.GetLineSpan(originalParent.Left.Span);
            var targetColumn = hasIsPatternColumn
                                   ? isPatternColumn
                                   : AdjustColumnForNormalization(originalParent.Left.GetFirstToken(), leftLineSpan.StartLinePosition.Character);
            var leftEndLine = leftLineSpan.EndLinePosition.Line;

            var operatorLineSpan = originalOperator.SyntaxTree.GetLineSpan(originalOperator.Span);
            var operatorLine = operatorLineSpan.StartLinePosition.Line;

            if (operatorLine > leftEndLine)
            {
                var newTrivia = BuildLogicalAlignedTrivia(visitedOperators[i], targetColumn);

                replacementPairs.Add((visitedOperators[i], visitedOperators[i].WithLeadingTrivia(newTrivia)));
            }
        }

        if (replacementPairs.Count == 0)
        {
            return visited;
        }

        return visited.ReplaceTokens(replacementPairs.ConvertAll(pair => pair.OldToken),
                                     (original, _) =>
                                     {
                                         var pair = replacementPairs.FirstOrDefault(pair => pair.OldToken == original);

                                         if (pair != default)
                                         {
                                             return pair.NewToken;
                                         }

                                         return original;
                                     });
    }

    /// <summary>
    /// Determines whether the given binary pattern is the outermost <c>or</c> pattern
    /// (i.e., its parent is not also an <c>or</c> binary pattern).
    /// </summary>
    /// <param name="node">The binary pattern to check.</param>
    /// <returns><c>true</c> if it is the outermost <c>or</c> pattern; otherwise, <c>false</c>.</returns>
    private static bool IsOutermostOrPattern(BinaryPatternSyntax node)
    {
        if (node.Parent is BinaryPatternSyntax parentPattern && parentPattern.IsKind(SyntaxKind.OrPattern))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Recursively collects all <c>or</c> pattern operator tokens from the given binary pattern
    /// and its nested <c>or</c> binary-pattern children.
    /// </summary>
    /// <param name="node">The binary pattern to collect operators from.</param>
    /// <param name="operators">The list to add operator tokens to.</param>
    private static void CollectOrPatternOperators(BinaryPatternSyntax node, List<SyntaxToken> operators)
    {
        if (node.IsKind(SyntaxKind.OrPattern) == false)
        {
            return;
        }

        if (node.Left is BinaryPatternSyntax leftPattern)
        {
            CollectOrPatternOperators(leftPattern, operators);
        }

        operators.Add(node.OperatorToken);

        if (node.Right is BinaryPatternSyntax rightPattern)
        {
            CollectOrPatternOperators(rightPattern, operators);
        }
    }

    #endregion // Logical Expression Layout

    #region Shared Helpers

    /// <summary>
    /// Gets the number of leading whitespace characters for a token on its current line.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The number of leading whitespace characters on the token's line.</returns>
    private static int GetLeadingWhitespaceLength(SyntaxToken token)
    {
        var leadingTrivia = token.LeadingTrivia;

        for (var index = leadingTrivia.Count - 1; index >= 0; index--)
        {
            var trivia = leadingTrivia[index];

            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                break;
            }

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return trivia.ToString().Length;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the line number of a token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The line number.</returns>
    private static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Gets the column of a token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The column.</returns>
    private static int GetColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Character;
    }

    /// <summary>
    /// Gets the end column of a token (the column after the last character).
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The end column.</returns>
    private static int GetEndColumn(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().EndLinePosition.Character;
    }

    /// <summary>
    /// Finds the index of the first invoked chain link in the list.
    /// An invoked link is a dot token whose parent <see cref="MemberAccessExpressionSyntax"/>
    /// is the expression of an <see cref="InvocationExpressionSyntax"/> or <see cref="ElementAccessExpressionSyntax"/>.
    /// </summary>
    /// <param name="links">The list of chain link tokens.</param>
    /// <returns>The index of the first invoked link, or -1 if none found.</returns>
    private static int FindFirstInvokedLinkIndex(List<SyntaxToken> links)
    {
        for (var i = 0; i < links.Count; i++)
        {
            if (IsNonInvokedMemberAccessLink(links[i]) == false)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// Determines whether any chain link on the specified original line is a non-invoked
    /// member access (property/field access without invocation).
    /// </summary>
    /// <param name="originalLinks">The list of original chain link tokens.</param>
    /// <param name="line">The line number to check.</param>
    /// <returns><c>true</c> if any link on the line is a non-invoked member access; otherwise, <c>false</c>.</returns>
    private static bool HasNonInvokedLinkOnLine(List<SyntaxToken> originalLinks, int line)
    {
        foreach (var link in originalLinks)
        {
            if (GetLine(link) == line && IsNonInvokedMemberAccessLink(link))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given chain link token represents a non-invoked member access
    /// (i.e., a property or field access that is not directly followed by an invocation or element access).
    /// </summary>
    /// <param name="linkToken">The chain link token (typically a dot operator).</param>
    /// <returns><c>true</c> if the link is a non-invoked member access; otherwise, <c>false</c>.</returns>
    private static bool IsNonInvokedMemberAccessLink(SyntaxToken linkToken)
    {
        if (linkToken.Parent is MemberAccessExpressionSyntax memberAccess)
        {
            return memberAccess.Parent is not InvocationExpressionSyntax
                   && memberAccess.Parent is not ElementAccessExpressionSyntax;
        }

        return false;
    }

    /// <summary>
    /// Strips all trailing end-of-line and whitespace trivia from the given trivia list.
    /// </summary>
    /// <param name="triviaList">The trivia list to modify.</param>
    /// <returns>The trivia list without trailing end-of-line/whitespace entries.</returns>
    private static SyntaxTriviaList StripTrailingEndOfLine(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList.Count);

        foreach (var trivia in triviaList)
        {
            result.Add(trivia);
        }

        while (result.Count > 0
               && (result[result.Count - 1].IsKind(SyntaxKind.EndOfLineTrivia)
                   || result[result.Count - 1].IsKind(SyntaxKind.WhitespaceTrivia)))
        {
            result.RemoveAt(result.Count - 1);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Strips all leading end-of-line and whitespace trivia from the given trivia list,
    /// preserving any non-whitespace trivia such as comments.
    /// </summary>
    /// <param name="triviaList">The trivia list to modify.</param>
    /// <returns>The trivia list with leading end-of-line and whitespace removed.</returns>
    private static SyntaxTriviaList StripLeadingEndOfLineAndWhitespace(SyntaxTriviaList triviaList)
    {
        var result = new List<SyntaxTrivia>(triviaList.Count);
        var foundContent = false;

        foreach (var trivia in triviaList)
        {
            if (foundContent == false
                && (trivia.IsKind(SyntaxKind.EndOfLineTrivia) || trivia.IsKind(SyntaxKind.WhitespaceTrivia)))
            {
                continue;
            }

            foundContent = true;

            result.Add(trivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Strips the trailing end-of-line (and any whitespace before it) from the token
    /// that immediately precedes <paramref name="target"/> in <paramref name="root"/>.
    /// This prevents double-EndOfLine artifacts when an alignment method inserts an
    /// <see cref="SyntaxKind.EndOfLineTrivia"/> into the target token's leading trivia.
    /// </summary>
    /// <typeparam name="T">The type of the root syntax node.</typeparam>
    /// <param name="root">The root node to modify.</param>
    /// <param name="target">The token whose predecessor's trailing end-of-line should be stripped.</param>
    /// <returns>The modified root with the previous token's trailing end-of-line removed.</returns>
    private static T StripTrailingEndOfLineFromPreviousToken<T>(T root, SyntaxToken target)
        where T : SyntaxNode
    {
        var previousToken = target.GetPreviousToken();

        if (previousToken.IsKind(SyntaxKind.None))
        {
            return root;
        }

        var stripped = StripTrailingEndOfLine(previousToken.TrailingTrivia);

        if (stripped.Count == previousToken.TrailingTrivia.Count)
        {
            return root;
        }

        return root.ReplaceToken(previousToken, previousToken.WithTrailingTrivia(stripped));
    }

    /// <summary>
    /// Retrieves the <see cref="TypeParameterConstraintClauseSyntax"/> list from a declaration node,
    /// regardless of whether it is a type, method, or delegate declaration.
    /// </summary>
    /// <typeparam name="T">The type of the syntax node.</typeparam>
    /// <param name="node">The declaration node.</param>
    /// <returns>The constraint clauses, or an empty list if the node type does not support them.</returns>
    private static SyntaxList<TypeParameterConstraintClauseSyntax> GetConstraintClauses<T>(T node)
        where T : SyntaxNode
    {
        if (node is TypeDeclarationSyntax typeDecl)
        {
            return typeDecl.ConstraintClauses;
        }

        if (node is MethodDeclarationSyntax methodDecl)
        {
            return methodDecl.ConstraintClauses;
        }

        if (node is DelegateDeclarationSyntax delegateDecl)
        {
            return delegateDecl.ConstraintClauses;
        }

        return default;
    }

    /// <summary>
    /// Determines whether the given token is a continuation token whose position is set
    /// by alignment rather than structural indentation (e.g., a logical operator or
    /// a member access dot on a continuation line).
    /// </summary>
    /// <param name="token">The token to check.</param>
    /// <returns><c>true</c> if the token is alignment-positioned; otherwise, <c>false</c>.</returns>
    private static bool IsContinuationToken(SyntaxToken token)
    {
        if (token.Parent is BinaryExpressionSyntax binary
            && binary.OperatorToken == token
            && (token.IsKind(SyntaxKind.AmpersandAmpersandToken)
                || token.IsKind(SyntaxKind.BarBarToken)
                || token.IsKind(SyntaxKind.QuestionQuestionToken)))
        {
            return true;
        }

        if (token.IsKind(SyntaxKind.DotToken)
            && token.Parent is MemberAccessExpressionSyntax)
        {
            return true;
        }

        if (token.IsKind(SyntaxKind.QuestionToken)
            && token.Parent is ConditionalAccessExpressionSyntax)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given token is the first token of an argument or parameter
    /// that has been aligned to its containing argument/parameter list's opening delimiter column.
    /// This is true when the token starts an argument/parameter on a line after the opening parenthesis or bracket.
    /// </summary>
    /// <param name="firstTokenOnLine">The first token on the line to check.</param>
    /// <returns><see langword="true"/> if the token is argument-aligned; otherwise, <see langword="false"/>.</returns>
    private static bool IsArgumentAligned(SyntaxToken firstTokenOnLine)
    {
        var node = firstTokenOnLine.Parent;

        while (node != null)
        {
            if (node is ArgumentSyntax argument && argument.GetFirstToken() == firstTokenOnLine)
            {
                if (argument.Parent is ArgumentListSyntax argumentList
                    && GetLine(argumentList.OpenParenToken) != GetLine(firstTokenOnLine))
                {
                    return true;
                }

                if (argument.Parent is BracketedArgumentListSyntax bracketedArgumentList
                    && GetLine(bracketedArgumentList.OpenBracketToken) != GetLine(firstTokenOnLine))
                {
                    return true;
                }
            }

            node = node.Parent;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the given token is the first token of an expression that is a direct
    /// element of an <see cref="InitializerExpressionSyntax"/>. When this is <see langword="true"/>,
    /// the token's indentation is determined by the enclosing initializer layout (relative to the
    /// <c>new</c> keyword), not by tree-depth indentation.
    /// </summary>
    /// <param name="firstTokenOnLine">The first token on the line to check.</param>
    /// <returns><see langword="true"/> if the token is an initializer element token; otherwise, <see langword="false"/>.</returns>
    private static bool IsInitializerElementToken(SyntaxToken firstTokenOnLine)
    {
        var node = firstTokenOnLine.Parent;

        while (node != null)
        {
            if (node.Parent is InitializerExpressionSyntax)
            {
                return node.GetFirstToken() == firstTokenOnLine;
            }

            node = node.Parent;
        }

        return false;
    }

    /// <summary>
    /// Adjusts a column position from the original syntax tree to account for block indent normalization.
    /// When <see cref="VisitToken"/> normalizes the indentation of the first token on a line,
    /// all column positions on that line shift by the same delta.
    /// </summary>
    /// <param name="tokenOnLine">Any token on the line whose column is being adjusted (from the original tree).</param>
    /// <param name="originalColumn">The column position in the original tree.</param>
    /// <returns>The adjusted column position accounting for block indent normalization.</returns>
    private int AdjustColumnForNormalization(SyntaxToken tokenOnLine, int originalColumn)
    {
        var line = GetLine(tokenOnLine);
        var firstTokenOnLine = tokenOnLine;

        while (true)
        {
            var previousToken = firstTokenOnLine.GetPreviousToken();

            if (previousToken.IsKind(SyntaxKind.None) || GetLine(previousToken) != line)
            {
                break;
            }

            firstTokenOnLine = previousToken;
        }

        var originalIndent = GetColumn(firstTokenOnLine);
        var normalizedIndent = ComputeIndentLevel(firstTokenOnLine) * FormattingContext.IndentSize;

        if (IsContinuationToken(firstTokenOnLine) || IsArgumentAligned(firstTokenOnLine) || IsInitializerElementToken(firstTokenOnLine))
        {
            normalizedIndent = originalIndent;
        }

        return originalColumn + normalizedIndent - originalIndent;
    }

    #endregion // Shared Helpers

    #region IFormattingRule

    /// <inheritdoc/>
    public override FormattingPhase Phase => FormattingPhase.Indentation;

    #endregion // IFormattingRule
}