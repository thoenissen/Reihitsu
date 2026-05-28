using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8020: Non-private methods must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8020NonPrivateMethodsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8020NonPrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8020";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8020NonPrivateMethodsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8020Title), nameof(AnalyzerResources.RH8020MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.MethodDeclaration)
    {
    }

    #endregion // Constructor
}