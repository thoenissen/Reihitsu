using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2110: Nested unions should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2110NestedUnionsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2110";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2110NestedUnionsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2110Title), nameof(AnalyzerResources.RH2110MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.UnionDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnUnionDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not UnionDeclarationSyntax unionDeclaration || NestedTypeAnalyzerHelper.IsNestedType(unionDeclaration) is false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(NestedTypeAnalyzerHelper.GetIdentifierLocation(unionDeclaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnUnionDeclaration, SyntaxKind.UnionDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}