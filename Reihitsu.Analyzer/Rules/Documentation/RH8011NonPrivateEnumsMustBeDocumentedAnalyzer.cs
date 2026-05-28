using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8011: Non-private enums must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8011NonPrivateEnumsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8011NonPrivateEnumsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8011";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8011NonPrivateEnumsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8011Title), nameof(AnalyzerResources.RH8011MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.EnumDeclaration)
    {
    }

    #endregion // Constructor
}