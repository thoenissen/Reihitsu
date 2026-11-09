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
    /// The most recently formatted output
    /// </summary>
    private string _output = string.Empty;

    /// <summary>
    /// A message describing why the input could not be formatted, or an empty string when the last format succeeded
    /// </summary>
    private string _errorMessage = string.Empty;

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Creates the message for an input the formatter refuses as unsupported, with the one-based line and column of the
    /// shape's first character and the reason the formatter gives (PLAY-008)
    /// </summary>
    /// <param name="unsupported">The shape the formatter refused the input for</param>
    /// <returns>The message</returns>
    private static string CreateUnsupportedMessage(UnsupportedSyntax unsupported)
    {
        var start = unsupported.Location.GetLineSpan().StartLinePosition;

        return $"The input contains syntax the formatter does not support and was left unchanged: line {start.Line + 1}, column {start.Character + 1} — {unsupported.Reason}.";
    }

    /// <summary>
    /// Formats the current input and updates the output and error message
    /// </summary>
    private void FormatInput()
    {
        var result = PlaygroundFormatter.Format(_input);

        _output = result.Text;

        if (result.Formatted)
        {
            _errorMessage = string.Empty;
        }
        else if (result.Unsupported != null)
        {
            _errorMessage = CreateUnsupportedMessage(result.Unsupported);
        }
        else
        {
            _errorMessage = "The input contains syntax errors and was left unchanged.";
        }
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