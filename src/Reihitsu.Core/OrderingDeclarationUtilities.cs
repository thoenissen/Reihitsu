using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods for declaration ordering analyzers and code fixes
/// </summary>
public static class OrderingDeclarationUtilities
{
    #region Constants

    /// <summary>
    /// The largest number of build configurations of one region of a type's body that the member-order guard reads.
    /// A region with more counts as reordered, so no fix moves the members of its type
    /// </summary>
    private const int MaximumConfigurationCount = 256;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Checks whether the declaration explicitly declares accessibility
    /// </summary>
    /// <param name="modifiers">Modifiers</param>
    /// <returns><see langword="true"/> if an accessibility modifier is present</returns>
    public static bool HasExplicitAccessibilityModifier(SyntaxTokenList modifiers)
    {
        return DeclarationModifierUtilities.HasAccessibilityModifier(modifiers);
    }

    /// <summary>
    /// Gets the accessibility group of the declaration
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The accessibility group</returns>
    public static OrderingAccessibilityGroup GetAccessibilityGroup(MemberDeclarationSyntax memberDeclaration)
    {
        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);
        var hasFile = modifiers.Any(SyntaxKind.FileKeyword);
        var hasPublic = modifiers.Any(SyntaxKind.PublicKeyword);
        var hasPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);
        var hasProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
        var hasInternal = modifiers.Any(SyntaxKind.InternalKeyword);

        if (hasFile)
        {
            return OrderingAccessibilityGroup.File;
        }

        if (hasPublic)
        {
            return OrderingAccessibilityGroup.Public;
        }

        if (hasProtected && hasInternal)
        {
            return OrderingAccessibilityGroup.ProtectedInternal;
        }

        if (hasPrivate && hasProtected)
        {
            return OrderingAccessibilityGroup.PrivateProtected;
        }

        if (hasInternal)
        {
            return OrderingAccessibilityGroup.Internal;
        }

        if (hasProtected)
        {
            return OrderingAccessibilityGroup.Protected;
        }

        if (hasPrivate)
        {
            return OrderingAccessibilityGroup.Private;
        }

        return OrderingAccessibilityGroup.None;
    }

    /// <summary>
    /// Gets the effective accessibility group used to order readonly fields, treating an interface member without
    /// an accessibility modifier as public
    /// </summary>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The effective accessibility group for readonly ordering</returns>
    public static OrderingAccessibilityGroup GetEffectiveAccessibilityGroupForReadonlyOrdering(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberDeclaration)
    {
        var accessibilityGroup = GetAccessibilityGroup(memberDeclaration);

        return typeDeclaration.IsKind(SyntaxKind.InterfaceDeclaration)
               && accessibilityGroup == OrderingAccessibilityGroup.None
                   ? OrderingAccessibilityGroup.Public
                   : accessibilityGroup;
    }

    /// <summary>
    /// Gets the member kind group of the declaration
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>The member kind group</returns>
    public static OrderingMemberKindGroup GetMemberKind(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax => OrderingMemberKindGroup.Type,
                   DelegateDeclarationSyntax => OrderingMemberKindGroup.Delegate,
                   FieldDeclarationSyntax => OrderingMemberKindGroup.Field,
                   ConstructorDeclarationSyntax => OrderingMemberKindGroup.Constructor,
                   DestructorDeclarationSyntax => OrderingMemberKindGroup.Destructor,
                   PropertyDeclarationSyntax => OrderingMemberKindGroup.Property,
                   IndexerDeclarationSyntax => OrderingMemberKindGroup.Indexer,
                   EventDeclarationSyntax => OrderingMemberKindGroup.Event,
                   EventFieldDeclarationSyntax => OrderingMemberKindGroup.EventField,
                   MethodDeclarationSyntax => OrderingMemberKindGroup.Method,
                   OperatorDeclarationSyntax => OrderingMemberKindGroup.Operator,
                   ConversionOperatorDeclarationSyntax => OrderingMemberKindGroup.ConversionOperator,
                   _ => OrderingMemberKindGroup.Unknown,
               };
    }

    /// <summary>
    /// Determines whether the declaration is static
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is static</returns>
    public static bool IsStatic(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration is OperatorDeclarationSyntax
                              or ConversionOperatorDeclarationSyntax)
        {
            return true;
        }

        return DeclarationModifierUtilities.GetModifiers(memberDeclaration).Any(SyntaxKind.StaticKeyword);
    }

    /// <summary>
    /// Determines whether the declaration is a const field
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration is a const field</returns>
    public static bool IsConst(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration is FieldDeclarationSyntax fieldDeclaration && fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword);
    }

    /// <summary>
    /// Determines whether a conditional directive — <c>#if</c>, <c>#elif</c>, <c>#else</c>, or <c>#endif</c> — or code
    /// that such a directive excludes lies inside the declaration's modifier list or between its last modifier and the
    /// token behind it, so that another build configuration compiles other modifiers (SHARED-DIAG-008)
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if a conditional directive splits the modifiers</returns>
    public static bool HasModifiersSplitByConditionalDirective(MemberDeclarationSyntax memberDeclaration)
    {
        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);

        if (modifiers.Count == 0)
        {
            return false;
        }

        var span = TextSpan.FromBounds(modifiers.First().SpanStart, modifiers.Last().GetNextToken().SpanStart);

        return SyntaxTriviaUtilities.ContainsConditionalCompilationBoundary(memberDeclaration, span);
    }

    /// <summary>
    /// Gets the preferred diagnostic location for a declaration
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns>Diagnostic location</returns>
    public static Location GetDiagnosticLocation(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.Identifier.GetLocation(),
                   DelegateDeclarationSyntax delegateDeclaration => delegateDeclaration.Identifier.GetLocation(),
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.GetLocation(),
                   DestructorDeclarationSyntax destructorDeclaration => destructorDeclaration.Identifier.GetLocation(),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.GetLocation(),
                   IndexerDeclarationSyntax indexerDeclaration => indexerDeclaration.ThisKeyword.GetLocation(),
                   EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier.GetLocation(),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables[0].Identifier.GetLocation(),
                   MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.GetLocation(),
                   OperatorDeclarationSyntax operatorDeclaration => operatorDeclaration.OperatorToken.GetLocation(),
                   ConversionOperatorDeclarationSyntax conversionOperatorDeclaration => conversionOperatorDeclaration.OperatorKeyword.GetLocation(),
                   _ => memberDeclaration.GetLocation(),
               };
    }

    /// <summary>
    /// Finds the containing type declaration and member declaration for a diagnostic
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="typeDeclaration">Containing type declaration</param>
    /// <param name="memberDeclaration">Containing member declaration</param>
    /// <returns><see langword="true"/> if both declarations were found</returns>
    public static bool TryGetContainingTypeAndMember(SyntaxNode root, Diagnostic diagnostic, out TypeDeclarationSyntax typeDeclaration, out MemberDeclarationSyntax memberDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        memberDeclaration = diagnosticNode?.AncestorsAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .FirstOrDefault(obj => obj.Parent is TypeDeclarationSyntax);
        typeDeclaration = memberDeclaration?.Parent as TypeDeclarationSyntax;

        return typeDeclaration != null && memberDeclaration != null;
    }

    /// <summary>
    /// Finds the declaration associated with a modifier-based diagnostic
    /// </summary>
    /// <param name="root">Root node</param>
    /// <param name="diagnostic">Diagnostic</param>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if a declaration was found</returns>
    public static bool TryGetMemberDeclaration(SyntaxNode root, Diagnostic diagnostic, out MemberDeclarationSyntax memberDeclaration)
    {
        var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;

        memberDeclaration = diagnosticNode?.AncestorsAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .FirstOrDefault(obj => obj is BaseTypeDeclarationSyntax
                                                                     or DelegateDeclarationSyntax
                                                                     or FieldDeclarationSyntax
                                                                     or ConstructorDeclarationSyntax
                                                                     or DestructorDeclarationSyntax
                                                                     or PropertyDeclarationSyntax
                                                                     or EventDeclarationSyntax
                                                                     or EventFieldDeclarationSyntax
                                                                     or MethodDeclarationSyntax
                                                                     or IndexerDeclarationSyntax
                                                                     or OperatorDeclarationSyntax
                                                                     or ConversionOperatorDeclarationSyntax);

        return memberDeclaration != null;
    }

    /// <summary>
    /// Moves a member before another member within the same type declaration. Blank-line separators are kept at
    /// the position they already occupied via
    /// <see cref="OrderingMoveSafety.MoveNodeBeforePreservingSeparators{TNode}"/>
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns>The updated type declaration</returns>
    public static TypeDeclarationSyntax MoveMemberBefore(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        var members = typeDeclaration.Members;
        var memberToMoveIndex = members.IndexOf(memberToMove);
        var targetMemberIndex = members.IndexOf(targetMember);

        if (memberToMoveIndex < 0
            || targetMemberIndex < 0
            || memberToMoveIndex <= targetMemberIndex)
        {
            return typeDeclaration;
        }

        var updatedMembers = OrderingMoveSafety.MoveNodeBeforePreservingSeparators(members, memberToMove, targetMember);

        return typeDeclaration.WithMembers(updatedMembers);
    }

    /// <summary>
    /// Determines whether moving a member before another member would relocate a preprocessor directive away from
    /// the code it governs, splitting a conditional-compilation pair or scrambling region structure.
    /// The analysis is shared with the accessor guard through
    /// <see cref="OrderingMoveSafety.MoveRangeContainsDirectives{TNode}"/>
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move would relocate a preprocessor directive</returns>
    public static bool MoveRangeContainsDirectives(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        return OrderingMoveSafety.MoveRangeContainsDirectives(typeDeclaration, typeDeclaration.Members, memberToMove, targetMember);
    }

    /// <summary>
    /// Determines whether moving a member before another member puts two members of the type into the other order
    /// where their order is behavior, as
    /// <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, Func{int, int})"/> decides it. The move
    /// keeps the text in front of the target member, the moved member with its leading trivia, the text from the
    /// target member up to the moved member, and the text behind the moved member, each in itself, and puts the moved
    /// member in front of the target member's leading trivia. Code that an <c>#if</c> branch excludes in the moved
    /// member's leading trivia or inside it therefore moves with it, and such code from the target member up to the
    /// moved member is crossed
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="typeSymbol">Symbol of the type, which decides whether the order of its instance fields sets its layout</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move changes initializer execution order or the layout</returns>
    public static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        return ChangesMemberOrder(typeDeclaration, FieldOrderSetsLayout(typeSymbol), memberToMove, targetMember);
    }

    /// <summary>
    /// Determines whether a reorder of the type's body puts two members of the type into the other order where
    /// their order is behavior: two members of one context — both static or both instance — that each have a runtime
    /// initializer (SHARED-ORDER-007), or two instance fields of a type whose field order sets its layout, which the
    /// type's symbol decides. Members in code that an <c>#if</c> branch excludes count as well, at the position of
    /// that code, because another build configuration compiles them, and a member whose declaration a conditional
    /// directive splits counts with what any configuration gives it (SHARED-FIX-003). The reorder is given as the
    /// rank, after the reorder, of the part of the body that holds a position; code within one part keeps its order
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="typeSymbol">Symbol of the type, which decides whether the order of its instance fields sets its layout</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    public static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, Func<int, int> getNewRank)
    {
        return ChangesMemberOrder(typeDeclaration, FieldOrderSetsLayout(typeSymbol), getNewRank);
    }

    /// <summary>
    /// Determines whether moving a member before another member would change the execution order of
    /// field, event field, or property initializers. Initializers run in declaration order within their execution
    /// context (static initializers in the static constructor, instance initializers in the instance
    /// constructor), so reordering a member carrying an initializer past another member of the same
    /// context that also carries an initializer can silently change observable values. The members are ranked as
    /// <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, MemberDeclarationSyntax, MemberDeclarationSyntax)"/>
    /// ranks them, without the layout check that only the type's symbol decides, so no code fix asks this question:
    /// a code fix asks <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, MemberDeclarationSyntax, MemberDeclarationSyntax)"/>
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move would change initializer execution order</returns>
    internal static bool ChangesInitializerExecutionOrder(TypeDeclarationSyntax typeDeclaration, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        return ChangesMemberOrder(typeDeclaration, false, memberToMove, targetMember);
    }

    /// <summary>
    /// Determines whether moving a member before another member puts two members of the type into the other order
    /// where their order is behavior, ranking the members as
    /// <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, MemberDeclarationSyntax, MemberDeclarationSyntax)"/>
    /// describes
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        var members = typeDeclaration.Members;
        var memberToMoveIndex = members.IndexOf(memberToMove);
        var targetMemberIndex = members.IndexOf(targetMember);

        if (memberToMoveIndex < 0
            || targetMemberIndex < 0
            || memberToMoveIndex <= targetMemberIndex)
        {
            return false;
        }

        var targetStart = targetMember.FullSpan.Start;
        var movedSpan = memberToMove.FullSpan;

        int GetNewRank(int position)
        {
            if (position < targetStart)
            {
                return 0;
            }

            if (movedSpan.Contains(position))
            {
                return 1;
            }

            return position < movedSpan.Start
                       ? 2
                       : 3;
        }

        return ChangesMemberOrder(typeDeclaration, fieldOrderSetsLayout, GetNewRank);
    }

    /// <summary>
    /// Determines whether a reorder of the type's body puts two members of the type into the other order where
    /// their order is behavior, as
    /// <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, Func{int, int})"/> describes, with the
    /// layout check given as a flag. A body whose conditional groups cannot be read per build configuration
    /// (<see cref="TryGetOrderedMembers"/>) counts as changed, so no fix moves its members
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, Func<int, int> getNewRank)
    {
        if (TryGetOrderedMembers(typeDeclaration, out var members) == false)
        {
            return true;
        }

        bool ChangesOrder(Func<OrderedMember, bool> isOrdered)
        {
            var writtenOrder = members.Where(isOrdered)
                                      .Select(member => member.Position)
                                      .OrderBy(position => position)
                                      .ToList();
            var newOrder = writtenOrder.OrderBy(getNewRank)
                                       .ThenBy(position => position);

            return writtenOrder.SequenceEqual(newOrder) == false;
        }

        return ChangesOrder(member => member.HasStaticInitializer)
               || ChangesOrder(member => member.HasInstanceInitializer)
               || (fieldOrderSetsLayout && ChangesOrder(member => member.IsLayoutField));
    }

    /// <summary>
    /// Gets what every member of the type's body, in any build configuration, contributes to the order of the
    /// initializers and of the layout fields, with its position. A member that no conditional group touches is read
    /// from the active code. Each conditional group of the body is read once, as a whole with all its branches; the
    /// groups that touch a common member, and the members they touch, form one region, which is read per build
    /// configuration (<see cref="TryAddOrderedMembersOfRegion"/>). A group touches a member when it lies in the
    /// member's declaration or in its leading trivia. A group inside a block, an expression body, or the body of a
    /// nested type changes nothing that a member of this type contributes and is not read
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="orderedMembers">The ordered members</param>
    /// <returns><see langword="false"/> if a region of the body has more configurations than <see cref="MaximumConfigurationCount"/></returns>
    private static bool TryGetOrderedMembers(TypeDeclarationSyntax typeDeclaration, out List<OrderedMember> orderedMembers)
    {
        orderedMembers = [];

        var regions = new List<(TextSpan Span, List<ConditionalGroup> Groups)>();

        if (typeDeclaration.ContainsDirectives
            && typeDeclaration.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken))
        {
            regions = GetConditionalRegions(typeDeclaration, GetConditionalGroups(typeDeclaration));
        }

        foreach (var member in typeDeclaration.Members.Where(member => regions.Exists(region => region.Span.Contains(member.FullSpan)) == false))
        {
            orderedMembers.Add(CreateOrderedMember(member.SpanStart, member));
        }

        foreach (var (span, groups) in regions)
        {
            if (TryAddOrderedMembersOfRegion(typeDeclaration, span, groups, orderedMembers) == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the conditional groups of the type's body, each with all its directives and with the groups nested in its
    /// branches. The directives inside excluded code count as well, because the parser reads them as directives. A
    /// group whose <c>#if</c> lies in front of the body starts at the start of the body, and a group whose <c>#endif</c>
    /// lies behind it ends at its end, as when an <c>#if</c> chooses between two headers of the type, each with its
    /// opening brace
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The outermost groups of the body, in source order</returns>
    private static List<ConditionalGroup> GetConditionalGroups(TypeDeclarationSyntax typeDeclaration)
    {
        var groups = new List<ConditionalGroup>();
        var bodySpan = TextSpan.FromBounds(typeDeclaration.OpenBraceToken.Span.End, Math.Max(typeDeclaration.OpenBraceToken.Span.End, typeDeclaration.CloseBraceToken.SpanStart));
        var openGroups = new Stack<ConditionalGroup>();

        foreach (var trivia in typeDeclaration.DescendantTrivia(node => node.ContainsDirectives))
        {
            if (bodySpan.Contains(trivia.SpanStart) == false)
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia))
            {
                var group = new ConditionalGroup(trivia.FullSpan);

                group.Directives.Add(trivia);

                if (openGroups.Count > 0)
                {
                    openGroups.Peek().Children.Add(group);
                }
                else
                {
                    groups.Add(group);
                }

                openGroups.Push(group);
            }
            else if (trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
                     || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia)
                     || trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
            {
                if (openGroups.Count == 0)
                {
                    var unopenedGroup = new ConditionalGroup(new TextSpan(bodySpan.Start, 0));

                    groups.Insert(0, unopenedGroup);
                    openGroups.Push(unopenedGroup);
                }

                var group = trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia)
                                ? openGroups.Pop()
                                : openGroups.Peek();

                group.AddDirective(trivia);
            }
        }

        foreach (var unclosedGroup in openGroups)
        {
            unclosedGroup.Boundaries.Add(new TextSpan(bodySpan.End, 0));
        }

        return groups;
    }

    /// <summary>
    /// Gets the regions of the type's body that are read per build configuration: each outermost conditional group
    /// that is not read as text (<see cref="IsReadAsText"/>), extended over the members it touches, with the regions
    /// that share a member merged
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="groups">The outermost groups of the body, in source order</param>
    /// <returns>The regions with their outermost groups, in source order</returns>
    private static List<(TextSpan Span, List<ConditionalGroup> Groups)> GetConditionalRegions(TypeDeclarationSyntax typeDeclaration, List<ConditionalGroup> groups)
    {
        var regions = new List<(TextSpan Span, List<ConditionalGroup> Groups)>();

        foreach (var group in groups.Where(group => IsReadAsText(typeDeclaration, group) == false))
        {
            var span = group.Span;

            foreach (var memberSpan in typeDeclaration.Members.Where(member => member.FullSpan.OverlapsWith(group.Span)).Select(member => member.FullSpan))
            {
                span = TextSpan.FromBounds(Math.Min(span.Start, memberSpan.Start), Math.Max(span.End, memberSpan.End));
            }

            var regionGroups = new List<ConditionalGroup>
                               {
                                   group
                               };

            while (regions.Count > 0
                   && regions[regions.Count - 1].Span.OverlapsWith(span))
            {
                var previousRegion = regions[regions.Count - 1];

                regions.RemoveAt(regions.Count - 1);
                span = TextSpan.FromBounds(Math.Min(span.Start, previousRegion.Span.Start), Math.Max(span.End, previousRegion.Span.End));
                regionGroups.InsertRange(0, previousRegion.Groups);
            }

            regions.Add((span, regionGroups));
        }

        return regions;
    }

    /// <summary>
    /// Determines whether a conditional group is read as text rather than per build configuration, because no
    /// configuration of it changes what a member of this type contributes: every directive of the group lies inside
    /// the same block, expression body, or body of a nested type, whose code holds statements, expressions, or the
    /// members of another type rather than a part of a member of this type; or every configuration of the group only
    /// writes text inside brackets (<see cref="WritesOnlyInsideBrackets"/>)
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="group">Conditional group</param>
    /// <returns><see langword="true"/> if the group is read as text</returns>
    private static bool IsReadAsText(TypeDeclarationSyntax typeDeclaration, ConditionalGroup group)
    {
        var body = GetEnclosingBody(typeDeclaration, group.Directives[0]);

        if (body != null
            && group.Directives.TrueForAll(directive => GetEnclosingBody(typeDeclaration, directive) == body))
        {
            return true;
        }

        return WritesOnlyInsideBrackets(typeDeclaration, group);
    }

    /// <summary>
    /// Determines whether every configuration of a conditional group only writes text inside brackets, which changes no
    /// modifier, initializer, accessor, or member boundary of a member of this type. Each piece of text between two
    /// boundaries of the group, or of the groups nested in it, closes every parenthesis, bracket, and brace that it
    /// opens and none that it does not open, and either the group lies inside a parenthesis, bracket, or brace of a
    /// member's active code other than an accessor list, such as an argument list or a collection initializer, or each
    /// piece holds nothing but square brackets and their content outside them, such as an attribute list
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="group">Conditional group</param>
    /// <returns><see langword="true"/> if every configuration of the group only writes text inside brackets</returns>
    private static bool WritesOnlyInsideBrackets(TypeDeclarationSyntax typeDeclaration, ConditionalGroup group)
    {
        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;
        var text = typeDeclaration.SyntaxTree.GetText();
        var boundaries = group.GetAllBoundaries();
        var holdsOnlySquareBrackets = true;

        for (var index = 0; index + 1 < boundaries.Count; index++)
        {
            var tokens = SyntaxFactory.ParseTokens(text.ToString(TextSpan.FromBounds(boundaries[index].End, boundaries[index + 1].Start)), options: options)
                                      .Where(token => token.IsKind(SyntaxKind.EndOfFileToken) == false)
                                      .ToList();
            var depth = 0;

            foreach (var token in tokens)
            {
                var delta = GetNestingDelta(token);

                if (depth == 0
                    && token.IsKind(SyntaxKind.OpenBracketToken) == false
                    && token.IsKind(SyntaxKind.CloseBracketToken) == false)
                {
                    holdsOnlySquareBrackets = false;
                }

                depth += delta;

                if (depth < 0)
                {
                    return false;
                }
            }

            if (depth != 0)
            {
                return false;
            }
        }

        return holdsOnlySquareBrackets
               || LiesInsideBrackets(typeDeclaration, group.Directives[0].SpanStart);
    }

    /// <summary>
    /// Determines whether a position lies inside a parenthesis, bracket, or brace of a member's active code, other than
    /// the braces of an accessor list, whose content decides whether a property has a backing field
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="position">Position</param>
    /// <returns><see langword="true"/> if the innermost bracket around the position is such a bracket</returns>
    private static bool LiesInsideBrackets(TypeDeclarationSyntax typeDeclaration, int position)
    {
        var member = typeDeclaration.Members.FirstOrDefault(obj => obj.Span.Contains(position));

        if (member == null)
        {
            return false;
        }

        var openBrackets = new Stack<SyntaxToken>();

        foreach (var token in member.DescendantTokens())
        {
            if (token.SpanStart >= position)
            {
                break;
            }

            var delta = GetNestingDelta(token);

            if (delta > 0)
            {
                openBrackets.Push(token);
            }
            else if (delta < 0
                     && openBrackets.Count > 0)
            {
                openBrackets.Pop();
            }
        }

        return openBrackets.Count > 0
               && openBrackets.Peek().Parent is not AccessorListSyntax;
    }

    /// <summary>
    /// Gets the outermost block, expression body, or body of a nested type inside the type's body that holds a
    /// directive
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="directive">Directive</param>
    /// <returns>The enclosing body, or <see langword="null"/> if the directive lies in no such body</returns>
    private static SyntaxNode GetEnclosingBody(TypeDeclarationSyntax typeDeclaration, SyntaxTrivia directive)
    {
        var position = directive.SpanStart;
        SyntaxNode body = null;

        for (var node = directive.Token.Parent; node != null && node != typeDeclaration; node = node.Parent)
        {
            var holdsDirective = node switch
                                 {
                                     BlockSyntax block => position >= block.OpenBraceToken.Span.End && position < block.CloseBraceToken.SpanStart,
                                     ArrowExpressionClauseSyntax arrowExpressionClause => position >= arrowExpressionClause.ArrowToken.Span.End && position < arrowExpressionClause.Span.End,
                                     BaseTypeDeclarationSyntax baseTypeDeclaration => baseTypeDeclaration.OpenBraceToken.IsKind(SyntaxKind.OpenBraceToken)
                                                                                      && position >= baseTypeDeclaration.OpenBraceToken.Span.End
                                                                                      && position < baseTypeDeclaration.CloseBraceToken.SpanStart,
                                     _ => false,
                                 };

            if (holdsDirective)
            {
                body = node;
            }
        }

        return body;
    }

    /// <summary>
    /// Gets the build configurations of conditional groups that lie side by side: each combination of one
    /// configuration of every group
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="groups">Conditional groups</param>
    /// <returns>For each configuration, the spans of the text it does not compile; an empty list if there are more than <see cref="MaximumConfigurationCount"/></returns>
    private static List<List<TextSpan>> GetConfigurations(TypeDeclarationSyntax typeDeclaration, IEnumerable<ConditionalGroup> groups)
    {
        var configurations = new List<List<TextSpan>>
                             {
                                 new()
                             };

        foreach (var group in groups)
        {
            var groupConfigurations = GetConfigurations(typeDeclaration, group);

            if (groupConfigurations.Count == 0
                || configurations.Count * groupConfigurations.Count > MaximumConfigurationCount)
            {
                return [];
            }

            configurations = configurations.SelectMany(configuration => groupConfigurations.Select(groupConfiguration => configuration.Concat(groupConfiguration).ToList()))
                                           .ToList();
        }

        return configurations;
    }

    /// <summary>
    /// Gets the build configurations of a conditional group: for each branch, the configurations that compile it,
    /// with every configuration of the groups nested in it that are not read as text, and, for a group without
    /// <c>#else</c>, the configuration that compiles none of its branches. No configuration compiles a directive of
    /// the group
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="group">Conditional group</param>
    /// <returns>For each configuration, the spans of the text it does not compile; an empty list if there are more than <see cref="MaximumConfigurationCount"/></returns>
    private static List<List<TextSpan>> GetConfigurations(TypeDeclarationSyntax typeDeclaration, ConditionalGroup group)
    {
        var directiveSpans = group.Boundaries;
        var branchSpans = group.GetBranchSpans();
        var configurations = new List<List<TextSpan>>();

        for (var branchIndex = 0; branchIndex < branchSpans.Count; branchIndex++)
        {
            var branchSpan = branchSpans[branchIndex];
            var excludedSpans = directiveSpans.Concat(branchSpans.Where((_, index) => index != branchIndex)).ToList();
            var nestedConfigurations = GetConfigurations(typeDeclaration, group.Children.Where(child => branchSpan.Contains(child.Span) && IsReadAsText(typeDeclaration, child) == false));

            if (nestedConfigurations.Count == 0
                || configurations.Count + nestedConfigurations.Count > MaximumConfigurationCount)
            {
                return [];
            }

            configurations.AddRange(nestedConfigurations.Select(nestedConfiguration => excludedSpans.Concat(nestedConfiguration).ToList()));
        }

        if (group.HasElseBranch == false)
        {
            configurations.Add(directiveSpans.Concat(branchSpans).ToList());
        }

        return configurations.Count > MaximumConfigurationCount
                   ? []
                   : configurations;
    }

    /// <summary>
    /// Gets the nesting depth of each token of a token sequence, starting at a given depth. A parenthesis, bracket, or
    /// brace gets the depth outside of it, so the tokens outside every bracket have depth zero
    /// </summary>
    /// <param name="tokens">Tokens</param>
    /// <param name="initialDepth">Depth in front of the first token</param>
    /// <returns>The depth of each token</returns>
    private static List<int> GetNestingDepths(List<SyntaxToken> tokens, int initialDepth)
    {
        var depths = new List<int>(tokens.Count);
        var depth = initialDepth;

        foreach (var token in tokens)
        {
            var delta = GetNestingDelta(token);

            if (delta < 0)
            {
                depth += delta;
            }

            depths.Add(depth);

            if (delta > 0)
            {
                depth += delta;
            }
        }

        return depths;
    }

    /// <summary>
    /// Gets how a token changes the nesting depth: an opening parenthesis, bracket, or brace opens a level, and a closing
    /// one closes it
    /// </summary>
    /// <param name="token">Token</param>
    /// <returns><c>1</c>, <c>-1</c>, or <c>0</c></returns>
    private static int GetNestingDelta(SyntaxToken token)
    {
        return token.Kind() switch
               {
                   SyntaxKind.OpenParenToken or SyntaxKind.OpenBracketToken or SyntaxKind.OpenBraceToken => 1,
                   SyntaxKind.CloseParenToken or SyntaxKind.CloseBracketToken or SyntaxKind.CloseBraceToken => -1,
                   _ => 0,
               };
    }

    /// <summary>
    /// Determines whether a token sequence holds a token of the given kind outside every parenthesis, bracket, and brace
    /// </summary>
    /// <param name="tokens">Tokens</param>
    /// <param name="kind">Kind of the token</param>
    /// <returns><see langword="true"/> if such a token lies at depth zero</returns>
    private static bool ContainsTopLevelToken(List<SyntaxToken> tokens, SyntaxKind kind)
    {
        var depths = GetNestingDepths(tokens, 0);

        return Enumerable.Range(0, tokens.Count).Any(index => depths[index] == 0 && tokens[index].IsKind(kind));
    }

    /// <summary>
    /// Gets what a member contributes to the order of the initializers and of the layout fields, read from its syntax
    /// </summary>
    /// <param name="position">Position the member counts at</param>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The ordered member</returns>
    private static OrderedMember CreateOrderedMember(int position, MemberDeclarationSyntax memberDeclaration)
    {
        var hasInitializer = HasInitializer(memberDeclaration);
        var isStatic = IsStatic(memberDeclaration);

        return new OrderedMember(position, hasInitializer && isStatic, hasInitializer && isStatic == false, IsLayoutField(memberDeclaration));
    }

    /// <summary>
    /// Determines whether the order of the type's instance fields sets its memory layout: a struct, which has
    /// sequential layout unless it declares <c>LayoutKind.Auto</c> or <c>LayoutKind.Explicit</c>, and any other class
    /// declared with <c>[StructLayout(LayoutKind.Sequential)]</c>. Under explicit layout each field's
    /// <c>[FieldOffset]</c> sets its offset. The attributes of every part of a partial type count, and a layout
    /// argument whose value is not known counts as sequential layout
    /// </summary>
    /// <param name="typeSymbol">Type</param>
    /// <returns><see langword="true"/> if reordering the instance fields changes the layout</returns>
    private static bool FieldOrderSetsLayout(INamedTypeSymbol typeSymbol)
    {
        var isStruct = typeSymbol.TypeKind == TypeKind.Struct;

        if (isStruct == false && typeSymbol.TypeKind != TypeKind.Class)
        {
            return false;
        }

        var layoutAttribute = typeSymbol.GetAttributes().FirstOrDefault(IsStructLayoutAttribute);

        if (layoutAttribute == null)
        {
            return isStruct;
        }

        var layoutKind = layoutAttribute.ConstructorArguments.Length > 0 && layoutAttribute.ConstructorArguments[0].Value is { } value
                             ? Convert.ToInt32(value, CultureInfo.InvariantCulture)
                             : (int?)null;

        return layoutKind is not ((int)LayoutKind.Auto or (int)LayoutKind.Explicit);
    }

    /// <summary>
    /// Determines whether the declaration declares an instance field of the type's layout: an instance field, an
    /// instance field-like event, or an instance property that the compiler gives a backing field, because it is an
    /// auto-property or uses the <c>field</c> keyword
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration declares an instance field</returns>
    private static bool IsLayoutField(MemberDeclarationSyntax memberDeclaration)
    {
        return IsStatic(memberDeclaration) == false
               && IsConst(memberDeclaration) == false
               && DeclaresLayoutField(memberDeclaration);
    }

    /// <summary>
    /// Determines whether the declaration declares a field when it is neither static nor constant: a field, a
    /// field-like event, or a property that the compiler gives a backing field
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration declares a field</returns>
    private static bool DeclaresLayoutField(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   FieldDeclarationSyntax => true,
                   EventFieldDeclarationSyntax => true,
                   PropertyDeclarationSyntax propertyDeclaration => HasBackingField(propertyDeclaration),
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the declaration carries a runtime initializer
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration carries a runtime initializer</returns>
    private static bool HasInitializer(MemberDeclarationSyntax memberDeclaration)
    {
        return IsConst(memberDeclaration) == false
               && HasInitializerSyntax(memberDeclaration);
    }

    /// <summary>
    /// Determines whether the declaration of a field, event field, or property carries an initializer, which is a
    /// runtime initializer unless the declaration is constant
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration carries an initializer</returns>
    private static bool HasInitializerSyntax(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration.Variables.Any(variable => variable.Initializer != null),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables.Any(variable => variable.Initializer != null),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Initializer != null,
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the property has a backing field the compiler declares: an auto-property, whose accessors
    /// have no body, or a property that uses the <c>field</c> keyword. The parser reads <c>field</c> as the keyword
    /// only from C# 14 on; before, it is an identifier that names a member. Abstract and extern properties have none,
    /// and neither has the bodiless defining declaration of a partial property
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <returns><see langword="true"/> if the property has a backing field</returns>
    private static bool HasBackingField(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
            || propertyDeclaration.Modifiers.Any(SyntaxKind.ExternKeyword))
        {
            return false;
        }

        if (propertyDeclaration.AccessorList is { } accessorList
            && accessorList.Accessors.All(accessor => accessor.Body == null && accessor.ExpressionBody == null))
        {
            return propertyDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) == false;
        }

        return propertyDeclaration.DescendantTokens().Any(token => token.IsKind(SyntaxKind.FieldKeyword));
    }

    /// <summary>
    /// Determines whether the attribute is <see cref="StructLayoutAttribute"/>
    /// </summary>
    /// <param name="attribute">Attribute</param>
    /// <returns><see langword="true"/> if the attribute is <c>StructLayout</c></returns>
    private static bool IsStructLayoutAttribute(AttributeData attribute)
    {
        return attribute.AttributeClass is { Name: nameof(StructLayoutAttribute) } attributeClass
               && attributeClass.ContainingNamespace?.ToDisplayString() == typeof(StructLayoutAttribute).Namespace;
    }

    /// <summary>
    /// Adds what a region of the type's body contributes, read per build configuration, to the order of the
    /// initializers and of the layout fields. Each configuration compiles one branch of each of the region's
    /// conditional groups: its text is the region with the directives and the other branches blanked, which keeps every
    /// position, and it is parsed as the body of a type. A member that configurations declare under one name counts
    /// with what any of them contributes (SHARED-ORDER-007), at each position where one of them declares it: a runtime
    /// initializer when a configuration that does not declare it <c>const</c> gives it one, the static context when
    /// such a configuration declares it <c>static</c>, the instance context when such a configuration declares it
    /// without <c>static</c>, and an instance field when a configuration declares it as one. Text of a configuration
    /// that parses as no complete member, such as the initializer of a property behind its accessor list, counts as a
    /// runtime initializer of both contexts when it holds an <c>=</c> outside every parenthesis, bracket, and brace,
    /// because it can complete a member of either. An <c>=</c> inside them, such as a named attribute argument or an
    /// assignment in an accessor body, is no initializer
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="span">Span of the region</param>
    /// <param name="groups">The outermost conditional groups of the region</param>
    /// <param name="orderedMembers">The ordered members to add to</param>
    /// <returns><see langword="false"/> if the region has more than <see cref="MaximumConfigurationCount"/> configurations</returns>
    private static bool TryAddOrderedMembersOfRegion(TypeDeclarationSyntax typeDeclaration, TextSpan span, List<ConditionalGroup> groups, List<OrderedMember> orderedMembers)
    {
        var configurations = GetConfigurations(typeDeclaration, groups);

        if (configurations.Count == 0)
        {
            return false;
        }

        const string prefix = "class C\n{\n";

        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;
        var text = typeDeclaration.SyntaxTree.GetText().ToString(span);
        var contributions = new List<(string Name, int Position, bool HasInitializer, bool IsStatic, bool IsInstance, bool IsLayoutField)>();

        // A parsed position, mapped back into the region; text that the parse appends behind the region counts at its end
        Func<int, int> getPosition = parsedPosition => Math.Max(span.Start, Math.Min(span.End - 1, span.Start + parsedPosition - prefix.Length));

        foreach (var excludedSpans in configurations)
        {
            var characters = text.ToCharArray();

            foreach (var excludedSpan in excludedSpans)
            {
                for (var position = excludedSpan.Start; position < excludedSpan.End; position++)
                {
                    var index = position - span.Start;

                    if (characters[index] != '\r'
                        && characters[index] != '\n')
                    {
                        characters[index] = ' ';
                    }
                }
            }

            var parsedType = SyntaxFactory.ParseCompilationUnit($"{prefix}{new string(characters)}\n}}", options: options)
                                          .Members
                                          .OfType<TypeDeclarationSyntax>()
                                          .FirstOrDefault();

            if (parsedType == null)
            {
                continue;
            }

            foreach (var member in parsedType.Members)
            {
                var isConst = IsConst(member);
                var isStatic = IsStatic(member);

                contributions.Add((GetMemberName(member),
                                   getPosition(member.SpanStart),
                                   isConst == false && HasInitializerSyntax(member),
                                   isConst == false && isStatic,
                                   isConst == false && isStatic == false,
                                   IsLayoutField(member)));
            }

            foreach (var incompleteTokens in parsedType.Members
                                                       .OfType<IncompleteMemberSyntax>()
                                                       .Select(member => (member.SpanStart, Tokens: member.DescendantTokens()))
                                                       .Concat(parsedType.DescendantTrivia(descendIntoTrivia: true)
                                                                         .Where(trivia => trivia.IsKind(SyntaxKind.SkippedTokensTrivia))
                                                                         .Select(trivia => (trivia.SpanStart, Tokens: trivia.GetStructure()?.DescendantTokens() ?? [])))
                                                       .Where(incompleteTokens => ContainsTopLevelToken(incompleteTokens.Tokens.ToList(), SyntaxKind.EqualsToken)))
            {
                contributions.Add((null, getPosition(incompleteTokens.SpanStart), true, true, true, false));
            }
        }

        foreach (var contribution in contributions)
        {
            var hasInitializer = contribution.HasInitializer;
            var isStatic = contribution.IsStatic;
            var isInstance = contribution.IsInstance;
            var isLayoutField = contribution.IsLayoutField;

            if (contribution.Name != null)
            {
                foreach (var sameMember in contributions.Where(other => other.Name == contribution.Name))
                {
                    hasInitializer |= sameMember.HasInitializer;
                    isStatic |= sameMember.IsStatic;
                    isInstance |= sameMember.IsInstance;
                    isLayoutField |= sameMember.IsLayoutField;
                }
            }

            orderedMembers.Add(new OrderedMember(contribution.Position, hasInitializer && isStatic, hasInitializer && isInstance, isLayoutField));
        }

        return true;
    }

    /// <summary>
    /// Gets the name under which a field, event field, or property is declared: the name of its first variable or its
    /// identifier
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The name, or <see langword="null"/> for any other member</returns>
    private static string GetMemberName(MemberDeclarationSyntax memberDeclaration)
    {
        return memberDeclaration switch
               {
                   FieldDeclarationSyntax fieldDeclaration when fieldDeclaration.Declaration.Variables.Count > 0 => fieldDeclaration.Declaration.Variables[0].Identifier.ValueText,
                   EventFieldDeclarationSyntax eventFieldDeclaration when eventFieldDeclaration.Declaration.Variables.Count > 0 => eventFieldDeclaration.Declaration.Variables[0].Identifier.ValueText,
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Identifier.ValueText,
                   _ => null,
               };
    }

    #endregion // Methods
}