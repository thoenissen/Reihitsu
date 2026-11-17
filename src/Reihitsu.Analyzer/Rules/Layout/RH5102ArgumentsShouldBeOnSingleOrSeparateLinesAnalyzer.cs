using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5102: Arguments should either all be on one line or each on its own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5102";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5102ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5102Title), nameof(AnalyzerResources.RH5102MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing argument lists to ensure arguments are either all on one line or each on its own line
    /// </summary>
    /// <param name="context">Context</param>
    private void OnArgumentList(SyntaxNodeAnalysisContext context)
    {
        var argumentList = (ArgumentListSyntax)context.Node;

        // The arguments are judged as the analyzed build compiles them; a conditional directive in a gap of the list does
        // not keep it from being reported, and the fix withholds itself there (RH5102-004, SHARED-DIAG-008)
        if (argumentList.Arguments.Count < 2)
        {
            return;
        }

        // The arguments alone decide whether the list is multi-line; a line break behind the opening or in front of the
        // closing parenthesis does not (RH5102-001)
        if (SyntaxNodeUtilities.IsPartlyWrapped(argumentList.Arguments))
        {
            context.ReportDiagnostic(CreateDiagnostic(argumentList.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnArgumentList, SyntaxKind.ArgumentList);
    }

    #endregion // DiagnosticAnalyzer
}