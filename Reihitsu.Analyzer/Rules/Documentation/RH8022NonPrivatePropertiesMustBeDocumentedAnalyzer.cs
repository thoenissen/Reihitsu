using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8022: Non-private properties must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8022";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8022NonPrivatePropertiesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8022Title), nameof(AnalyzerResources.RH8022MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.PropertyDeclaration)
    {
    }

    #endregion // Constructor
}