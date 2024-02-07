using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH0202ClassNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0202ClassNameCasingCodeFixProvider))]
public class RH0202ClassNameCasingCodeFixProvider : CodeFixProvider
{
    #region Methods

    private async Task<Document> ApplyCodeFixAsync(Document document, ClassDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root != null)
        {
            var identifier = CasingUtilities.ToPascalCase(memberDeclaration.Identifier.ValueText);

            root = root.ReplaceNode(memberDeclaration, memberDeclaration.WithIdentifier(SyntaxFactory.Identifier(identifier)));

            document = document.WithSyntaxRoot(root);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0202ClassNameCasingAnalyzer.DiagnosticId);

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
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                if (root.FindNode(diagnosticSpan) is ClassDeclarationSyntax declaration)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0401Title,
                                                              c => ApplyCodeFixAsync(context.Document, declaration, c),
                                                              nameof(CodeFixResources.RH0401Title)),
                                            diagnostic);
                }
            }
        }
    }


    #endregion // CodeFixProvider
}

/// <summary>
/// Providing fixes for <see cref="RH0203StructNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0203StructNameCasingCodeFixProvider))]
public class RH0203StructNameCasingCodeFixProvider : CodeFixProvider
{
    #region Methods

    private async Task<Document> ApplyCodeFixAsync(Document document, StructDeclarationSyntax memberDeclaration, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken)
                                 .ConfigureAwait(false);

        if (root != null)
        {
            var identifier = CasingUtilities.ToPascalCase(memberDeclaration.Identifier.ValueText);

            root = root.ReplaceNode(memberDeclaration, memberDeclaration.WithIdentifier(SyntaxFactory.Identifier(identifier)));

            document = document.WithSyntaxRoot(root);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RH0203StructNameCasingAnalyzer.DiagnosticId);

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
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                if (root.FindNode(diagnosticSpan) is StructDeclarationSyntax declaration)
                {
                    context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0401Title,
                                                              c => ApplyCodeFixAsync(context.Document, declaration, c),
                                                              nameof(CodeFixResources.RH0401Title)),
                                            diagnostic);
                }
            }
        }
    }


    #endregion // CodeFixProvider
}

/// <summary>
/// Providing fixes for <see cref="RH0212PrivateFieldCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0212PrivateFieldCasingCodeFixProvider))]
public class RH0212PrivateFieldCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0212PrivateFieldCasingCodeFixProvider()
        : base(RH0212PrivateFieldCasingAnalyzer.DiagnosticId, CodeFixResources.RH0212Title, CasingUtilities.ToCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(VariableDeclaratorSyntax node)
    {
        return node.Identifier.ValueText;
    }

    /// <inheritdoc/>
    protected override SyntaxNode ReplaceIdentifier(VariableDeclaratorSyntax node, string identifier)
    {
        return node.WithIdentifier(SyntaxFactory.Identifier(identifier));
    }

    #endregion // CasingCodeFixProviderBase
}

/// <summary>
/// Code fix provider for casing rules
/// </summary>
/// <typeparam name="T">Node type</typeparam>
public abstract class CasingCodeFixProviderBase<T> : CodeFixProvider where T : SyntaxNode
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private string _title;

    /// <summary>
    /// Casing conversion
    /// </summary>
    private Func<string, string> _casingConversion;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="casingConversion">Casing conversion</param>
    protected CasingCodeFixProviderBase(string diagnosticId, string title, Func<string, string> casingConversion)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _casingConversion = casingConversion;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Replace the identifier with new one
    /// </summary>
    /// <param name="node">Node</param>
    /// <param name="identifier">Fixed identifier value text</param>
    /// <returns>Fixed identifier</returns>
    protected abstract SyntaxNode ReplaceIdentifier(T node, string identifier);

    /// <summary>
    /// Get identifier value text
    /// </summary>
    /// <param name="node">Node</param>
    /// <returns>Identifier value text</returns>
    protected abstract string GetIdentifier(T node);

    /// <summary>
    /// Applying the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">Node</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated <see cref="Document"/> with the code fix applied.</returns>
    private async Task<Document> ApplyCodeFixAsync(Document document, T node, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root != null)
        {
            var identifier = GetIdentifier(node);

            identifier = _casingConversion(identifier);

            root = root.ReplaceNode(node, ReplaceIdentifier(node, identifier));

            document = document.WithSyntaxRoot(root);
        }

        return document;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(_diagnosticId);

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
                var diagnosticSpan = diagnostic.Location.SourceSpan;

                if (root.FindNode(diagnosticSpan) is T node)
                {
                    context.RegisterCodeFix(CodeAction.Create(_title,
                                                              c => ApplyCodeFixAsync(context.Document, node, c)),
                                            diagnostic);
                }
            }
        }
    }

    #endregion // CodeFixProvider
}