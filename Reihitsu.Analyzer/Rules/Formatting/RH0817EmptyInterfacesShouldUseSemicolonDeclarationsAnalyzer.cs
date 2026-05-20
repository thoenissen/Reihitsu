using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0817: Empty interfaces should use semicolon declarations
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0817EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer : EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase<RH0817EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0817";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0817EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0817Title), nameof(AnalyzerResources.RH0817MessageFormat), SyntaxKind.InterfaceDeclaration, LanguageVersion.CSharp12)
    {
    }

    #endregion // Constructor
}