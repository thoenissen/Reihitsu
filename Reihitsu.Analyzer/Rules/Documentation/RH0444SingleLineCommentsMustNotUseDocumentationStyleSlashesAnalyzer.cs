using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH0444: Single-line comments must not use documentation style slashes
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer : DiagnosticAnalyzerBase<RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0444";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0444SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH0444Title), nameof(AnalyzerResources.RH0444MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Analyze a documentation trivia node
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DocumentationCommentTriviaSyntax documentationComment
            || documentationComment.Content.All(obj => obj.IsKind(SyntaxKind.XmlText)) == false)
        {
            return;
        }

        foreach (var trivia in documentationComment.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia) == false)
            {
                continue;
            }

            var fullSpan = trivia.GetLocation().SourceSpan;
            var diagnosticSpan = TextSpan.FromBounds(fullSpan.End - 3, fullSpan.End);

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(trivia.SyntaxTree, diagnosticSpan)));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    #endregion // DiagnosticAnalyzer
}