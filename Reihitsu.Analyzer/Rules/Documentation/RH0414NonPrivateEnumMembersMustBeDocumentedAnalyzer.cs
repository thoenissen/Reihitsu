using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0414: Enum members in non-private enums must be documented.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer : DiagnosticAnalyzerBase<RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID.
    /// </summary>
    public const string DiagnosticId = "RH0414";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer"/> class.
    /// </summary>
    public RH0414NonPrivateEnumMembersMustBeDocumentedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0414Title), nameof(AnalyzerResources.RH0414MessageFormat))
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
            || DocumentationAnalysisUtilities.MatchesAccessibilityGroup(declaration, DocumentationAccessibilityGroup.NonPrivate) == false
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