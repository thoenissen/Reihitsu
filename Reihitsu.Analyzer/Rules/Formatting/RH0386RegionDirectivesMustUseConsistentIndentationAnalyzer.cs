using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0386: Region directives must use consistent indentation with containing code
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer : DiagnosticAnalyzerBase<RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0386";

    /// <summary>
    /// Indentation size used for region directives
    /// </summary>
    private const int IndentationSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0386RegionDirectivesMustUseConsistentIndentationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0386Title), nameof(AnalyzerResources.RH0386MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Computes the indentation level to use for the given child
    /// </summary>
    /// <param name="child">Child</param>
    /// <param name="indentLevel">Current indentation level</param>
    /// <param name="braceRange">Current brace range</param>
    /// <param name="isSwitchSection">Is switch section</param>
    /// <returns>Indentation level</returns>
    private static int GetChildIndentLevel(SyntaxNodeOrToken child, int indentLevel, (int OpenEnd, int CloseStart)? braceRange, bool isSwitchSection)
    {
        var childIndent = indentLevel;

        if (IsInsideBraceRange(child.SpanStart, braceRange))
        {
            childIndent = indentLevel + 1;
        }

        if (isSwitchSection && child.IsNode && child.AsNode() is StatementSyntax)
        {
            childIndent = indentLevel + 1;
        }

        return childIndent;
    }

    /// <summary>
    /// Gets the brace range for nodes that indent their contents
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
    /// Determines whether the given trivia is a supported region directive
    /// </summary>
    /// <param name="trivia">Trivia</param>
    /// <returns><see langword="true"/> if supported</returns>
    private static bool IsRegionDirective(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.RegionDirectiveTrivia)
               || trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia);
    }

    /// <summary>
    /// Determines whether the given position lies inside the provided brace range
    /// </summary>
    /// <param name="spanStart">Span start</param>
    /// <param name="braceRange">Brace range</param>
    /// <returns><see langword="true"/> if inside the range</returns>
    private static bool IsInsideBraceRange(int spanStart, (int OpenEnd, int CloseStart)? braceRange)
    {
        if (braceRange == null)
        {
            return false;
        }

        var (openEnd, closeStart) = braceRange.Value;

        return spanStart >= openEnd && spanStart < closeStart;
    }

    /// <summary>
    /// Analyzes directive indentation for the given token
    /// </summary>
    /// <param name="token">Token</param>
    /// <param name="indentLevel">Current indentation level</param>
    /// <param name="braceRange">Current brace range</param>
    /// <param name="context">Context</param>
    private void AnalyzeDirectiveIndentation(SyntaxToken token, int indentLevel, (int OpenEnd, int CloseStart)? braceRange, SyntaxTreeAnalysisContext context)
    {
        foreach (var directiveTrivia in token.LeadingTrivia.Where(IsRegionDirective))
        {
            if (RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia))
            {
                continue;
            }

            var expectedIndent = IsInsideBraceRange(directiveTrivia.SpanStart, braceRange)
                                     ? indentLevel + 1
                                     : indentLevel;
            var expectedColumn = expectedIndent * IndentationSize;
            var actualColumn = directiveTrivia.GetLocation().GetLineSpan().StartLinePosition.Character;

            if (actualColumn != expectedColumn)
            {
                context.ReportDiagnostic(CreateDiagnostic(directiveTrivia.GetLocation()));
            }
        }
    }

    /// <summary>
    /// Analyzes a node and its descendants for incorrectly indented region directives
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="indentLevel">Current indentation level</param>
    /// <param name="context">Context</param>
    private void AnalyzeNode(SyntaxNode node, int indentLevel, SyntaxTreeAnalysisContext context)
    {
        var braceRange = GetIndentingBraceRange(node);
        var isSwitchSection = node is SwitchSectionSyntax;

        foreach (var child in node.ChildNodesAndTokens())
        {
            var childIndent = GetChildIndentLevel(child, indentLevel, braceRange, isSwitchSection);

            if (child.IsToken)
            {
                AnalyzeDirectiveIndentation(child.AsToken(), indentLevel, braceRange, context);
            }
            else if (child.AsNode() is { } childNode)
            {
                AnalyzeNode(childNode, childIndent, context);
            }
        }
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);

        AnalyzeNode(root, 0, context);
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