using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5409: Final enum member must not have trailing comma
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5409";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5409FinalEnumMemberMustNotHaveTrailingCommaAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5409Title), nameof(AnalyzerResources.RH5409MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes enum declarations
    /// </summary>
    /// <param name="context">Context</param>
    private void OnEnumDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not EnumDeclarationSyntax enumDeclaration)
        {
            return;
        }

        var membersAndSeparators = enumDeclaration.Members.GetWithSeparators();

        if (membersAndSeparators.Count == 0)
        {
            return;
        }

        var lastItem = membersAndSeparators[membersAndSeparators.Count - 1];

        if (lastItem.IsToken == false || lastItem.AsToken().IsKind(SyntaxKind.CommaToken) == false)
        {
            return;
        }

        var lastSeparator = lastItem.AsToken();
        var gap = TextSpan.FromBounds(lastSeparator.Span.End, enumDeclaration.CloseBraceToken.SpanStart);

        if (SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary(enumDeclaration, gap))
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(lastSeparator.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnEnumDeclaration, SyntaxKind.EnumDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}