using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Core;
using Reihitsu.Core.Enumerations;
using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Data.Enumerations;
using Reihitsu.Formatter.Extensions;
using Reihitsu.Formatter.Pipeline.LineBreaks.Utilities;
using Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities;

namespace Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter;

/// <summary>
/// Applies line-break rules for target-based attribute lists: places the lists of every owner, and shapes the lists of an
/// accessor, whose shape follows the layout this phase gives its accessor list
/// </summary>
internal sealed class AttributeTargetFormattingRewriter : CSharpSyntaxRewriter
{
    #region Fields

    /// <summary>
    /// The formatting context
    /// </summary>
    private readonly FormattingContext _context;

    /// <summary>
    /// The cancellation token
    /// </summary>
    private readonly CancellationToken _cancellationToken;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="context">The formatting context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public AttributeTargetFormattingRewriter(FormattingContext context,
                                             CancellationToken cancellationToken)
    {
        _context = context;
        _cancellationToken = cancellationToken;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether an accessor or an accessor list belongs to a property, indexer, or event declaration that stands
    /// on one line apart from its attribute lists
    /// </summary>
    /// <param name="node">The accessor or accessor list</param>
    /// <returns><see langword="true"/> when the node belongs to such a declaration; otherwise, <see langword="false"/></returns>
    internal static bool StandsInOneLineProperty(SyntaxNode node)
    {
        var owner = node switch
                    {
                        AccessorDeclarationSyntax accessorDeclaration => accessorDeclaration.Parent?.Parent,
                        AccessorListSyntax accessorList => accessorList.Parent,
                        _ => null
                    };

        return owner is BasePropertyDeclarationSyntax basePropertyDeclaration
               && SyntaxNodeUtilities.IsSingleLineExcludingAttributeLists(basePropertyDeclaration);
    }

    /// <summary>
    /// Resolves the placement mode for an attribute list, honoring the single-line accessor context
    /// </summary>
    /// <param name="attributeList">Attribute list</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Effective placement mode</returns>
    private static TargetAttributePlacementMode ResolvePlacementMode(AttributeListSyntax attributeList,
                                                                     bool keepAccessorListsSingleLine)
    {
        return keepAccessorListsSingleLine
                   ? TargetAttributePlacementMode.SingleLine
                   : AttributeTargetFormattingShared.ResolvePlacementMode(attributeList);
    }

    /// <summary>
    /// Adopts a placement result, reporting a change only when the rewrite actually altered the owner.
    /// The line-break helpers return the original node unchanged when a rewrite is refused (for example,
    /// when a single-line join would move a token into a comment), so an unconditional "changed" would
    /// spin <see cref="ApplyPlacement"/> forever on that shape
    /// </summary>
    /// <param name="owner">Owner node, updated when the placement changed</param>
    /// <param name="updatedOwner">Candidate owner produced by the placement rewrite</param>
    /// <returns><see langword="true"/> when the owner was changed; otherwise, <see langword="false"/></returns>
    private static bool TryReportChange(ref SyntaxNode owner, SyntaxNode updatedOwner)
    {
        if (ReferenceEquals(updatedOwner, owner))
        {
            return false;
        }

        owner = updatedOwner;

        return true;
    }

    /// <summary>
    /// Shapes the attribute lists of an accessor by the layout of its accessor list: on the property's single line its lists
    /// of one target are merged, with the separators the code fixes write, and on lines of their own its lists are split
    /// into one list per attribute. The structural transforms already shaped the lists of every other owner
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private static SyntaxNode ShapeAccessorAttributeLists(SyntaxNode owner,
                                                          bool keepAccessorListsSingleLine)
    {
        if (owner is not AccessorDeclarationSyntax)
        {
            return owner;
        }

        return keepAccessorListsSingleLine
                   ? AttributeListShapeUtilities.MergeAttributeLists(owner, _ => true, AttributeListMergeSeparators.Spaced)
                   : AttributeListShapeUtilities.SplitAttributeLists(owner);
    }

    /// <summary>
    /// Formats the attribute lists attached to an owner node: shapes the lists of an accessor, and places the lists of every
    /// owner
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode FormatAttributeLists(SyntaxNode owner,
                                            bool keepAccessorListsSingleLine)
    {
        if (AttributeTargetUtilities.GetAttributeLists(owner).Count == 0)
        {
            return owner;
        }

        owner = ShapeAccessorAttributeLists(owner, keepAccessorListsSingleLine);
        owner = ApplyPlacement(owner, keepAccessorListsSingleLine);

        return owner;
    }

    /// <summary>
    /// Applies the placement policy to all attribute lists on the owner node
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns>Updated owner node</returns>
    private SyntaxNode ApplyPlacement(SyntaxNode owner,
                                      bool keepAccessorListsSingleLine)
    {
        while (true)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var changed = false;

            foreach (var attributeList in AttributeTargetUtilities.GetAttributeLists(owner))
            {
                if (TryApplyPlacementToList(ref owner, attributeList, keepAccessorListsSingleLine))
                {
                    changed = true;

                    break;
                }
            }

            if (changed == false)
            {
                return owner;
            }
        }
    }

