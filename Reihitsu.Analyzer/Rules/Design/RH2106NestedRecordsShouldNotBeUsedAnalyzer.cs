using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2106: Nested records should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2106NestedRecordsShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase<RH2106NestedRecordsShouldNotBeUsedAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2106";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2106NestedRecordsShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2106Title), nameof(AnalyzerResources.RH2106MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.RecordDeclaration"/> and <see cref="SyntaxKind.RecordStructDeclaration"/> nodes
    /// </summary>
    /// <param name="context">Context</param>
    private void OnRecordDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not RecordDeclarationSyntax recordDeclaration || NestedTypeAnalyzerHelper.IsNestedType(recordDeclaration) is false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(NestedTypeAnalyzerHelper.GetIdentifierLocation(recordDeclaration)));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnRecordDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}