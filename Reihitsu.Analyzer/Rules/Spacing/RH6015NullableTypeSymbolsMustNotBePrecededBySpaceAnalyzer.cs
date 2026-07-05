using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
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
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var tokenStart in root.DescendantNodes()
                                       .OfType<NullableTypeSyntax>()
                                       .Select(node => node.QuestionToken.SpanStart))
        {
            var start = tokenStart;

            while (start > 0
                   && (sourceText[start - 1] == ' ' || sourceText[start - 1] == '\t'))
            {
                start--;
            }

            if (start < tokenStart)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(start, tokenStart))));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}