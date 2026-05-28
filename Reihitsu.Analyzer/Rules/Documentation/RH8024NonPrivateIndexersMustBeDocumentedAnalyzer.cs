using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8024: Non-private indexers must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8024NonPrivateIndexersMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8024NonPrivateIndexersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8024";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8024NonPrivateIndexersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8024Title), nameof(AnalyzerResources.RH8024MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor
}