using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8204: Do not use placeholder elements
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8204DoNotUsePlaceholderElementsAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8204";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8204DoNotUsePlaceholderElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8204Title), nameof(AnalyzerResources.RH8204MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze an XML element node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnXmlElement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not XmlNodeSyntax xmlNode
            || string.Equals(XmlDocumentationElementOrderingUtilities.GetTagName(xmlNode), "placeholder", StringComparison.OrdinalIgnoreCase) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(xmlNode.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnXmlElement, SyntaxKind.XmlElement, SyntaxKind.XmlEmptyElement);
    }

    #endregion // DiagnosticAnalyzer
}