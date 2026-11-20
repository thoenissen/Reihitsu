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
public class RH5307IndexerBracketedArgumentsShouldBeSingleLinedAnalyzer : DiagnosticAnalyzerBase
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
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the list belongs to indexer/element access syntax. The code fix asks it for the list a diagnostic
    /// is located on, so the analyzer's reporting condition has one statement
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> when the parent is indexer/element access syntax; otherwise, <see langword="false"/></returns>
    internal static bool IsIndexerArgumentList(BracketedArgumentListSyntax bracketedArgumentList)
    {
        return bracketedArgumentList.Parent is ElementAccessExpressionSyntax or ImplicitElementAccessSyntax;
    }

    /// <summary>
    /// Determines whether a bracketed argument list can be safely collapsed to one line
    /// </summary>
    /// <param name="bracketedArgumentList">Bracketed argument list</param>
    /// <returns><see langword="true"/> if collapsing is safe; otherwise, <see langword="false"/></returns>
    /// <remarks>
    /// The interior-scoped check matches the region the code fix and the formatter actually rewrite,
    /// which runs from the opening to the closing bracket. A comment behind the closing bracket lies
    /// outside it and must not suppress the diagnostic. The reported location ends at
    /// the closing bracket as well, so narrowing the guard cannot produce a diagnostic the fix is
    /// unable to clear. The code fix asks this check for the list a diagnostic is located on, so it is
    /// offered for exactly the lists the analyzer reports
    /// </remarks>
    internal static bool CanSafelyCollapseToSingleLine(BracketedArgumentListSyntax bracketedArgumentList)
    {
        return SyntaxNodeUtilities.InteriorContainsCommentOrDirective(bracketedArgumentList) == false
               && SyntaxNodeUtilities.AreAllSingleLine(bracketedArgumentList.Arguments);
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