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
    /// member's leading trivia therefore moves with it, and such code from the target member up to the moved member
    /// is crossed. The move counts as changing the order as well when the moved or crossed span holds a directive of
    /// a conditional group that does not wrap whole units, and a type whose declaration holds a conditional
    /// directive in front of its body counts as one whose field order sets its layout (SHARED-ORDER-008)
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
    /// that code, because another build configuration compiles them. The reorder counts as changing the order as
    /// well when the reordered span holds a directive of a conditional group that does not wrap whole units, and a
    /// type whose declaration holds a conditional directive in front of its body counts as one whose field order
    /// sets its layout (SHARED-ORDER-008). The reorder is given
    /// as the rank, after the reorder, of the part of the body that holds a position; code within one part keeps its
    /// order
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
    /// layout check given as a flag. The reorder counts as changed as well when the reordered span holds a directive
    /// of a conditional group that does not wrap whole units (<see cref="WrapsWholeUnits"/>, SHARED-ORDER-008). A
    /// type whose declaration holds a conditional directive in front of its body
    /// (<see cref="DeclarationHoldsConditionalDirective"/>) counts as one whose field order sets its layout
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, Func<int, int> getNewRank)
    {
        var reorderedSpan = GetReorderedSpan(typeDeclaration, getNewRank);
        var groups = typeDeclaration.DescendantTrivia(reorderedSpan)
                                    .Where(trivia => reorderedSpan.Contains(trivia.SpanStart))
                                    .Select(trivia => trivia.GetStructure())
                                    .OfType<DirectiveTriviaSyntax>()
                                    .Where(IsConditionalDirective)
                                    .Select(directive => directive.GetRelatedDirectives())
                                    .GroupBy(group => group[0].SpanStart, (_, sameGroups) => sameGroups.First());

        if (groups.Any(group => WrapsWholeUnits(typeDeclaration, group) == false))
        {
            return true;
        }

        var members = typeDeclaration.Members
                                     .Select(member => (Position: member.SpanStart, Member: member))
                                     .Concat(GetMembersOfDisabledText(typeDeclaration))
                                     .ToList();

        bool ChangesOrder(Func<MemberDeclarationSyntax, bool> isOrdered)
        {
            var writtenOrder = members.Where(member => isOrdered(member.Member))
                                      .Select(member => member.Position)
                                      .OrderBy(position => position)
                                      .ToList();
            var newOrder = writtenOrder.OrderBy(getNewRank)
                                       .ThenBy(position => position);

            return writtenOrder.SequenceEqual(newOrder) == false;
        }

        return ChangesOrder(member => HasInitializer(member) && IsStatic(member))
               || ChangesOrder(member => HasInitializer(member) && IsStatic(member) == false)
               || ((fieldOrderSetsLayout || DeclarationHoldsConditionalDirective(typeDeclaration)) && ChangesOrder(IsLayoutField));
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
    /// instance field-like event, or an instance property that can have a backing field (<see cref="HasBackingField"/>)
    /// </summary>
    /// <param name="memberDeclaration">Declaration</param>
    /// <returns><see langword="true"/> if the declaration declares an instance field</returns>
    private static bool IsLayoutField(MemberDeclarationSyntax memberDeclaration)
    {
        if (IsStatic(memberDeclaration) || IsConst(memberDeclaration))
        {
            return false;
        }

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
        return memberDeclaration switch
               {
                   FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword) == false
                                                              && fieldDeclaration.Declaration.Variables.Any(variable => variable.Initializer != null),
                   EventFieldDeclarationSyntax eventFieldDeclaration => eventFieldDeclaration.Declaration.Variables.Any(variable => variable.Initializer != null),
                   PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration.Initializer != null,
                   _ => false,
               };
    }

    /// <summary>
    /// Determines whether the property counts as having a backing field the compiler declares (SHARED-ORDER-007): a
    /// property with an accessor that has neither a body nor an expression body, or a property that uses the
    /// <c>field</c> keyword. The count goes by the shape of the declaration alone, also for either declaration of a
    /// partial property, so it can count a field the compiler does not declare; that only withholds a reorder. The
    /// parser reads <c>field</c> as the keyword only from C# 14 on; before, it is an identifier that names a member.
    /// Abstract and extern properties have none
    /// </summary>
    /// <param name="propertyDeclaration">Property declaration</param>
    /// <returns><see langword="true"/> if the property counts as having a backing field</returns>
    private static bool HasBackingField(PropertyDeclarationSyntax propertyDeclaration)
    {
        if (propertyDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword)
            || propertyDeclaration.Modifiers.Any(SyntaxKind.ExternKeyword))
        {
            return false;
        }

        return propertyDeclaration.AccessorList?.Accessors.Any(IsWithoutBody) == true
               || propertyDeclaration.DescendantTokens().Any(token => token.IsKind(SyntaxKind.FieldKeyword));
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
    /// Gets the members declared in code that an <c>#if</c> branch excludes in the type's body, outside every member,
    /// with the position of that code
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The members of the excluded code</returns>
    private static IEnumerable<(int Position, MemberDeclarationSyntax Member)> GetMembersOfDisabledText(TypeDeclarationSyntax typeDeclaration)
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

            var parsedType = ParseInTypeBody(trivia.ToFullString(), options).Members
                                                                            .OfType<TypeDeclarationSyntax>()
                                                                            .FirstOrDefault();

            if (parsedType == null)
            {
                continue;
            }

            foreach (var member in parsedType.Members)
            {
                yield return (trivia.SpanStart, member);
            }
        }
    }

    /// <summary>
    /// Parses text as the content of the body of a type <c>C</c>, as excluded code of a type's body is read
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="options">Parse options of the document</param>
    /// <returns>The parsed compilation unit</returns>
    private static CompilationUnitSyntax ParseInTypeBody(string text, CSharpParseOptions options)
    {
        return SyntaxFactory.ParseCompilationUnit($"class C\n{{\n{text}\n}}", options: options);
    }

    /// <summary>
    /// Gets the span of the type's body whose order the reorder changes: from the first position that a later
    /// position goes in front of, up to the last position that goes in front of an earlier one. It covers the moved
    /// and the crossed span (SHARED-ORDER-003)
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns>The reordered span; empty when the reorder changes nothing</returns>
    private static TextSpan GetReorderedSpan(TypeDeclarationSyntax typeDeclaration, Func<int, int> getNewRank)
    {
        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var ranks = Enumerable.Range(bodyStart, Math.Max(0, typeDeclaration.CloseBraceToken.SpanStart - bodyStart))
                              .Select(getNewRank)
                              .ToList();
        var first = -1;
        var last = -1;
        var maximum = int.MinValue;
        var minimum = int.MaxValue;

        for (var index = 0; index < ranks.Count; index++)
        {
            last = ranks[index] < maximum ? index : last;
            maximum = Math.Max(maximum, ranks[index]);
        }

        for (var index = ranks.Count - 1; index >= 0; index--)
        {
            first = ranks[index] > minimum ? index : first;
            minimum = Math.Min(minimum, ranks[index]);
        }

        return first < 0
                   ? new TextSpan(bodyStart, 0)
                   : TextSpan.FromBounds(bodyStart + first, bodyStart + last + 1);
    }

    /// <summary>
    /// Determines whether the directive is an <c>#if</c>, <c>#elif</c>, <c>#else</c>, or <c>#endif</c> directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns><see langword="true"/> if the directive belongs to a conditional group</returns>
    private static bool IsConditionalDirective(DirectiveTriviaSyntax directive)
    {
        return directive is ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax;
    }

    /// <summary>
    /// Determines whether a conditional group wraps whole units of one unit list, deciding only from where its
    /// directives stand (SHARED-ORDER-008): every directive stands at a boundary of the same list
    /// (<see cref="GetBoundaryLists"/>), no other conditional group lies inside or around it, the text of every
    /// excluded branch parses on its own as whole units of that list (<see cref="ReadsAsWholeUnits"/>), for the
    /// accessors of a property or the statements in an accessor's body, no branch holds a <c>field</c> token, and
    /// for the accessors of a property, no branch holds an accessor without a body unless no accessor of the list
    /// has one, so that the group cannot decide whether the property has a backing field. No condition is evaluated
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="group">Directives of the group, from <c>#if</c> to <c>#endif</c></param>
    /// <returns><see langword="true"/> if the group wraps whole units</returns>
    private static bool WrapsWholeUnits(TypeDeclarationSyntax typeDeclaration, IReadOnlyList<DirectiveTriviaSyntax> group)
    {
        var root = typeDeclaration.SyntaxTree.GetRoot();
        var conditionalDirectives = root.DescendantTrivia()
                                        .Select(trivia => trivia.GetStructure())
                                        .OfType<DirectiveTriviaSyntax>()
                                        .Where(IsConditionalDirective)
                                        .ToList();
        var groupStart = group[0].SpanStart;
        var groupEnd = group[group.Count - 1].SpanStart;

        if (group[group.Count - 1] is not EndIfDirectiveTriviaSyntax
            || conditionalDirectives.Count(directive => directive.SpanStart > groupStart && directive.SpanStart < groupEnd) != group.Count - 2
            || conditionalDirectives.Count(directive => directive.SpanStart < groupStart && directive is IfDirectiveTriviaSyntax)
               != conditionalDirectives.Count(directive => directive.SpanStart < groupStart && directive is EndIfDirectiveTriviaSyntax))
        {
            return false;
        }

        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;
        var excludedTexts = group.Select(GetExcludedText)
                                 .Where(text => text != null)
                                 .ToList();
        var activeTokens = root.DescendantTokens(TextSpan.FromBounds(groupStart, groupEnd))
                               .Where(token => token.SpanStart > groupStart && token.Span.End <= groupEnd);

        return group.Select(directive => GetBoundaryLists(typeDeclaration, directive.ParentTrivia.Token))
                    .Aggregate((lists, nextLists) => lists.Intersect(nextLists))
                    .Any(list =>
                         {
                             var branches = excludedTexts.Select(text => ReadsAsWholeUnits(text, list, options, out var units) ? units : null)
                                                         .ToList();

                             if (branches.Contains(null))
                             {
                                 return false;
                             }

                             var listAccessors = list.Owner.ChildNodes().OfType<AccessorDeclarationSyntax>().ToList();
                             var branchAccessors = listAccessors.Where(accessor => accessor.SpanStart > groupStart && accessor.SpanStart < groupEnd)
                                                                .Concat(branches.SelectMany(units => units.DescendantNodes().OfType<AccessorDeclarationSyntax>()))
                                                                .ToList();

                             return (ForbidsFieldKeyword(list) == false
                                     || activeTokens.Concat(branches.SelectMany(units => units.DescendantTokens())).Any(token => token.Text == "field") == false)
                                    && (list.Owner is not AccessorListSyntax
                                        || branchAccessors.Any(IsWithoutBody) == false
                                        || listAccessors.Concat(branchAccessors).All(IsWithoutBody));
                         });
    }

    /// <summary>
    /// Determines whether the declaration of the type holds a conditional directive in front of its body
    /// (SHARED-ORDER-008), which can make the type's layout depend on the build, so that the type counts as one whose
    /// field order sets its layout: one in the leading trivia of a token
    /// behind the declaration's first token, up to and including the opening brace, or one in the leading trivia of
    /// the first token whose excluded branch does not read as whole members
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <returns><see langword="true"/> if the declaration holds such a directive</returns>
    private static bool DeclarationHoldsConditionalDirective(TypeDeclarationSyntax typeDeclaration)
    {
        var firstToken = typeDeclaration.GetFirstToken();
        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;

        return typeDeclaration.DescendantTokens()
                              .TakeWhile(token => token.SpanStart <= typeDeclaration.OpenBraceToken.SpanStart)
                              .SelectMany(token => token.LeadingTrivia.Select(trivia => (Token: token, Directive: trivia.GetStructure() as DirectiveTriviaSyntax)))
                              .Any(entry => entry.Directive != null
                                            && IsConditionalDirective(entry.Directive)
                                            && (entry.Token != firstToken
                                                || (GetExcludedText(entry.Directive) is { } text
                                                    && ReadsAsWholeUnits(text, (typeDeclaration, false), options, out _) == false)));
    }

    /// <summary>
    /// Gets the text of the branch behind the directive when a build configuration other than the analyzed one
    /// compiles it: the excluded code that follows the directive in its token's leading trivia
    /// </summary>
    /// <param name="directive">Conditional directive</param>
    /// <returns>The excluded text, or <see langword="null"/> when the branch is active or empty</returns>
    private static string GetExcludedText(DirectiveTriviaSyntax directive)
    {
        var leadingTrivia = directive.ParentTrivia.Token.LeadingTrivia;
        var index = leadingTrivia.IndexOf(directive.ParentTrivia) + 1;

        return index < leadingTrivia.Count && leadingTrivia[index].IsKind(SyntaxKind.DisabledTextTrivia)
                   ? leadingTrivia[index].ToFullString()
                   : null;
    }

    /// <summary>
    /// Gets the unit lists at whose boundary a token stands (SHARED-ORDER-008): the token begins a unit of the list,
    /// or follows its last unit. A list is given by its owner — a type declaration for its members, an accessor list
    /// for its accessors, a block or a switch section for its statements — and, for the attribute lists of a member,
    /// by the member with <c>OfAttributes</c> set. Members count only in the body of the reordered type or a type
    /// nested in it, and statements only inside the body block of a member
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="token">Token whose leading trivia holds a directive</param>
    /// <returns>The unit lists</returns>
    private static IEnumerable<(SyntaxNode Owner, bool OfAttributes)> GetBoundaryLists(TypeDeclarationSyntax typeDeclaration, SyntaxToken token)
    {
        var lists = new List<(SyntaxNode Owner, bool OfAttributes)>();

        switch (token.Parent)
        {
            case TypeDeclarationSyntax type when token == type.CloseBraceToken:
                lists.Add((type, false));
                break;

            case AccessorListSyntax accessorList when token == accessorList.CloseBraceToken:
                lists.Add((accessorList, false));
                break;

            case BlockSyntax block when token == block.CloseBraceToken:
                lists.Add((block, false));
                break;

            case SwitchStatementSyntax switchStatement when token == switchStatement.CloseBraceToken && switchStatement.Sections.Count > 0:
                lists.Add((switchStatement.Sections.Last(), false));
                break;
        }

        foreach (var node in token.Parent.AncestorsAndSelf().TakeWhile(node => node.GetFirstToken() == token))
        {
            switch (node)
            {
                case MemberDeclarationSyntax { Parent: TypeDeclarationSyntax type } member:
                    {
                        lists.Add((type, false));
                        lists.Add((member, true));
                    }
                    break;

                case AttributeListSyntax { Parent: MemberDeclarationSyntax member }:
                    {
                        lists.Add((member, true));
                    }
                    break;

                case AccessorDeclarationSyntax { Parent: AccessorListSyntax accessorList }:
                    {
                        lists.Add((accessorList, false));
                    }
                    break;

                case StatementSyntax { Parent: BlockSyntax or SwitchSectionSyntax } statement:
                    {
                        lists.Add((statement.Parent, false));
                    }
                    break;

                case SwitchSectionSyntax { Parent: SwitchStatementSyntax switchStatement } section when switchStatement.Sections.IndexOf(section) > 0:
                    {
                        lists.Add((switchStatement.Sections[switchStatement.Sections.IndexOf(section) - 1], false));
                    }
                    break;
            }
        }

        if (token.Parent?.FirstAncestorOrSelf<MemberDeclarationSyntax>() is { AttributeLists.Count: > 0 } attributedMember
            && attributedMember.AttributeLists.Last().GetLastToken().GetNextToken() == token)
        {
            lists.Add((attributedMember, true));
        }

        return lists.Where(list => typeDeclaration.Contains(list.Owner) && IsUnitListOfTypeBody(list));
    }

    /// <summary>
    /// Determines whether a unit list belongs to a type body as SHARED-ORDER-008 lists them: the members of a type,
    /// the attribute lists of a member of a type, the accessors of such a member, or the statements of a block or a
    /// switch section inside the body block of a method, constructor, destructor, operator, conversion operator, or
    /// accessor, also in a lambda or local function there
    /// </summary>
    /// <param name="list">Unit list</param>
    /// <returns><see langword="true"/> if the list is one of a type body</returns>
    private static bool IsUnitListOfTypeBody((SyntaxNode Owner, bool OfAttributes) list)
    {
        return list switch
               {
                   { OfAttributes: true } => list.Owner.Parent is TypeDeclarationSyntax,
                   { Owner: TypeDeclarationSyntax } => true,
                   { Owner: AccessorListSyntax accessorList } => accessorList.Parent?.Parent is TypeDeclarationSyntax,
                   _ => list.Owner.AncestorsAndSelf().Any(node => node is BlockSyntax { Parent: BaseMethodDeclarationSyntax or AccessorDeclarationSyntax }),
               };
    }

    /// <summary>
    /// Determines whether the <c>field</c> clause of SHARED-ORDER-008 applies to a unit list: the accessors of a
    /// property, or the statements of a block inside an accessor's body
    /// </summary>
    /// <param name="list">Unit list</param>
    /// <returns><see langword="true"/> if no branch of a group of the list may hold a <c>field</c> token</returns>
    private static bool ForbidsFieldKeyword((SyntaxNode Owner, bool OfAttributes) list)
    {
        return list.OfAttributes == false
               && (list.Owner is AccessorListSyntax
                   || (list.Owner is not TypeDeclarationSyntax && list.Owner.FirstAncestorOrSelf<AccessorDeclarationSyntax>() != null));
    }

    /// <summary>
    /// Determines whether excluded text, parsed on its own as the content of an empty unit list of the list's kind,
    /// reads as a sequence of whole units of that list without a syntax error, closing no brace it did not open
    /// </summary>
    /// <param name="text">Excluded text</param>
    /// <param name="list">Unit list at whose boundary the text stands</param>
    /// <param name="options">Parse options of the document</param>
    /// <param name="units">The node that holds the parsed units</param>
    /// <returns><see langword="true"/> if the text reads as whole units</returns>
    private static bool ReadsAsWholeUnits(string text, (SyntaxNode Owner, bool OfAttributes) list, CSharpParseOptions options, out SyntaxNode units)
    {
        var (prefix, suffix) = list switch
                               {
                                   { OfAttributes: true } => (string.Empty, "\nvoid M()\n{\n}"),
                                   { Owner: TypeDeclarationSyntax } => (string.Empty, string.Empty),
                                   { Owner: AccessorListSyntax } => ("int P\n{\n", "\n}"),
                                   _ => ("void M()\n{\n", "\n}"),
                               };

        var compilationUnit = ParseInTypeBody($"{prefix}{text}{suffix}", options);

        units = compilationUnit.Members.Count == 1 && compilationUnit.Members[0] is ClassDeclarationSyntax parsedType
                    ? parsedType
                    : null;

        if (units is not ClassDeclarationSyntax { Members: var members }
            || compilationUnit.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error))
        {
            return false;
        }

        var onlyMember = members.Count == 1
                             ? members[0]
                             : null;

        return list switch
               {
                   { OfAttributes: true } => onlyMember is MethodDeclarationSyntax { Modifiers.Count: 0 },
                   { Owner: TypeDeclarationSyntax } => true,
                   { Owner: AccessorListSyntax } => onlyMember is PropertyDeclarationSyntax { AccessorList: not null },
                   _ => onlyMember is MethodDeclarationSyntax { Body: not null },
               };
    }

    /// <summary>
    /// Determines whether the accessor has neither a body nor an expression body
    /// </summary>
    /// <param name="accessor">Accessor</param>
    /// <returns><see langword="true"/> if the accessor has no body</returns>
    private static bool IsWithoutBody(AccessorDeclarationSyntax accessor)
    {
        return accessor.Body == null && accessor.ExpressionBody == null;
    }

    #endregion // Methods
}