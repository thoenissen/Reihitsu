using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0410: Non-private record structs must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0410";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0410NonPrivateRecordStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0410Title), nameof(AnalyzerResources.RH0410MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.RecordStructDeclaration)
    {
    }

    #endregion // Constructor
}