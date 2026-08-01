using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.DocumentationComments;

/// <summary>
/// Shared annotations for trivia created while relocating documentation comments
/// </summary>
internal static class DocumentationCommentRelocationAnnotations
{
    #region Properties

    /// <summary>
    /// Marks a line break that separates relocated documentation from source at a formatting-root boundary
    /// </summary>
    public static SyntaxAnnotation BoundaryLineBreak { get; } = new("DocumentationCommentRelocationBoundaryLineBreak");

    #endregion // Properties
}