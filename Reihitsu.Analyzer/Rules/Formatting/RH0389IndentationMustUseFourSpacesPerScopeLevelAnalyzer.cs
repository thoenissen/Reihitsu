using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0389: Indentation must use 4 spaces per scope level
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer : DiagnosticAnalyzerBase<RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0389";

    /// <summary>
    /// Indent size
    /// </summary>
    private const int IndentSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0389IndentationMustUseFourSpacesPerScopeLevelAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0389Title), nameof(AnalyzerResources.RH0389MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Aligns comments to the indentation of the token they precede
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <param name="expectedIndentationByLine">Expected indentation by line</param>
    private static void AlignCommentIndentation(SyntaxNode root, Dictionary<int, (int Indentation, Location Location)> expectedIndentationByLine)
    {
        foreach (var token in root.DescendantTokens())
        {
            var tokenLine = GetLine(token);

            if (expectedIndentationByLine.TryGetValue(tokenLine, out var expectation) == false)
            {
                continue;
            }

            var alignColumn = expectation.Indentation;

            if (token.IsKind(SyntaxKind.CloseBraceToken))
            {
                alignColumn += IndentSize;
            }

            foreach (var trivia in token.LeadingTrivia)
            {
                if (IsComment(trivia) == false)
                {
                    continue;
                }

                var commentLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (commentLine != tokenLine)
                {
                    expectedIndentationByLine[commentLine] = (alignColumn, trivia.GetLocation());
                }
            }
        }
    }

    /// <summary>
    /// Builds the expected indentation by line
    /// </summary>
    /// <param name="root">Syntax root</param>
    /// <returns>Expected indentation by line</returns>
    private static Dictionary<int, (int Indentation, Location Location)> BuildExpectedIndentationMap(SyntaxNode root)
    {
        var expectedIndentationByLine = new Dictionary<int, (int Indentation, Location Location)>();

        ComputeBlockIndentation(root, 0, expectedIndentationByLine);
        AlignCommentIndentation(root, expectedIndentationByLine);

        return expectedIndentationByLine;
    }

    /// <summary>
    /// Computes block indentation recursively
    /// </summary>
    /// <param name="node">Current node</param>
    /// <param name="indentLevel">Current indent level</param>
    /// <param name="expectedIndentationByLine">Expected indentation by line</param>
    private static void ComputeBlockIndentation(SyntaxNode node, int indentLevel, Dictionary<int, (int Indentation, Location Location)> expectedIndentationByLine)
    {
        if (node is BlockSyntax { Parent: AnonymousFunctionExpressionSyntax })
        {
            return;
        }

        var braceRange = GetIndentingBraceRange(node);
        var isSwitchSection = node is SwitchSectionSyntax;

        foreach (var child in node.ChildNodesAndTokens())
        {
            var childIndent = GetChildIndentLevel(child, indentLevel, braceRange, isSwitchSection);

            if (child.IsToken)
            {
                var token = child.AsToken();

                SetDirectiveIndentation(token, indentLevel, braceRange, expectedIndentationByLine);
                SetTokenIndentation(token, childIndent, expectedIndentationByLine);

                continue;
            }

            ComputeBlockIndentation(child.AsNode(), childIndent, expectedIndentationByLine);
        }
    }

    /// <summary>
    /// Gets the child indentation level
    /// </summary>
    /// <param name="child">Child node or token</param>
    /// <param name="indentLevel">Current indent level</param>
    /// <param name="braceRange">Brace range</param>
    /// <param name="isSwitchSection"><see langword="true"/> if the parent is a switch section</param>
    /// <returns>The child indentation level</returns>
    private static int GetChildIndentLevel(SyntaxNodeOrToken child, int indentLevel, (int OpenEnd, int CloseStart)? braceRange, bool isSwitchSection)
    {
        var childIndent = indentLevel;

        if (IsInsideBraceRange(child.SpanStart, braceRange))
        {
            childIndent = indentLevel + 1;
        }

        if (isSwitchSection
            && child.IsNode
            && child.AsNode() is StatementSyntax)
        {
            childIndent = indentLevel + 1;
        }

        return childIndent;
    }

    /// <summary>
    /// Gets the 0-based line number of a token
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns>Line number</returns>
    private static int GetLine(SyntaxToken token)
    {
        return token.GetLocation().GetLineSpan().StartLinePosition.Line;
    }

    /// <summary>
    /// Gets the brace range of indenting constructs
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Brace range</returns>
    private static (int OpenEnd, int CloseStart)? GetIndentingBraceRange(SyntaxNode node)
    {
        SyntaxToken openBrace;
        SyntaxToken closeBrace;

        switch (node)
        {
            case NamespaceDeclarationSyntax namespaceDeclaration:
                {
                    openBrace = namespaceDeclaration.OpenBraceToken;
                    closeBrace = namespaceDeclaration.CloseBraceToken;
                }
                break;

            case BaseTypeDeclarationSyntax typeDeclaration:
                {
                    openBrace = typeDeclaration.OpenBraceToken;
                    closeBrace = typeDeclaration.CloseBraceToken;
                }
                break;

            case BlockSyntax block:
                {
                    openBrace = block.OpenBraceToken;
                    closeBrace = block.CloseBraceToken;
                }
                break;

            case SwitchStatementSyntax switchStatement:
                {
                    openBrace = switchStatement.OpenBraceToken;
                    closeBrace = switchStatement.CloseBraceToken;
                }
                break;

            case AccessorListSyntax accessorList:
                {
                    openBrace = accessorList.OpenBraceToken;
                    closeBrace = accessorList.CloseBraceToken;
                }
                break;

            default:
                {
                    return null;
                }
        }

        if (openBrace.IsMissing || closeBrace.IsMissing)
        {
            return null;
        }

        return (openBrace.Span.End, closeBrace.SpanStart);
    }

    /// <summary>
    /// Determines whether a trivia is a comment
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a comment</returns>
    private static bool IsComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
               || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
               || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia);
    }

    /// <summary>
    /// Determines whether the position is within the specified brace range
    /// </summary>
    /// <param name="position">Position</param>
    /// <param name="braceRange">Brace range</param>
    /// <returns><see langword="true"/> if the position is inside the brace range</returns>
    private static bool IsInsideBraceRange(int position, (int OpenEnd, int CloseStart)? braceRange)
    {
        if (braceRange == null)
        {
            return false;
        }

        var (openEnd, closeStart) = braceRange.Value;

        return position >= openEnd && position < closeStart;
    }

    /// <summary>
    /// Determines whether the specified token is the first token on its line
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token starts a line</returns>
    private static bool IsFirstOnLine(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None))
        {
            return false;
        }

        var previousToken = token.GetPreviousToken();

        if (previousToken == default || previousToken.IsKind(SyntaxKind.None))
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
    /// Determines whether the specified trivia is a region directive
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if the trivia is a region directive</returns>
    private static bool IsRegionDirective(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
               || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
    }

    /// <summary>
    /// Determines whether a first-on-line token should participate in scope indentation analysis
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><see langword="true"/> if the token begins a scope-relevant line</returns>
    private static bool ShouldAnalyzeToken(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.OpenBraceToken) || token.IsKind(SyntaxKind.CloseBraceToken))
        {
            return token.Parent != null && GetIndentingBraceRange(token.Parent) != null;
        }

        if (token.Parent == null || token.Parent.GetFirstToken() != token)
        {
            return false;
        }

        return token.Parent is BaseNamespaceDeclarationSyntax
               || token.Parent is MemberDeclarationSyntax
               || token.Parent is EnumMemberDeclarationSyntax
               || token.Parent is StatementSyntax
               || token.Parent is AccessorDeclarationSyntax
               || token.Parent is ElseClauseSyntax
               || token.Parent is CatchClauseSyntax
               || token.Parent is FinallyClauseSyntax
               || token.Parent is SwitchLabelSyntax;
    }

    /// <summary>
    /// Sets the indentation for region directives
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="indentLevel">Current indent level</param>
    /// <param name="braceRange">Brace range</param>
    /// <param name="expectedIndentationByLine">Expected indentation by line</param>
    private static void SetDirectiveIndentation(SyntaxToken token, int indentLevel, (int OpenEnd, int CloseStart)? braceRange, Dictionary<int, (int Indentation, Location Location)> expectedIndentationByLine)
    {
        foreach (var trivia in token.LeadingTrivia)
        {
            if (IsRegionDirective(trivia) == false)
            {
                continue;
            }

            var directiveIndentLevel = IsInsideBraceRange(trivia.SpanStart, braceRange)
                                           ? indentLevel + 1
                                           : indentLevel;
            var directiveLine = trivia.GetLocation().GetLineSpan().StartLinePosition.Line;

            expectedIndentationByLine[directiveLine] = (directiveIndentLevel * IndentSize, trivia.GetLocation());
        }
    }

    /// <summary>
    /// Sets the indentation for first-on-line tokens
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="indentLevel">Indent level</param>
    /// <param name="expectedIndentationByLine">Expected indentation by line</param>
    private static void SetTokenIndentation(SyntaxToken token, int indentLevel, Dictionary<int, (int Indentation, Location Location)> expectedIndentationByLine)
    {
        if (IsFirstOnLine(token)
            && ShouldAnalyzeToken(token))
        {
            expectedIndentationByLine[GetLine(token)] = (indentLevel * IndentSize, token.GetLocation());
        }
    }

    /// <summary>
    /// Tries to read the indentation at the start of a line
    /// </summary>
    /// <param name="lineText">Line text</param>
    /// <param name="indentation">Indentation width</param>
    /// <param name="firstSignificantIndex">Index of the first non-whitespace character</param>
    /// <returns><see langword="true"/> if the line uses only spaces for indentation</returns>
    private static bool TryGetActualIndentation(string lineText, out int indentation, out int firstSignificantIndex)
    {
        indentation = 0;
        firstSignificantIndex = 0;

        while (firstSignificantIndex < lineText.Length)
        {
            var currentCharacter = lineText[firstSignificantIndex];

            if (currentCharacter == '\uFEFF')
            {
                firstSignificantIndex++;

                continue;
            }

            if (currentCharacter == ' ')
            {
                indentation++;
                firstSignificantIndex++;

                continue;
            }

            if (currentCharacter == '\t')
            {
                return false;
            }

            if (char.IsWhiteSpace(currentCharacter))
            {
                indentation++;
                firstSignificantIndex++;

                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var stringLineIndices = FormattingTextAnalysisUtilities.GetStringLineIndices(root, sourceText);
        var expectedIndentationByLine = BuildExpectedIndentationMap(root);

        foreach (var pair in expectedIndentationByLine)
        {
            if (stringLineIndices.Contains(pair.Key))
            {
                continue;
            }

            var line = sourceText.Lines[pair.Key];
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);

            if (string.IsNullOrWhiteSpace(lineText))
            {
                continue;
            }

            if (TryGetActualIndentation(lineText, out var actualIndentation, out var firstSignificantIndex) == false
                || actualIndentation == pair.Value.Indentation)
            {
                continue;
            }

            context.ReportDiagnostic(CreateDiagnostic(pair.Value.Location));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}