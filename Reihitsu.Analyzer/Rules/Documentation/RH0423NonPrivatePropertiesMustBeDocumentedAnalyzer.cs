using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0423: Non-private properties must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer : SplitElementDocumentationAnalyzerBase<RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0423";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0423NonPrivatePropertiesMustBeDocumentedAnalyzer()
        : base(DiagnosticId, nameof(AnalyzerResources.RH0423Title), nameof(AnalyzerResources.RH0423MessageFormat), DocumentationAccessibilityGroup.NonPrivate, SyntaxKind.PropertyDeclaration)
    {
    }

    #endregion // Constructor
}