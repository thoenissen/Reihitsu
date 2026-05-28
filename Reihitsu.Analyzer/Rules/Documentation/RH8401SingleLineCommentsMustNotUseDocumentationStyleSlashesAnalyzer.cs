using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8401: Single-line comments must not use documentation style slashes
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer : DiagnosticAnalyzerBase<RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8401";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8401Title), nameof(AnalyzerResources.RH8401MessageFormat))
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

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.SingleLineDocumentationCommentTrivia);
    }

    #endregion // DiagnosticAnalyzer
}