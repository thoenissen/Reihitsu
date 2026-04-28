using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0417: Private delegates must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0417PrivateDelegatesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0417PrivateDelegatesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0417";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0417PrivateDelegatesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0417Title), nameof(AnalyzerResources.RH0417MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.DelegateDeclaration)
    {
    }

    #endregion // Constructor
}