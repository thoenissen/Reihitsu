using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8001: Non-private classes must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8001NonPrivateClassesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8001NonPrivateClassesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8001";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8001NonPrivateClassesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8001Title), nameof(AnalyzerResources.RH8001MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.ClassDeclaration)
    {
    }

    #endregion // Constructor
}