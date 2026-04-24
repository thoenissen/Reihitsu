using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0336: Semicolons must be spaced correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0336SemicolonsMustBeSpacedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0336SemicolonsMustBeSpacedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0336";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0336SemicolonsMustBeSpacedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0336Title), nameof(AnalyzerResources.RH0336MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyzes the syntax tree.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var token in root.DescendantTokens().Where(currentToken => currentToken.IsKind(SyntaxKind.SemicolonToken)))
        {
            var start = token.SpanStart;
            var end = start;

            while (start > 0
                   && (sourceText[start - 1] == ' ' || sourceText[start - 1] == '\t'))
            {
                start--;
            }

            if (start < end)
            {
                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, TextSpan.FromBounds(start, end))));
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