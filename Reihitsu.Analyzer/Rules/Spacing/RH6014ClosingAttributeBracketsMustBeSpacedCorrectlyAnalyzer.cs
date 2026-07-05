using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Spacing;

/// <summary>
/// RH6014: Closing attribute brackets must be spaced correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH6014";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH6014ClosingAttributeBracketsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Spacing, nameof(AnalyzerResources.RH6014Title), nameof(AnalyzerResources.RH6014MessageFormat))
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
                                       .OfType<AttributeListSyntax>()
                                       .Select(node => node.CloseBracketToken.SpanStart))
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