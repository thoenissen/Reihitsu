using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0425: Non-private indexers must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0425NonPrivateIndexersMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0425NonPrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0425";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0425NonPrivateIndexersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0425Title), nameof(AnalyzerResources.RH0425MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor
}