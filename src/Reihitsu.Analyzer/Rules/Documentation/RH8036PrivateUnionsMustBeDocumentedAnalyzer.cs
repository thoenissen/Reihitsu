using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8036: Private unions must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8036PrivateUnionsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8036";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8036PrivateUnionsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8036Title), nameof(AnalyzerResources.RH8036MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.UnionDeclaration)
    {
    }

    #endregion // Constructor
}