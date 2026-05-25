using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class for analyzers that validate region descriptions
/// </summary>
/// <typeparam name="TAnalyzer">Type of the analyzer</typeparam>
public abstract class RegionDescriptionAnalyzerBase<TAnalyzer> : DiagnosticAnalyzerBase<TAnalyzer>
    where TAnalyzer : DiagnosticAnalyzer
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="category">Category</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format</param>
    private protected RegionDescriptionAnalyzerBase(string diagnosticId, DiagnosticCategory category, string titleResourceName, string messageFormatResourceName)
        : base(diagnosticId, category, titleResourceName, messageFormatResourceName)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the specified description violates the rule
    /// </summary>
    /// <param name="description">Description to inspect</param>
    /// <returns><see langword="true"/> if the description violates the rule</returns>
    protected abstract bool IsInvalidDescription(string description);

    /// <summary>
    /// Analyzes endregion directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEndRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is EndRegionDirectiveTriviaSyntax node
            && IsInvalidDescription(RegionDirectiveUtilities.GetEndRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    /// <summary>
    /// Analyzes region directives
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRegion(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is RegionDirectiveTriviaSyntax node
            && IsInvalidDescription(RegionDirectiveUtilities.GetRegionDescription(node)))
        {
            context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnRegion, SyntaxKind.RegionDirectiveTrivia);
        context.RegisterSyntaxNodeAction(OnEndRegion, SyntaxKind.EndRegionDirectiveTrivia);
    }

    #endregion // DiagnosticAnalyzer
}