using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0422: Private methods must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0422PrivateMethodsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0422PrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0422";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0422PrivateMethodsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0422Title), nameof(AnalyzerResources.RH0422MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.MethodDeclaration)
    {
    }

    #endregion // Constructor
}