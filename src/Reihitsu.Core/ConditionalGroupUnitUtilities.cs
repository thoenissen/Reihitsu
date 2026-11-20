using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods that decide, from where the directives of a conditional group stand and from the text of its excluded
/// branches, whether the group wraps whole units of a unit list (SHARED-ORDER-008), for
/// <see cref="OrderingDeclarationUtilities"/>
/// </summary>
internal static class ConditionalGroupUnitUtilities
{
    #region Methods

    /// <summary>
    /// Parses text as the content of the body of a type <c>C</c>, as excluded code of a type's body is read
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="options">Parse options of the document</param>
    /// <returns>The parsed compilation unit</returns>
    internal static CompilationUnitSyntax ParseInTypeBody(string text, CSharpParseOptions options)
    {
        return SyntaxFactory.ParseCompilationUnit($"class C\n{{\n{text}\n}}", options: options);
    }

    /// <summary>
    /// Determines whether the directive is an <c>#if</c>, <c>#elif</c>, <c>#else</c>, or <c>#endif</c> directive
    /// </summary>
    /// <param name="directive">Directive</param>
    /// <returns><see langword="true"/> if the directive belongs to a conditional group</returns>
    internal static bool IsConditionalDirective(DirectiveTriviaSyntax directive)
    {
        return directive is ConditionalDirectiveTriviaSyntax or ElseDirectiveTriviaSyntax or EndIfDirectiveTriviaSyntax;
    }

    /// <summary>
    /// Gets the conditional directives that start inside a span of the tree
    /// </summary>
    /// <param name="root">Root of the tree</param>
    /// <param name="span">Span</param>
    /// <returns>The conditional directives</returns>
    internal static List<DirectiveTriviaSyntax> GetConditionalDirectives(SyntaxNode root, TextSpan span)
    {
        return root.DescendantTrivia(span)
                   .Where(trivia => span.Contains(trivia.SpanStart))
                   .Select(trivia => trivia.GetStructure())
                   .OfType<DirectiveTriviaSyntax>()
                   .Where(IsConditionalDirective)
                   .ToList();
    }

    /// <summary>
    /// Determines whether a conditional group wraps whole units of one unit list, deciding only from where its
    /// directives stand (SHARED-ORDER-008): every directive stands at a boundary of the same list
    /// (<see cref="GetBoundaryLists"/>), the group lies inside no branch of another conditional group, it wraps whole
    /// units of that list with no other conditional group inside it (<see cref="WrapsWholeUnitsOf"/>), for the accessors of a property or the statements in
    /// an accessor's body, no branch holds a <c>field</c> token, and for the accessors of a property, no branch holds
    /// an accessor without a body unless no accessor of the list has one, so that the group cannot decide whether the
    /// property has a backing field. No condition is evaluated
    /// </summary>
    /// <param name="typeDeclaration">Type declaration whose body is reordered</param>
    /// <param name="group">Directives of the group, from <c>#if</c> to <c>#endif</c></param>
    /// <returns><see langword="true"/> if the group wraps whole units</returns>
    internal static bool WrapsWholeUnits(TypeDeclarationSyntax typeDeclaration, IReadOnlyList<DirectiveTriviaSyntax> group)
    {
        var groupStart = group[0].SpanStart;
        var groupEnd = group[group.Count - 1].SpanStart;
        var root = typeDeclaration.SyntaxTree.GetRoot();
        var activeTokens = root.DescendantTokens(TextSpan.FromBounds(groupStart, groupEnd))
                               .Where(token => token.SpanStart > groupStart && token.Span.End <= groupEnd);
        var directivesInFront = GetConditionalDirectives(root, TextSpan.FromBounds(0, groupStart));

        if (directivesInFront.Count(directive => directive is IfDirectiveTriviaSyntax) != directivesInFront.Count(directive => directive is EndIfDirectiveTriviaSyntax))
        {
            return false;
        }

        return group.Select(directive => GetBoundaryLists(typeDeclaration, directive.ParentTrivia.Token))
                    .Aggregate((lists, nextLists) => lists.Intersect(nextLists))
                    .Any(list =>
                         {
                             if (WrapsWholeUnitsOf(group, list, true, out var branches) == false)
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
    /// Determines whether a conditional group wraps whole units of a unit list, the one test that every reader of a
    /// group decides by (SHARED-ORDER-008, RH2001-010): the text of each branch the analyzed build excludes, parsed on
    /// its own, reads as whole units of the list (<see cref="ReadsAsWholeUnits"/>), and, when nesting is checked, the
    /// group ends with <c>#endif</c> and no other conditional group lies inside one of its branches. Whether the group
    /// lies inside a branch of another group is not read here. The active branch is not read, and no condition is
    /// evaluated
    /// </summary>
    /// <param name="group">Directives of the group, from <c>#if</c> to <c>#endif</c></param>
    /// <param name="list">Unit list whose units the branches are read as</param>
    /// <param name="checksNesting">Whether a group with another group inside a branch does not wrap whole units</param>
    /// <param name="branches">The nodes that hold the parsed units of each excluded branch</param>
    /// <returns><see langword="true"/> if the group wraps whole units</returns>
    internal static bool WrapsWholeUnitsOf(IReadOnlyList<DirectiveTriviaSyntax> group, (SyntaxNode Owner, bool OfAttributes) list, bool checksNesting, out List<SyntaxNode> branches)
    {
        branches = new List<SyntaxNode>();

        if (checksNesting
            && (group[group.Count - 1] is not EndIfDirectiveTriviaSyntax
                || GetConditionalDirectives(list.Owner.SyntaxTree.GetRoot(), TextSpan.FromBounds(group[0].SpanStart + 1, group[group.Count - 1].SpanStart)).Count != group.Count - 2))
        {
            return false;
        }

        var options = list.Owner.SyntaxTree.Options as CSharpParseOptions;

        foreach (var text in group.Select(GetExcludedText).Where(text => text != null))
        {
            if (ReadsAsWholeUnits(text, list, options, out var units) == false)
            {
                return false;
            }

            branches.Add(units);
        }

        return true;
    }

    /// <summary>
    /// Gets the text of the branch behind the directive when a build configuration other than the analyzed one
    /// compiles it: the excluded code that follows the directive in its token's leading trivia
    /// </summary>
    /// <param name="directive">Conditional directive</param>
    /// <returns>The excluded text, or <see langword="null"/> when the branch is active or empty</returns>
    internal static string GetExcludedText(DirectiveTriviaSyntax directive)
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
    internal static IEnumerable<(SyntaxNode Owner, bool OfAttributes)> GetBoundaryLists(TypeDeclarationSyntax typeDeclaration, SyntaxToken token)
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
            && SyntaxTokenNavigationUtilities.GetNextToken(attributedMember.AttributeLists.Last().GetLastToken()) == token)
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
    internal static bool IsUnitListOfTypeBody((SyntaxNode Owner, bool OfAttributes) list)
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
    internal static bool ForbidsFieldKeyword((SyntaxNode Owner, bool OfAttributes) list)
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
    internal static bool ReadsAsWholeUnits(string text, (SyntaxNode Owner, bool OfAttributes) list, CSharpParseOptions options, out SyntaxNode units)
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
    internal static bool IsWithoutBody(AccessorDeclarationSyntax accessor)
    {
        return accessor.Body == null && accessor.ExpressionBody == null;
    }

    #endregion // Methods
}