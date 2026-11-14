using Microsoft.CodeAnalysis;

namespace Reihitsu.Core;

/// <summary>
/// The facts about the code above the root of a tree that <see cref="FluentChain"/> needs to decide which node of a chain
/// inside that tree is the outermost one: whether the parent of the root continues the root's chain, which a parentless
/// root cannot read from its tree. It is a plain value, captured from the document by <see cref="From"/> before the root is
/// detached. The default value describes a root that has nothing above it, such as a compilation unit
/// </summary>
public readonly struct FluentChainBoundary
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rootContinuesChain">Whether the parent of the root continues the root's chain</param>
    public FluentChainBoundary(bool rootContinuesChain)
    {
        RootContinuesChain = rootContinuesChain;
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// Whether the parent of the root continues the root's chain, so that the root is not the outermost node of its chain
    /// </summary>
    public bool RootContinuesChain { get; }

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
                   : new FluentChainBoundary(FluentChain.ContinuesChain(root.Parent, root, default));
    }

    #endregion // Methods
}