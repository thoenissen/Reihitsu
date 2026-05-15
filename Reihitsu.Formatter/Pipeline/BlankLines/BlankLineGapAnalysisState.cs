namespace Reihitsu.Formatter.Pipeline.BlankLines;

/// <summary>
/// Immutable state used while analyzing blank-line gaps
/// </summary>
internal readonly struct BlankLineGapAnalysisState
{
    #region Fields

    /// <summary>
    /// Whether any line break has been seen
    /// </summary>
    private readonly bool _sawLineBreak;

    /// <summary>
    /// Whether the current logical line has content
    /// </summary>
    private readonly bool _lineHasContent;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="sawLineBreak">Whether any line break has been seen</param>
    /// <param name="lineHasContent">Whether the current logical line has content</param>
    /// <param name="blankLineCount">Number of blank lines encountered so far</param>
    private BlankLineGapAnalysisState(bool sawLineBreak, bool lineHasContent, int blankLineCount)
    {
        _sawLineBreak = sawLineBreak;
        _lineHasContent = lineHasContent;
        BlankLineCount = blankLineCount;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Initial analysis state
    /// </summary>
    public static BlankLineGapAnalysisState Initial => default;

    /// <summary>
    /// Number of blank lines encountered so far
    /// </summary>
    public int BlankLineCount { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Marks that the current logical line contains content
    /// </summary>
    /// <returns>Updated state with current-line content set</returns>
    public BlankLineGapAnalysisState MarkLineHasContent()
    {
        return new BlankLineGapAnalysisState(sawLineBreak: _sawLineBreak,
                                             lineHasContent: true,
                                             blankLineCount: BlankLineCount);
    }

    /// <summary>
    /// Advances state for one logical line break
    /// </summary>
    /// <param name="nextLineHasContent">Whether the next logical line already has content</param>
    /// <returns>Updated state after processing one logical line break</returns>
    public BlankLineGapAnalysisState AdvanceOnLineBreak(bool nextLineHasContent)
    {
        var blankLineCount = BlankLineCount;

        if (_sawLineBreak && _lineHasContent == false)
        {
            blankLineCount++;
        }

        return new BlankLineGapAnalysisState(sawLineBreak: true,
                                             lineHasContent: nextLineHasContent,
                                             blankLineCount);
    }

    #endregion // Methods
}