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

        var indentLevel = ComputeIndentLevel(token);
        var expectedWhitespace = new string(' ', indentLevel * FormattingContext.IndentSize);

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
        switch (node)
        {
            case BlockSyntax block:
                return IsInsideBraces(token, block.OpenBraceToken, block.CloseBraceToken);

            case TypeDeclarationSyntax typeDecl:
                return IsInsideBraces(token, typeDecl.OpenBraceToken, typeDecl.CloseBraceToken);

            case NamespaceDeclarationSyntax nsDecl:
                return IsInsideBraces(token, nsDecl.OpenBraceToken, nsDecl.CloseBraceToken);

            case EnumDeclarationSyntax enumDecl:
                return IsInsideBraces(token, enumDecl.OpenBraceToken, enumDecl.CloseBraceToken);

            case SwitchStatementSyntax switchStmt:
                return IsInsideBraces(token, switchStmt.OpenBraceToken, switchStmt.CloseBraceToken);

            case SwitchSectionSyntax switchSection:
                return IsInsideSwitchSectionStatements(token, switchSection);

            case AccessorListSyntax accessorList:
                return IsInsideBraces(token, accessorList.OpenBraceToken, accessorList.CloseBraceToken);

            case InitializerExpressionSyntax initializer:
                return IsInsideBraces(token, initializer.OpenBraceToken, initializer.CloseBraceToken);

            case AnonymousObjectCreationExpressionSyntax anon:
                return IsInsideBraces(token, anon.OpenBraceToken, anon.CloseBraceToken);

            case CollectionExpressionSyntax collection:
                return IsInsideBraces(token, collection.OpenBracketToken, collection.CloseBracketToken);

            default:
                return false;
        }
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
            if (ancestor is StatementSyntax && switchSection.Statements.Contains(ancestor))
            {
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
        if (visited.Arguments.Count <= 1)
        {
            return visited;
        }

        var alignColumn = adjustedOpenParenColumn + 1;
        var firstArgLine = originalNode.Arguments[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        var newArguments = AlignNodesToColumn(originalNode.Arguments, visited.Arguments, firstArgLine, alignColumn);

        if (newArguments == null)
        {
            return visited;
        }

        ShiftLambdaBlockBodies(newArguments, alignColumn);

        return visited.WithArguments(SyntaxFactory.SeparatedList(newArguments, visited.Arguments.GetSeparators()));
    }

    /// <summary>
    /// Shifts the block bodies of lambda arguments so that their indentation
    /// matches the argument alignment column instead of tree-based indentation.
    /// </summary>
    /// <param name="arguments">The list of aligned arguments to process.</param>
    /// <param name="alignColumn">The target column for the argument alignment.</param>
    private void ShiftLambdaBlockBodies(List<ArgumentSyntax> arguments, int alignColumn)
    {
        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];

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

            var currentIndent = ComputeIndentLevel(blockOpenBrace) * FormattingContext.IndentSize;
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

                var tokenIndent = ComputeIndentLevel(token) * FormattingContext.IndentSize;
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
            }
        }
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

        foreach (var arm in visited.Arms)
        {
            var alignedArm = AlignNodeToColumn(arm, armIndent);

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
        var questionLine = node.QuestionToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        // Only format if ? is on a different line than the condition end
        if (questionLine == conditionEndLine)
        {
            return visited;
        }

        // Align ? and : relative to the condition's start column (adjusted for normalization)
        var conditionFirstToken = node.Condition.GetFirstToken();
        var conditionStartColumn = conditionFirstToken.GetLocation().GetLineSpan().StartLinePosition.Character;
        var alignColumn = AdjustColumnForNormalization(conditionFirstToken, conditionStartColumn) + FormattingContext.IndentSize;

        var colonLine = node.ColonToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        var newQuestion = visited.QuestionToken.WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine),
                                                                  SyntaxFactory.Whitespace(new string(' ', alignColumn)));

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

    #endregion // Conditional Expression Layout

    #region Object Initializer Layout

    /// <inheritdoc/>
    public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
    {
        int newKeywordColumn;

        if (_parentAssignmentWhitespaceStack.Count > 0
            && node.Parent is AssignmentExpressionSyntax { Parent: InitializerExpressionSyntax } assignment)
        {
            var parentAssignmentWhitespace = _parentAssignmentWhitespaceStack.Peek();
            var offsetToNew = node.NewKeyword.SpanStart - assignment.SpanStart;

            newKeywordColumn = parentAssignmentWhitespace + offsetToNew;
        }
        else
        {
            newKeywordColumn = AdjustColumnForNormalization(node.NewKeyword, node.NewKeyword.GetLocation().GetLineSpan().StartLinePosition.Character);
        }

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

        if (node.Parent is MemberAccessExpressionSyntax || node.Parent is ConditionalAccessExpressionSyntax)
        {
            return visited;
        }

        var correctedInitializer = RebuildInitializer(visited, newKeywordColumn);

        return visited.WithInitializer(correctedInitializer)
                      .WithLeadingTrivia(visited.GetLeadingTrivia())
                      .WithTrailingTrivia(visited.GetTrailingTrivia());
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
    private static ExpressionSyntax RebuildAssignment(ExpressionSyntax expression, string whitespace)
    {
        var result = expression.WithoutLeadingTrivia()
                               .WithLeadingTrivia(SyntaxFactory.Whitespace(whitespace));

        if (result is AssignmentExpressionSyntax assignmentExpression)
        {
            result = AlignMultilineAssignmentChain(assignmentExpression, whitespace);
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
    private static ExpressionSyntax AlignMultilineAssignmentChain(AssignmentExpressionSyntax assignmentExpression, string whitespace)
    {
        var assignmentLineSpan = assignmentExpression.GetLocation().GetLineSpan();
        var assignmentLine = assignmentLineSpan.StartLinePosition.Line;
        var assignmentColumn = assignmentLineSpan.StartLinePosition.Character;
        var firstDotToken = assignmentExpression.Right
                                                .DescendantTokens()
                                                .FirstOrDefault(token => token.IsKind(SyntaxKind.DotToken));

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
        var targetDotColumn = whitespace.Length + relativeDotColumn;
        var replacementMap = new Dictionary<SyntaxToken, SyntaxToken>();

        foreach (var dotToken in assignmentExpression.Right.DescendantTokens().Where(token => token.IsKind(SyntaxKind.DotToken)))
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
    /// Removes trailing end-of-line and whitespace trivia from a trivia list.
    /// Returns the result as a <see cref="List{T}"/> to allow count comparison with the original.
    /// </summary>
    /// <param name="triviaList">The trivia list to strip.</param>
    /// <returns>The stripped trivia as a list.</returns>
    private static List<SyntaxTrivia> StripTrailingEndOfLinesFromTrivia(SyntaxTriviaList triviaList)
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

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(SyntaxFactory.Whitespace(braceWhitespace))
                                     .WithTrailingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine));

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                      .WithLeadingTrivia(SyntaxFactory.EndOfLine(Context.EndOfLine), SyntaxFactory.Whitespace(braceWhitespace));

        var expressions = RebuildAssignments(initializer, assignmentWhitespace);

        return SyntaxFactory.InitializerExpression(initializer.Kind(), openBrace, expressions, closeBrace);
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

        if (visited == null || visited.Elements.Count <= 1)
        {
            return visited;
        }

        var openBracketColumn = AdjustColumnForNormalization(node.OpenBracketToken, node.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Character);
        var alignColumn = openBracketColumn + FormattingContext.IndentSize;
        var openBracketLine = node.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        var newElements = AlignNodesToColumn(node.Elements, visited.Elements, openBracketLine, alignColumn);

        if (newElements != null)
        {
            visited = visited.WithElements(SyntaxFactory.SeparatedList(newElements, visited.Elements.GetSeparators()));
        }

        // Align the closing bracket to the same column as the opening bracket
        var closeBracketLine = node.CloseBracketToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (closeBracketLine > openBracketLine)
        {
            var closeBracketToken = visited.CloseBracketToken;
            var newLeading = ReplaceLeadingWhitespace(closeBracketToken.LeadingTrivia, new string(' ', openBracketColumn));
            var newCloseBracket = closeBracketToken.WithLeadingTrivia(newLeading);

            if (newCloseBracket != closeBracketToken)
            {
                visited = visited.WithCloseBracketToken(newCloseBracket);
            }
        }

        return visited;
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
                referenceColumn = AdjustColumnForNormalization(originalLinks[firstInvokedLinkIndex], GetColumn(originalLinks[firstInvokedLinkIndex]));
            }
            else if (firstLinkLine == previousTokenLine)
            {
                referenceColumn = AdjustColumnForNormalization(firstLink, GetColumn(firstLink));
            }
            else
            {
                referenceColumn = AdjustColumnForNormalization(previousToken, GetEndColumn(previousToken));
                moveFirstLinkToSameLine = true;
            }
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

        var replacementPairs = new List<(SyntaxToken OldToken, SyntaxToken NewToken)>();

        if (moveFirstLinkToSameLine)
        {
            var visitedPreviousToken = visitedLinks[0].GetPreviousToken();
            var strippedTrailing = StripTrailingEndOfLine(visitedPreviousToken.TrailingTrivia);

            if (strippedTrailing.Count != visitedPreviousToken.TrailingTrivia.Count)
            {
                replacementPairs.Add((visitedPreviousToken, visitedPreviousToken.WithTrailingTrivia(strippedTrailing)));
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

        if (IsLogicalExpression(node) == false)
        {
            return visited;
        }

        if (IsOutermostLogicalExpression(node) == false)
        {
            return visited;
        }

        var originalOperators = new List<SyntaxToken>();

        CollectLogicalOperators(node, originalOperators);

        if (originalOperators.Count == 0)
        {
            return visited;
        }

        var visitedOperators = new List<SyntaxToken>();

        CollectLogicalOperators(visited, visitedOperators);

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
    /// (<c>&amp;&amp;</c> or <c>||</c>).
    /// </summary>
    /// <param name="node">The binary expression to check.</param>
    /// <returns><c>true</c> if the expression is logical; otherwise, <c>false</c>.</returns>
    private static bool IsLogicalExpression(BinaryExpressionSyntax node)
    {
        return node.IsKind(SyntaxKind.LogicalAndExpression) || node.IsKind(SyntaxKind.LogicalOrExpression);
    }

    /// <summary>
    /// Determines whether the given logical binary expression is the outermost one
    /// (i.e., its parent is not also a logical binary expression).
    /// </summary>
    /// <param name="node">The binary expression to check.</param>
    /// <returns><c>true</c> if it is the outermost logical expression; otherwise, <c>false</c>.</returns>
    private static bool IsOutermostLogicalExpression(BinaryExpressionSyntax node)
    {
        if (node.Parent is BinaryExpressionSyntax parentBinary
            && (parentBinary.IsKind(SyntaxKind.LogicalAndExpression) || parentBinary.IsKind(SyntaxKind.LogicalOrExpression)))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Recursively collects all logical operator tokens from the given binary expression
    /// and its nested logical binary children.
    /// </summary>
    /// <param name="node">The binary expression to collect operators from.</param>
    /// <param name="operators">The list to add operator tokens to.</param>
    private static void CollectLogicalOperators(BinaryExpressionSyntax node, List<SyntaxToken> operators)
    {
        if (IsLogicalExpression(node) == false)
        {
            return;
        }

        if (node.Left is BinaryExpressionSyntax leftBinary)
        {
            CollectLogicalOperators(leftBinary, operators);
        }

        operators.Add(node.OperatorToken);

        if (node.Right is BinaryExpressionSyntax rightBinary)
        {
            CollectLogicalOperators(rightBinary, operators);
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

    #endregion // Logical Expression Layout

    #region Shared Helpers

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
            && (token.IsKind(SyntaxKind.AmpersandAmpersandToken) || token.IsKind(SyntaxKind.BarBarToken)))
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