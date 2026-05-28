using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8003: Non-private structs must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8003NonPrivateStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8003NonPrivateStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8003";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8003NonPrivateStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8003Title), nameof(AnalyzerResources.RH8003MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.StructDeclaration)
    {
    }

    #endregion // Constructor
}