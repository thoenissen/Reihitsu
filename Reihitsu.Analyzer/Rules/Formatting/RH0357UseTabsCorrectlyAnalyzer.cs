using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0357: Use tabs correctly.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0357UseTabsCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH0357UseTabsCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0357";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0357UseTabsCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0357Title), nameof(AnalyzerResources.RH0357MessageFormat))
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

        foreach (var line in sourceText.Lines)
        {
            for (var index = line.Start; index < line.End; index++)
            {
                if (sourceText[index] != '\t')
                {
                    continue;
                }

                var token = root.FindToken(index);

                if (token.IsKind(SyntaxKind.StringLiteralToken)
                    || token.IsKind(SyntaxKind.CharacterLiteralToken)
                    || token.IsKind(SyntaxKind.InterpolatedStringTextToken))
                {
                    continue;
                }

                context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, new TextSpan(index, 1))));
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