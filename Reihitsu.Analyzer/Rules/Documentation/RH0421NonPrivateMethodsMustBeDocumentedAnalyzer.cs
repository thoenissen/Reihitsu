using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0421: Non-private methods must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0421NonPrivateMethodsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0421NonPrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0421";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0421NonPrivateMethodsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0421Title), nameof(AnalyzerResources.RH0421MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.MethodDeclaration)
    {
    }

    #endregion // Constructor
}