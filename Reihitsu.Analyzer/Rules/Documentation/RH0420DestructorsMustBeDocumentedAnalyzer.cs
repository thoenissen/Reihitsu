using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0420: Destructors must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0420DestructorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0420DestructorsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0420";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0420DestructorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0420Title), nameof(AnalyzerResources.RH0420MessageFormat), DocumentationAccessibilityGroup.Any, SyntaxKind.DestructorDeclaration)
    {
    }

    #endregion // Constructor
}