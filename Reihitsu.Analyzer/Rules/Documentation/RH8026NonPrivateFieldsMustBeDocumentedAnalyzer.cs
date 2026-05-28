using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8026: Non-private fields must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8026NonPrivateFieldsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8026NonPrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8026";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8026NonPrivateFieldsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8026Title), nameof(AnalyzerResources.RH8026MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.FieldDeclaration)
    {
    }

    #endregion // Constructor
}