using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8010: Private record structs must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8010PrivateRecordStructsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8010PrivateRecordStructsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8010";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8010PrivateRecordStructsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8010Title), nameof(AnalyzerResources.RH8010MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.RecordStructDeclaration)
    {
    }

    #endregion // Constructor
}