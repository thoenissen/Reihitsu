using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0405: Private structs must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0405PrivateStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0405PrivateStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0405";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0405PrivateStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0405Title), nameof(AnalyzerResources.RH0405MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.StructDeclaration)
    {
    }

    #endregion // Constructor
}