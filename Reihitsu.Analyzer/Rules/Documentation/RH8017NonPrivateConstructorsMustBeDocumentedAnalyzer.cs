using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8017: Non-private constructors must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8017";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8017NonPrivateConstructorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8017Title), nameof(AnalyzerResources.RH8017MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.ConstructorDeclaration)
    {
    }

    #endregion // Constructor
}