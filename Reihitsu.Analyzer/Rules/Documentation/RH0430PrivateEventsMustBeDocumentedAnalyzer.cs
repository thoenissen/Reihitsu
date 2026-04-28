using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0430: Private events must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0430PrivateEventsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0430PrivateEventsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0430";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0430PrivateEventsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0430Title), nameof(AnalyzerResources.RH0430MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.EventDeclaration, SyntaxKind.EventFieldDeclaration)
    {
    }

    #endregion // Constructor
}