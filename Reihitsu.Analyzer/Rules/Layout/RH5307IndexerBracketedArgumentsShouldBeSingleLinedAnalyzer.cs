using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5307: Indexer bracketed arguments should be single lined
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase<RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5307";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5307Title), nameof(AnalyzerResources.RH5307MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the list belongs to indexer/element access syntax
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> when the parent is indexer/element access syntax; otherwise, <see langword="false"/></returns>
    private static bool IsIndexerArgumentList(BracketedArgumentListSyntax bracketedArgumentList)
    {
        return bracketedArgumentList.Parent is ElementAccessExpressionSyntax or ImplicitElementAccessSyntax;
    }

    /// <summary>
    /// Determines whether a bracketed argument list can be safely collapsed to one line
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    private static bool CanSafelyCollapseToSingleLine(BracketedArgumentListSyntax bracketedArgumentList)
    {
        return FormattingSafetyUtilities.HasCommentsOrDirectives(bracketedArgumentList) == false
               && FormattingSafetyUtilities.AreAllSingleLine(bracketedArgumentList.Arguments);
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.BracketedArgumentList"/> occurrences used by indexer access
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBracketedArgumentList(SyntaxNodeAnalysisContext context)
    {
        var bracketedArgumentList = (BracketedArgumentListSyntax)context.Node;

        if (IsIndexerArgumentList(bracketedArgumentList) == false
            || CanSafelyCollapseToSingleLine(bracketedArgumentList) == false)
        {
            return;
        }

        var openBracketPosition = bracketedArgumentList.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition;
        var closeBracketPosition = bracketedArgumentList.CloseBracketToken.GetLocation().GetLineSpan().StartLinePosition;

        if (openBracketPosition.Line != closeBracketPosition.Line)
        {
            var diagnosticLocation = bracketedArgumentList.Parent is ElementAccessExpressionSyntax elementAccessExpression
                                         ? elementAccessExpression.GetLocation()
                                         : bracketedArgumentList.GetLocation();

            context.ReportDiagnostic(CreateDiagnostic(diagnosticLocation));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBracketedArgumentList, SyntaxKind.BracketedArgumentList);
    }

    #endregion // DiagnosticAnalyzer
}