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

namespace Reihitsu.Analyzer.Rules.Design;

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

            if (symbol != null)
            {
                solution = await Renamer.RenameSymbolAsync(solution, symbol, default, fieldName, cancellationToken).ConfigureAwait(false);

                location = new TextSpan(location.Start, location.Length + (fieldName.Length - node.Identifier.ValueText.Length));
            }
        }

        document = solution.GetDocument(document.Id);
        if (document != null)
        {
            var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken);
            if (syntaxRoot != null)
            {
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

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId);

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root != null)
        {
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
    }

    #endregion // CodeFixProvider
}