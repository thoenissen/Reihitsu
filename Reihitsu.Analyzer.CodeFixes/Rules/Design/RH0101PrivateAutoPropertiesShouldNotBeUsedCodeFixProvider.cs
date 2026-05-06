using System.Collections.Immutable;
using System.Composition;
using System.Linq;
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

using Reihitsu.Analyzer.Core;
using Reihitsu.Formatter;

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
    private static async Task<Solution> ApplyCodeFixAsync(Document document, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
    {
        if (TryCreateFieldDeclaration(node, out _) == false)
        {
            return document.Project.Solution;
        }

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

                if (node != null
                    && TryCreateFieldDeclaration(node, out var fieldDeclaration))
                {
                    var formattingAnnotation = new SyntaxAnnotation();
                    var updatedFieldDeclaration = fieldDeclaration.WithAdditionalAnnotations(formattingAnnotation);
                    var updatedDocument = document.WithSyntaxRoot(syntaxRoot.ReplaceNode(node, updatedFieldDeclaration));
                    var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
                    var formattedFieldDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<FieldDeclarationSyntax>().FirstOrDefault();

                    solution = formattedFieldDeclaration == null
                                   ? updatedDocument.Project.Solution
                                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedFieldDeclaration, cancellationToken).ConfigureAwait(false)).Project.Solution;
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
    private static bool GetFieldName(PropertyDeclarationSyntax node, out string fieldName)
    {
        fieldName = GetFieldName(node);

        return fieldName != node.Identifier.ValueText;
    }

    /// <summary>
    /// Tries to create a field declaration that preserves the supported property semantics
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <returns><see langword="true"/> if the property shape is safe to convert</returns>
    private static bool TryCreateFieldDeclaration(PropertyDeclarationSyntax node, out FieldDeclarationSyntax fieldDeclaration)
    {
        fieldDeclaration = null;

        if (IsSupportedPropertyShape(node) == false)
        {
            return false;
        }

        var fieldName = GetFieldName(node);
        var modifiers = node.Modifiers;

        if (IsGetOnlyProperty(node))
        {
            modifiers = ModifierOrderingUtilities.OrderModifiersForRh0604(modifiers.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
        }

        fieldDeclaration = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(node.Type)
                                                                       .AddVariables(SyntaxFactory.VariableDeclarator(fieldName)
                                                                                                  .WithInitializer(node.Initializer)))
                                        .WithAttributeLists(node.AttributeLists)
                                        .WithModifiers(modifiers)
                                        .WithLeadingTrivia(node.GetLeadingTrivia())
                                        .WithTrailingTrivia(node.GetTrailingTrivia());

        return true;
    }

    /// <summary>
    /// Gets the field name for a property declaration
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns>Field name</returns>
    private static string GetFieldName(PropertyDeclarationSyntax node)
    {
        var propertyName = node.Identifier.ValueText.TrimStart('_');

        if (string.IsNullOrEmpty(propertyName))
        {
            return node.Identifier.ValueText;
        }

        var builder = new StringBuilder(propertyName.Length + 1);

        builder.Append('_');
        builder.Append(char.ToLower(propertyName[0]));

        if (propertyName.Length > 1)
        {
            builder.Append(propertyName.Substring(1));
        }

        return builder.ToString();
    }

    /// <summary>
    /// Checks whether the property shape is safe to convert automatically
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if the fixer can safely convert the property</returns>
    private static bool IsSupportedPropertyShape(PropertyDeclarationSyntax node)
    {
        if (node.AttributeLists.Count > 0 || node.AccessorList == null || node.ExpressionBody != null)
        {
            return false;
        }

        var hasGetter = false;
        var hasSetter = false;

        foreach (var accessor in node.AccessorList.Accessors)
        {
            if (accessor.AttributeLists.Count > 0
                || accessor.Modifiers.Count > 0
                || accessor.Body != null
                || accessor.ExpressionBody != null)
            {
                return false;
            }

            switch (accessor.Kind())
            {
                case SyntaxKind.GetAccessorDeclaration:
                    {
                        if (hasGetter)
                        {
                            return false;
                        }

                        hasGetter = true;
                    }
                    break;

                case SyntaxKind.SetAccessorDeclaration:
                    {
                        if (hasSetter)
                        {
                            return false;
                        }

                        hasSetter = true;
                    }
                    break;

                default:
                    {
                        return false;
                    }
            }
        }

        return hasGetter;
    }

    /// <summary>
    /// Checks whether the property only exposes a getter
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if the property is get-only</returns>
    private static bool IsGetOnlyProperty(PropertyDeclarationSyntax node)
    {
        return node.AccessorList?.Accessors.Count == 1 && node.AccessorList.Accessors[0].IsKind(SyntaxKind.GetAccessorDeclaration);
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0101PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId];

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
                if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax node && IsSupportedPropertyShape(node))
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0101Title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c),
                                                              nameof(RH0101PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}