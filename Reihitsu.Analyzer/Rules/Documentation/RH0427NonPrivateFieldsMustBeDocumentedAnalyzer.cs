using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0427: Non-private fields must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0427NonPrivateFieldsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0427NonPrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0427";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0427NonPrivateFieldsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0427Title), nameof(AnalyzerResources.RH0427MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.FieldDeclaration)
    {
    }

    #endregion // Constructor
}