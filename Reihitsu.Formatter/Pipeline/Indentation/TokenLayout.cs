namespace Reihitsu.Formatter.Pipeline.Indentation;

/// <summary>
/// Describes the desired indentation for a first-on-line token.
/// </summary>
/// <param name="Column">The desired column (0-based) for the token's first character.</param>
/// <param name="Source">Optional debug label identifying which contributor set this layout.</param>
internal readonly record struct TokenLayout(int Column, string Source = null)
{
    #region Properties

    /// <summary>
    /// The desired column (0-based) for the token's first character.
    /// </summary>
    public int Column { get; } = Column;

    /// <summary>
    /// Optional debug label identifying which contributor set this layout.
    /// </summary>
    public string Source { get; } = Source;

    #endregion // Properties
}