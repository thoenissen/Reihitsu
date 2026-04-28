using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0413: Private enums must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0413PrivateEnumsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0413PrivateEnumsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0413";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0413PrivateEnumsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0413Title), nameof(AnalyzerResources.RH0413MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.EnumDeclaration)
    {
    }

    #endregion // Constructor
}