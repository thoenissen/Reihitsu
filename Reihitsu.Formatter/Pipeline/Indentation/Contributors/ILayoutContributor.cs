using Microsoft.CodeAnalysis;

namespace Reihitsu.Formatter.Pipeline.Indentation.Contributors;

/// <summary>
/// Interface for layout contributors that compute indentation for specific syntax constructs.
/// Contributors are called top-down (parent before children) during the compute phase
/// </summary>
internal interface ILayoutContributor
{
    /// <summary>
    /// Computes layout instructions for tokens within the given node.
    /// A contributor may set layout for any descendant token by writing to the layout model
    /// </summary>
    /// <param name="node">The syntax node being processed</param>
    /// <param name="scope">The current formatting scope</param>
    /// <param name="model">The layout model to write computed layouts to</param>
    /// <param name="context">The formatting context</param>
    void Contribute(SyntaxNode node, FormattingScope scope, LayoutModel model, FormattingContext context);
}