using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0415: Enum members in private enums must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0415PrivateEnumMembersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0415PrivateEnumMembersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID.
    /// </summary>
    public const string DiagnosticId = "RH0415";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RH0415PrivateEnumMembersMustBeDocumentedAnalyzer"/> class.
    /// </summary>
    public RH0415PrivateEnumMembersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0415Title), nameof(AnalyzerResources.RH0415MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes an enum member declaration.
    /// </summary>
    /// <param name="context">Analysis context.</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not EnumMemberDeclarationSyntax declaration
            || DocumentationAnalysisUtilities.MatchesAccessibilityGroup(declaration, DocumentationAccessibilityGroup.Private) == false
            || DocumentationAnalysisUtilities.HasRequiredDocumentation(declaration, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(DocumentationAnalysisUtilities.GetDiagnosticLocation(declaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDeclaration, SyntaxKind.EnumMemberDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}