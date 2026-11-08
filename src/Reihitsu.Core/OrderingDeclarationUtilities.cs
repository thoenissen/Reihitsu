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
    /// layout check given as a flag
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, Func<int, int> getNewRank)
    {
        var members = GetOrderedMembers(typeDeclaration).ToList();

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
    /// initializers and of the layout fields, with its position: the members of the active code, the members of code
    /// that an <c>#if</c> branch excludes outside every member, and the further members that such code declares
    /// inside a member whose declaration a conditional directive splits
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The ordered members</returns>
    private static IEnumerable<OrderedMember> GetOrderedMembers(TypeDeclarationSyntax typeDeclaration)
    {
        foreach (var member in typeDeclaration.Members)
        {
            foreach (var orderedMember in GetOrderedMembersOfDeclaration(member))
            {
                yield return orderedMember;
            }
        }

        foreach (var orderedMember in GetOrderedMembersOfDisabledText(typeDeclaration))
        {
            yield return orderedMember;
        }
    }

    /// <summary>
    /// Gets what a member of the active code contributes to the order of the initializers and of the layout fields.
    /// A field, event field, or property whose declaration a conditional directive splits counts with what any build
    /// configuration gives it: a runtime initializer when one configuration gives it one, both contexts when one
    /// configuration declares it <c>static</c> and another does not, and an instance field when one configuration
    /// declares it as one. The split includes excluded code in front of the declaration's first token that the
    /// declaration continues, such as a <c>static</c> modifier that only one configuration compiles. Further members that
    /// excluded code inside its declaration declares count at the position of that code
    /// </summary>
    /// <param name="memberDeclaration">Member of the active code</param>
    /// <returns>The ordered members</returns>
    private static IEnumerable<OrderedMember> GetOrderedMembersOfDeclaration(MemberDeclarationSyntax memberDeclaration)
    {
        if (memberDeclaration is not (FieldDeclarationSyntax or EventFieldDeclarationSyntax or PropertyDeclarationSyntax)
            || memberDeclaration.ContainsDirectives == false)
        {
            yield return CreateOrderedMember(memberDeclaration.SpanStart, memberDeclaration);

            yield break;
        }

        var options = memberDeclaration.SyntaxTree.Options as CSharpParseOptions;
        var conditionalTrivia = GetConditionalTriviaOfDeclaration(memberDeclaration);
        var isStaticInExcludedCodeInFront = IsStaticInExcludedCodeInFront(memberDeclaration, options);

        if (conditionalTrivia.Count == 0
            && isStaticInExcludedCodeInFront == false)
        {
            yield return CreateOrderedMember(memberDeclaration.SpanStart, memberDeclaration);

            yield break;
        }

        var conditionalSpans = GetConditionalSpans(memberDeclaration.Span, conditionalTrivia);
        var modifiers = DeclarationModifierUtilities.GetModifiers(memberDeclaration);
        var staticModifiers = modifiers.Where(modifier => modifier.IsKind(SyntaxKind.StaticKeyword)).ToList();
        var constModifiers = modifiers.Where(modifier => modifier.IsKind(SyntaxKind.ConstKeyword)).ToList();

        var isStaticInAnyConfiguration = staticModifiers.Count > 0 || isStaticInExcludedCodeInFront;
        var isInstanceInAnyConfiguration = staticModifiers.Count == 0 || staticModifiers.TrueForAll(modifier => conditionalSpans.Any(span => span.Contains(modifier.SpanStart)));
        var isNonConstantInAnyConfiguration = constModifiers.Count == 0 || constModifiers.TrueForAll(modifier => conditionalSpans.Any(span => span.Contains(modifier.SpanStart)));
        var hasInitializerInAnyConfiguration = HasInitializerSyntax(memberDeclaration);

        foreach (var disabledText in conditionalTrivia.Where(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia)))
        {
            var text = disabledText.ToFullString();
            var tokens = SyntaxFactory.ParseTokens(text, options: options).ToList();
            var depths = GetNestingDepths(tokens, GetNestingDepth(memberDeclaration, disabledText.SpanStart));
            var headLength = 0;

            while (headLength < tokens.Count
                   && (depths[headLength] != 0 || tokens[headLength].IsKind(SyntaxKind.SemicolonToken) == false))
            {
                headLength++;
            }

            for (var index = 0; index < headLength; index++)
            {
                if (depths[index] == 0)
                {
                    isStaticInAnyConfiguration |= tokens[index].IsKind(SyntaxKind.StaticKeyword);
                    hasInitializerInAnyConfiguration |= tokens[index].IsKind(SyntaxKind.EqualsToken);
                }
            }

            if (headLength < tokens.Count)
            {
                foreach (var orderedMember in GetOrderedMembersOfText(text.Substring(tokens[headLength].Span.End), disabledText.SpanStart, options))
                {
                    yield return orderedMember;
                }
            }
        }

        var hasInitializer = hasInitializerInAnyConfiguration && isNonConstantInAnyConfiguration;

        yield return new OrderedMember(memberDeclaration.SpanStart,
                                       hasInitializer && isStaticInAnyConfiguration,
                                       hasInitializer && isInstanceInAnyConfiguration,
                                       isInstanceInAnyConfiguration && isNonConstantInAnyConfiguration && DeclaresLayoutField(memberDeclaration));
    }

    /// <summary>
    /// Determines whether excluded code in the leading trivia of the declaration's first token declares the
    /// declaration <c>static</c> in some build configuration: a <c>static</c> outside every bracket in the part of an
    /// excluded branch behind its last complete member, which the declaration continues. The excluded code of a
    /// conditional group in front of a group whose excluded code completes a member continues that member instead, so it
    /// does not count. The members that the excluded code declares count where the excluded code outside every member
    /// counts (<see cref="GetOrderedMembersOfDisabledText"/>)
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="options">Parse options of the document</param>
    /// <returns><see langword="true"/> if some build configuration declares the declaration <c>static</c> in front of its first token</returns>
    private static bool IsStaticInExcludedCodeInFront(MemberDeclarationSyntax memberDeclaration, CSharpParseOptions options)
    {
        var leadingTrivia = memberDeclaration.GetFirstToken().LeadingTrivia;

        if (leadingTrivia.Any(trivia => trivia.IsKind(SyntaxKind.DisabledTextTrivia)) == false)
        {
            return false;
        }

        // Each excluded text, with the start of the outermost conditional group that holds it in this trivia
        var disabledTexts = new List<(SyntaxTrivia Trivia, int GroupStart)>();
        var groupStarts = new Stack<int>();

        for (var index = 0; index < leadingTrivia.Count; index++)
        {
            var trivia = leadingTrivia[index];

            if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
                || ((trivia.IsKind(SyntaxKind.ElifDirectiveTrivia) || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia)) && groupStarts.Count == 0))
            {
                groupStarts.Push(index);
            }
            else if (trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
            {
                if (groupStarts.Count > 0)
                {
                    groupStarts.Pop();
                }
            }
            else if (trivia.IsKind(SyntaxKind.DisabledTextTrivia))
            {
                disabledTexts.Add((trivia, groupStarts.Count > 0 ? groupStarts.Last() : index));
            }
        }

        var completedGroupStart = -1;

        for (var index = disabledTexts.Count - 1; index >= 0; index--)
        {
            var (trivia, groupStart) = disabledTexts[index];

            if (completedGroupStart >= 0
                && groupStart < completedGroupStart)
            {
                break;
            }

            var tokens = SyntaxFactory.ParseTokens(trivia.ToFullString(), options: options).ToList();
            var depths = GetNestingDepths(tokens, 0);
            var tailStart = 0;

            for (var tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
            {
                if (depths[tokenIndex] == 0
                    && (tokens[tokenIndex].IsKind(SyntaxKind.SemicolonToken) || tokens[tokenIndex].IsKind(SyntaxKind.CloseBraceToken)))
                {
                    tailStart = tokenIndex + 1;
                }
            }

            for (var tokenIndex = tailStart; tokenIndex < tokens.Count; tokenIndex++)
            {
                if (depths[tokenIndex] == 0
                    && tokens[tokenIndex].IsKind(SyntaxKind.StaticKeyword))
                {
                    return true;
                }
            }

            if (tailStart > 0
                && completedGroupStart < 0)
            {
                completedGroupStart = groupStart;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the nesting depth of the declaration's active code at a position: the number of parentheses, brackets, and
    /// braces of the declaration that open in front of the position and are not closed there
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <param name="position">Position inside the declaration</param>
    /// <returns>The nesting depth</returns>
    private static int GetNestingDepth(MemberDeclarationSyntax memberDeclaration, int position)
    {
        var depth = 0;

        foreach (var token in memberDeclaration.DescendantTokens())
        {
            if (token.SpanStart >= position)
            {
                break;
            }

            depth += GetNestingDelta(token);
        }

        return depth;
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
    /// Gets the conditional directives and the code they exclude inside the declaration of a field, event field, or
    /// property, apart from those inside an accessor body, an expression body, or a lambda body of an initializer,
    /// which hold statements and expressions rather than a part of the declaration. Only the tokens that carry a
    /// directive are read
    /// </summary>
    /// <param name="memberDeclaration">Member declaration</param>
    /// <returns>The conditional trivia, in source order</returns>
    private static List<SyntaxTrivia> GetConditionalTriviaOfDeclaration(MemberDeclarationSyntax memberDeclaration)
    {
        var span = memberDeclaration.Span;

        return memberDeclaration.DescendantTokens(node => node.ContainsDirectives)
                                .Where(token => token.ContainsDirectives
                                                && token.Parent?.AncestorsAndSelf()
                                                               .TakeWhile(node => node != memberDeclaration)
                                                               .Any(node => node is BlockSyntax or ArrowExpressionClauseSyntax) != true)
                                .SelectMany(token => token.LeadingTrivia.Concat(token.TrailingTrivia))
                                .Where(trivia => span.Contains(trivia.SpanStart)
                                                 && (trivia.IsKind(SyntaxKind.DisabledTextTrivia)
                                                     || trivia.IsKind(SyntaxKind.IfDirectiveTrivia)
                                                     || trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
                                                     || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia)
                                                     || trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia)))
                                .ToList();
    }

    /// <summary>
    /// Gets the parts of a declaration's span that only some build configurations compile: the code between a
    /// conditional directive inside the span and the directive that ends its branch. A branch whose <c>#if</c> lies in
    /// front of the span is conditional from the start of the span, and a branch whose <c>#endif</c> lies behind it is
    /// conditional up to its end
    /// </summary>
    /// <param name="span">Span of the declaration</param>
    /// <param name="conditionalTrivia">Conditional directives and excluded code inside the span, in source order</param>
    /// <returns>The conditional parts of the span</returns>
    private static List<TextSpan> GetConditionalSpans(TextSpan span, List<SyntaxTrivia> conditionalTrivia)
    {
        var conditionalSpans = new List<TextSpan>();
        var branchStarts = new Stack<int>();

        foreach (var trivia in conditionalTrivia)
        {
            if (trivia.IsKind(SyntaxKind.IfDirectiveTrivia))
            {
                branchStarts.Push(trivia.Span.End);
            }
            else if (trivia.IsKind(SyntaxKind.ElifDirectiveTrivia)
                     || trivia.IsKind(SyntaxKind.ElseDirectiveTrivia))
            {
                if (branchStarts.Count == 0)
                {
                    conditionalSpans.Add(TextSpan.FromBounds(span.Start, trivia.SpanStart));
                    branchStarts.Push(trivia.Span.End);
                }
            }
            else if (trivia.IsKind(SyntaxKind.EndIfDirectiveTrivia))
            {
                conditionalSpans.Add(TextSpan.FromBounds(branchStarts.Count > 0 ? branchStarts.Pop() : span.Start, trivia.SpanStart));
            }
        }

        while (branchStarts.Count > 0)
        {
            conditionalSpans.Add(TextSpan.FromBounds(branchStarts.Pop(), span.End));
        }

        return conditionalSpans;
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
    /// Gets what the code that an <c>#if</c> branch excludes in the type's body, outside every member, contributes to
    /// the order of the initializers and of the layout fields, at the position of that code
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The ordered members of the excluded code</returns>
    private static IEnumerable<OrderedMember> GetOrderedMembersOfDisabledText(TypeDeclarationSyntax typeDeclaration)
    {
        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var bodyEnd = typeDeclaration.CloseBraceToken.SpanStart;
        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;

        foreach (var trivia in typeDeclaration.DescendantTrivia())
        {
            if (trivia.IsKind(SyntaxKind.DisabledTextTrivia) == false
                || trivia.SpanStart < bodyStart
                || trivia.SpanStart >= bodyEnd
                || typeDeclaration.Members.Any(member => member.Span.Contains(trivia.SpanStart)))
            {
                continue;
            }

            foreach (var orderedMember in GetOrderedMembersOfText(trivia.ToFullString(), trivia.SpanStart, options))
            {
                yield return orderedMember;
            }
        }
    }

    /// <summary>
    /// Gets what excluded code, parsed as the body of a type, contributes to the order of the initializers and of the
    /// layout fields, at the position of that code. Each member it declares counts as written. Text that parses as no
    /// complete member, such as the initializer of a property behind its accessor list, counts as a runtime
    /// initializer of both contexts when it holds an <c>=</c> outside every parenthesis, bracket, and brace, because it
    /// can complete a member of either. An <c>=</c> inside them, such as a named attribute argument or an assignment in
    /// an accessor body, is no initializer
    /// </summary>
    /// <param name="text">Excluded code</param>
    /// <param name="position">Position of the excluded code</param>
    /// <param name="options">Parse options of the document</param>
    /// <returns>The ordered members of the excluded code</returns>
    private static IEnumerable<OrderedMember> GetOrderedMembersOfText(string text, int position, CSharpParseOptions options)
    {
        var parsedType = SyntaxFactory.ParseCompilationUnit($"class C\n{{\n{text}\n}}", options: options)
                                      .Members
                                      .OfType<TypeDeclarationSyntax>()
                                      .FirstOrDefault();

        if (parsedType == null)
        {
            yield break;
        }

        foreach (var member in parsedType.Members)
        {
            yield return CreateOrderedMember(position, member);
        }

        if (parsedType.Members
                      .OfType<IncompleteMemberSyntax>()
                      .Select(member => member.DescendantTokens())
                      .Concat(parsedType.DescendantTrivia(descendIntoTrivia: true)
                                        .Where(trivia => trivia.IsKind(SyntaxKind.SkippedTokensTrivia))
                                        .Select(trivia => trivia.GetStructure()?.DescendantTokens() ?? []))
                      .Any(tokens => ContainsTopLevelToken(tokens.ToList(), SyntaxKind.EqualsToken)))
        {
            yield return new OrderedMember(position, true, true, false);
        }
    }

    #endregion // Methods
}