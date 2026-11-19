using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3209: Get-only properties should use an expression body
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer : GetOnlyMemberExpressionBodyAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3209";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3209GetOnlyPropertiesShouldUseAnExpressionBodyAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH3209Title), nameof(AnalyzerResources.RH3209MessageFormat), SyntaxKind.PropertyDeclaration)
    {
    }

    #endregion // Constructor
}