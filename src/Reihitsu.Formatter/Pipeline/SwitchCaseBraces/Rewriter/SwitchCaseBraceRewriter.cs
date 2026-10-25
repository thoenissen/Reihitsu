using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Pipeline.BlankLines.Utilities;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Pipeline.SwitchCaseBraces.Rewriter;

/// <summary>
/// Rewrites switch statements to add or remove braces from case sections
/// </summary>
internal sealed class SwitchCaseBraceRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// Cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public SwitchCaseBraceRewriter(FormattingContext context, CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a switch section is a fall-through section (no statements, or only labels)
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is a fall-through; otherwise, <see langword="false"/></returns>
    private static bool IsFallThroughSection(SwitchSectionSyntax section)
    {
        return section.Statements.Count == 0;
    }

    /// <summary>
    /// Determines whether a switch section is multi-line.
    /// A section is multi-line if its label contains a multi-line delimited pattern, it has more
    /// than one non-terminal statement, a single non-terminal statement that spans multiple lines,
    /// a non-terminal statement followed by return or throw on a later line, or any statement
    /// containing a multi-line switch expression
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is multi-line; otherwise, <see langword="false"/></returns>
    private static bool IsMultiLineSection(SwitchSectionSyntax section)
    {
        if (LabelContainsMultiLinePattern(section))
        {
            return true;
        }

        var statements = GetNonTerminalStatements(section);

        if (statements.Count > 1)
        {
            return true;
        }

        if (statements.Count == 1)
        {
            return SpansMultipleLines(statements[0])
                   || HasMultiLineBodyEndingInReturnOrThrow(section);
        }

        return ContainsMultiLineSwitchExpression(section);
    }

    /// <summary>
    /// Determines whether a switch section is empty (contains no statements)
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section is empty; otherwise, <see langword="false"/></returns>
    private static bool IsEmptySection(SwitchSectionSyntax section)
    {
        return section.Statements.Any(statement => statement.IsKind(SyntaxKind.BreakStatement) == false) == false;
    }

    /// <summary>
    /// Determines whether a switch section has multiple statements on separate lines and ends in
    /// return or throw. A trailing break is intentionally excluded because it remains outside an
    /// inserted brace block
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section ends in return or throw on a later line; otherwise, <see langword="false"/></returns>
    private static bool HasMultiLineBodyEndingInReturnOrThrow(SwitchSectionSyntax section)
    {
        if (section.Statements.Count < 2)
        {
            return false;
        }

        var lastStatement = section.Statements[section.Statements.Count - 1];

        if (lastStatement is not ReturnStatementSyntax and not ThrowStatementSyntax)
        {
            return false;
        }

        var firstStatementSpan = section.Statements[0].GetLocation().GetLineSpan();
        var lastStatementSpan = lastStatement.GetLocation().GetLineSpan();

        return firstStatementSpan.StartLinePosition.Line != lastStatementSpan.EndLinePosition.Line;
    }

    /// <summary>
    /// Determines whether an ordinary goto statement targets a label owned by another section of
    /// the same switch. Adding blocks in that case would make the target unreachable
    /// </summary>
    /// <param name="node">The switch statement to check</param>
    /// <returns><see langword="true"/> if a goto targets a label in another section; otherwise, <see langword="false"/></returns>
    private static bool HasCrossSectionGotoTarget(SwitchStatementSyntax node)
    {
        var labelSections = new Dictionary<string, SwitchSectionSyntax>();

        foreach (var (section, labeledStatement) in node.Sections.SelectMany(section => GetExecutableScopeDescendants(section).OfType<LabeledStatementSyntax>()
                                                                                                                              .Where(labeledStatement => labeledStatement.Ancestors()
                                                                                                                                                                         .OfType<SwitchSectionSyntax>()
                                                                                                                                                                         .FirstOrDefault() == section)
                                                                                                                              .Select(labeledStatement => (section, labeledStatement))))
        {
            labelSections[labeledStatement.Identifier.ValueText] = section;
        }

        foreach (var section in node.Sections)
        {
            foreach (var gotoStatement in GetExecutableScopeDescendants(section).OfType<GotoStatementSyntax>())
            {
                if (gotoStatement.IsKind(SyntaxKind.GotoStatement)
                    && gotoStatement.Expression is IdentifierNameSyntax identifierName
                    && labelSections.TryGetValue(identifierName.Identifier.ValueText, out var targetSection)
                    && targetSection != section)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a local declared directly in one section of the switch is used by simple
    /// name in another section. C# scopes such a local to the whole switch block, so a block around
    /// the declaring section would hide it from the other section. A name that a declaration inside the
    /// other section binds, such as a lambda parameter that shadows the local, is not a use of it
    /// </summary>
    /// <param name="node">The switch statement to check</param>
    /// <returns><see langword="true"/> if another section uses a directly declared local; otherwise, <see langword="false"/></returns>
    private static bool HasCrossSectionLocalUse(SwitchStatementSyntax node)
    {
        var declaringSections = new Dictionary<string, List<SwitchSectionSyntax>>();

        foreach (var section in node.Sections)
        {
            foreach (var name in GetDirectlyDeclaredLocalNames(section))
            {
                if (declaringSections.TryGetValue(name, out var sections) == false)
                {
                    sections = [];
                    declaringSections[name] = sections;
                }

                sections.Add(section);
            }
        }

        if (declaringSections.Count == 0)
        {
            return false;
        }

        foreach (var section in node.Sections)
        {
            foreach (var simpleName in section.DescendantNodes().OfType<SimpleNameSyntax>())
            {
                if (declaringSections.TryGetValue(simpleName.Identifier.ValueText, out var sections)
                    && sections.Any(declaringSection => declaringSection != section)
                    && IsBoundInsideSection(simpleName, section) == false)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a simple name in a switch section is bound by a declaration of that section
    /// that encloses it: a parameter of an enclosing lambda, anonymous method, or local function, a
    /// variable that an enclosing lambda's expression body declares, or a local that an enclosing block
    /// declares directly. Such a name does not refer to a local of another section, which C# 8 and later
    /// let a lambda or local function shadow
    /// </summary>
    /// <param name="simpleName">The simple name to inspect</param>
    /// <param name="section">The switch section that holds the name</param>
    /// <returns><see langword="true"/> if a declaration inside the section binds the name; otherwise, <see langword="false"/></returns>
    private static bool IsBoundInsideSection(SimpleNameSyntax simpleName, SwitchSectionSyntax section)
    {
        var name = simpleName.Identifier.ValueText;

        foreach (var ancestor in simpleName.Ancestors())
        {
            if (ancestor == section)
            {
                return false;
            }

            var isBound = ancestor switch
                          {
                              SimpleLambdaExpressionSyntax simpleLambda => simpleLambda.Parameter.Identifier.ValueText == name
                                                                           || IsDesignatedIn(simpleLambda.ExpressionBody, name),
                              ParenthesizedLambdaExpressionSyntax parenthesizedLambda => HasParameter(parenthesizedLambda.ParameterList, name)
                                                                                         || IsDesignatedIn(parenthesizedLambda.ExpressionBody, name),
                              AnonymousMethodExpressionSyntax anonymousMethod => HasParameter(anonymousMethod.ParameterList, name),
                              LocalFunctionStatementSyntax localFunction => HasParameter(localFunction.ParameterList, name)
                                                                            || IsDesignatedIn(localFunction.ExpressionBody?.Expression, name),
                              BlockSyntax block => GetDirectlyDeclaredLocalNames(block.Statements).Contains(name),
                              _ => false,
                          };

            if (isBound)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether a parameter list declares a parameter of the given name
    /// </summary>
    /// <param name="parameterList">The parameter list, or <see langword="null"/></param>
    /// <param name="name">The name to look for</param>
    /// <returns><see langword="true"/> if the list declares the name; otherwise, <see langword="false"/></returns>
    private static bool HasParameter(ParameterListSyntax parameterList, string name)
    {
        return parameterList?.Parameters.Any(parameter => parameter.Identifier.ValueText == name) == true;
    }

    /// <summary>
    /// Determines whether an expression body declares a variable of the given name
    /// </summary>
    /// <param name="expressionBody">The expression body, or <see langword="null"/></param>
    /// <param name="name">The name to look for</param>
    /// <returns><see langword="true"/> if the expression body declares the name; otherwise, <see langword="false"/></returns>
    private static bool IsDesignatedIn(SyntaxNode expressionBody, string name)
    {
        return expressionBody != null
               && GetDesignatedNames(expressionBody).Contains(name);
    }

    /// <summary>
    /// Gets the names of the locals that a switch section declares directly: through a statement the
    /// section owns, or such a statement behind a label, outside any nested body
    /// </summary>
    /// <param name="section">The switch section to inspect</param>
    /// <returns>The names of the directly declared locals</returns>
    private static IEnumerable<string> GetDirectlyDeclaredLocalNames(SwitchSectionSyntax section)
    {
        return GetDirectlyDeclaredLocalNames(section.Statements);
    }

    /// <summary>
    /// Gets the names of the locals that a list of statements declares directly, in the scope that owns
    /// the list: through a statement of the list, or such a statement behind a label, outside any nested
    /// body. Statements whose header variables C# scopes to the statement itself (loops, <c>using</c>,
    /// <c>fixed</c>, nested blocks, and the embedded statements and <c>else</c> branch of an <c>if</c>)
    /// declare nothing here
    /// </summary>
    /// <param name="statements">The statements to inspect</param>
    /// <returns>The names of the directly declared locals</returns>
    private static IEnumerable<string> GetDirectlyDeclaredLocalNames(SyntaxList<StatementSyntax> statements)
    {
        foreach (var sectionStatement in statements)
        {
            var statement = sectionStatement;

            while (statement is LabeledStatementSyntax labeledStatement)
            {
                statement = labeledStatement.Statement;
            }

            switch (statement)
            {
                case LocalDeclarationStatementSyntax localDeclaration:
                    {
                        foreach (var variable in localDeclaration.Declaration.Variables)
                        {
                            yield return variable.Identifier.ValueText;
                        }

                        foreach (var name in GetDesignatedNames(localDeclaration.Declaration))
                        {
                            yield return name;
                        }
                    }
                    break;

                case LocalFunctionStatementSyntax localFunction:
                    {
                        yield return localFunction.Identifier.ValueText;
                    }
                    break;

                case ExpressionStatementSyntax expressionStatement:
                    {
                        foreach (var name in GetDesignatedNames(expressionStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;

                case ReturnStatementSyntax { Expression: not null } returnStatement:
                    {
                        foreach (var name in GetDesignatedNames(returnStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;

                case ThrowStatementSyntax { Expression: not null } throwStatement:
                    {
                        foreach (var name in GetDesignatedNames(throwStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;

                case YieldStatementSyntax { Expression: not null } yieldStatement:
                    {
                        foreach (var name in GetDesignatedNames(yieldStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;

                case IfStatementSyntax ifStatement:
                    {
                        foreach (var name in GetDesignatedNames(ifStatement.Condition))
                        {
                            yield return name;
                        }
                    }
                    break;

                case SwitchStatementSyntax switchStatement:
                    {
                        foreach (var name in GetDesignatedNames(switchStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;

                case LockStatementSyntax lockStatement:
                    {
                        foreach (var name in GetDesignatedNames(lockStatement.Expression))
                        {
                            yield return name;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the names that <c>out var</c>, deconstruction, and pattern designations declare in a node,
    /// skipping lambda and anonymous-method bodies, switch-expression arms, and the clauses of a query
    /// behind its first <c>from</c> expression, which scope their designations to themselves
    /// </summary>
    /// <param name="node">The node to inspect</param>
    /// <returns>The designated names</returns>
    private static IEnumerable<string> GetDesignatedNames(SyntaxNode node)
    {
        return node.DescendantNodesAndSelf(static descendant => descendant is not AnonymousFunctionExpressionSyntax and not SwitchExpressionArmSyntax and not QueryBodySyntax)
                   .OfType<SingleVariableDesignationSyntax>()
                   .Select(static designation => designation.Identifier.ValueText);
    }

    /// <summary>
    /// Gets descendants that belong to a switch section's executable scope. Local functions and
    /// anonymous functions own independent label scopes and are not traversed
    /// </summary>
    /// <param name="section">The switch section whose descendants should be enumerated</param>
    /// <returns>The descendants in the section's executable scope</returns>
    private static IEnumerable<SyntaxNode> GetExecutableScopeDescendants(SwitchSectionSyntax section)
    {
        return section.DescendantNodes(static node => node is not LocalFunctionStatementSyntax && node is not AnonymousFunctionExpressionSyntax);
    }

    /// <summary>
    /// Determines whether a switch section's label contains a multi-line delimited pattern
    /// (recursive, list, or parenthesized). Combinator chains and guard clauses that merely wrap
    /// across lines do not count
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if a label contains a multi-line delimited pattern; otherwise, <see langword="false"/></returns>
    private static bool LabelContainsMultiLinePattern(SwitchSectionSyntax section)
    {
        return section.Labels
                      .SelectMany(label => label.DescendantNodes())
                      .Any(node => node is RecursivePatternSyntax or ListPatternSyntax or ParenthesizedPatternSyntax
                                   && SpansMultipleLines(node));
    }

    /// <summary>
    /// Determines whether any statement in a switch section contains a switch expression that
    /// spans multiple lines, including switch expressions nested inside terminal statements
    /// such as <c>return</c> or <c>throw</c>
    /// </summary>
    /// <param name="section">The switch section to check</param>
    /// <returns><see langword="true"/> if the section contains a multi-line switch expression; otherwise, <see langword="false"/></returns>
    private static bool ContainsMultiLineSwitchExpression(SwitchSectionSyntax section)
    {
        return section.Statements
                      .SelectMany(statement => statement.DescendantNodes().OfType<SwitchExpressionSyntax>())
                      .Any(SpansMultipleLines);
    }

    /// <summary>
    /// Checks whether a syntax node spans multiple lines
    /// </summary>
    /// <param name="node">The syntax node to check</param>
    /// <returns><see langword="true"/> if the node spans multiple lines; otherwise, <see langword="false"/></returns>
    private static bool SpansMultipleLines(SyntaxNode node)
    {
        return SyntaxNodeUtilities.IsSingleLine(node) == false;
    }

    /// <summary>
    /// Gets the non-terminal statements from a switch section (excludes break, return, and throw)
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>A list of non-terminal statements</returns>
    private static List<StatementSyntax> GetNonTerminalStatements(SwitchSectionSyntax section)
    {
        return section.Statements
                      .Where(statement => IsTerminalStatement(statement) == false)
                      .ToList();
    }

    /// <summary>
    /// Determines whether a statement is a terminal statement (break, return, or throw)
    /// </summary>
    /// <param name="statement">The statement to check</param>
    /// <returns><see langword="true"/> if the statement is terminal; otherwise, <see langword="false"/></returns>
    private static bool IsTerminalStatement(StatementSyntax statement)
    {
        return statement is BreakStatementSyntax
               || statement is ReturnStatementSyntax
               || statement is ThrowStatementSyntax;
    }

    /// <summary>
    /// Removes braces from a switch section, extracting the block's statements.
    /// If the section does not have braces, it is returned as-is.
    /// When the braces carry comment trivia that cannot be re-attached without loss, the
    /// braces are kept so the comments are not silently deleted
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with braces removed</returns>
    private static SwitchSectionSyntax RemoveBraces(SwitchSectionSyntax section)
    {
        var statements = section.Statements;

        if (statements.Count == 0 || (statements[0] is BlockSyntax) == false)
        {
            return section;
        }

        var block = (BlockSyntax)statements[0];

        // Removing the braces discards the brace tokens, so any comments or directives attached to them
        // would be lost. Keep the braces in that case rather than deleting them.
        if (BraceTokensCarryCommentsOrDirectives(block))
        {
            return section;
        }

        var extractedStatements = new List<StatementSyntax>();

        foreach (var statement in block.Statements)
        {
            extractedStatements.Add(statement);
        }

        // Add remaining statements after the block (e.g., break after braces)
        for (var statementIndex = 1; statementIndex < statements.Count; statementIndex++)
        {
            extractedStatements.Add(statements[statementIndex]);
        }

        return section.WithStatements(SyntaxFactory.List(extractedStatements));
    }

    /// <summary>
    /// Builds leading trivia that preserves any comments from the original leading trivia while
    /// resetting the layout to a single leading end-of-line per comment
    /// </summary>
    /// <param name="original">The original leading trivia</param>
    /// <param name="eolTrivia">The end-of-line trivia to use</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList BuildLeadingTrivia(SyntaxTriviaList original, SyntaxTrivia eolTrivia)
    {
        var result = new List<SyntaxTrivia>
                     {
                         eolTrivia
                     };

        foreach (var comment in CollectComments(original))
        {
            result.Add(comment);
            result.Add(eolTrivia);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds the leading trivia of a terminal <c>break</c> that moves behind the new block: the line
    /// break that ends the block's closing-brace line, then each comment on a line of its own, ended by a
    /// line break unless the comment carries its own, as a documentation comment does. The blank
    /// lines in front of a comment — above the first one and between two of them — are kept as the
    /// blank-line phase placed them; blank lines that no comment follows are dropped
    /// </summary>
    /// <param name="original">The original leading trivia of the break</param>
    /// <param name="eolTrivia">The end-of-line trivia to use</param>
    /// <returns>The rebuilt leading trivia</returns>
    private static SyntaxTriviaList BuildTerminalBreakLeadingTrivia(SyntaxTriviaList original, SyntaxTrivia eolTrivia)
    {
        var result = new List<SyntaxTrivia>
                     {
                         eolTrivia
                     };

        var pendingBlankLines = 0;
        var lineHasContent = false;

        foreach (var trivia in original)
        {
            if (ReihitsuFormatterHelpers.IsCommentTrivia(trivia))
            {
                for (var blankLineIndex = 0; blankLineIndex < pendingBlankLines; blankLineIndex++)
                {
                    result.Add(eolTrivia);
                }

                pendingBlankLines = 0;

                result.Add(trivia);

                // A documentation comment carries the line break that ends its last line, so the next line starts behind it
                if (BlankLineTriviaUtilities.EndsWithLineBreak(trivia))
                {
                    lineHasContent = false;
                }
                else
                {
                    lineHasContent = true;

                    result.Add(eolTrivia);
                }
            }
            else if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                if (lineHasContent == false)
                {
                    pendingBlankLines++;
                }

                lineHasContent = false;
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds trailing trivia that preserves a trailing comment (for example "DoWork(); // note")
    /// while terminating the line with the given end-of-line trivia
    /// </summary>
    /// <param name="original">The original trailing trivia</param>
    /// <param name="eolTrivia">The end-of-line trivia to use</param>
    /// <returns>The rebuilt trailing trivia</returns>
    private static SyntaxTriviaList BuildTrailingTrivia(SyntaxTriviaList original, SyntaxTrivia eolTrivia)
    {
        var comments = CollectComments(original);

        if (comments.Count == 0)
        {
            return SyntaxFactory.TriviaList(eolTrivia);
        }

        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.Space
                     };

        foreach (var comment in comments)
        {
            result.Add(comment);
        }

        result.Add(eolTrivia);

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Builds the trailing trivia for the last label, keeping any trailing comment but dropping
    /// trailing whitespace so the open brace is not preceded by a blank line
    /// </summary>
    /// <param name="original">The original label trailing trivia</param>
    /// <returns>The rebuilt label trailing trivia</returns>
    private static SyntaxTriviaList BuildLabelTrailingTrivia(SyntaxTriviaList original)
    {
        var comments = CollectComments(original);

        if (comments.Count == 0)
        {
            return SyntaxFactory.TriviaList();
        }

        var result = new List<SyntaxTrivia>
                     {
                         SyntaxFactory.Space
                     };

        foreach (var comment in comments)
        {
            result.Add(comment);
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Determines whether the open or close brace tokens of a block carry comment trivia,
    /// a preprocessor directive, or disabled text
    /// </summary>
    /// <param name="block">The block to inspect</param>
    /// <returns><see langword="true"/> if any brace token carries a comment or directive; otherwise, <see langword="false"/></returns>
    private static bool BraceTokensCarryCommentsOrDirectives(BlockSyntax block)
    {
        return BraceTriviaCarriesCommentOrDirective(block.OpenBraceToken.LeadingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.OpenBraceToken.TrailingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.CloseBraceToken.LeadingTrivia)
               || BraceTriviaCarriesCommentOrDirective(block.CloseBraceToken.TrailingTrivia);
    }

    /// <summary>
    /// Determines whether a trivia list carries comment trivia, a preprocessor directive, or disabled text
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns><see langword="true"/> if the list carries a comment or directive; otherwise, <see langword="false"/></returns>
    private static bool BraceTriviaCarriesCommentOrDirective(SyntaxTriviaList triviaList)
    {
        return triviaList.Any(static trivia => ReihitsuFormatterHelpers.IsCommentTrivia(trivia)
                                               || ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia(trivia));
    }

    /// <summary>
    /// Collects the comment trivia from a trivia list, preserving their order
    /// </summary>
    /// <param name="triviaList">The trivia list to inspect</param>
    /// <returns>The comment trivia contained in the list</returns>
    private static List<SyntaxTrivia> CollectComments(SyntaxTriviaList triviaList)
    {
        return triviaList.Where(ReihitsuFormatterHelpers.IsCommentTrivia)
                         .ToList();
    }

    /// <summary>
    /// Determines whether a switch section carries a preprocessor directive or disabled text in the
    /// section-level trivia of its labels or statements — the trivia that brace insertion rebuilds
    /// from comments only and would therefore drop
    /// </summary>
    /// <param name="section">The switch section to inspect</param>
    /// <returns><see langword="true"/> if the section carries a directive or disabled text; otherwise, <see langword="false"/></returns>
    private static bool SectionCarriesDirectives(SwitchSectionSyntax section)
    {
        return section.Labels.Cast<SyntaxNode>()
                             .Concat(section.Statements)
                             .Any(static node => node.GetLeadingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia)
                                                 || node.GetTrailingTrivia().Any(ReihitsuFormatterHelpers.IsDirectiveOrDisabledTextTrivia));
    }

    /// <summary>
    /// Determines whether any non-fall-through section in the switch spans multiple lines
    /// </summary>
    /// <param name="sections">The switch sections to inspect</param>
    /// <returns><see langword="true"/> if at least one non-fall-through section is multi-line; otherwise, <see langword="false"/></returns>
    private static bool HasAnyMultiLineSection(SyntaxList<SwitchSectionSyntax> sections)
    {
        foreach (var section in sections)
        {
            if (IsFallThroughSection(section))
            {
                continue;
            }

            if (IsMultiLineSection(section))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Adds braces around the non-terminal statements of a switch section.
    /// If braces already exist, the section is returned as-is
    /// </summary>
    /// <param name="section">The switch section</param>
    /// <returns>The section with braces added</returns>
    private SwitchSectionSyntax AddBraces(SwitchSectionSyntax section)
    {
        var statements = section.Statements;

        // Already has a block as the first statement — nothing to do
        if (statements.Count > 0 && statements[0] is BlockSyntax)
        {
            return section;
        }

        // Adding braces rebuilds the label, first-statement, and trailing-break trivia from comments
        // only. A preprocessor directive or disabled text in that section-level trivia would be dropped,
        // silently removing conditional compilation, so leave a directive-bearing section unbraced.
        if (SectionCarriesDirectives(section))
        {
            return section;
        }

        var eolTrivia = SyntaxFactory.EndOfLine(_context.EndOfLine);

        // Strip trailing whitespace from the last label to prevent a blank line before the open brace,
        // but keep any trailing comment (for example "case 1: // note") so it is not deleted
        var labels = section.Labels;
        var lastLabel = labels.Last();
        section = section.WithLabels(labels.Replace(lastLabel, lastLabel.WithTrailingTrivia(BuildLabelTrailingTrivia(lastLabel.GetTrailingTrivia()))));

        // Separate trailing break from the statements that go into the block
        StatementSyntax trailingBreak = null;

        if (statements.Count > 0
            && BlankLineSpacingPolicy.IsTerminalDirectSwitchSectionBreak(statements[statements.Count - 1]))
        {
            trailingBreak = statements[statements.Count - 1];
        }

        var statementsForBlock = trailingBreak != null
                                     ? statements.Count - 1
                                     : statements.Count;

        var blockStatements = new List<StatementSyntax>(statementsForBlock);

        for (var statementIndex = 0; statementIndex < statementsForBlock; statementIndex++)
        {
            var stmt = statements[statementIndex];

            if (statementIndex == 0)
            {
                stmt = stmt.WithLeadingTrivia(BuildLeadingTrivia(stmt.GetLeadingTrivia(), eolTrivia));
            }

            if (statementIndex == statementsForBlock - 1)
            {
                var lastToken = stmt.GetLastToken();
                stmt = stmt.ReplaceToken(lastToken, lastToken.WithTrailingTrivia(BuildTrailingTrivia(lastToken.TrailingTrivia, eolTrivia)));
            }

            blockStatements.Add(stmt);
        }

        var openBrace = SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                                     .WithLeadingTrivia(eolTrivia)
                                     .WithTrailingTrivia(SyntaxFactory.TriviaList());

        var closeBraceTrailing = trailingBreak != null
                                     ? SyntaxFactory.TriviaList()
                                     : SyntaxFactory.TriviaList(eolTrivia);

        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                                      .WithLeadingTrivia(SyntaxFactory.TriviaList())
                                      .WithTrailingTrivia(closeBraceTrailing);

        var block = SyntaxFactory.Block(openBrace,
                                        SyntaxFactory.List(blockStatements),
                                        closeBrace);

        var sectionStatements = new List<StatementSyntax>
                                {
                                    block
                                };

        if (trailingBreak != null)
        {
            trailingBreak = trailingBreak.WithLeadingTrivia(BuildTerminalBreakLeadingTrivia(trailingBreak.GetLeadingTrivia(), eolTrivia));

            var lastBreakToken = trailingBreak.GetLastToken();
            trailingBreak = trailingBreak.ReplaceToken(lastBreakToken, lastBreakToken.WithTrailingTrivia(BuildTrailingTrivia(lastBreakToken.TrailingTrivia, eolTrivia)));

            sectionStatements.Add(trailingBreak);
        }

        return section.WithStatements(SyntaxFactory.List(sectionStatements));
    }

    /// <summary>
    /// Builds the rewritten sections for a switch statement, adding or removing braces as appropriate
    /// </summary>
    /// <param name="sections">The original switch sections</param>
    /// <param name="anyMultiLine">Whether any non-fall-through section in the switch is multi-line</param>
    /// <returns>The rewritten sections</returns>
    private List<SwitchSectionSyntax> BuildSections(SyntaxList<SwitchSectionSyntax> sections, bool anyMultiLine)
    {
        var newSections = new List<SwitchSectionSyntax>(sections.Count);

        foreach (var section in sections)
        {
            newSections.Add(BuildSection(section, anyMultiLine));
        }

        return newSections;
    }

    /// <summary>
    /// Builds the rewritten form of a single switch section
    /// </summary>
    /// <param name="section">The original switch section</param>
    /// <param name="anyMultiLine">Whether any non-fall-through section in the switch is multi-line</param>
    /// <returns>The rewritten section</returns>
    private SwitchSectionSyntax BuildSection(SwitchSectionSyntax section, bool anyMultiLine)
    {
        if (IsFallThroughSection(section))
        {
            return section;
        }

        if (anyMultiLine == false)
        {
            return RemoveBraces(section);
        }

        return IsEmptySection(section) ? section : AddBraces(section);
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode VisitSwitchStatement(SwitchStatementSyntax node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        node = (SwitchStatementSyntax)base.VisitSwitchStatement(node);

        if (node == null)
        {
            return null;
        }

        var sections = node.Sections;

        if (sections.Count == 0)
        {
            return node;
        }

        var anyMultiLine = HasAnyMultiLineSection(sections);

        // A goto into another section, or a local shared between sections, ties the sections
        // together: braces would hide the target or the local, so the whole switch stays as written
        if (anyMultiLine
            && (HasCrossSectionGotoTarget(node)
                || HasCrossSectionLocalUse(node)))
        {
            return node;
        }

        var newSections = BuildSections(sections, anyMultiLine);

        return node.WithSections(SyntaxFactory.List(newSections));
    }

    #endregion // CSharpSyntaxVisitor
}