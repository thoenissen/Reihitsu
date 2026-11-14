using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8401: Single-line comments must not use documentation style slashes
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8401SingleLineCommentsMustNotUseDocumentationStyleSlashesAnalyzer : DiagnosticAnalyzerBase
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
    /// Analyze a single-line documentation comment
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="documentationComment">Documentation comment, parsed from the text of the analyzed tree</param>
    private void AnalyzeDocumentationComment(SyntaxTreeAnalysisContext context, DocumentationCommentTriviaSyntax documentationComment)
    {
        if (documentationComment.Content.All(obj => obj.IsKind(SyntaxKind.XmlText)) == false)
        {
            return;
        }

        foreach (var trivia in documentationComment.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia) == false)
            {
                continue;
            }

            var diagnosticSpan = TextSpan.FromBounds(trivia.Span.End - 3, trivia.Span.End);

            context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, diagnosticSpan)));
        }
    }

    /// <summary>
    /// Analyze the syntax tree. The rule's subject is an ordinary comment written with documentation slashes, so a tree
    /// whose documentation comments are not parsed is judged on the same text parsed with documentation comments, which
    /// decides the lines of one comment and whether it holds XML as in mode <see cref="DocumentationMode.Parse"/>
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = DocumentationAnalysisUtilities.GetRootWithParsedDocumentation(context.Tree, context.CancellationToken);

        foreach (var trivia in root.DescendantTrivia())
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
                && trivia.GetStructure() is DocumentationCommentTriviaSyntax documentationComment)
            {
                AnalyzeDocumentationComment(context, documentationComment);
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