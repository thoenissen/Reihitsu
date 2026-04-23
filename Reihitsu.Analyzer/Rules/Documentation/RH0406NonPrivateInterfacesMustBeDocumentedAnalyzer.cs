using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0406: Non-private interfaces must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0406";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0406NonPrivateInterfacesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0406Title), nameof(AnalyzerResources.RH0406MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.InterfaceDeclaration)
    {
    }

    #endregion // Constructor
}