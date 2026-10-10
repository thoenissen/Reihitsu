using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Clarity;

/// <summary>
/// RH3201: Comments must contain text
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH3201CommentsMustContainTextAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH3201";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH3201CommentsMustContainTextAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Clarity, nameof(AnalyzerResources.RH3201Title), nameof(AnalyzerResources.RH3201MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determine whether the comment trivia is empty
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <returns><see langword="true"/> if the comment is empty</returns>
    private static bool IsEmptyComment(SyntaxTrivia commentTrivia)
    {
        var text = commentTrivia.ToString();

        var commentText = commentTrivia.Kind() switch
                          {
                              SyntaxKind.SingleLineCommentTrivia => text.Substring(2),
                              SyntaxKind.MultiLineCommentTrivia => GetMultiLineCommentContent(text),
                              _ => string.Empty
                          };

        return string.IsNullOrWhiteSpace(commentText);
    }

    /// <summary>
    /// Extract the content of a multi-line comment, excluding the surrounding <c>/*</c> and <c>*/</c> delimiters
    /// </summary>
    /// <param name="text">Full comment trivia text</param>
    /// <returns>The comment content, or the full text when the comment is unterminated</returns>
    private static string GetMultiLineCommentContent(string text)
    {
        // An unterminated block comment (for example "/*", "/**" or "/*/" while still typing at the end of a file)
        // is still MultiLineCommentTrivia but has no closing "*/". Returning the full, non-empty text treats it as
        // non-empty so mid-typing states never report the diagnostic and the slicing below never runs with a
        // negative length or chops trailing content characters.
        if ((text.Length < 4)
            || (text.EndsWith("*/", StringComparison.Ordinal) == false))
        {
            return text;
        }

        return text.Substring(2, text.Length - 4);
    }

    /// <summary>
    /// Determine whether the specified line is a regular single-line comment
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="lineIndex">Line index</param>
    /// <returns><see langword="true"/> if the line contains a regular single-line comment</returns>
    private static bool IsSingleLineCommentLine(SourceText sourceText, int lineIndex)
    {
        var line = sourceText.Lines[lineIndex];
        var lineText = sourceText.ToString(TextSpan.FromBounds(line.Start, line.End)).TrimStart();

        return lineText.StartsWith("//", StringComparison.Ordinal)
               && DocumentationAnalysisUtilities.IsDocumentationLine(lineText) == false;
    }

    /// <summary>
    /// Determine whether an empty single-line comment represents a separator inside a comment block
    /// </summary>
    /// <param name="commentTrivia">Comment trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns><see langword="true"/> if the comment is an empty separator inside a comment block</returns>
    private static bool IsBlankLineInsideSingleLineCommentBlock(SyntaxTrivia commentTrivia, SourceText sourceText)
    {
        if (commentTrivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false)
        {
            return false;
        }

        var lineIndex = sourceText.Lines.GetLineFromPosition(commentTrivia.SpanStart).LineNumber;

        if (lineIndex == 0
            || lineIndex >= sourceText.Lines.Count - 1)
        {
            return false;
        }

        return IsSingleLineCommentLine(sourceText, lineIndex - 1)
               && IsSingleLineCommentLine(sourceText, lineIndex + 1);
    }

    /// <summary>
    /// Analyze the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var triviaList = root.DescendantTrivia(descendIntoTrivia: true);

        foreach (var trivia in triviaList)
        {
            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) == false
                && trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) == false)
            {
                continue;
            }

            if (IsEmptyComment(trivia))
            {
                if (IsBlankLineInsideSingleLineCommentBlock(trivia, sourceText))
                {
                    continue;
                }

                context.ReportDiagnostic(CreateDiagnostic(trivia.GetLocation()));
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