using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0426: Private indexers must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0426PrivateIndexersMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0426PrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0426";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0426PrivateIndexersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0426Title), nameof(AnalyzerResources.RH0426MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor
}