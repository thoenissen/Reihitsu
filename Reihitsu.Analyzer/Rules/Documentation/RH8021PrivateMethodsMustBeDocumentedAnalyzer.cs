using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8021: Private methods must be documented
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8021PrivateMethodsMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH8021PrivateMethodsMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8021";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8021PrivateMethodsMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH8021Title), nameof(AnalyzerResources.RH8021MessageFormat), DocumentationAccessibilityGroup.Private, SyntaxKind.MethodDeclaration)
    {
    }

    #endregion // Constructor
}