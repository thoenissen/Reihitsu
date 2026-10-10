using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8034: Operators must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8034OperatorsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8034";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8034OperatorsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8034Title), nameof(AnalyzerResources.RH8034MessageFormat), DocumentationAccessibilityGroup.Any, SyntaxKind.OperatorDeclaration, SyntaxKind.ConversionOperatorDeclaration)
    {
    }

    #endregion // Constructor
}