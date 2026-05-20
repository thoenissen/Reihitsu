using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0816: Empty structs should use semicolon declarations
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer : EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase<RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0816";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0816EmptyStructsShouldUseSemicolonDeclarationsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0816Title), nameof(AnalyzerResources.RH0816MessageFormat), SyntaxKind.StructDeclaration, LanguageVersion.CSharp12)
    {
    }

    #endregion // Constructor
}