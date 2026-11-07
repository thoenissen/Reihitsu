using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Design;

/// <summary>
/// Code fix provider for <see cref="RH2005FieldsMustBePrivateAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH2005FieldsMustBePrivateCodeFixProvider))]
public class RH2005FieldsMustBePrivateCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var updatedFieldDeclaration = DeclarationModifierUtilities.AddAccessibilityModifier(fieldDeclaration, SyntaxKind.PrivateKeyword);
        var updatedRoot = root.ReplaceNode(fieldDeclaration, updatedFieldDeclaration);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Checks whether a position lies in one of the bodies of a type's declarations
    /// </summary>
    /// <param name="bodies">The text between the braces of each declaration of the type, with its syntax tree</param>
    /// <param name="syntaxTree">Syntax tree of the position</param>
    /// <param name="position">Position</param>
    /// <returns><see langword="true"/> if the position lies in one of the bodies</returns>
    private static bool IsInsideBody(ImmutableArray<(SyntaxTree SyntaxTree, TextSpan Span)> bodies, SyntaxTree syntaxTree, int position)
    {
        return bodies.Any(body => body.SyntaxTree == syntaxTree
                                  && body.Span.Contains(position));
    }

    /// <summary>
    /// Checks whether code outside the field's containing type refers to any field the declaration declares
    /// (RH2005-004): a reference in active code anywhere in the solution, or a name of one of the fields in code that a
    /// conditional directive excludes (SHARED-FIX-003)
    /// </summary>
    /// <param name="document">Document holding the declaration</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if code outside the containing type refers to one of the fields</returns>
    /// <remarks>
    /// Code inside the containing type is code in the body of one of its declarations, between its braces, where the
    /// types nested in it belong to the body. Every other position is outside, such as an attribute of the type itself,
    /// a derived type, or a <c>cref</c> in another type's documentation comment. Excluded code is not bound, so an
    /// occurrence of a field's name there cannot be told apart from an unrelated name, and every occurrence outside the
    /// containing type counts
    /// </remarks>
    private static async Task<bool> IsReferencedOutsideContainingTypeAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken)
    {
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (semanticModel == null)
        {
            return true;
        }

        var fields = fieldDeclaration.Declaration
                                     .Variables
                                     .Select(variable => semanticModel.GetDeclaredSymbol(variable, cancellationToken))
                                     .ToImmutableArray();

        if (fields.Any(static field => field == null))
        {
            return true;
        }

        var containingType = fields[0].ContainingType;
        var bodies = containingType.DeclaringSyntaxReferences
                                   .Select(reference => reference.GetSyntax(cancellationToken))
                                   .OfType<TypeDeclarationSyntax>()
                                   .Where(static declaration => declaration.OpenBraceToken.IsMissing == false)
                                   .Select(static declaration => (declaration.SyntaxTree, Span: TextSpan.FromBounds(declaration.OpenBraceToken.Span.End, declaration.CloseBraceToken.SpanStart)))
                                   .ToImmutableArray();

        var solution = document.Project.Solution;

        foreach (var field in fields)
        {
            var references = await SymbolFinder.FindReferencesAsync(field, solution, cancellationToken).ConfigureAwait(false);

            if (references.SelectMany(static reference => reference.Locations)
                          .Any(location => IsInsideBody(bodies, location.Location.SourceTree, location.Location.SourceSpan.Start) == false))
            {
                return true;
            }
        }

        var names = fields.Select(static field => field.Name).ToImmutableHashSet(StringComparer.Ordinal);

        return await ExcludedCodeUtilities.HasExcludedCodeOccurrenceAsync(solution,
                                                                          fields,
                                                                          names,
                                                                          trivia => IsInsideBody(bodies, trivia.SyntaxTree, trivia.SpanStart) == false,
                                                                          cancellationToken)
                                          .ConfigureAwait(false);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH2005FieldsMustBePrivateAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                var fieldDeclaration = root.FindToken(diagnostic.Location.SourceSpan.Start)
                                           .Parent
                                           ?.AncestorsAndSelf()
                                           .OfType<FieldDeclarationSyntax>()
                                           .FirstOrDefault();

                if (fieldDeclaration != null
                    && await IsReferencedOutsideContainingTypeAsync(context.Document, fieldDeclaration, context.CancellationToken).ConfigureAwait(false) == false)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH2005Title,
                                                              token => ApplyCodeFixAsync(context.Document, fieldDeclaration, token),
                                                              nameof(RH2005FieldsMustBePrivateCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}