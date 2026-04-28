using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0404: Non-private structs must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0404NonPrivateStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0404NonPrivateStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0404";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0404NonPrivateStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0404Title), nameof(AnalyzerResources.RH0404MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.StructDeclaration)
    {
    }

    #endregion // Constructor
}