using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0419: Private constructors must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0419PrivateConstructorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0419PrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0419";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0419PrivateConstructorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0419Title), nameof(AnalyzerResources.RH0419MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.ConstructorDeclaration)
    {
    }

    #endregion // Constructor
}