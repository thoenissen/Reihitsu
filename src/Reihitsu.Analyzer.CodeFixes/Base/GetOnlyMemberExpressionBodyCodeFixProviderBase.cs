using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.CodeFixes.Core;
using Reihitsu.Core;
using Reihitsu.Formatter;

namespace Reihitsu.Analyzer.CodeFixes.Base;

/// <summary>
/// Base class for code-fix providers that replace the accessor list of a get-only property or indexer with the expression
/// body the formatter writes for it (SHARED-GETONLY-005). The conversion is the formatter's own,
/// <see cref="GetOnlyMemberExpressionBodyRewriteUtilities.TryConvertToExpressionBody"/>; the formatter lays out the converted
/// member, and of its result only the arrow clause, the semicolon, and the gap in front of the arrow replace the accessor
/// list, while the rest of the member stays as written (SHARED-FIX-006)
/// </summary>
public abstract class GetOnlyMemberExpressionBodyCodeFixProviderBase : ReihitsuCodeFixProviderBase
{
    #region Fields

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    private readonly string _diagnosticId;

    /// <summary>
    /// Title
    /// </summary>
    private readonly string _title;

    /// <summary>
    /// Analyzer that reports the diagnostics the code fix corrects
    /// </summary>
    private readonly DiagnosticAnalyzer _analyzer;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="title">Title</param>
    /// <param name="analyzer">Analyzer that reports the diagnostics the code fix corrects</param>
    private protected GetOnlyMemberExpressionBodyCodeFixProviderBase(string diagnosticId, string title, DiagnosticAnalyzer analyzer)
    {
        _diagnosticId = diagnosticId;
        _title = title;
        _analyzer = analyzer;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Converts the member as the formatter converts it
    /// </summary>
    /// <param name="member">The property or indexer declaration as written</param>
    /// <param name="convertedMember">The expression-bodied member</param>
    /// <returns><see langword="true"/> if the formatter's conversion converts the member; otherwise, <see langword="false"/></returns>
    private static bool TryConvert(BasePropertyDeclarationSyntax member, out BasePropertyDeclarationSyntax convertedMember)
    {
        convertedMember = null;

        return member.SyntaxTree.Options is CSharpParseOptions parseOptions
               && GetOnlyMemberExpressionBodyRewriteUtilities.TryConvertToExpressionBody(member,
                                                                                         parseOptions.LanguageVersion,
                                                                                         LineEndingUtilities.DetectEndOfLine(member.SyntaxTree.GetRoot()),
                                                                                         out convertedMember);
    }

    /// <summary>
    /// Determines whether the fix can be offered for the member. It is withheld where the formatter keeps the accessor list
    /// for a comment, a directive, or disabled text (SHARED-GETONLY-003, SHARED-GETONLY-007); where the getter as written
    /// holds a shape the formatter refuses, which the conversion would delete with the gaps around the getter's arrow
    /// (SHARED-GETONLY-006, SHARED-FIX-003); where the converted member the formatter receives holds such a shape
    /// (SHARED-FIX-010); and where a comment separates the member from a sibling member on one line (SHARED-GETONLY-008)
    /// </summary>
    /// <param name="member">The property or indexer declaration as written</param>
    /// <returns><see langword="true"/> if the fix can be registered; otherwise, <see langword="false"/></returns>
    private static bool CanConvert(BasePropertyDeclarationSyntax member)
    {
        return TryConvert(member, out var convertedMember)
               && ReihitsuFormatter.FindUnsupportedSyntax(member.AccessorList.Accessors[0]) == null
               && ReihitsuFormatter.FindUnsupportedSyntax(convertedMember) == null
               && BlankLineCodeFixUtilities.SharesLineWithSiblingMemberAcrossComment(member) == false;
    }

    /// <summary>
    /// Applying code fix. A member that shares its line with another member is first separated from it, so that both stand
    /// on lines of their own (SHARED-FIX-007, SHARED-GETONLY-008); then the accessor list is replaced with the arrow clause
    /// and semicolon the formatter writes for the converted member, and the rest of the member stays as written
    /// (SHARED-GETONLY-005)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="member">The property or indexer declaration as written</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The fixed document</returns>
    private static Task<Document> ApplyCodeFixAsync(Document document, BasePropertyDeclarationSyntax member, CancellationToken cancellationToken)
    {
        return BlankLineCodeFixUtilities.FormatMemberOnLinesOfItsOwnAsync(document, member, ConvertAsync, cancellationToken);
    }

    /// <summary>
    /// Replaces the accessor list with the arrow clause and semicolon the formatter writes for the converted member where it
    /// stands in the document. The gap in front of the arrow is the formatter's as well; everything in front of that gap
    /// stays as written (<see cref="ExpressionBodyCodeFixUtilities.ReplaceBodyAsync"/>)
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="node">The property or indexer declaration, which belongs to <paramref name="document"/></param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document with the accessor list replaced, or the unchanged document when the member is not converted</returns>
    private static Task<Document> ConvertAsync(Document document, SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is not BasePropertyDeclarationSyntax member
            || TryConvert(member, out var convertedMember) == false)
        {
            return Task.FromResult(document);
        }

        return ExpressionBodyCodeFixUtilities.ReplaceBodyAsync(document,
                                                               member,
                                                               convertedMember,
                                                               GetBodyStartToken,
                                                               (signature, formattedMember) => formattedMember is BasePropertyDeclarationSyntax formattedProperty
                                                                                               && GetExpressionBody(formattedProperty) is { } expressionBody
                                                                                                   ? WithExpressionBody((BasePropertyDeclarationSyntax)signature, expressionBody, GetSemicolonToken(formattedProperty))
                                                                                                   : null,
                                                               BlankLineCodeFixUtilities.FormatNodeInDocumentAsync,
                                                               cancellationToken);
    }

