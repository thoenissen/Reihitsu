using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0815: Empty classes should use semicolon declarations
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer : EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase<RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0815";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0815EmptyClassesShouldUseSemicolonDeclarationsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0815Title), nameof(AnalyzerResources.RH0815MessageFormat), SyntaxKind.ClassDeclaration, LanguageVersion.CSharp12)
    {
    }

    #endregion // Constructor
}