using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5414: Empty interfaces should use semicolon declarations
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer : EmptyTypeDeclarationShouldUseSemicolonAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5414";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5414EmptyInterfacesShouldUseSemicolonDeclarationsAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH5414Title), nameof(AnalyzerResources.RH5414MessageFormat), SyntaxKind.InterfaceDeclaration, LanguageVersion.CSharp12)
    {
    }

    #endregion // Constructor
}