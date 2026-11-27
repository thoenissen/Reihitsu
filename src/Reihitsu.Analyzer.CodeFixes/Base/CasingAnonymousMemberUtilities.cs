using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// The one owner of how a casing rename keeps the name of an anonymous object member that takes its name from a
/// renamed reference (SHARED-CASING-015)
/// </summary>
internal static class CasingAnonymousMemberUtilities
{
    #region Methods

    /// <summary>
    /// Writes the name of every anonymous object member that takes its name from a reference to a symbol of the rename
    /// explicitly, as <c>Name = </c> in front of the member's expression, so the rename keeps the member's name (SHARED-CASING-015)
    /// </summary>
    /// <param name="solution">Solution before the rename</param>
    /// <param name="symbol">Declared symbol to rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution with the member names written, or <paramref name="solution"/> when no member takes its name from a reference</returns>
    /// <remarks>
    /// The references are the ones the renamer renames, read from the same reference search, so a reference to every
    /// symbol of the rename cascade is covered
    /// </remarks>
    internal static async Task<Solution> NameAnonymousObjectMembersAsync(Solution solution, ISymbol symbol, CancellationToken cancellationToken)
    {
        var referencedSymbols = await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false);
        var locationsByDocument = referencedSymbols.SelectMany(static referencedSymbol => referencedSymbol.Locations)
                                                   .GroupBy(static location => location.Document.Id)
                                                   .ToList();

        foreach (var locations in locationsByDocument)
        {
            var document = solution.GetDocument(locations.Key);
            var root = document == null
                           ? null
                           : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (root == null)
            {
                continue;
            }

            var members = locations.Select(location => GetMemberNamedByReference(root, location.Location.SourceSpan))
                                   .Where(static member => member != null)
                                   .Distinct()
                                   .ToList();

            if (members.Count > 0)
            {
                solution = document.WithSyntaxRoot(root.ReplaceNodes(members, static (_, member) => WriteMemberName(member))).Project.Solution;
            }
        }

        return solution;
    }

    /// <summary>
    /// Gets the anonymous object member that takes its name from a reference: a member without a name of its own whose
    /// whole expression is the reference, or a member access or conditional access that ends with it
    /// </summary>
    /// <param name="root">Root of the document holding the reference</param>
    /// <param name="span">Span of the reference</param>
    /// <returns>The member, or <see langword="null"/> when the reference names no member</returns>
    private static AnonymousObjectMemberDeclaratorSyntax GetMemberNamedByReference(SyntaxNode root, TextSpan span)
    {
        var token = root.FindToken(span.Start);

        if (token.Span != span
            || token.Parent is not IdentifierNameSyntax name)
        {
            return null;
        }

        SyntaxNode expression = name.Parent switch
                                {
                                    MemberAccessExpressionSyntax memberAccess when memberAccess.Name == name => memberAccess,
                                    MemberBindingExpressionSyntax memberBinding => memberBinding,
                                    _ => name
                                };

        while (expression.Parent is ConditionalAccessExpressionSyntax conditionalAccess
               && conditionalAccess.WhenNotNull == expression)
        {
            expression = conditionalAccess;
        }

        return expression.Parent is AnonymousObjectMemberDeclaratorSyntax { NameEquals: null } member
               && member.Expression == expression
                   ? member
                   : null;
    }

    /// <summary>
    /// Writes the name an anonymous object member takes from the identifier that ends its expression in front of the
    /// expression; the expression's leading trivia moves in front of the name
    /// </summary>
    /// <param name="member">Member without a name of its own</param>
    /// <returns>The member with its name written</returns>
    private static AnonymousObjectMemberDeclaratorSyntax WriteMemberName(AnonymousObjectMemberDeclaratorSyntax member)
    {
        var expression = member.Expression;
        var nameToken = expression.GetLastToken();
        var name = SyntaxFactory.IdentifierName(nameToken.WithoutTrivia())
                                .WithLeadingTrivia(expression.GetLeadingTrivia())
                                .WithTrailingTrivia(SyntaxFactory.Space);
        var nameEquals = SyntaxFactory.NameEquals(name, SyntaxFactory.Token(SyntaxKind.EqualsToken).WithTrailingTrivia(SyntaxFactory.Space));

        return member.WithNameEquals(nameEquals)
                     .WithExpression(expression.WithoutLeadingTrivia());
    }

    #endregion // Methods
}