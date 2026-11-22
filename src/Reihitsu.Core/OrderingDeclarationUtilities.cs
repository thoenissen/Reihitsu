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
    /// member in front of the target member's leading trivia. The move counts as changing the order as well when the
    /// moved or crossed span holds a conditional directive, and a type that a conditional directive in a declaration
    /// of it or in front of the spans may give a layout counts as one whose field order sets its layout
    /// (SHARED-ORDER-008)
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="typeSymbol">Symbol of the type, which decides whether the order of its instance fields sets its layout</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move changes initializer execution order or the layout</returns>
    public static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, MemberDeclarationSyntax memberToMove, MemberDeclarationSyntax targetMember)
    {
        return ChangesMemberOrder(typeDeclaration, FieldOrderSetsLayout(typeSymbol) || AnyDeclarationHoldsConditionalDirective(typeSymbol), memberToMove, targetMember);
    }

    /// <summary>
    /// Determines whether a reorder of the type's body puts two members of the type into the other order where
    /// their order is behavior: two members of one context — both static or both instance — that each have a runtime
    /// initializer (SHARED-ORDER-007), or two instance fields of a type whose field order sets its layout, which the
    /// type's symbol decides. Only the members the analyzed build compiles are counted. The reorder counts as changing
    /// the order as well when a conditional directive stands in the body where the reorder moves code or moves code
    /// across it. A type counts as one whose field order sets its layout when a declaration of it, also another
    /// declaration of a partial type, holds a conditional directive in front of its body or in front of it, unless
    /// that directive's group wraps the whole declaration, or when the reordered declaration's body holds a
    /// conditional directive in front of the code the reorder moves (SHARED-ORDER-008). The reorder is given as the
    /// rank, after the reorder, of the part of the body that holds a position; code within one part keeps its order
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="typeSymbol">Symbol of the type, which decides whether the order of its instance fields sets its layout</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    public static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, INamedTypeSymbol typeSymbol, Func<int, int> getNewRank)
    {
        return ChangesMemberOrder(typeDeclaration, FieldOrderSetsLayout(typeSymbol) || AnyDeclarationHoldsConditionalDirective(typeSymbol), getNewRank);
    }

    /// <summary>
    /// Determines whether every conditional group with a directive in the token's leading trivia wraps whole members
    /// of the type body that holds the token (<see cref="ConditionalGroupUnitUtilities.WrapsWholeMembers"/>), with no other conditional group
    /// inside it (RH2001-010). A group that lies inside a branch of another group is decided by its own branches. The
    /// active branch is not read, and no condition is evaluated
    /// </summary>
    /// <param name="token">Token whose leading trivia is read, such as the first modifier of a member or the token behind a member</param>
    /// <returns><see langword="true"/> if every such group wraps whole members, also when there is none</returns>
    public static bool ConditionalGroupsWrapWholeMembers(SyntaxToken token)
    {
        var typeDeclaration = token.Parent?.FirstAncestorOrSelf<TypeDeclarationSyntax>();

        return token.LeadingTrivia
                    .Select(trivia => trivia.GetStructure())
                    .OfType<DirectiveTriviaSyntax>()
                    .Where(ConditionalGroupUnitUtilities.IsConditionalDirective)
                    .All(directive => typeDeclaration != null
                                      && ConditionalGroupUnitUtilities.WrapsWholeMembers(directive.GetRelatedDirectives(), typeDeclaration));
    }

    /// <summary>
    /// Determines whether moving a member before another member would change the execution order of
    /// field, event field, or property initializers. Initializers run in declaration order within their execution
    /// context (static initializers in the static constructor, instance initializers in the instance
    /// constructor), so reordering a member carrying an initializer past another member of the same
    /// context that also carries an initializer can silently change observable values. The members are ranked as
    /// <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, MemberDeclarationSyntax, MemberDeclarationSyntax)"/>
    /// ranks them, without the layout check that the type's symbol decides. The layout check still applies to a type
    /// whose declaration holds a conditional directive in front of its body or in front of it
    /// (<see cref="DeclarationHoldsConditionalDirective"/>), or whose body holds a conditional directive in front of the
    /// spans, so the move of an instance field in such a type counts as changed as well, and a conditional directive in
    /// the spans counts as a change. No code fix asks this question:
    /// a code fix asks <see cref="ChangesMemberOrder(TypeDeclarationSyntax, INamedTypeSymbol, MemberDeclarationSyntax, MemberDeclarationSyntax)"/>
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <param name="memberToMove">Member to move</param>
    /// <param name="targetMember">Target member</param>
    /// <returns><see langword="true"/> if the move would change initializer execution order, or the layout of a type whose declaration holds a conditional directive or whose body holds one in front of the spans, or if the spans hold a conditional directive</returns>
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
    /// layout check given as a flag. The reorder counts as changed as well when a conditional directive stands at a
    /// position of the body that the reorder moves or moves code across (<see cref="GetCrossedPositions"/>,
    /// SHARED-ORDER-008). The type counts as one whose field order sets its layout when its declaration holds a
    /// conditional directive in front of its body or in front of it (<see cref="DeclarationHoldsConditionalDirective"/>),
    /// or when its body holds a conditional directive in front of the first such position; one behind them is not read
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, Func<int, int> getNewRank)
    {
        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var isCrossed = GetCrossedPositions(typeDeclaration, getNewRank);
        var directiveOffsets = ConditionalGroupUnitUtilities.GetConditionalDirectives(typeDeclaration, new TextSpan(bodyStart, isCrossed.Length))
                                                            .Select(directive => directive.SpanStart - bodyStart)
                                                            .ToList();

        if (directiveOffsets.Any(offset => isCrossed[offset]))
        {
            return true;
        }

        bool ChangesOrder(Func<MemberDeclarationSyntax, bool> isOrdered)
        {
            var writtenOrder = typeDeclaration.Members
                                              .Where(isOrdered)
                                              .Select(member => member.SpanStart)
                                              .ToList();
            var newOrder = writtenOrder.OrderBy(getNewRank)
                                       .ThenBy(position => position);

            return writtenOrder.SequenceEqual(newOrder) == false;
        }

        return ChangesOrder(member => HasInitializer(member) && IsStatic(member))
               || ChangesOrder(member => HasInitializer(member) && IsStatic(member) == false)
               || (ChangesOrder(IsLayoutField)
                   && (fieldOrderSetsLayout
                       || DeclarationHoldsConditionalDirective(typeDeclaration)
                       || directiveOffsets.Any(offset => offset < Array.IndexOf(isCrossed, true))));
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

        return propertyDeclaration.AccessorList?.Accessors.Any(ConditionalGroupUnitUtilities.IsWithoutBody) == true
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
    /// Gets, for each position of the type's body from its opening brace up to its closing brace, whether the reorder
    /// moves code across it or moves it across other code: an earlier position goes behind it, or a later position
    /// goes in front of it. These positions are the moved and the crossed span (SHARED-ORDER-003); for a reorder of
    /// region blocks, the blocks whose place changes, the blocks they cross, and the code between blocks they cross
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns>For each position of the body, counted from the end of the opening brace, whether it lies in the moved or the crossed span</returns>
    private static bool[] GetCrossedPositions(TypeDeclarationSyntax typeDeclaration, Func<int, int> getNewRank)
    {
        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var ranks = Enumerable.Range(bodyStart, Math.Max(0, typeDeclaration.CloseBraceToken.SpanStart - bodyStart))
                              .Select(getNewRank)
                              .ToList();
        var isCrossed = new bool[ranks.Count];
        var maximum = int.MinValue;
        var minimum = int.MaxValue;

        for (var index = 0; index < ranks.Count; index++)
        {
            isCrossed[index] = ranks[index] < maximum;
            maximum = Math.Max(maximum, ranks[index]);
        }

        for (var index = ranks.Count - 1; index >= 0; index--)
        {
            isCrossed[index] |= ranks[index] > minimum;
            minimum = Math.Min(minimum, ranks[index]);
        }

        return isCrossed;
    }

    /// <summary>
    /// Determines whether a declaration of the type, the given one or, for a partial type, another one that the
    /// analyzed build compiles, in the same document or in another, holds a conditional directive in front of its
    /// body (<see cref="DeclarationHoldsConditionalDirective"/>, SHARED-ORDER-008). The bodies of the declarations
    /// are not read
    /// </summary>
    /// <param name="typeSymbol">Type</param>
    /// <returns><see langword="true"/> if a declaration of the type holds such a directive</returns>
    private static bool AnyDeclarationHoldsConditionalDirective(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.DeclaringSyntaxReferences
                         .Select(reference => reference.GetSyntax())
                         .OfType<TypeDeclarationSyntax>()
                         .Any(DeclarationHoldsConditionalDirective);
    }

    /// <summary>
    /// Determines whether a declaration of the type holds a conditional directive in front of its body or in front of
    /// it (SHARED-ORDER-008), which can make the type's layout depend on the build, so that the type counts as one whose
    /// field order sets its layout: one in the leading trivia of a token behind the declaration's first token, up to
    /// and including the opening brace, or, for a declaration that ends in <c>;</c> instead of a body, up to and
    /// including that <c>;</c>, or one in the leading trivia of the first token, unless its group wraps the whole
    /// declaration (<see cref="WrapsWholeDeclaration"/>). No excluded branch is read
    /// </summary>
    /// <param name="typeDeclaration">Declaration of the type</param>
    /// <returns><see langword="true"/> if the declaration holds such a directive</returns>
    private static bool DeclarationHoldsConditionalDirective(TypeDeclarationSyntax typeDeclaration)
    {
        var firstToken = typeDeclaration.GetFirstToken();
        var bodyToken = typeDeclaration.OpenBraceToken.IsKind(SyntaxKind.None)
                            ? typeDeclaration.SemicolonToken
                            : typeDeclaration.OpenBraceToken;

        return typeDeclaration.DescendantTokens()
                              .TakeWhile(token => token.SpanStart <= bodyToken.SpanStart)
                              .SelectMany(token => token.LeadingTrivia.Select(trivia => (Token: token, Directive: trivia.GetStructure() as DirectiveTriviaSyntax)))
                              .Any(entry => entry.Directive != null
                                            && ConditionalGroupUnitUtilities.IsConditionalDirective(entry.Directive)
                                            && (entry.Token != firstToken
                                                || WrapsWholeDeclaration(entry.Directive, typeDeclaration) == false));
    }

    /// <summary>
    /// Determines whether the group of a conditional directive wraps the whole declaration (SHARED-ORDER-008): it is an
    /// <c>#if</c> and its <c>#endif</c> with no <c>#elif</c> and no <c>#else</c>, the <c>#if</c> stands in the leading
    /// trivia of the declaration's first token, and the <c>#endif</c> in the leading trivia of the token behind the
    /// declaration's last token, also the end of the file. A build then compiles the declaration as written or not at all
    /// </summary>
    /// <param name="directive">Conditional directive</param>
    /// <param name="typeDeclaration">Declaration of the type</param>
    /// <returns><see langword="true"/> if the directive's group wraps the whole declaration</returns>
    private static bool WrapsWholeDeclaration(DirectiveTriviaSyntax directive, TypeDeclarationSyntax typeDeclaration)
    {
        var group = directive.GetRelatedDirectives();

        return group.Count == 2
               && group[0] is IfDirectiveTriviaSyntax
               && group[1] is EndIfDirectiveTriviaSyntax
               && group[0].ParentTrivia.Token == typeDeclaration.GetFirstToken()
               && group[1].ParentTrivia.Token == typeDeclaration.GetLastToken().GetNextToken(includeZeroWidth: true);
    }

    #endregion // Methods
}