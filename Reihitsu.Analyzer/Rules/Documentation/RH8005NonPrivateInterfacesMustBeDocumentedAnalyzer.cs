using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8005: Non-private interfaces must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8005";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8005NonPrivateInterfacesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8005Title), nameof(AnalyzerResources.RH8005MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.InterfaceDeclaration)
    {
    }

    #endregion // Constructor
}