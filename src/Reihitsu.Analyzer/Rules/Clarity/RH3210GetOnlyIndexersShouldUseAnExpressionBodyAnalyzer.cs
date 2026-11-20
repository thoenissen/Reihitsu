using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3210: Get-only indexers should use an expression body
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer : GetOnlyMemberExpressionBodyAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3210";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3210GetOnlyIndexersShouldUseAnExpressionBodyAnalyzer()
        : base(DiagnosticId, SyntaxKind.IndexerDeclaration)
    {
    }

    #endregion // Constructor
}