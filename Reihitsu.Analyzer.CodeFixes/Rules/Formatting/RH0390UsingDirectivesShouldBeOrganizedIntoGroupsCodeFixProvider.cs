using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider))]
public class RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix by reorganizing using directives into groups
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, SyntaxNode scope, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var usings = GetUsings(scope);

        if (usings.Count < 2)
        {
            return document;
        }

        var organizedUsings = OrganizeUsings(usings);
        var updatedScope = WithUsings(scope, organizedUsings);
        var updatedRoot = root.ReplaceNode(scope, updatedScope);

        return document.WithSyntaxRoot(updatedRoot);
    }

    /// <summary>
    /// Gets the indentation trivia (whitespace only) from the given leading trivia
    /// </summary>
    /// <param name="leadingTrivia">Leading trivia to extract indentation from</param>
    /// <returns>Trivia list containing only the indentation whitespace</returns>
    private static SyntaxTriviaList GetIndentationTrivia(SyntaxTriviaList leadingTrivia)
    {
        var result = new List<SyntaxTrivia>();

        for (var i = leadingTrivia.Count - 1; i >= 0; i--)
        {
            if (leadingTrivia[i].IsKind(SyntaxKind.WhitespaceTrivia))
            {
                result.Insert(0, leadingTrivia[i]);
            }
            else
            {
                break;
            }
        }

        return SyntaxFactory.TriviaList(result);
    }

    /// <summary>
    /// Gets the using directives from the given scope node
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <returns>Using directives</returns>
    private static SyntaxList<UsingDirectiveSyntax> GetUsings(SyntaxNode scope)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.Usings,
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.Usings,
                   _ => default,
               };
    }

    /// <summary>
    /// Applies updated using directives to the given scope node
    /// </summary>
    /// <param name="scope">Compilation unit or namespace declaration</param>
    /// <param name="usingDirectives">New using directives</param>
    /// <returns>Updated scope node</returns>
    private static SyntaxNode WithUsings(SyntaxNode scope, SyntaxList<UsingDirectiveSyntax> usingDirectives)
    {
        return scope switch
               {
                   CompilationUnitSyntax compilationUnit => compilationUnit.WithUsings(usingDirectives),
                   BaseNamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.WithUsings(usingDirectives),
                   _ => scope,
               };
    }

    /// <summary>
    /// Reorganizes using directives into groups sorted by using type and root namespace
    /// </summary>
    /// <param name="usings">Original using directives</param>
    /// <returns>Reorganized using directives</returns>
    private static SyntaxList<UsingDirectiveSyntax> OrganizeUsings(SyntaxList<UsingDirectiveSyntax> usings)
    {
        var firstLeadingTrivia = usings.First().GetLeadingTrivia();
        var canonical = RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.ComputeCanonicalOrder(usings);
        var result = new List<UsingDirectiveSyntax>();

        for (var i = 0; i < canonical.Count; i++)
        {
            var current = canonical[i];

            if (i == 0)
            {
                result.Add(current.WithLeadingTrivia(firstLeadingTrivia));

                continue;
            }

            var indentation = GetIndentationTrivia(current.GetLeadingTrivia());

            if (RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.AreInSameGroup(canonical[i - 1], current))
            {
                result.Add(current.WithLeadingTrivia(indentation));

                continue;
            }

            var blankLineTriviaList = new List<SyntaxTrivia>
                                      {
                                          SyntaxFactory.EndOfLine(Environment.NewLine)
                                      };

            blankLineTriviaList.AddRange(indentation);
            result.Add(current.WithLeadingTrivia(SyntaxFactory.TriviaList(blankLineTriviaList)));
        }

        return SyntaxFactory.List(result);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0390UsingDirectivesShouldBeOrganizedIntoGroupsAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticNode = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            var usingDirective = diagnosticNode?.AncestorsAndSelf().OfType<UsingDirectiveSyntax>().FirstOrDefault();
            var scope = usingDirective?.Parent;

            if (scope is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0390Title,
                                                          token => ApplyCodeFixAsync(context.Document, scope, token),
                                                          nameof(RH0390UsingDirectivesShouldBeOrganizedIntoGroupsCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}