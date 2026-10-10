using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6022: No space after new for implicitly typed arrays
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6022";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6022Title), nameof(AnalyzerResources.RH6022MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes an implicitly typed array creation expression
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var node = (ImplicitArrayCreationExpressionSyntax)context.Node;
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);
        var start = node.NewKeyword.Span.End;
        var end = start;

        while (end < sourceText.Length
               && (sourceText[end] == ' ' || sourceText[end] == '\t'))
        {
            end++;
        }

        if (end > start)
        {
            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Node.SyntaxTree, TextSpan.FromBounds(start, end))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnSyntaxNode, SyntaxKind.ImplicitArrayCreationExpression);
    }

    #endregion // DiagnosticAnalyzer
}