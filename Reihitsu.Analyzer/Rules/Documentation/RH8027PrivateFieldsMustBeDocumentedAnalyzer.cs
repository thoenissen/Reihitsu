using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8027: Private fields must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8027PrivateFieldsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8027PrivateFieldsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8027";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8027PrivateFieldsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8027Title), nameof(AnalyzerResources.RH8027MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.FieldDeclaration)
    {
    }

    #endregion // Constructor
}