using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0409: Private records must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0409PrivateRecordsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0409PrivateRecordsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0409";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0409PrivateRecordsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0409Title), nameof(AnalyzerResources.RH0409MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.RecordDeclaration)
    {
    }

    #endregion // Constructor
}