using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8023: Private properties must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8023PrivatePropertiesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8023PrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8023";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8023PrivatePropertiesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8023Title), nameof(AnalyzerResources.RH8023MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.PropertyDeclaration)
    {
    }

    #endregion // Constructor
}