namespace Reihitsu.Formatter;

/// <summary>
/// The result of a formatting entry point of <see cref="ReihitsuFormatter"/>: its output, and whether the formatter refused
/// the input as unsupported (FMT-UNSUP-001)
/// </summary>
/// <typeparam name="T">The type of the formatted input: a document, a syntax tree, or a syntax node</typeparam>
public sealed class FormattingResult<T>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="output">The formatted input, or the input unchanged</param>
    /// <param name="unsupported">The unsupported shape the formatter refused the input for, or <see langword="null"/></param>
    internal FormattingResult(T output, UnsupportedSyntax unsupported)
    {
        Output = output;
        Unsupported = unsupported;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The formatted input, or the input unchanged when the formatter refuses it, and also when it leaves code with syntax
    /// errors or generated code untouched (FMT-PIPE-005, FMT-PIPE-006)
    /// </summary>
    public T Output { get; }

    /// <summary>
    /// The unsupported shape the formatter refused the input for: the first such shape in the text. <see langword="null"/>
    /// when the input is not refused
    /// </summary>
    public UnsupportedSyntax Unsupported { get; }

    #endregion // Properties
}