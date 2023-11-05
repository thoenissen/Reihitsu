using System.Collections.Immutable;
using System.Composition;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Design;

/// <summary>
/// Providing fixes for <see cref="RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider))]
public class RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applying code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node with diagnostics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    private async Task<Solution> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;
        var location = node.GetLocation().SourceSpan;

        if (GetFieldName(node, out var fieldName))
        {
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var symbol = semanticModel.GetDeclaredSymbol(node, cancellationToken);

            solution = await Renamer.RenameSymbolAsync(solution, symbol, fieldName, null, cancellationToken).ConfigureAwait(false);

            location = new TextSpan(location.Start, location.Length + (fieldName.Length - node.Identifier.ValueText.Length));
        }

        document = solution.GetDocument(document.Id);
        if (document != null)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);

            node = syntaxRoot.FindNode(location) as PropertyDeclarationSyntax;
            if (node != null)
            {
                var variableDeclaration = SyntaxFactory.VariableDeclaration(node.Type)
                                                       .AddVariables(SyntaxFactory.VariableDeclarator(fieldName));

                var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                                                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword))
                                                    .WithLeadingTrivia(node.GetLeadingTrivia())
                                                    .WithTrailingTrivia(node.GetTrailingTrivia());

                syntaxRoot = syntaxRoot.ReplaceNode(node, fieldDeclaration);

                solution = solution.WithDocumentSyntaxRoot(document.Id, syntaxRoot);
            }
        }

        return solution;
    }

    /// <summary>
    /// Get field name
    /// </summary>
    /// <param name="node">Property node</param>
    /// <param name="fieldName">Field name</param>
    /// <returns>Is the name changed?</returns>
    private bool GetFieldName(PropertyDeclarationSyntax node, out string fieldName)
    {
        var propertyName = node.Identifier.ValueText.TrimStart('_');

        var builder = new StringBuilder(propertyName.Length + 1);

        builder.Append('_');
        builder.Append(char.ToLower(propertyName[0]));

        if (propertyName.Length > 1)
        {
            builder.Append(propertyName.Substring(1));
        }

        fieldName = builder.ToString();

        return fieldName != node.Identifier.ValueText;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <summary>
    /// A list of diagnostic IDs that this provider can provide fixes for.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId);

    /// <summary>
    /// Gets an optional <see cref="T:Microsoft.CodeAnalysis.CodeFixes.FixAllProvider" /> that can fix all/multiple occurrences of diagnostics fixed by this code fix provider.
    /// Return null if the provider doesn't support fix all/multiple occurrences.
    /// Otherwise, you can return any of the well known fix all providers from <see cref="T:Microsoft.CodeAnalysis.CodeFixes.WellKnownFixAllProviders" /> or implement your own fix all provider.
    /// </summary>
    /// <returns>Provider</returns>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <summary>
    /// Computes one or more fixes for the specified <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" />.
    /// </summary>
    /// <param name="context">
    /// A <see cref="T:Microsoft.CodeAnalysis.CodeFixes.CodeFixContext" /> containing context information about the diagnostics to fix.
    /// The context must only contain diagnostics with a <see cref="P:Microsoft.CodeAnalysis.Diagnostic.Id" /> included in the <see cref="P:Microsoft.CodeAnalysis.CodeFixes.CodeFixProvider.FixableDiagnosticIds" /> for the current provider.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax node)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0101Title,
                                                          c => ApplyCodeFixAsync(context.Document, node, c),
                                                          nameof(CodeFixResources.RH0101Title)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}