    /// <summary>
    /// Applies the placement policy to a single attribute list, updating the owner in place
    /// </summary>
    /// <param name="owner">Owner node, updated when the placement changes</param>
    /// <param name="attributeList">Attribute list to place</param>
    /// <param name="keepAccessorListsSingleLine">Whether the owner's attribute lists must stay single-line</param>
    /// <returns><see langword="true"/> when the owner was changed; otherwise, <see langword="false"/></returns>
    private bool TryApplyPlacementToList(ref SyntaxNode owner, AttributeListSyntax attributeList, bool keepAccessorListsSingleLine)
    {
        if (SyntaxNodeUtilities.InteriorContainsCommentOrDirective(attributeList)
            || AttributeTargetUtilities.TryGetTokenAfterAttributeList(attributeList, out var tokenAfter) == false)
        {
            return false;
        }

        var placementMode = ResolvePlacementMode(attributeList, keepAccessorListsSingleLine);
        var closeLine = attributeList.GetLocation().GetLineSpan().EndLinePosition.Line;
        var nextLine = tokenAfter.GetLocation().GetLineSpan().StartLinePosition.Line;
        var refreshedTokenAfter = TokenLocator.GetCurrentToken(owner, tokenAfter);

        if (placementMode == TargetAttributePlacementMode.SeparateLine
            && closeLine == nextLine)
        {
            var updatedOwner = LineBreakTriviaUtilities.MoveTokenToNewLine(owner, refreshedTokenAfter, _context.EndOfLine);

            return TryReportChange(ref owner, updatedOwner);
        }

        if (placementMode == TargetAttributePlacementMode.SingleLine
            && closeLine != nextLine)
        {
            var updatedOwner = LineBreakTriviaUtilities.CollapseTokenToSameLine(owner, refreshedTokenAfter);

            return TryReportChange(ref owner, updatedOwner);
        }

        return false;
    }

    /// <summary>
    /// Determines whether the owner is an accessor whose attribute lists must stay on the property's single line. When the
    /// accessor or its accessor list is the formatting root, which has no parent, the facts the entry point captured from
    /// the document say whether the property stood on one line. The root itself is part of the property's line, so the
    /// property still stands on one line only while an earlier rewrite has put no line break into the root, as the property
    /// itself would show it in the document
    /// </summary>
    /// <param name="owner">Owner node</param>
    /// <returns><see langword="true"/> when the accessor belongs to a single-line property; otherwise, <see langword="false"/></returns>
    private bool ShouldKeepAccessorListsSingleLine(SyntaxNode owner)
    {
        if (owner is not AccessorDeclarationSyntax accessorDeclaration)
        {
            return false;
        }

        if (StandsInOneLineProperty(accessorDeclaration))
        {
            return true;
        }

        var root = accessorDeclaration.Parent is AccessorListSyntax accessorList
                       ? accessorList
                       : (SyntaxNode)accessorDeclaration;

        return _context.RootConstruct.IsRootIn(root, RootConstruct.OneLineProperty)
               && LineBreakDetection.IsMultiLine(root) == false
               && root.GetLeadingTrivia().Any(SyntaxTriviaUtilities.EndsLine) == false;
    }

    #endregion // Methods

    #region CSharpSyntaxVisitor

    /// <inheritdoc/>
    public override SyntaxNode Visit(SyntaxNode node)
    {
        _cancellationToken.ThrowIfCancellationRequested();

        // Resolved from the original node while its parent chain is intact: the merge step rebuilds
        // the accessor and detaches it, which otherwise hides the single-line property context
        var keepAccessorListsSingleLine = ShouldKeepAccessorListsSingleLine(node);
        var updated = base.Visit(node);

        if (updated == null)
        {
            return null;
        }

        return FormatAttributeLists(updated, keepAccessorListsSingleLine);
    }

    #endregion // CSharpSyntaxVisitor
}