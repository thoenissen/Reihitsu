using System.Threading;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8201: The &lt;inheritdoc&gt; Tag should be used if possible
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8201InheritdocShouldBeUsedAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8201";

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8201InheritdocShouldBeUsedAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8201Title), nameof(AnalyzerResources.RH8201MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the header of any declaration of the type — from its identifier to its opening brace, including
    /// the base list and constraint clauses — carries preprocessor directives or disabled text
    /// </summary>
    /// <param name="typeSymbol">Type symbol</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the implemented interfaces of the type may differ between build configurations</returns>
    private static bool HasConditionalTypeHeader(INamedTypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        static bool IsConditional(SyntaxTrivia trivia)
        {
            return trivia.IsDirective
                   || trivia.IsKind(SyntaxKind.DisabledTextTrivia);
        }

        foreach (var reference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(cancellationToken) is TypeDeclarationSyntax typeDeclaration)
            {
                var lastToken = typeDeclaration.OpenBraceToken.IsKind(SyntaxKind.None)
                                    ? typeDeclaration.GetLastToken()
                                    : typeDeclaration.OpenBraceToken;
                var headerSpan = TextSpan.FromBounds(typeDeclaration.Identifier.SpanStart, lastToken.Span.End);

                foreach (var token in typeDeclaration.DescendantTokens(headerSpan))
                {
                    // Directives and disabled text are always leading trivia, so the tokens after the identifier cover the header
                    if (token != typeDeclaration.Identifier
                        && token.LeadingTrivia.Any(IsConditional))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines whether the member inherits documentation from an overridden or implemented member
    /// </summary>
    /// <param name="node">Member declaration</param>
    /// <param name="semanticModel">Semantic model</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns><see langword="true"/> if the member overrides a base member or implements an interface member</returns>
    /// <remarks>
    /// The <see langword="override"/> modifier is checked syntactically before any semantic lookup. A field-like event
    /// only qualifies when every declarator inherits, because the code fix replaces the documentation shared by all of
    /// them. An implicit interface implementation does not qualify when the header of its containing type is
    /// conditionally compiled, because the member may implement nothing in another build configuration, where the
    /// replaced documentation would be lost
    /// </remarks>
    private static bool InheritsDocumentation(MemberDeclarationSyntax node, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        if (node.Modifiers.Any(SyntaxKind.OverrideKeyword))
        {
            return true;
        }

        var inherits = node is EventFieldDeclarationSyntax eventFieldDeclaration
                           ? eventFieldDeclaration.Declaration.Variables.All(variable => DocumentationAnalysisUtilities.OverridesOrImplementsMember(semanticModel.GetDeclaredSymbol(variable, cancellationToken)))
                           : DocumentationAnalysisUtilities.CanInheritDocumentation(node, semanticModel, cancellationToken);

        if (inherits == false
            || DocumentationAnalysisUtilities.IsExplicitInterfaceImplementation(node))
        {
            return inherits;
        }

        return node.Parent is not TypeDeclarationSyntax typeDeclaration
               || semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken) is not INamedTypeSymbol typeSymbol
               || HasConditionalTypeHeader(typeSymbol, cancellationToken) == false;
    }

    /// <summary>
    /// Analyzes documentation comments on members that override a base member or implement an interface member and
    /// reports when no &lt;inheritdoc&gt; tag is present
    /// </summary>
    /// <param name="context">Context</param>
    private void OnDocumentationCommentTrivia(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MemberDeclarationSyntax node)
        {
            var documentation = node.GetLeadingTrivia()
                                    .FirstOrDefault(SyntaxTriviaUtilities.IsDocumentationCommentTrivia);

            if (documentation != default
                && documentation.HasStructure)
            {
                static bool ContainsInheritDoc(SyntaxNode checkNode)
                {
                    return checkNode switch
                           {
                               XmlElementStartTagSyntax element => element.Name.LocalName.ValueText.Equals("inheritdoc", StringComparison.InvariantCultureIgnoreCase),
                               XmlEmptyElementSyntax element => element.Name.LocalName.ValueText.Equals("inheritdoc", StringComparison.InvariantCultureIgnoreCase),
                               _ => checkNode.ChildNodes().Any(ContainsInheritDoc)
                           };
                }

                if (ContainsInheritDoc(documentation.GetStructure()) == false
                    && InheritsDocumentation(node, context.SemanticModel, context.CancellationToken))
                {
                    context.ReportDiagnostic(CreateDiagnostic(documentation.GetLocation()));
                }
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.EventDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.EventFieldDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.IndexerDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.OperatorDeclaration);
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnDocumentationCommentTrivia, SyntaxKind.ConversionOperatorDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}