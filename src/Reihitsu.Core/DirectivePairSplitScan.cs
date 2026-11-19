using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Core;

/// <summary>
/// Scans the directives of a document in source order and determines whether inserting a pair of braces at two positions
/// splits a directive pair across a brace; <see cref="SyntaxTriviaUtilities.SplitsDirectivePair"/> states which
/// directives pair
/// </summary>
internal sealed class DirectivePairSplitScan
{
    #region Fields

    /// <summary>
    /// Position the opening brace is inserted at
    /// </summary>
    private readonly int _openBracePosition;

    /// <summary>
    /// Position the closing brace is inserted at
    /// </summary>
    private readonly int _closeBracePosition;

    /// <summary>
    /// Position of the last directive of every open <c>#if</c> group, innermost on top
    /// </summary>
    private readonly Stack<int> _conditionalGroups = new();

    /// <summary>
    /// Position of every open <c>#region</c>, innermost on top
    /// </summary>
    private readonly Stack<int> _regionStarts = new();

    /// <summary>
    /// Position of the last directive of a kind that names a setting, by kind and setting
    /// </summary>
    private readonly Dictionary<(SyntaxKind Kind, string Setting), int> _lastDirectiveOfSetting = [];

    /// <summary>
    /// Position of the last directive of a kind that names no setting and so stands for every setting, by kind
    /// </summary>
    private readonly Dictionary<SyntaxKind, int> _lastDirectiveOfAllSettings = [];

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectivePairSplitScan"/> class
    /// </summary>
    /// <param name="openBracePosition">Position the opening brace is inserted at</param>
    /// <param name="closeBracePosition">Position the closing brace is inserted at</param>
    internal DirectivePairSplitScan(int openBracePosition, int closeBracePosition)
    {
        _openBracePosition = openBracePosition;
        _closeBracePosition = closeBracePosition;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Takes the next directive of the document and determines whether the braces split the pair it closes
    /// </summary>
    /// <param name="directive">The next directive trivia in source order</param>
    /// <returns><see langword="true"/> if the braces split a pair the directive ends</returns>
    internal bool Splits(SyntaxTrivia directive)
    {
        var position = directive.SpanStart;

        switch (directive.GetStructure())
        {
            case IfDirectiveTriviaSyntax:
                {
                    _conditionalGroups.Push(position);
                }
                break;

            case ElifDirectiveTriviaSyntax:
            case ElseDirectiveTriviaSyntax:
            case EndIfDirectiveTriviaSyntax:
                {
                    return SplitsConditionalGroup(directive, position);
                }

            case RegionDirectiveTriviaSyntax:
                {
                    _regionStarts.Push(position);
                }
                break;

            case EndRegionDirectiveTriviaSyntax:
                {
                    return _regionStarts.Count > 0
                           && SplitsSpan(_regionStarts.Pop(), position);
                }

            case PragmaWarningDirectiveTriviaSyntax pragmaWarningDirective:
                {
                    return SplitsSettingPair(SyntaxKind.PragmaWarningDirectiveTrivia, pragmaWarningDirective.ErrorCodes.Select(GetWarningCode).ToList(), position);
                }

            case NullableDirectiveTriviaSyntax nullableDirective:
                {
                    IReadOnlyList<string> settings = nullableDirective.TargetToken.IsKind(SyntaxKind.None)
                                                         ? []
                                                         : [nullableDirective.TargetToken.ValueText];

                    return SplitsSettingPair(SyntaxKind.NullableDirectiveTrivia, settings, position);
                }

            case PragmaChecksumDirectiveTriviaSyntax:
                {
                    return SplitsSettingPair(SyntaxKind.PragmaChecksumDirectiveTrivia, [], position);
                }

            case LineDirectiveTriviaSyntax:
            case LineSpanDirectiveTriviaSyntax:
                {
                    return SplitsSettingPair(SyntaxKind.LineDirectiveTrivia, [], position);
                }
        }

        return false;
    }

    /// <summary>
    /// Gets the warning a <c>#pragma warning</c> directive names, so that <c>CS0219</c> and <c>219</c> name the same one
    /// </summary>
    /// <param name="errorCode">Error code as written in the directive</param>
    /// <returns>The warning</returns>
    private static string GetWarningCode(ExpressionSyntax errorCode)
    {
        return errorCode is LiteralExpressionSyntax { Token.Value: int number }
                   ? "CS" + number.ToString("D4", CultureInfo.InvariantCulture)
                   : errorCode.ToString().Trim();
    }

    /// <summary>
    /// Determines whether the braces split the code between an <c>#elif</c>, <c>#else</c>, or <c>#endif</c> directive
    /// and the directive in front of it in its <c>#if</c> group, and records the directive as the group's last unless it
    /// ends the group
    /// </summary>
    /// <param name="directive">The directive</param>
    /// <param name="position">Position of the directive</param>
    /// <returns><see langword="true"/> if the braces split the pair</returns>
    private bool SplitsConditionalGroup(SyntaxTrivia directive, int position)
    {
        if (_conditionalGroups.Count == 0)
        {
            return false;
        }

        if (SplitsSpan(_conditionalGroups.Pop(), position))
        {
            return true;
        }

        if (directive.IsKind(SyntaxKind.EndIfDirectiveTrivia) == false)
        {
            _conditionalGroups.Push(position);
        }

        return false;
    }

    /// <summary>
    /// Determines whether exactly one of the two braces lands inside the code between two partner directives
    /// </summary>
    /// <param name="start">Position of the first directive</param>
    /// <param name="end">Position of the second directive</param>
    /// <returns><see langword="true"/> if the braces split the span</returns>
    private bool SplitsSpan(int start, int end)
    {
        var openInside = start < _openBracePosition && _openBracePosition <= end;
        var closeInside = start < _closeBracePosition && _closeBracePosition <= end;

        return openInside != closeInside;
    }

    /// <summary>
    /// Determines whether the braces split the pair a directive of a kind that names settings forms with the directive
    /// of that kind in front of it for each setting it names, and records it as the last directive of those settings. A
    /// directive that names no setting stands for every setting
    /// </summary>
    /// <param name="kind">Kind of the directive</param>
    /// <param name="settings">Settings the directive names</param>
    /// <param name="position">Position of the directive</param>
    /// <returns><see langword="true"/> if the braces split a pair</returns>
    private bool SplitsSettingPair(SyntaxKind kind, IReadOnlyList<string> settings, int position)
    {
        var splits = false;

        if (settings.Count == 0)
        {
            foreach (var entry in _lastDirectiveOfSetting.Where(entry => entry.Key.Kind == kind).ToList())
            {
                splits |= SplitsSpan(entry.Value, position);
                _lastDirectiveOfSetting.Remove(entry.Key);
            }

            if (_lastDirectiveOfAllSettings.TryGetValue(kind, out var previousPosition))
            {
                splits |= SplitsSpan(previousPosition, position);
            }

            _lastDirectiveOfAllSettings[kind] = position;

            return splits;
        }

        foreach (var setting in settings)
        {
            var hasPrevious = _lastDirectiveOfSetting.TryGetValue((kind, setting), out var previousPosition);

            if (_lastDirectiveOfAllSettings.TryGetValue(kind, out var allSettingsPosition)
                && (hasPrevious == false || allSettingsPosition > previousPosition))
            {
                previousPosition = allSettingsPosition;
                hasPrevious = true;
            }

            splits |= hasPrevious && SplitsSpan(previousPosition, position);
            _lastDirectiveOfSetting[(kind, setting)] = position;
        }

        return splits;
    }

    #endregion // Methods
}