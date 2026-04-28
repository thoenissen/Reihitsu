using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0424: Private properties must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0424PrivatePropertiesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0424PrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0424";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0424PrivatePropertiesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0424Title), nameof(AnalyzerResources.RH0424MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.PropertyDeclaration)
    {
    }

    #endregion // Constructor
}