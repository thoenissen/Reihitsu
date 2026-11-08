using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// A conditional group of a type's body: an <c>#if</c> directive with its <c>#elif</c>, <c>#else</c>, and
/// <c>#endif</c> directives, read as a whole with every branch, active or excluded, and with the conditional groups
/// nested in its branches. A group that the body does not open or close, because its <c>#if</c> lies in front of the
/// body or its <c>#endif</c> behind it, is bounded by the start or the end of the body instead
/// </summary>
internal sealed class ConditionalGroup
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="start">The span of the <c>#if</c> directive that opens the group, or an empty span at the start of the body</param>
    public ConditionalGroup(TextSpan start)
    {
        Boundaries = [start];
        Directives = [];
        Children = [];
    }

    #endregion // Constructor

    #region Properties

    /// <summary>
    /// The boundaries of the group's branches in source order: the span of each of its directives, with an empty span
    /// at the start or the end of the body in place of an <c>#if</c> or <c>#endif</c> that lies outside it
    /// </summary>
    public List<TextSpan> Boundaries { get; }

    /// <summary>
    /// The directives of the group that lie in the body, in source order
    /// </summary>
    public List<SyntaxTrivia> Directives { get; }

    /// <summary>
    /// The groups nested in the branches of this group, in source order
    /// </summary>
    public List<ConditionalGroup> Children { get; }

    /// <summary>
    /// The text from the first to the last boundary of the group
    /// </summary>
    public TextSpan Span => TextSpan.FromBounds(Boundaries[0].Start, Boundaries[Boundaries.Count - 1].End);

    /// <summary>
    /// Whether the group has an <c>#else</c> branch. A group without one has a further, empty branch: the
    /// configuration that compiles none of its branches
    /// </summary>
    public bool HasElseBranch => Directives.Exists(directive => directive.IsKind(SyntaxKind.ElseDirectiveTrivia));

    /// <summary>
    /// Whether the body both opens and closes the group: its first directive is an <c>#if</c> and its last an
    /// <c>#endif</c>, both in the body
    /// </summary>
    public bool IsComplete => Directives.Count > 1
                              && Directives[0].IsKind(SyntaxKind.IfDirectiveTrivia)
                              && Directives[Directives.Count - 1].IsKind(SyntaxKind.EndIfDirectiveTrivia)
                              && Boundaries.Count == Directives.Count;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Adds a directive of the group behind its last boundary
    /// </summary>
    /// <param name="directive">Directive</param>
    public void AddDirective(SyntaxTrivia directive)
    {
        Directives.Add(directive);
        Boundaries.Add(directive.FullSpan);
    }

    /// <summary>
    /// Gets the text of each branch: the text between a boundary of the group and the boundary behind it
    /// </summary>
    /// <returns>The spans of the branches, in source order</returns>
    public List<TextSpan> GetBranchSpans()
    {
        var branchSpans = new List<TextSpan>(Boundaries.Count - 1);

        for (var index = 0; index + 1 < Boundaries.Count; index++)
        {
            branchSpans.Add(TextSpan.FromBounds(Boundaries[index].End, Boundaries[index + 1].Start));
        }

        return branchSpans;
    }

    /// <summary>
    /// Gets the group and every group nested in it, at any depth
    /// </summary>
    /// <returns>The group, followed by its nested groups</returns>
    public IEnumerable<ConditionalGroup> GetSelfAndNestedGroups()
    {
        return Children.SelectMany(child => child.GetSelfAndNestedGroups())
                       .Prepend(this);
    }

    /// <summary>
    /// Gets the boundaries of the group and of every group nested in it, in source order
    /// </summary>
    /// <returns>The boundaries</returns>
    public List<TextSpan> GetAllBoundaries()
    {
        return Boundaries.Concat(Children.SelectMany(child => child.GetAllBoundaries()))
                         .OrderBy(boundary => boundary.Start)
                         .ToList();
    }

    #endregion // Methods
}