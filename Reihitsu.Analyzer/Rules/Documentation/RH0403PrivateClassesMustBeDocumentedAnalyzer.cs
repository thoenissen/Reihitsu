using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0403: Private classes must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0403PrivateClassesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0403PrivateClassesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0403";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0403PrivateClassesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0403Title), nameof(AnalyzerResources.RH0403MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.ClassDeclaration)
    {
    }

    #endregion // Constructor
}