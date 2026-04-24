using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0446: Do not use placeholder elements.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0446DoNotUsePlaceholderElementsAnalyzer : DiagnosticAnalyzerBase<RH0446DoNotUsePlaceholderElementsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0446";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0446DoNotUsePlaceholderElementsAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0446Title), nameof(AnalyzerResources.RH0446MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze an XML element node.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnXmlElement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not XmlNodeSyntax xmlNode
            || string.Equals(DocumentationAnalysisUtilities.GetTagName(xmlNode), "placeholder", StringComparison.OrdinalIgnoreCase) == false)
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

        context.RegisterSyntaxNodeAction(OnXmlElement, SyntaxKind.XmlElement, SyntaxKind.XmlEmptyElement);
    }

    #endregion // DiagnosticAnalyzer
}