using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8035: Non-private unions must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8035NonPrivateUnionsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8035";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8035NonPrivateUnionsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8035Title), nameof(AnalyzerResources.RH8035MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.UnionDeclaration)
    {
    }

    #endregion // Constructor
}