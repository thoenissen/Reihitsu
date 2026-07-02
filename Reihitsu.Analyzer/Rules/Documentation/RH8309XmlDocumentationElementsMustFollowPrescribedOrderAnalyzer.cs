using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8309: XML documentation elements must follow a prescribed order
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer : DiagnosticAnalyzerBase<RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8309";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8309XmlDocumentationElementsMustFollowPrescribedOrderAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8309Title), nameof(AnalyzerResources.RH8309MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a documentation comment
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DocumentationCommentTriviaSyntax documentationComment)
        {
            return;
        }

        var highestRank = -1;

        foreach (var node in documentationComment.Content)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (node is not (XmlElementSyntax or XmlEmptyElementSyntax))
            {
                continue;
            }

            var rank = XmlDocumentationElementOrderingUtilities.GetCanonicalElementRank(XmlDocumentationElementOrderingUtilities.GetTagName(node));

            // Unknown or custom elements are tolerated and do not constrain the order
            if (rank == XmlDocumentationElementOrderingUtilities.UnknownElementRank)
            {
                continue;
            }

            if (rank < highestRank)
            {
                context.ReportDiagnostic(CreateDiagnostic(node.GetLocation()));

                return;
            }

            highestRank = rank;
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    #endregion // DiagnosticAnalyzer
}