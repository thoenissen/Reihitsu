using System.Collections.Generic;

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

        // A conditional directive in a gap of the list lets another build configuration compile other arguments
        // (RH5102-004, SHARED-DIAG-008)
        if (argumentList.Arguments.Count < 2
            || SyntaxTriviaUtilities.ContainsConditionalCompilationBoundaryInElementGaps(argumentList.Arguments.GetWithSeparators(), argumentList.CloseParenToken))
        {
            return;
        }

        // The arguments alone decide whether the list is multi-line: they start on different lines, or one of them
        // spans lines. A line break behind the opening or in front of the closing parenthesis does not (RH5102-001)
        var startLines = new HashSet<int>();
        var isMultiLine = false;
        var sharesLine = false;

        foreach (var argument in argumentList.Arguments)
        {
            var argumentLineSpan = argument.GetLocation().GetLineSpan();

            isMultiLine |= argumentLineSpan.StartLinePosition.Line != argumentLineSpan.EndLinePosition.Line;
            sharesLine |= startLines.Add(argumentLineSpan.StartLinePosition.Line) == false;
        }

        if ((isMultiLine || startLines.Count > 1)
            && sharesLine)
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