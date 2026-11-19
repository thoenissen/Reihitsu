using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5603: File must not end with a newline
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5603FileMustNotEndWithANewlineAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5603";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5603FileMustNotEndWithANewlineAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the character is a line break character
    /// </summary>
    /// <param name="character">Character</param>
    /// <returns><see langword="true"/> if the character is <c>\r</c> or <c>\n</c></returns>
    private static bool IsLineBreakCharacter(char character)
    {
        return character is '\r' or '\n';
    }

    /// <summary>
    /// Analyzes the syntax tree for trailing newline characters. The reported tail starts at the first line break
    /// behind the last non-whitespace character, so whitespace-only lines between the trailing line breaks belong to
    /// it, while whitespace behind that character on its own line does not.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var sourceText = context.Tree.GetText(context.CancellationToken);
        var length = sourceText.Length;

        if (length == 0
            || IsLineBreakCharacter(sourceText[length - 1]) == false)
        {
            return;
        }

        var contentEnd = length;

        while (contentEnd > 0
               && (IsLineBreakCharacter(sourceText[contentEnd - 1])
                   || SyntaxFacts.IsWhitespace(sourceText[contentEnd - 1])))
        {
            contentEnd--;
        }

        var trailingNewlineStart = contentEnd;

        while (IsLineBreakCharacter(sourceText[trailingNewlineStart]) == false)
        {
            trailingNewlineStart++;
        }

        var diagnosticSpan = TextSpan.FromBounds(trailingNewlineStart, length);

        context.ReportDiagnostic(CreateDiagnostic(Location.Create(context.Tree, diagnosticSpan)));
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