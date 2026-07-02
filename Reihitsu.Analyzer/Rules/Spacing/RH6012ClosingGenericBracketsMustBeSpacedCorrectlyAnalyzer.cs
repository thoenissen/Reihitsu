using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6012: Closing generic brackets must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6012";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6012ClosingGenericBracketsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6012Title), nameof(AnalyzerResources.RH6012MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the start position of the closing generic bracket for type argument and type parameter lists
    /// </summary>
    /// <param name="node">Node to inspect</param>
    /// <returns>The span start of the <c>&gt;</c> token, or <c>-1</c> if the node is not a generic list</returns>
    private static int GetGreaterThanTokenStart(SyntaxNode node)
    {
        return node switch
               {
                   TypeArgumentListSyntax typeArgumentList => typeArgumentList.GreaterThanToken.SpanStart,
                   TypeParameterListSyntax typeParameterList => typeParameterList.GreaterThanToken.SpanStart,
                   _ => -1
               };
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var tokenStart in root.DescendantNodes()
                                       .Select(GetGreaterThanTokenStart)
                                       .Where(spanStart => spanStart >= 0))
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