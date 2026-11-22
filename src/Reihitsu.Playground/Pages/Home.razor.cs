﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.AspNetCore.Components;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Formatter;

namespace Reihitsu.Playground.Pages;

/// <summary>
/// Code-behind for the playground home page
/// </summary>
public partial class Home
{
    #region Fields

    /// <summary>
    /// Example C# source shown on first load, before the user edits it
    /// </summary>
    private const string ExampleSource = "public class Sample{public int Add(int a,int b){return a+b;}}";

    /// <summary>
    /// The user-entered C# source
    /// </summary>
    private string _input = ExampleSource;

    /// <summary>
    /// The most recently formatted output, coloured by its syntax tree
    /// </summary>
    private RenderFragment _outputContent;

    /// <summary>
    /// The lines of the message describing why the input could not be formatted; empty when the last format succeeded
    /// </summary>
    private List<string> _messageLines = [];

    /// <summary>
    /// The spans the input marks, set by the last format and removed at the first change of the input (PLAY-014)
    /// </summary>
    private List<TextSpan> _marks = [];

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Formats the one-based line and column of the first character of a location, a tab counted as one column
    /// (PLAY-006, PLAY-008)
    /// </summary>
    /// <param name="location">The location</param>
    /// <returns>The text <c>line &lt;line&gt;, column &lt;column&gt;</c></returns>
    private static string FormatPosition(Location location)
    {
        var start = location.GetLineSpan().StartLinePosition;

        return $"line {start.Line + 1}, column {start.Character + 1}";
    }

    /// <summary>
    /// Creates the message for an input the formatter refuses as unsupported, with the one-based line and column of the
    /// shape's first character and the reason the formatter gives (PLAY-008)
    /// </summary>
    /// <param name="unsupported">The shape the formatter refused the input for</param>
    /// <returns>The message</returns>
    private static string CreateUnsupportedMessage(UnsupportedSyntax unsupported)
    {
        return $"The input contains syntax the formatter does not support and was left unchanged: {FormatPosition(unsupported.Location)} — {unsupported.Reason}.";
    }

    /// <summary>
    /// Creates the message line of one syntax error, with the parser's message in English (PLAY-006)
    /// </summary>
    /// <param name="error">The error</param>
    /// <returns>The message line</returns>
    private static string CreateErrorLine(Diagnostic error)
    {
        return $"{FormatPosition(error.Location)} — {error.Id}: {error.GetMessage(CultureInfo.InvariantCulture)}";
    }

    /// <summary>
    /// Formats the current input and updates the output, the message, and the input's marks
    /// </summary>
    private void FormatInput()
    {
        var result = PlaygroundFormatter.Format(_input);
        var errors = result.Errors ?? [];

        _outputContent = PlaygroundHighlighter.Render(result.Text, []);

        if (result.Formatted)
        {
            _messageLines = [];
            _marks = [];
        }
        else if (result.Unsupported != null)
        {
            _messageLines = [CreateUnsupportedMessage(result.Unsupported)];
            _marks = [PlaygroundHighlighter.GetMarkSpan(result.Unsupported.Location.SourceSpan, _input)];
        }
        else if (result.HoldsHiddenCode)
        {
            // The formatter reports no location for hidden code, so nothing is marked (PLAY-014)
            _messageLines = ["The input contains code hidden in another build configuration and was left unchanged."];
            _marks = [];
        }
        else
        {
            _messageLines = ["The input contains syntax errors and was left unchanged.", .. errors.Select(CreateErrorLine)];
            _marks = errors.Select(error => PlaygroundHighlighter.GetMarkSpan(error.Location.SourceSpan, _input))
                           .ToList();
        }
    }

    /// <summary>
    /// Removes every mark at a change of the input, because a mark can point at code it was not about after an edit; the
    /// message stays until the next format (PLAY-014)
    /// </summary>
    private void OnInputChanged()
    {
        _marks = [];
    }

    #endregion // Methods

    #region ComponentBase

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        FormatInput();
    }

    #endregion // ComponentBase
}