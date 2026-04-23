using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0418: Non-private constructors must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0418";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0418NonPrivateConstructorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0418Title), nameof(AnalyzerResources.RH0418MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.ConstructorDeclaration)
    {
    }

    #endregion // Constructor
}