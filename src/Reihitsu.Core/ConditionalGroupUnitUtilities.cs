using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Core;

/// <summary>
/// Helper methods that read the conditional directives of a type body, and decide from the text of a group's excluded
/// branches whether the group wraps whole members (RH2001-010), for <see cref="OrderingDeclarationUtilities"/> and
/// <see cref="OrderingMoveSafety"/>
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
    /// Determines whether a conditional group wraps whole members of a type body (RH2001-010): the group ends with
    /// <c>#endif</c>, no other conditional group lies inside one of its branches, and the text of each branch the
    /// analyzed build excludes, parsed on its own, reads as whole members (<see cref="ReadsAsWholeMembers"/>). Whether
    /// the group lies inside a branch of another group is not read here. The active branch is not read, and no
    /// condition is evaluated
    /// </summary>
    /// <param name="group">Directives of the group, from <c>#if</c> to <c>#endif</c></param>
    /// <param name="typeDeclaration">Type declaration whose body holds the group</param>
    /// <returns><see langword="true"/> if the group wraps whole members</returns>
    internal static bool WrapsWholeMembers(IReadOnlyList<DirectiveTriviaSyntax> group, TypeDeclarationSyntax typeDeclaration)
    {
        if (group[group.Count - 1] is not EndIfDirectiveTriviaSyntax
            || GetConditionalDirectives(typeDeclaration.SyntaxTree.GetRoot(), TextSpan.FromBounds(group[0].SpanStart + 1, group[group.Count - 1].SpanStart)).Count != group.Count - 2)
        {
            return false;
        }

        var options = typeDeclaration.SyntaxTree.Options as CSharpParseOptions;

        return group.Select(GetExcludedText)
                    .Where(text => text != null)
                    .All(text => ReadsAsWholeMembers(text, options));
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
    /// Determines whether excluded text, parsed on its own as the content of an empty type body, reads as a sequence of
    /// whole members without a syntax error, closing no brace it did not open
    /// </summary>
    /// <param name="text">Excluded text</param>
    /// <param name="options">Parse options of the document</param>
    /// <returns><see langword="true"/> if the text reads as whole members</returns>
    internal static bool ReadsAsWholeMembers(string text, CSharpParseOptions options)
    {
        var compilationUnit = ParseInTypeBody(text, options);

        return compilationUnit.Members.Count == 1
               && compilationUnit.Members[0] is ClassDeclarationSyntax
               && compilationUnit.GetDiagnostics().Any(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error) == false;
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