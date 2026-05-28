using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8016: Private delegates must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8016PrivateDelegatesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8016PrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8016";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8016PrivateDelegatesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8016Title), nameof(AnalyzerResources.RH8016MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.DelegateDeclaration)
    {
    }

    #endregion // Constructor
}