    /// <summary>
    /// Gets the first token of the body of a property or indexer: the arrow of its expression body, or the opening brace of
    /// its accessor list
    /// </summary>
    /// <param name="node">Property or indexer declaration</param>
    /// <returns>The first token of the body, or <see langword="default"/> when there is none</returns>
    private static SyntaxToken GetBodyStartToken(SyntaxNode node)
    {
        return node is BasePropertyDeclarationSyntax member
                   ? GetExpressionBody(member)?.ArrowToken ?? member.AccessorList?.OpenBraceToken ?? default
                   : default;
    }

    /// <summary>
    /// Gets the expression body of a property or indexer
    /// </summary>
    /// <param name="member">Property or indexer declaration</param>
    /// <returns>The expression body, or <see langword="null"/> when there is none</returns>
    private static ArrowExpressionClauseSyntax GetExpressionBody(BasePropertyDeclarationSyntax member)
    {
        return member switch
               {
                   PropertyDeclarationSyntax property => property.ExpressionBody,
                   IndexerDeclarationSyntax indexer => indexer.ExpressionBody,
                   _ => null
               };
    }

    /// <summary>
    /// Gets the semicolon of a property or indexer
    /// </summary>
    /// <param name="member">Property or indexer declaration</param>
    /// <returns>The semicolon token</returns>
    private static SyntaxToken GetSemicolonToken(BasePropertyDeclarationSyntax member)
    {
        return member switch
               {
                   PropertyDeclarationSyntax property => property.SemicolonToken,
                   IndexerDeclarationSyntax indexer => indexer.SemicolonToken,
                   _ => default
               };
    }

    /// <summary>
    /// Replaces the accessor list of a property or indexer with an expression body and its semicolon
    /// </summary>
    /// <param name="member">Property or indexer declaration with an accessor list</param>
    /// <param name="expressionBody">Expression body</param>
    /// <param name="semicolonToken">Semicolon, which carries the trivia behind the member</param>
    /// <returns>The expression-bodied member, or <see langword="null"/> for any other declaration</returns>
    private static BasePropertyDeclarationSyntax WithExpressionBody(BasePropertyDeclarationSyntax member, ArrowExpressionClauseSyntax expressionBody, SyntaxToken semicolonToken)
    {
        return member switch
               {
                   PropertyDeclarationSyntax property => property.WithAccessorList(null)
                                                                 .WithExpressionBody(expressionBody)
                                                                 .WithSemicolonToken(semicolonToken),
                   IndexerDeclarationSyntax indexer => indexer.WithAccessorList(null)
                                                              .WithExpressionBody(expressionBody)
                                                              .WithSemicolonToken(semicolonToken),
                   _ => null
               };
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [_diagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        // Two members on one line are separated by the fixes of both, so the fixes run one after another
        return DocumentOrderFixAllProvider.Create(_analyzer);
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

        foreach (var diagnostic in context.Diagnostics)
        {
            // The diagnostic covers the accessor list, so the nearest owner is the property or indexer that declares it
            var member = root.FindNode(diagnostic.Location.SourceSpan)
                             .FirstAncestorOrSelf<BasePropertyDeclarationSyntax>();

            if (member?.AccessorList != null
                && CanConvert(member))
            {
                context.RegisterCodeFix(CodeAction.Create(_title,
                                                          cancellationToken => ApplyCodeFixAsync(context.Document, member, cancellationToken),
                                                          GetType().Name),
                                        diagnostic);
            }
        }
    }

    #endregion // ReihitsuCodeFixProviderBase
}