using Microsoft.CodeAnalysis;

namespace Reihitsu.Core;

/// <summary>
/// The facts about the code above the root of a tree that <see cref="FluentChain"/> needs to decide which node of a chain
/// inside that tree is the outermost one. The chain rule reads up to two levels above a node: whether the parent continues
/// the node's chain, and, for the target of a null-conditional assignment, whether the assignment is the branch of a
/// conditional access. For a node whose parent or grandparent lies above a parentless root, the answer comes from these
/// facts. They are plain values, captured from the document by <see cref="From"/> before the root is detached. The default
/// value describes a root that has nothing above it, such as a compilation unit
/// </summary>
public readonly struct FluentChainBoundary
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rootContinuesChain">Whether the parent of the root continues the root's chain</param>
    /// <param name="rootIsConditionalAccessBranch">Whether the root is the branch that a conditional access evaluates when its expression is not null</param>
    public FluentChainBoundary(bool rootContinuesChain, bool rootIsConditionalAccessBranch)
    {
        RootContinuesChain = rootContinuesChain;
        RootIsConditionalAccessBranch = rootIsConditionalAccessBranch;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether the parent of the root continues the root's chain, so that the root is not the outermost node of its chain
    /// </summary>
    public bool RootContinuesChain { get; }

    /// <summary>
    /// Whether the root is the branch that a conditional access evaluates when its expression is not null
    /// (<c>WhenNotNull</c>), which makes the target of a root assignment continue the conditional access's chain
    /// </summary>
    public bool RootIsConditionalAccessBranch { get; }

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Captures the facts about the code above the specified node, while it is still part of its tree
    /// </summary>
    /// <param name="root">The node that becomes the root of a tree of its own</param>
    /// <returns>The facts</returns>
    public static FluentChainBoundary From(SyntaxNode root)
    {
        return root.Parent == null
                   ? default
                   : new FluentChainBoundary(FluentChain.ContinuesChain(root.Parent, root, default),
                                             FluentChain.IsConditionalAccessBranch(root, default));
    }

    #endregion // Methods
}