using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8018: Private constructors must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8018PrivateConstructorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8018PrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8018";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8018PrivateConstructorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8018Title), nameof(AnalyzerResources.RH8018MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.ConstructorDeclaration)
    {
    }

    #endregion // Constructor
}