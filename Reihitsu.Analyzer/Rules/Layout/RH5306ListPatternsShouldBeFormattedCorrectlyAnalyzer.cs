using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Layout;

/// <summary>
/// RH5306: List patterns should be formatted correctly
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer : DiagnosticAnalyzerBase<RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH5306";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5306ListPatternsShouldBeFormattedCorrectlyAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Layout, nameof(AnalyzerResources.RH5306Title), nameof(AnalyzerResources.RH5306MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether a list pattern can be formatted safely by the formatter-backed code fix
    /// </summary>
    /// <param name="listPattern">List pattern</param>
    /// <returns><see langword="true"/> if formatting is safe; otherwise, <see langword="false"/></returns>
    private static bool CanSafelyFormat(ListPatternSyntax listPattern)
    {
        foreach (var trivia in listPattern.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.IsDirective || trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                return false;
            }
        }

        foreach (var pattern in listPattern.Patterns)
        {
            var lineSpan = pattern.GetLocation().GetLineSpan();

            if (lineSpan.StartLinePosition.Line != lineSpan.EndLinePosition.Line)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Analyzing all <see cref="SyntaxKind.ListPattern"/> occurrences
    /// </summary>
    /// <param name="context">Context</param>
    private void OnListPattern(SyntaxNodeAnalysisContext context)
    {
        var listPattern = (ListPatternSyntax)context.Node;

        if (CanSafelyFormat(listPattern) == false)
        {
            return;
        }

        var openBracketPosition = listPattern.OpenBracketToken.GetLocation().GetLineSpan().StartLinePosition;
        var closeBracketPosition = listPattern.CloseBracketToken.GetLocation().GetLineSpan().StartLinePosition;
        var patternLinePositions = listPattern.Patterns.Select(pattern => pattern.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition).ToArray();
        var isSingleLinePattern = openBracketPosition.Line == closeBracketPosition.Line;

        // Rule 1: Single-line list patterns are allowed only when every inner pattern
        // is also on that same line.
        if (isSingleLinePattern)
        {
            if (patternLinePositions.Any(position => position.Line != openBracketPosition.Line))
            {
                context.ReportDiagnostic(CreateDiagnostic(listPattern.GetLocation()));
            }

            return;
        }

        // Rule 2: In multi-line form, opening and closing brackets must be in the same column.
        if (openBracketPosition.Character != closeBracketPosition.Character)
        {
            context.ReportDiagnostic(CreateDiagnostic(listPattern.GetLocation()));

            return;
        }

        // Rule 3: In multi-line form, every inner pattern must be on its own line between
        // the opening and closing brackets.
        if (patternLinePositions.Any(position => position.Line <= openBracketPosition.Line || position.Line >= closeBracketPosition.Line)
            || patternLinePositions.Select(position => position.Line).Distinct().Count() != patternLinePositions.Length)
        {
            context.ReportDiagnostic(CreateDiagnostic(listPattern.GetLocation()));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeAction(OnListPattern, SyntaxKind.ListPattern);
    }

    #endregion // DiagnosticAnalyzer
}