using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0408: Non-private records must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0408NonPrivateRecordsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0408NonPrivateRecordsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0408";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0408NonPrivateRecordsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0408Title), nameof(AnalyzerResources.RH0408MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.RecordDeclaration)
    {
    }

    #endregion // Constructor
}