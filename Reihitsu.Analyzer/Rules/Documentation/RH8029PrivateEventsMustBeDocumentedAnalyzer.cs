using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8029: Private events must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8029PrivateEventsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8029PrivateEventsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8029";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8029PrivateEventsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8029Title), nameof(AnalyzerResources.RH8029MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
    {
    }

    #endregion // Constructor
}