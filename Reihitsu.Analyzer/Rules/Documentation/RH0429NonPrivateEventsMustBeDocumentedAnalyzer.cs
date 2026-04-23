using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0429: Non-private events must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0429NonPrivateEventsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0429NonPrivateEventsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0429";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0429NonPrivateEventsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0429Title), nameof(AnalyzerResources.RH0429MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
    {
    }

    #endregion // Constructor
}