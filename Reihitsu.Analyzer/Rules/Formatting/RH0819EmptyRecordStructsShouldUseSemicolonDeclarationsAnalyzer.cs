using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0819: Empty record structs should use semicolon declarations
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0819EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer : EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase<RH0819EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0819";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0819EmptyRecordStructsShouldUseSemicolonDeclarationsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0819Title), nameof(AnalyzerResources.RH0819MessageFormat), SyntaxKind.RecordStructDeclaration, LanguageVersion.CSharp10)
    {
    }

    #endregion // Constructor
}