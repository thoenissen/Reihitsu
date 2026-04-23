using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0407: Private interfaces must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0407PrivateInterfacesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0407PrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0407";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0407PrivateInterfacesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0407Title), nameof(AnalyzerResources.RH0407MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.InterfaceDeclaration)
    {
    }

    #endregion // Constructor
}