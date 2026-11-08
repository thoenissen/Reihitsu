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
    /// The largest number of preprocessor symbols whose assignments the member-order guard reads for one reorder,
    /// which gives 256 build configurations. A reorder whose reordered span names more counts as changed
    /// </summary>
    private const int MaximumSymbolCount = 8;

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
    /// type's symbol decides. Each build configuration — an assignment of the preprocessor symbols that the
    /// conditional directives in the reordered span name — is read as the compiler reads it and judged on its own, so
    /// members in code that an <c>#if</c> branch excludes count in the configurations that compile them, and a member
    /// whose declaration a conditional directive splits counts in each configuration with what that configuration
    /// gives it. The reorder counts as changed as well when, in some configuration, it does not stay among the members
    /// of the type, or when the reordered span names too many symbols (SHARED-FIX-003). The reorder is given as the
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
    /// layout check given as a flag. Each build configuration is an assignment of the preprocessor symbols that the
    /// conditional directives in the reordered span name (<see cref="GetConditionSymbols"/>); every other symbol keeps
    /// the definition the document is analyzed with. The document is parsed once per assignment, so the parser
    /// evaluates every directive, and each configuration is judged on its own with the members its parse compiles. The
    /// reorder counts as changed when, in some configuration, it does not stay among the members of the type
    /// (<see cref="KeepsReorderAmongMembers"/>), when it puts two members of one context that each have a runtime
    /// initializer, or two layout fields, into the other order, or when the reordered span names more than
    /// <see cref="MaximumSymbolCount"/> symbols
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="fieldOrderSetsLayout">Whether the order of the type's instance fields sets its layout (<see cref="FieldOrderSetsLayout"/>)</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <returns><see langword="true"/> if the reorder changes initializer execution order or the layout</returns>
    private static bool ChangesMemberOrder(TypeDeclarationSyntax typeDeclaration, bool fieldOrderSetsLayout, Func<int, int> getNewRank)
    {
        if (TryGetReorderedSpan(typeDeclaration, getNewRank, out var reorderedSpan, out var boundaries) == false)
        {
            return false;
        }

        var syntaxTree = typeDeclaration.SyntaxTree;
        var symbols = GetConditionSymbols(syntaxTree.GetRoot(), reorderedSpan);

        if (symbols.Count > MaximumSymbolCount)
        {
            return true;
        }

        var options = syntaxTree.Options as CSharpParseOptions ?? CSharpParseOptions.Default;
        var analyzedSymbols = new HashSet<string>(options.PreprocessorSymbolNames, StringComparer.Ordinal);
        var otherSymbols = analyzedSymbols.Where(symbol => symbols.Contains(symbol) == false).ToList();
        var analyzedAssignment = Enumerable.Range(0, symbols.Count)
                                           .Where(index => analyzedSymbols.Contains(symbols[index]))
                                           .Sum(index => 1 << index);
        var text = syntaxTree.GetText();

        for (var assignment = 0; assignment < 1 << symbols.Count; assignment++)
        {
            var configurationType = typeDeclaration;
            var isAnalyzedConfiguration = assignment == analyzedAssignment;

            if (isAnalyzedConfiguration == false)
            {
                var definedSymbols = otherSymbols.Concat(Enumerable.Range(0, symbols.Count)
                                                                   .Where(index => (assignment & (1 << index)) != 0)
                                                                   .Select(index => symbols[index]));
                var root = CSharpSyntaxTree.ParseText(text, options.WithPreprocessorSymbols(definedSymbols)).GetRoot();

                configurationType = FindConfigurationType(root, typeDeclaration);

                if (KeepsReorderAmongMembers(root, configurationType, reorderedSpan, boundaries) == false)
                {
                    return true;
                }

                if (configurationType == null)
                {
                    continue;
                }
            }

            var members = GetOrderedMembersOfConfiguration(configurationType, isAnalyzedConfiguration == false);

            if (ChangesOrder(members, getNewRank, member => member.HasStaticInitializer)
                || ChangesOrder(members, getNewRank, member => member.HasInstanceInitializer)
                || (fieldOrderSetsLayout && ChangesOrder(members, getNewRank, member => member.IsLayoutField)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the span of the type's body whose parts the reorder puts into another order: from the first position
    /// that the reorder puts behind a later position up to the last position that it puts in front of an earlier
    /// one. Every position in between is moved or crossed. The boundaries are the start and the end of that span
    /// and every position inside it where the part of the body changes
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <param name="reorderedSpan">The span whose parts change their order</param>
    /// <param name="boundaries">The boundaries of the parts within the span, in source order</param>
    /// <returns><see langword="false"/> if the reorder changes the order of no two positions</returns>
    private static bool TryGetReorderedSpan(TypeDeclarationSyntax typeDeclaration, Func<int, int> getNewRank, out TextSpan reorderedSpan, out List<int> boundaries)
    {
        reorderedSpan = default;
        boundaries = [];

        var bodyStart = typeDeclaration.OpenBraceToken.Span.End;
        var length = Math.Max(0, typeDeclaration.CloseBraceToken.SpanStart - bodyStart);
        var ranks = new int[length];
        var suffixMinimums = new int[length + 1];

        for (var index = 0; index < length; index++)
        {
            ranks[index] = getNewRank(bodyStart + index);
        }

        suffixMinimums[length] = int.MaxValue;

        for (var index = length - 1; index >= 0; index--)
        {
            suffixMinimums[index] = Math.Min(ranks[index], suffixMinimums[index + 1]);
        }

        var first = -1;
        var last = -1;
        var prefixMaximum = int.MinValue;

        for (var index = 0; index < length; index++)
        {
            if (first < 0
                && ranks[index] > suffixMinimums[index + 1])
            {
                first = index;
            }

            if (ranks[index] < prefixMaximum)
            {
                last = index;
            }

            prefixMaximum = Math.Max(prefixMaximum, ranks[index]);
        }

        if (first < 0
            || last < first)
        {
            return false;
        }

        reorderedSpan = TextSpan.FromBounds(bodyStart + first, bodyStart + last + 1);
        boundaries.Add(reorderedSpan.Start);

        for (var index = first + 1; index <= last; index++)
        {
            if (ranks[index] != ranks[index - 1])
            {
                boundaries.Add(bodyStart + index);
            }
        }

        boundaries.Add(reorderedSpan.End);

        return true;
    }

    /// <summary>
    /// Gets the preprocessor symbols that the conditions of the conditional groups with a directive in a span name,
    /// in a stable order. A group counts with the conditions of all its <c>#if</c> and <c>#elif</c> directives when
    /// any of its directives, also an <c>#else</c> or an <c>#endif</c>, lies in the span, because the code of the span
    /// that the group's branches hold depends on those conditions. The directives inside excluded code count as well,
    /// because the parser reads them as directives
    /// </summary>
    /// <param name="root">Root of the syntax tree</param>
    /// <param name="span">Span</param>
    /// <returns>The symbols</returns>
    private static List<string> GetConditionSymbols(SyntaxNode root, TextSpan span)
    {
        var symbols = new SortedSet<string>(StringComparer.Ordinal);

        if (root.ContainsDirectives == false)
        {
            return [];
        }

        var openGroups = new Stack<List<DirectiveTriviaSyntax>>();

        void AddSymbolsOfGroup(List<DirectiveTriviaSyntax> group)
        {
            if (group.Exists(directive => span.Contains(directive.SpanStart)))
            {
                foreach (var condition in group.OfType<ConditionalDirectiveTriviaSyntax>().Select(directive => directive.Condition))
                {
                    symbols.UnionWith(condition.DescendantNodesAndSelf()
                                               .OfType<IdentifierNameSyntax>()
                                               .Select(identifier => identifier.Identifier.ValueText));
                }
            }
        }

        foreach (var directive in root.DescendantTrivia(node => node.ContainsDirectives)
                                      .Select(trivia => trivia.GetStructure())
                                      .OfType<DirectiveTriviaSyntax>())
        {
            switch (directive)
            {
                case IfDirectiveTriviaSyntax:
                    {
                        openGroups.Push([directive]);
                    }
                    break;

                case ElifDirectiveTriviaSyntax:
                case ElseDirectiveTriviaSyntax:
                case EndIfDirectiveTriviaSyntax:
                    {
                        if (openGroups.Count == 0)
                        {
                            openGroups.Push([]);
                        }

                        openGroups.Peek().Add(directive);

                        if (directive is EndIfDirectiveTriviaSyntax)
                        {
                            AddSymbolsOfGroup(openGroups.Pop());
                        }
                    }
                    break;
            }
        }

        foreach (var unclosedGroup in openGroups)
        {
            AddSymbolsOfGroup(unclosedGroup);
        }

        return symbols.ToList();
    }

    /// <summary>
    /// Finds, in the parse of one build configuration, the declaration of the type whose body is reordered: the type
    /// whose opening brace lies in the text of the analyzed declaration up to its opening brace, which holds every
    /// header that an <c>#if</c> may choose for the type. When several do, the last one is the type, because the
    /// others are declared in the declaration's leading trivia
    /// </summary>
    /// <param name="root">Root of the configuration's parse</param>
    /// <param name="typeDeclaration">Analyzed type declaration</param>
    /// <returns>The type declaration, or <see langword="null"/> if the configuration compiles none</returns>
    private static TypeDeclarationSyntax FindConfigurationType(SyntaxNode root, TypeDeclarationSyntax typeDeclaration)
    {
        var headerSpan = TextSpan.FromBounds(typeDeclaration.FullSpan.Start, typeDeclaration.OpenBraceToken.Span.End);

        return root.DescendantNodes(node => node.FullSpan.IntersectsWith(headerSpan))
                   .OfType<TypeDeclarationSyntax>()
                   .Where(type => type.OpenBraceToken.IsMissing == false
                                  && headerSpan.Contains(type.OpenBraceToken.Span))
                   .OrderBy(type => type.OpenBraceToken.SpanStart)
                   .LastOrDefault();
    }

    /// <summary>
    /// Determines whether, in one build configuration, the reorder stays among the members of the type
    /// (SHARED-ORDER-001): every token that the configuration compiles in the reordered span is part of a member of
    /// the type, so no branch has closed the type's body in front of it or opened another type around it, and no
    /// member of the type reaches across a boundary of the reordered parts, as a nested type or a method block does
    /// that one branch opens and another closes
    /// </summary>
    /// <param name="root">Root of the configuration's parse</param>
    /// <param name="configurationType">The type in the configuration's parse, or <see langword="null"/> if it compiles none</param>
    /// <param name="reorderedSpan">The span whose parts change their order</param>
    /// <param name="boundaries">The boundaries of the parts within the span</param>
    /// <returns><see langword="true"/> if the reorder moves and crosses only whole members of the type</returns>
    private static bool KeepsReorderAmongMembers(SyntaxNode root, TypeDeclarationSyntax configurationType, TextSpan reorderedSpan, List<int> boundaries)
    {
        foreach (var token in root.DescendantTokens(reorderedSpan).Where(token => reorderedSpan.OverlapsWith(token.Span)))
        {
            if (configurationType == null
                || token.Parent?.AncestorsAndSelf().Any(node => node is MemberDeclarationSyntax && node.Parent == configurationType) != true)
            {
                return false;
            }
        }

        return configurationType == null
               || configurationType.Members
                                   .Where(member => member.Span.OverlapsWith(reorderedSpan))
                                   .All(member => boundaries.TrueForAll(boundary => boundary <= member.SpanStart || boundary >= member.Span.End));
    }

    /// <summary>
    /// Determines whether a reorder puts two members of one kind of one build configuration into the other order
    /// </summary>
    /// <param name="members">The members of the configuration</param>
    /// <param name="getNewRank">Rank after the reorder of the part of the body that holds a position</param>
    /// <param name="isOrdered">Whether a member is of the kind whose order is kept</param>
    /// <returns><see langword="true"/> if two members of the kind change their order</returns>
    private static bool ChangesOrder(List<OrderedMember> members, Func<int, int> getNewRank, Func<OrderedMember, bool> isOrdered)
    {
        var maximumRank = int.MinValue;

        foreach (var rank in members.Where(isOrdered)
                                    .OrderBy(member => member.Position)
                                    .Select(member => getNewRank(member.Position)))
        {
            if (rank < maximumRank)
            {
                return true;
            }

            maximumRank = Math.Max(maximumRank, rank);
        }

        return false;
    }

    /// <summary>
    /// Gets what each member of one build configuration contributes to the order of the initializers and of the
    /// layout fields, at the position of its first token (SHARED-ORDER-007). In a configuration other than the
    /// analyzed one, which no build may compile without errors, text that parses as no complete member counts as a
    /// runtime initializer of both contexts when it holds an <c>=</c> outside every parenthesis, bracket, and brace,
    /// because it can complete a member of either. An <c>=</c> inside them, such as a named attribute argument or an
    /// assignment in an accessor body, is no initializer
    /// </summary>
    /// <param name="configurationType">The type in the configuration's parse</param>
    /// <param name="countsIncompleteMembers">Whether text that parses as no complete member counts</param>
    /// <returns>The ordered members of the configuration</returns>
    private static List<OrderedMember> GetOrderedMembersOfConfiguration(TypeDeclarationSyntax configurationType, bool countsIncompleteMembers)
    {
        var orderedMembers = configurationType.Members
                                              .Select(member => CreateOrderedMember(member.SpanStart, member))
                                              .ToList();

        if (countsIncompleteMembers == false)
        {
            return orderedMembers;
        }

        foreach (var incompleteTokens in configurationType.Members
                                                          .OfType<IncompleteMemberSyntax>()
                                                          .Select(member => (member.SpanStart, Tokens: member.DescendantTokens()))
                                                          .Concat(configurationType.DescendantTrivia(descendIntoTrivia: true)
                                                                                   .Where(trivia => trivia.IsKind(SyntaxKind.SkippedTokensTrivia))
                                                                                   .Select(trivia => (trivia.SpanStart, Tokens: trivia.GetStructure()?.DescendantTokens() ?? [])))
                                                          .Where(incompleteTokens => ContainsTopLevelToken(incompleteTokens.Tokens.ToList(), SyntaxKind.EqualsToken)))
        {
            orderedMembers.Add(new OrderedMember(incompleteTokens.SpanStart, true, true, false));
        }

        return orderedMembers;
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

    #endregion // Methods
}