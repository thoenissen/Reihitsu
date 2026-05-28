using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8015: Non-private delegates must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8015NonPrivateDelegatesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8015NonPrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8015";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8015NonPrivateDelegatesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8015Title), nameof(AnalyzerResources.RH8015MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.DelegateDeclaration)
    {
    }

    #endregion // Constructor
}