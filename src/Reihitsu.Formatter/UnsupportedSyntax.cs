using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter;

/// <summary>
/// A shape the formatter refuses as unsupported: the reason the requirement of the shape gives, and the location of the shape
/// in the caller's syntax tree (FMT-UNSUP-001)
/// </summary>
public sealed class UnsupportedSyntax
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="reason">The description of the unsupported shape</param>
    /// <param name="location">The span of the shape</param>
    public UnsupportedSyntax(string reason, Location location)
    {
        Reason = reason;
        Location = location;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The description of the unsupported shape, such as <c>a comment between a variable declaration's own tokens</c>
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// The span of the shape in the caller's syntax tree; a host derives the line and the column of its first character from it
    /// </summary>
    public Location Location { get; }

    #endregion // Properties
}