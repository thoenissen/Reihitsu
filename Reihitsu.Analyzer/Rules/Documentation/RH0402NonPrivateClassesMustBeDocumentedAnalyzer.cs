using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0402: Non-private classes must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0402NonPrivateClassesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0402NonPrivateClassesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0402";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0402NonPrivateClassesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0402Title), nameof(AnalyzerResources.RH0402MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.ClassDeclaration)
    {
    }

    #endregion // Constructor
}