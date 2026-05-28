using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8028: Non-private events must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8028NonPrivateEventsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8028NonPrivateEventsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8028";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8028NonPrivateEventsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8028Title), nameof(AnalyzerResources.RH8028MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
    {
    }

    #endregion // Constructor
}