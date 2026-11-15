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

using Reihitsu.Analyzer.CodeFixes.Base;
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
public class RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider : ReihitsuCodeFixProviderBase
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

        var symbol = await GetAnnotatedPropertySymbolAsync(document, propertyAnnotation, cancellationToken).ConfigureAwait(false);

        return symbol == null
                   ? solution
                   : await Renamer.RenameSymbolAsync(solution, symbol, default, fieldName, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the symbol of the property declaration that carries the annotation
    /// </summary>
    /// <param name="document">Document holding the annotated property</param>
    /// <param name="propertyAnnotation">Annotation tracking the property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The property symbol, or <see langword="null"/> when the document holds no annotated property</returns>
    private static async Task<IPropertySymbol> GetAnnotatedPropertySymbolAsync(Document document, SyntaxAnnotation propertyAnnotation, CancellationToken cancellationToken)
    {
        if (document == null)
        {
            return null;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var annotatedRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var annotatedNode = annotatedRoot?.GetAnnotatedNodes(propertyAnnotation).OfType<PropertyDeclarationSyntax>().FirstOrDefault();

        return annotatedNode != null
                   ? semanticModel?.GetDeclaredSymbol(annotatedNode, cancellationToken)
                   : null;
    }

    /// <summary>
    /// Checks whether the rename to the field name would change which symbol a reference binds to, or rewrite a reference
    /// to another symbol (RH2001-011)
    /// </summary>
    /// <param name="document">Document holding the property</param>
    /// <param name="node">Property declaration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>
    /// <see langword="true"/> if the rename changes a binding, rewrites a reference to another symbol, or renames an
    /// inferred anonymous-object member or tuple element, or its result cannot be checked
    /// </returns>
    /// <remarks>
    /// The check runs the rename and compares, in every document that can refer to the property, the references spelled
    /// like the field name that bind to a symbol other than the property, before and after the rename. A reference to
    /// another symbol that the renamer rewrites, such as qualifying it with <c>base.</c> or a type, or turning an
    /// extension call into a static call, changes its qualifier. A reference to another symbol left as written that then
    /// binds to the renamed property disappears from the references to that symbol, and a renamed reference to the
    /// property that binds to another symbol adds one. A reference to the property that the renamer qualifies with
    /// <c>this.</c> or its type binds to the renamed property and is not compared, so the fix stays offered for it. The
    /// references include the <c>cref</c> names of documentation comments. A reference to the property that gives an
    /// anonymous-object member or a tuple element its inferred name withholds the fix, because the rename renames that
    /// member as well
    /// </remarks>
    private static async Task<bool> RenameChangesBindingAsync(Document document, PropertyDeclarationSyntax node, CancellationToken cancellationToken)
    {
        if (GetFieldName(node, out var fieldName) == false)
        {
            return false;
        }

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return true;
        }

        var propertyAnnotation = new SyntaxAnnotation();
        var annotatedDocument = document.WithSyntaxRoot(root.ReplaceNode(node, node.WithAdditionalAnnotations(propertyAnnotation)));
        var originalSolution = annotatedDocument.Project.Solution;
        var renamedSolution = await RenamePropertyToFieldNameAsync(annotatedDocument, node, propertyAnnotation, cancellationToken).ConfigureAwait(false);
        var originalSymbol = await GetAnnotatedPropertySymbolAsync(annotatedDocument, propertyAnnotation, cancellationToken).ConfigureAwait(false);
        var renamedSymbol = await GetAnnotatedPropertySymbolAsync(renamedSolution.GetDocument(document.Id), propertyAnnotation, cancellationToken).ConfigureAwait(false);

        if (originalSymbol == null
            || renamedSymbol == null)
        {
            return true;
        }

        foreach (var documentId in ExcludedCodeUtilities.GetReferringDocuments(renamedSolution, renamedSymbol).Select(static referringDocument => referringDocument.Id))
        {
            if (await GivesInferredMemberNameAsync(originalSolution.GetDocument(documentId), originalSymbol, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }

            var originalReferences = await GetOtherSymbolReferencesAsync(originalSolution.GetDocument(documentId), fieldName, originalSymbol, cancellationToken).ConfigureAwait(false);
            var renamedReferences = await GetOtherSymbolReferencesAsync(renamedSolution.GetDocument(documentId), fieldName, renamedSymbol, cancellationToken).ConfigureAwait(false);

            if (originalReferences.SequenceEqual(renamedReferences, StringComparer.Ordinal) == false)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks whether a reference to the property gives an anonymous-object member without <c>Name =</c> or a tuple
    /// element without <c>Name:</c> its inferred name
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="property">Property before the rename</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if a member or element takes its inferred name from a reference to the property</returns>
    private static async Task<bool> GivesInferredMemberNameAsync(Document document, IPropertySymbol property, CancellationToken cancellationToken)
    {
        var root = document == null
                       ? null
                       : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return false;
        }

        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        foreach (var expression in root.DescendantNodes().Select(static node => node switch
                                                                                {
                                                                                    AnonymousObjectMemberDeclaratorSyntax { NameEquals: null } declarator => declarator.Expression,
                                                                                    ArgumentSyntax { NameColon: null, Parent: TupleExpressionSyntax } argument => argument.Expression,
                                                                                    _ => null
                                                                                }))
        {
            if (expression != null
                && SyntaxFacts.TryGetInferredMemberName(expression) == property.Name
                && expression.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().LastOrDefault() is { } name
                && SymbolEqualityComparer.Default.Equals(semanticModel?.GetSymbolInfo(name, cancellationToken).Symbol, property))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets a description of every reference spelled like the field name that does not bind to the property, including
    /// the <c>cref</c> names of documentation comments: the symbol it binds to, or its candidates, and the qualifier
    /// written in front of it, with every reference to the property in the qualifier described by one marker, sorted
    /// ordinally
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="fieldName">Field name</param>
    /// <param name="property">Property whose references are skipped and marked in qualifiers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sorted descriptions</returns>
    private static async Task<List<string>> GetOtherSymbolReferencesAsync(Document document, string fieldName, IPropertySymbol property, CancellationToken cancellationToken)
    {
        var references = new List<string>();
        var root = document == null
                       ? null
                       : await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = document == null
                                ? null
                                : await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

        if (root == null
            || semanticModel == null)
        {
            return references;
        }

        foreach (var identifierName in root.DescendantNodes(descendIntoTrivia: true).OfType<IdentifierNameSyntax>().Where(identifierName => identifierName.Identifier.ValueText == fieldName))
        {
            var symbolInfo = semanticModel.GetSymbolInfo(identifierName, cancellationToken);

            if (SymbolEqualityComparer.Default.Equals(symbolInfo.Symbol, property))
            {
                continue;
            }

            var symbols = symbolInfo.Symbol != null
                              ? [symbolInfo.Symbol]
                              : symbolInfo.CandidateSymbols;
            var qualifier = identifierName.Parent switch
                            {
                                MemberAccessExpressionSyntax memberAccess when memberAccess.Name == identifierName => DescribeQualifier(memberAccess.Expression, semanticModel, property, cancellationToken),
                                QualifiedNameSyntax qualifiedName when qualifiedName.Right == identifierName => DescribeQualifier(qualifiedName.Left, semanticModel, property, cancellationToken),
                                MemberBindingExpressionSyntax => "?",
                                _ => string.Empty
                            };

            var boundSymbols = string.Join("|", symbols.Select(static symbol => $"{symbol.Kind} {symbol.ContainingSymbol?.ToDisplayString()} {symbol.ToDisplayString()}"));

            references.Add($"{boundSymbols} {symbolInfo.CandidateReason} {qualifier}");
        }

        references.Sort(StringComparer.Ordinal);

        return references;
    }

    /// <summary>
    /// Describes a qualifier by its tokens, with every reference to the property replaced by one marker, so that the
    /// rename of the property does not change the description
    /// </summary>
    /// <param name="qualifier">Qualifier</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="property">Property</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The description</returns>
    private static string DescribeQualifier(SyntaxNode qualifier, SemanticModel semanticModel, IPropertySymbol property, CancellationToken cancellationToken)
    {
        return string.Join(" ",
                           qualifier.DescendantTokens()
                                    .Select(token => token.Parent is IdentifierNameSyntax name
                                                     && SymbolEqualityComparer.Default.Equals(semanticModel.GetSymbolInfo(name, cancellationToken).Symbol, property)
                                                         ? "\u0001"
                                                         : token.Text));
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
                   : (await ReihitsuFormatter.FormatNodeInDocumentAsync(updatedDocument, formattedFieldDeclaration, cancellationToken).ConfigureAwait(false)).Output.Project.Solution;
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
    /// directive between the modifiers or in front of the type withholds the conversion in any case (RH2001-009), and
    /// one in front of the first modifier, or behind a property without an initializer, unless its group wraps whole
    /// members (RH2001-010). The field gets a new
    /// name, a new <c>;</c>, and no accessor list, so the trivia of the property's name, of its accessor list, of its
    /// <c>;</c>, and in front of the initializer's <c>=</c> is not carried over. When that trivia holds a comment or a
    /// directive, the field would lose it, and the conversion is withheld (RH2001-007, SHARED-FIX-003)
    /// </remarks>
    private static bool TryCreateFieldDeclaration(PropertyDeclarationSyntax node, out FieldDeclarationSyntax fieldDeclaration)
    {
        fieldDeclaration = null;

        if (IsSupportedPropertyShape(node) == false
            || HasConditionalDirectiveBetweenModifiers(node)
            || (node.Modifiers.Count > 0
                && OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers(node.Modifiers[0]) == false)
            || (node.Initializer == null
                && OrderingDeclarationUtilities.ConditionalGroupsWrapWholeMembers(node.GetLastToken().GetNextToken()) == false))
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
    /// Checks whether a document the rename can change, besides the diagnostic's own, holds code that another build
    /// configuration hides, so that the fix is not offered (SHARED-FIX-012)
    /// </summary>
    /// <param name="document">Document holding the property</param>
    /// <param name="symbol">Property symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if such a document holds a hiding carrier</returns>
    /// <remarks>
    /// The property is private, so the rename reaches only the documents that declare a part of its containing type
    /// </remarks>
    private static async Task<bool> HasHidingCarrierInReferringDocumentAsync(Document document, IPropertySymbol symbol, CancellationToken cancellationToken)
    {
        foreach (var referringDocument in ExcludedCodeUtilities.GetReferringDocuments(document.Project.Solution, symbol))
        {
            if (await HoldsHidingCarrierAsync(referringDocument, cancellationToken).ConfigureAwait(false))
            {
                return true;
            }
        }

        return false;
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

    #endregion // CodeFixProvider

    #region ReihitsuCodeFixProviderBase

    /// <inheritdoc/>
    protected override bool FormatsThroughFormatter => true;

    /// <inheritdoc/>
    protected sealed override async Task RegisterCodeFixesCoreAsync(CodeFixContext context)
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
                && TryCreateFieldDeclaration(node, out var fieldDeclaration)
                && ReihitsuFormatter.FindUnsupportedSyntax(fieldDeclaration) == null
                && HasFieldNameCollision(semanticModel, node) == false
                && semanticModel?.GetDeclaredSymbol(node, context.CancellationToken) is { } symbol
                && HasValueSafeFieldType(symbol.Type)
                && await HasExcludedCodeOccurrenceAsync(context.Document, symbol, node, context.CancellationToken).ConfigureAwait(false) == false
                && await HasHidingCarrierInReferringDocumentAsync(context.Document, symbol, context.CancellationToken).ConfigureAwait(false) == false
                && await RenameChangesBindingAsync(context.Document, node, context.CancellationToken).ConfigureAwait(false) == false)
            {
                context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH2001Title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, node, cancellationToken),
                                                          nameof(RH2001PrivateAutoPropertiesShouldNotBeUsedCodeFixProvider)),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}