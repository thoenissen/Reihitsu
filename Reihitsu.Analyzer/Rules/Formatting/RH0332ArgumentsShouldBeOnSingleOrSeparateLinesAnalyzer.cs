using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0332: Arguments should either all be on one line or each on its own line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer : DiagnosticAnalyzerBase<RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0332";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0332ArgumentsShouldBeOnSingleOrSeparateLinesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0332Title), nameof(AnalyzerResources.RH0332MessageFormat))
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

        if (argumentList.Arguments.Count < 2)
        {
            return;
        }

        var openParenLine = argumentList.OpenParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        var closeParenLine = argumentList.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openParenLine == closeParenLine)
        {
            return;
        }

        var startLines = new HashSet<int>();

        foreach (var argument in argumentList.Arguments)
        {
            var argumentStartLine = argument.GetLocation().GetLineSpan().StartLinePosition.Line;

            if (startLines.Add(argumentStartLine) == false)
            {
                context.ReportDiagnostic(CreateDiagnostic(argumentList.GetLocation()));

                return;
            }
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