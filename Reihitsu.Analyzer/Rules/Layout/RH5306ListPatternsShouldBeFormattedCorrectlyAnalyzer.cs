using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

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
        return FormattingSafetyUtilities.HasCommentsOrDirectives(listPattern) == false
               && FormattingSafetyUtilities.AreAllSingleLine(listPattern.Patterns);
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

        // Rule 1: Single-line list patterns are always allowed, because every inner pattern is
        // necessarily on the bracket line when the opening and closing brackets share that line.
        if (isSingleLinePattern)
        {
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