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
    /// Formats the current input and updates the output and error message
    /// </summary>
    private void FormatInput()
    {
        var result = PlaygroundFormatter.Format(_input);

        _output = result.Text;
        _errorMessage = result.Formatted ? string.Empty : "The input contains syntax errors and was left unchanged.";
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