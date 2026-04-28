using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0416: Non-private delegates must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0416";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0416NonPrivateDelegatesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0416Title), nameof(AnalyzerResources.RH0416MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.DelegateDeclaration)
    {
    }

    #endregion // Constructor
}