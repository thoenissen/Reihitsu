using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;

using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Design;

/// <summary>
/// Providing fixes for <see cref="RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider))]
public class RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider : CodeFixProvider
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

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (HasFieldNameCollision(semanticModel, node))
        {
            return document.Project.Solution;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document.Project.Solution;
        }

        // Track the declaration with an annotation instead of span arithmetic, so renamed references appearing before
        // the declaration cannot shift it out from under the re-location step and silently degrade the fix to a rename.
        var propertyAnnotation = new SyntaxAnnotation();

        document = document.WithSyntaxRoot(root.ReplaceNode(node, node.WithAdditionalAnnotations(propertyAnnotation)));

        var solution = await RenamePropertyToFieldNameAsync(document, node, propertyAnnotation, cancellationToken).ConfigureAwait(false);

        document = solution.GetDocument(document.Id);

        return document == null
                   ? solution
                   : await ReplaceAnnotatedPropertyWithFieldAsync(document, propertyAnnotation, solution, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Renames the annotated property to its backing field name when the name actually changes
    /// </summary>
    /// <param name="document">Document holding the annotated property</param>
    /// <param name="node">Original property declaration used to compute the field name</param>
    /// <param name="propertyAnnotation">Annotation tracking the property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution after the optional rename</returns>
    private static async Task<Solution> RenamePropertyToFieldNameAsync(Document document, PropertyDeclarationSyntax node, SyntaxAnnotation propertyAnnotation, CancellationToken cancellationToken)
    {
        var solution = document.Project.Solution;

        if (GetFieldName(node, out var fieldName) == false)
        {
            return solution;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var annotatedRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var annotatedNode = annotatedRoot?.GetAnnotatedNodes(propertyAnnotation).OfType<PropertyDeclarationSyntax>().FirstOrDefault();
        var symbol = annotatedNode != null
                         ? semanticModel?.GetDeclaredSymbol(annotatedNode, cancellationToken)
                         : null;

        return symbol == null
                   ? solution
                   : await Renamer.RenameSymbolAsync(solution, symbol, default, fieldName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces the annotated property with its backing field declaration and formats the result
    /// </summary>
    /// <param name="document">Document holding the annotated property</param>
    /// <param name="propertyAnnotation">Annotation tracking the property declaration</param>
    /// <param name="solution">Solution to fall back to when the replacement cannot be applied</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The solution after replacing the property with the field declaration</returns>
    private static async Task<Solution> ReplaceAnnotatedPropertyWithFieldAsync(Document document, SyntaxAnnotation propertyAnnotation, Solution solution, CancellationToken cancellationToken)
    {
        var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var node = syntaxRoot?.GetAnnotatedNodes(propertyAnnotation).OfType<PropertyDeclarationSyntax>().FirstOrDefault();

        if (node == null
            || TryCreateFieldDeclaration(node, out var fieldDeclaration) == false)
        {
            return solution;
        }

        var formattingAnnotation = new SyntaxAnnotation();
        var updatedFieldDeclaration = fieldDeclaration.WithAdditionalAnnotations(formattingAnnotation);
        var updatedDocument = document.WithSyntaxRoot(syntaxRoot.ReplaceNode(node, updatedFieldDeclaration));
        var formattedRoot = await updatedDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var formattedFieldDeclaration = formattedRoot?.GetAnnotatedNodes(formattingAnnotation).OfType<FieldDeclarationSyntax>().FirstOrDefault();

        return formattedFieldDeclaration == null
                   ? updatedDocument.Project.Solution
                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedFieldDeclaration, cancellationToken).ConfigureAwait(false)).Project.Solution;
    }

    /// <summary>
    /// Gets the field name for a property declaration
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns>Field name</returns>
    private static string GetFieldName(PropertyDeclarationSyntax node)
    {
        return CasingUtilities.ToUnderlineCamelCase(node.Identifier.ValueText);
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
    /// Checks whether the computed field name is already used by another member of the containing type
    /// </summary>
    /// <param name="semanticModel">Semantic model used to resolve the property symbol and its containing type</param>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if converting the property would introduce a duplicate member declaration</returns>
    private static bool HasFieldNameCollision(SemanticModel semanticModel, PropertyDeclarationSyntax node)
    {
        var symbol = semanticModel?.GetDeclaredSymbol(node);

        if (symbol == null)
        {
            return false;
        }

        var fieldName = GetFieldName(node);

        return symbol.ContainingType.GetMembers(fieldName).Any(member => SymbolEqualityComparer.Default.Equals(member, symbol) == false);
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
            modifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(modifiers.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));
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
    /// Checks whether the property shape is safe to convert automatically
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if the fixer can safely convert the property</returns>
    private static bool IsSupportedPropertyShape(PropertyDeclarationSyntax node)
    {
        if (node.AttributeLists.Count > 0 || node.AccessorList == null || node.ExpressionBody != null || node.ExplicitInterfaceSpecifier != null)
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
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzer.DiagnosticId];

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

        var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan) is PropertyDeclarationSyntax node
                && IsSupportedPropertyShape(node)
                && HasFieldNameCollision(semanticModel, node) == false)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH2001Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, node, cancellationToken),
                                                          nameof(RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // CodeFixProvider
}