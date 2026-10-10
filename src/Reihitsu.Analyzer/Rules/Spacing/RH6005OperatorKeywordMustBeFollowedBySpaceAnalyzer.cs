using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6005: Operator keyword must be followed by space
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6005";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6005Title), nameof(AnalyzerResources.RH6005MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the operator keyword token of an operator or conversion operator declaration. Both declaration
    /// kinds are the only node kinds that own an <c>operator</c> keyword outside trivia; an
    /// <c>OperatorMemberCref</c>/<c>ConversionOperatorMemberCref</c> inside a documentation-comment
    /// <c>cref</c> also owns one, but those node kinds are deliberately not registered below, so they never
    /// reach this method
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns>The operator keyword token</returns>
    private static SyntaxToken GetOperatorKeyword(SyntaxNode node)
    {
        return node switch
               {
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.OperatorKeyword,
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.OperatorKeyword,
                   _ => default
               };
    }

    /// <summary>
    /// Analyzes an operator or conversion operator declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var token = GetOperatorKeyword(context.Node);

        if (token.IsKind(SyntaxKind.OperatorKeyword) == false)
        {
            return;
        }

        if (token.TrailingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(token.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.OperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}