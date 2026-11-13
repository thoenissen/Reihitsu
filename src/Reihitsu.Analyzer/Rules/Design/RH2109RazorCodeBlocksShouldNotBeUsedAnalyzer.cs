using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Design;

/// <summary>
/// RH2109: Razor @code blocks should not be used
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH2109";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Pattern for a Razor <c>@code</c> or <c>@functions</c> directive that opens a block, anchored at the position the match starts at.
    /// This pattern carries a single quantifier and cannot backtrack pathologically, and it only ever matches an additional file
    /// Roslyn already accepted into the compilation, so it uses <see cref="Regex.InfiniteMatchTimeout"/> rather than a wall-clock
    /// budget: a timeout would surface as AD0001 and silently disable this analyzer for the whole compilation
    /// </summary>
    private static readonly Regex _blockDirectivePattern = new(@"\G@(?<keyword>code|functions)\s*\{", RegexOptions.Compiled | RegexOptions.CultureInvariant, Regex.InfiniteMatchTimeout);

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH2109RazorCodeBlocksShouldNotBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Design, nameof(AnalyzerResources.RH2109Title), nameof(AnalyzerResources.RH2109MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Finds every <c>@code</c> directive that opens a block. A directive starts a line after optional white space;
    /// Razor comments, HTML comments, and the escape <c>@@</c> are text, and the C# of a <c>@code</c> or <c>@functions</c>
    /// block, up to its closing brace, is skipped, so its comments and literals are text as well
    /// </summary>
    /// <param name="text">Text of the Razor file</param>
    /// <returns>The positions of the <c>@</c> of every reported directive</returns>
    private static List<int> FindCodeDirectives(string text)
    {
        var directivePositions = new List<int>();
        var position = 0;

        while (position < text.Length)
        {
            if (StartsWith(text, position, "@*"))
            {
                position = SkipPast(text, position + 2, "*@");
            }
            else if (StartsWith(text, position, "<!--"))
            {
                position = SkipPast(text, position + 4, "-->");
            }
            else if (StartsWith(text, position, "@@"))
            {
                position += 2;
            }
            else if (text[position] == '@'
                     && StartsLine(text, position)
                     && _blockDirectivePattern.Match(text, position) is { Success: true } match)
            {
                if (match.Groups["keyword"].Value == "code")
                {
                    directivePositions.Add(position);
                }

                position = SkipCSharpBlock(text, match.Index + match.Length - 1);
            }
            else
            {
                position++;
            }
        }

        return directivePositions;
    }

    /// <summary>
    /// Skips a C# block from its opening brace to its matching closing brace. The C# lexer decides which braces are tokens,
    /// so braces inside comments and literals are not counted; a Razor comment inside the block is skipped as a whole
    /// </summary>
    /// <param name="text">Text of the Razor file</param>
    /// <param name="openBracePosition">Position of the block's opening brace</param>
    /// <returns>The position behind the closing brace, or the end of the text when the block is not closed</returns>
    private static int SkipCSharpBlock(string text, int openBracePosition)
    {
        var depth = 0;
        var position = openBracePosition;

        while (position < text.Length)
        {
            var resumePosition = text.Length;

            foreach (var token in SyntaxFactory.ParseTokens(text, position, position))
            {
                if (StartsWith(text, token.SpanStart, "@*"))
                {
                    resumePosition = SkipPast(text, token.SpanStart + 2, "*@");

                    break;
                }

                if (token.IsKind(SyntaxKind.OpenBraceToken))
                {
                    depth++;
                }
                else if (token.IsKind(SyntaxKind.CloseBraceToken))
                {
                    depth--;

                    if (depth == 0)
                    {
                        return token.Span.End;
                    }
                }
            }

            position = resumePosition;
        }

        return text.Length;
    }

    /// <summary>
    /// Determines whether only spaces and tabs stand between the start of the line and the given position
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position</param>
    /// <returns><see langword="true"/> when the position starts its line after optional white space</returns>
    private static bool StartsLine(string text, int position)
    {
        var index = position;

        while (index > 0 && text[index - 1] is ' ' or '\t')
        {
            index--;
        }

        return index == 0 || text[index - 1] is '\n' or '\r';
    }

    /// <summary>
    /// Determines whether the text holds the value at the given position
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position</param>
    /// <param name="value">Value</param>
    /// <returns><see langword="true"/> when the value starts at the position</returns>
    private static bool StartsWith(string text, int position, string value)
    {
        return position + value.Length <= text.Length
               && string.CompareOrdinal(text, position, value, 0, value.Length) == 0;
    }

    /// <summary>
    /// Gets the position behind the next occurrence of the terminator
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="position">Position the search starts at</param>
    /// <param name="terminator">Terminator</param>
    /// <returns>The position behind the terminator, or the end of the text when it does not occur</returns>
    private static int SkipPast(string text, int position, string terminator)
    {
        var index = text.IndexOf(terminator, position, StringComparison.Ordinal);

        return index < 0 ? text.Length : index + terminator.Length;
    }

    /// <summary>
    /// Analyze compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilation(CompilationAnalysisContext context)
    {
        foreach (var file in context.Options.AdditionalFiles)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (file.Path.EndsWith(".razor", StringComparison.OrdinalIgnoreCase) is false)
            {
                continue;
            }

            var text = file.GetText(context.CancellationToken);

            if (text == null)
            {
                continue;
            }

            foreach (var directivePosition in FindCodeDirectives(text.ToString()))
            {
                var span = new TextSpan(directivePosition, "@code".Length);
                var lineSpan = text.Lines.GetLinePositionSpan(span);
                var location = Location.Create(file.Path, span, lineSpan);

                context.ReportDiagnostic(CreateDiagnostic(location));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationAction(OnCompilation);
    }

    #endregion // DiagnosticAnalyzer
}