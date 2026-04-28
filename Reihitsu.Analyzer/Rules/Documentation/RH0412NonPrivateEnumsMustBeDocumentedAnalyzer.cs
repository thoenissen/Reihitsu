using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0412: Non-private enums must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0412NonPrivateEnumsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0412NonPrivateEnumsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0412";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0412NonPrivateEnumsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0412Title), nameof(AnalyzerResources.RH0412MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.EnumDeclaration)
    {
    }

    #endregion // Constructor
}