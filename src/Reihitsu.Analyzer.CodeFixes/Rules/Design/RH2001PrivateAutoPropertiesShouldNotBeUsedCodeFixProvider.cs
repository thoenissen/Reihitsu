using System.Collections.Generic;
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

using Reihitsu.Analyzer.CodeFixes.Core;
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
    /// Tries to create a field declaration that preserves the supported property semantics. A get-only property becomes
    /// a <see langword="readonly"/> field; a property that already carries <see langword="readonly"/> keeps that one modifier
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <param name="fieldDeclaration">Field declaration</param>
    /// <returns><see langword="true"/> if the property shape is safe to convert</returns>
    /// <remarks>
    /// The field keeps the property's leading and trailing trivia, its modifiers, its type, and its initializer. When
    /// the fix adds <see langword="readonly"/>, it writes the modifiers in the order of RH7105 through
    /// <see cref="ModifierOrderingUtilities.TryWriteModifiersInOrder"/>: the leading trivia stays in front of the new first
    /// modifier, the modifiers stand on one line, a comment or directive between them keeps its place, and the
    /// conversion is withheld when a reordered modifier would cross one (RH2001-004, RH2001-009). A conditional
    /// directive between the modifiers or in front of the type withholds the conversion in any case (RH2001-009). The field gets a new
    /// name, a new <c>;</c>, and no accessor list, so the trivia of the property's name, of its accessor list, of its
    /// <c>;</c>, and in front of the initializer's <c>=</c> is not carried over. When that trivia holds a comment or a
    /// directive, the field would lose it, and the conversion is withheld (RH2001-007, SHARED-FIX-003)
    /// </remarks>
    private static bool TryCreateFieldDeclaration(PropertyDeclarationSyntax node, out FieldDeclarationSyntax fieldDeclaration)
    {
        fieldDeclaration = null;

        if (IsSupportedPropertyShape(node) == false
            || HasConditionalDirectiveBetweenModifiers(node))
        {
            return false;
        }

        var fieldName = GetFieldName(node);
        var modifiers = node.Modifiers;
        var type = node.Type;

        if (IsGetOnlyProperty(node)
            && modifiers.Any(SyntaxKind.ReadOnlyKeyword) == false)
        {
            var orderedModifiers = ModifierOrderingUtilities.OrderModifiersForRh7105(modifiers.Add(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)));

            if (ModifierOrderingUtilities.TryWriteModifiersInOrder(modifiers, type.GetFirstToken(), orderedModifiers, out modifiers, out var typeLeadingTrivia) == false)
            {
                return false;
            }

            type = type.WithLeadingTrivia(typeLeadingTrivia);
        }

        var initializer = node.Initializer?.WithEqualsToken(node.Initializer.EqualsToken.WithLeadingTrivia(SyntaxFactory.TriviaList()));
        var candidate = SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(type)
                                                                    .AddVariables(SyntaxFactory.VariableDeclarator(fieldName)
                                                                                               .WithInitializer(initializer)))
                                     .WithAttributeLists(node.AttributeLists)
                                     .WithModifiers(modifiers)
                                     .WithTrailingTrivia(node.GetTrailingTrivia());

        if (KeepsEveryCommentAndDirective(node, candidate) == false)
        {
            return false;
        }

        fieldDeclaration = candidate;

        return true;
    }

    /// <summary>
    /// Determines whether an <c>#if</c>, <c>#elif</c>, <c>#else</c>, or <c>#endif</c> stands between two modifiers, or
    /// in front of the type: behind the last modifier, or, for a property without modifiers, in front of the type as
    /// its first token. Another build configuration can compile other modifiers there, so the conversion is withheld
    /// whatever the branches hold (RH2001-009)
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if such a directive stands between the modifiers or in front of the type</returns>
    private static bool HasConditionalDirectiveBetweenModifiers(PropertyDeclarationSyntax node)
    {
        return node.Modifiers
                   .Skip(1)
                   .SelectMany(modifier => modifier.LeadingTrivia)
                   .Concat(node.Type.GetLeadingTrivia())
                   .Any(trivia => trivia.Kind() is SyntaxKind.IfDirectiveTrivia or SyntaxKind.ElifDirectiveTrivia or SyntaxKind.ElseDirectiveTrivia or SyntaxKind.EndIfDirectiveTrivia);
    }

    /// <summary>
    /// Checks whether the field keeps every comment, directive, and disabled text of the property, in the same order
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <param name="fieldDeclaration">Field declaration built from the property</param>
    /// <returns><see langword="true"/> if the field holds the same comments, directives, and disabled text</returns>
    private static bool KeepsEveryCommentAndDirective(PropertyDeclarationSyntax node, FieldDeclarationSyntax fieldDeclaration)
    {
        return GetCommentsAndDirectives(node).SequenceEqual(GetCommentsAndDirectives(fieldDeclaration), StringComparer.Ordinal);
    }

    /// <summary>
    /// Gets the text of every comment, directive, and disabled text of a declaration, in document order
    /// </summary>
    /// <param name="declaration">Declaration</param>
    /// <returns>The texts</returns>
    private static IEnumerable<string> GetCommentsAndDirectives(SyntaxNode declaration)
    {
        return declaration.DescendantTrivia()
                          .Where(static trivia => SyntaxTriviaUtilities.IsCommentTrivia(trivia)
                                                  || SyntaxTriviaUtilities.IsDirectiveOrDisabledTextTrivia(trivia))
                          .Select(static trivia => trivia.ToFullString());
    }

    /// <summary>
    /// Checks whether the property has the shape the fix converts: an auto-property with a getter, an optional setter,
    /// and no attributes, accessor modifiers, accessor bodies, expression body, or explicit interface specifier
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns><see langword="true"/> if the fixer can convert the property's shape</returns>
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
    /// Checks whether a field of the property's type would behave as the property: the type is a reference type, a type
    /// parameter constrained to a reference type, an enum, a struct declared <see langword="readonly"/>, or a
    /// <see cref="Nullable{T}"/> whose underlying type is an enum or a struct declared <see langword="readonly"/>
    /// (RH2001-008). Whether a struct is declared <see langword="readonly"/> is read from the type symbol, so from source
    /// or from the metadata of the referenced assembly
    /// </summary>
    /// <param name="type">Property type</param>
    /// <returns><see langword="true"/> if every use of the field behaves as the use of the property</returns>
    private static bool HasValueSafeFieldType(ITypeSymbol type)
    {
        switch (type)
        {
            case null:
                {
                    return false;
                }

            case ITypeParameterSymbol typeParameter:
                {
                    return typeParameter.IsReferenceType;
                }

            case INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments.Length: 1 } nullableType:
                {
                    return nullableType.TypeArguments[0] is { TypeKind: TypeKind.Enum } or { TypeKind: TypeKind.Struct, IsReadOnly: true };
                }

            case { TypeKind: TypeKind.Struct }:
                {
                    return type.IsReadOnly;
                }

            default:
                {
                    return true;
                }
        }
    }

    /// <summary>
    /// Checks whether code that a conditional directive excludes names the property or the field name the fix would give
    /// it. Excluded code is not bound, so the rename cannot reach a reference there (SHARED-FIX-003)
    /// </summary>
    /// <param name="document">Document holding the property</param>
    /// <param name="symbol">Property symbol</param>
    /// <param name="node">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if excluded code names the old or the new name</returns>
    /// <remarks>
    /// The property is private and neither overrides nor implements a member, so the rename reaches only the property
    /// itself, and code can only refer to it in the declarations of its containing type
    /// </remarks>
    private static Task<bool> HasExcludedCodeOccurrenceAsync(Document document, IPropertySymbol symbol, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
    {
        return ExcludedCodeUtilities.HasExcludedCodeOccurrenceAsync(document.Project.Solution,
                                                                    [symbol],
                                                                    ImmutableHashSet.Create(StringComparer.Ordinal, node.Identifier.ValueText, GetFieldName(node)),
                                                                    cancellationToken);
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
                && TryCreateFieldDeclaration(node, out _)
                && HasFieldNameCollision(semanticModel, node) == false
                && semanticModel?.GetDeclaredSymbol(node, context.CancellationToken) is { } symbol
                && HasValueSafeFieldType(symbol.Type)
                && await HasExcludedCodeOccurrenceAsync(context.Document, symbol, node, context.CancellationToken).ConfigureAwait(false) == false)
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