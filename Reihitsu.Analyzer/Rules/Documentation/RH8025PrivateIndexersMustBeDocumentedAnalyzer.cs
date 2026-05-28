using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8025: Private indexers must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8025PrivateIndexersMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8025PrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8025";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8025PrivateIndexersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8025Title), nameof(AnalyzerResources.RH8025MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor
}