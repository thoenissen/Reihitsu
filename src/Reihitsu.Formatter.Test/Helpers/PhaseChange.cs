namespace Reihitsu.Formatter.Test.Helpers;

/// <summary>
/// The change one phase, run alone, makes to the final output of a fixture
/// </summary>
public sealed class PhaseChange
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="phase">The name under which the pipeline observer reports the phase</param>
    /// <param name="input">The text the phase received</param>
    /// <param name="output">The text the phase returned</param>
    public PhaseChange(string phase, string input, string output)
    {
        Phase = phase;
        Output = output;
        OutputFingerprint = PhaseStabilityCheck.ComputeFingerprint(output);
        Difference = DescribeFirstDifference(input, output);
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The name under which the pipeline observer reports the phase
    /// </summary>
    public string Phase { get; }

    /// <summary>
    /// The text the phase returned
    /// </summary>
    public string Output { get; }

    /// <summary>
    /// The fingerprint of the text the phase returned, which identifies what the phase changed
    /// </summary>
    public string OutputFingerprint { get; }

    /// <summary>
    /// A description of the first line the phase changed
    /// </summary>
    public string Difference { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Describes the first line in which two texts differ
    /// </summary>
    /// <param name="input">The text the phase received</param>
    /// <param name="output">The text the phase returned</param>
    /// <returns>The description</returns>
    private static string DescribeFirstDifference(string input, string output)
    {
        var inputLines = input.Split('\n');
        var outputLines = output.Split('\n');
        var lineCount = Math.Max(inputLines.Length, outputLines.Length);

        for (var index = 0; index < lineCount; index++)
        {
            var inputLine = index < inputLines.Length ? inputLines[index] : null;
            var outputLine = index < outputLines.Length ? outputLines[index] : null;

            if (inputLine != outputLine)
            {
                return $"First difference at line {index + 1}: expected {Quote(inputLine)}, phase returned {Quote(outputLine)}.";
            }
        }

        return "The texts differ only in their line breaks.";
    }

    /// <summary>
    /// Quotes a line for a failure message, making a carriage return visible
    /// </summary>
    /// <param name="line">The line, or <see langword="null"/> when the text has no such line</param>
    /// <returns>The quoted line</returns>
    private static string Quote(string line)
    {
        return line == null ? "<no line>" : $"\"{line.Replace("\r", "\\r")}\"";
    }

    #endregion // Methods
}