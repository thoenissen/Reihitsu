using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0411: Private record structs must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0411PrivateRecordStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0411PrivateRecordStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0411";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0411PrivateRecordStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0411Title), nameof(AnalyzerResources.RH0411MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.RecordStructDeclaration)
    {
    }

    #endregion // Constructor
}