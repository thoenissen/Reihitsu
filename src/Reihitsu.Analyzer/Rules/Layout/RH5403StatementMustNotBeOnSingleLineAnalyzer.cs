using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5403: Statement must not be on a single line
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5403StatementMustNotBeOnSingleLineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5403";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5403StatementMustNotBeOnSingleLineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a block. A block inside an interpolation that cannot hold a line break is not reported, because its
    /// statements cannot be placed on their own lines there (RH5403-004)
    /// </summary>
    /// <param name="context">Context</param>
    private void OnBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block)
        {
            return;
        }

        if (StatementBlockParentPolicy.IsCovered(block) == false
            || StringInterpolationUtilities.IsInsideSingleLineInterpolation(block))
        {
            return;
        }

        if (block.Statements.Count > 0
            && block.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line == block.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line)
        {
            context.ReportDiagnostic(CreateDiagnostic(block.OpenBraceToken.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnBlock, SyntaxKind.Block);
    }

    #endregion // DiagnosticAnalyzer
}