namespace Reihitsu.Core.Enumerations;

/// <summary>
/// The separators a merge of attribute lists writes into the stretch it rewrites
/// </summary>
public enum AttributeListMergeSeparators
{
    /// <summary>
    /// One space behind the merged list and behind a token the merge leaves touching a joined token, and commas with
    /// elastic trivia between the merged list's attributes; the line breaks of the stretch are dropped. The attribute-list
    /// code fixes write these
    /// </summary>
    Spaced,

    /// <summary>
    /// One space behind the merged list and behind a token the merge leaves touching a joined token, and commas without
    /// any trivia between the merged list's attributes; the line breaks of the stretch are dropped. The formatter's merge
    /// of accessor attribute lists writes these, so it adds no elastic trivia and leaves the space behind each comma to
    /// the horizontal-spacing phase
    /// </summary>
    SpacedBehindList,

    /// <summary>
    /// No whitespace at all, and no elastic trivia: the formatter's later phases space and place the merged list. One line
    /// break of the stretch is kept between the merged list and the token behind the last merged list when the author
    /// wrote one anywhere in the stretch
    /// </summary>
    None
}