using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8006: Private interfaces must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8006PrivateInterfacesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8006PrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8006";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8006PrivateInterfacesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8006Title), nameof(AnalyzerResources.RH8006MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.InterfaceDeclaration)
    {
    }

    #endregion // Constructor
}