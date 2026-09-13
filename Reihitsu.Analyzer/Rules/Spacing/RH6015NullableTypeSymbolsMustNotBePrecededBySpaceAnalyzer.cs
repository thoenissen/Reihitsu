using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6015: Nullable type symbols must not be preceded by space
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6015";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6015NullableTypeSymbolsMustNotBePrecededBySpaceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6015Title), nameof(AnalyzerResources.RH6015MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes a nullable type
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        // Documentation-comment cref syntax (e.g. <see cref="M(int?)"/>) parses a nullable parameter type into
        // the same NullableTypeSyntax shape as ordinary code, so it must be excluded explicitly: the formatter
        // never rewrites inside a cref, and this rule must not report a diagnostic there either.
        if (context.Node.IsPartOfStructuredTrivia())
        {
            return;
        }

        var tokenStart = ((NullableTypeSyntax)context.Node).QuestionToken.SpanStart;
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);

        if (SameLinePrecedingWhitespaceAnalysis.GetSpan(sourceText, tokenStart) is { } whitespaceSpan)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, whitespaceSpan)));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.NullableType);
    }

    #endregion // DiagnosticAnalyzer
}