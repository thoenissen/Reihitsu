using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0428: Private fields must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0428PrivateFieldsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0428PrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0428";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0428PrivateFieldsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0428Title), nameof(AnalyzerResources.RH0428MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.FieldDeclaration)
    {
    }

    #endregion // Constructor
}