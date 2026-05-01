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
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0391AssignmentsMustHaveProperLineBreaksAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0391AssignmentsMustHaveProperLineBreaksCodeFixProvider))]
public class RH0391AssignmentsMustHaveProperLineBreaksCodeFixProvider : CodeFixProvider
{
    #region Methods

    /// <summary>
    /// Applies the code fix
    /// </summary>
    /// <param name="document">Document</param>
    /// <param name="diagnosticSpan">Diagnostic span</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated document</returns>
    private static async Task<Document> ApplyCodeFixAsync(Document document, TextSpan diagnosticSpan, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root == null)
        {
            return document;
        }

        var diagnosticNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);
        var assignmentNode = GetFormattingNode(diagnosticNode);
        var updatedNode = assignmentNode == null ? null : FixAssignmentNode(assignmentNode);

        return assignmentNode == null || updatedNode == null
                   ? document
                   : document.WithSyntaxRoot(root.ReplaceNode(assignmentNode, updatedNode));
    }

    /// <summary>
    /// Gets the narrowest formatting node that can still fix the assignment
    /// </summary>
    /// <param name="diagnosticNode">Diagnostic node</param>
    /// <returns>Formatting node</returns>
    private static SyntaxNode GetFormattingNode(SyntaxNode diagnosticNode)
    {
        var formattingNode = (diagnosticNode.FirstAncestorOrSelf<AssignmentExpressionSyntax>()
                                  ?? (SyntaxNode)diagnosticNode.FirstAncestorOrSelf<VariableDeclaratorSyntax>())
                                 ?? diagnosticNode.FirstAncestorOrSelf<PropertyDeclarationSyntax>();

        return formattingNode;
    }

    /// <summary>
    /// Fixes the reported assignment node without formatting the surrounding scope
    /// </summary>
    /// <param name="assignmentNode">Assignment node</param>
    /// <returns>Updated assignment node</returns>
    private static SyntaxNode FixAssignmentNode(SyntaxNode assignmentNode)
    {
        return assignmentNode switch
               {
                   AssignmentExpressionSyntax assignmentExpression => FixAssignmentExpression(assignmentExpression),
                   VariableDeclaratorSyntax variableDeclarator => FixVariableDeclarator(variableDeclarator),
                   PropertyDeclarationSyntax propertyDeclaration => FixPropertyDeclaration(propertyDeclaration),
                   _ => assignmentNode,
               };
    }

    /// <summary>
    /// Fixes a simple assignment expression
    /// </summary>
    /// <param name="node">Assignment expression</param>
    /// <returns>Updated assignment expression</returns>
    private static AssignmentExpressionSyntax FixAssignmentExpression(AssignmentExpressionSyntax node)
    {
        node = CollapseOperatorToTargetLine(node, node.Left.GetLastToken(), node.OperatorToken);

        return CollapseValueToEqualsLine(node, node.OperatorToken, node.Right.GetFirstToken());
    }

    /// <summary>
    /// Fixes a variable or field declarator initializer
    /// </summary>
    /// <param name="node">Variable declarator</param>
    /// <returns>Updated variable declarator</returns>
    private static VariableDeclaratorSyntax FixVariableDeclarator(VariableDeclaratorSyntax node)
    {
        if (node.Initializer == null)
        {
            return node;
        }

        node = CollapseOperatorToTargetLine(node, node.Identifier, node.Initializer.EqualsToken);

        if (node.Initializer == null)
        {
            return node;
        }

        return CollapseValueToEqualsLine(node, node.Initializer.EqualsToken, node.Initializer.Value.GetFirstToken());
    }

    /// <summary>
    /// Fixes a property initializer
    /// </summary>
    /// <param name="node">Property declaration</param>
    /// <returns>Updated property declaration</returns>
    private static PropertyDeclarationSyntax FixPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        if (node.Initializer == null || node.AccessorList == null)
        {
            return node;
        }

        var targetToken = node.AccessorList.CloseBraceToken;
        node = CollapseOperatorToTargetLine(node, targetToken, node.Initializer.EqualsToken);

        if (node.Initializer == null)
        {
            return node;
        }

        return CollapseValueToEqualsLine(node, node.Initializer.EqualsToken, node.Initializer.Value.GetFirstToken());
    }

    /// <summary>
    /// Moves the equals operator onto the same line as the assignment target
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="node">Node to update</param>
    /// <param name="targetToken">Assignment target token</param>
    /// <param name="operatorToken">Equals token</param>
    /// <returns>Updated node</returns>
    private static TNode CollapseOperatorToTargetLine<TNode>(TNode node, SyntaxToken targetToken, SyntaxToken operatorToken)
        where TNode : SyntaxNode
    {
        if (ContainsEndOfLine(targetToken.TrailingTrivia) == false
            && ContainsEndOfLine(operatorToken.LeadingTrivia) == false)
        {
            return node;
        }

        var newTargetToken = targetToken.WithTrailingTrivia(RemoveTrailingWhitespaceAndLineBreaks(targetToken.TrailingTrivia));
        var newOperatorToken = operatorToken.WithLeadingTrivia(EnsureSingleLeadingSpace(RemoveLeadingWhitespaceAndLineBreaks(operatorToken.LeadingTrivia)));

        return node.ReplaceTokens([targetToken, operatorToken],
                                  (original, _) =>
                                  {
                                      if (original == targetToken)
                                      {
                                          return newTargetToken;
                                      }

                                      return newOperatorToken;
                                  });
    }

    /// <summary>
    /// Moves the value start onto the same line as the equals operator
    /// </summary>
    /// <typeparam name="TNode">Node type</typeparam>
    /// <param name="node">Node to update</param>
    /// <param name="operatorToken">Equals token</param>
    /// <param name="valueFirstToken">First token of the value</param>
    /// <returns>Updated node</returns>
    private static TNode CollapseValueToEqualsLine<TNode>(TNode node, SyntaxToken operatorToken, SyntaxToken valueFirstToken)
        where TNode : SyntaxNode
    {
        if (ContainsEndOfLine(operatorToken.TrailingTrivia) == false
            && ContainsEndOfLine(valueFirstToken.LeadingTrivia) == false)
        {
            return node;
        }

        var newOperatorToken = operatorToken.WithTrailingTrivia(EnsureSingleTrailingSpace(RemoveTrailingWhitespaceAndLineBreaks(operatorToken.TrailingTrivia)));
        var newValueFirstToken = valueFirstToken.WithLeadingTrivia(RemoveLeadingWhitespaceAndLineBreaks(valueFirstToken.LeadingTrivia));

        return node.ReplaceTokens([operatorToken, valueFirstToken],
                                  (original, _) =>
                                  {
                                      if (original == operatorToken)
                                      {
                                          return newOperatorToken;
                                      }

                                      return newValueFirstToken;
                                  });
    }

    /// <summary>
    /// Removes leading whitespace and line breaks from trivia
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trimmed trivia list</returns>
    private static SyntaxTriviaList RemoveLeadingWhitespaceAndLineBreaks(SyntaxTriviaList trivia)
    {
        var trimmedTrivia = trivia.ToList();

        while (trimmedTrivia.Count > 0
               && (trimmedTrivia[0].IsKind(SyntaxKind.WhitespaceTrivia) || trimmedTrivia[0].IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            trimmedTrivia.RemoveAt(0);
        }

        return SyntaxFactory.TriviaList(trimmedTrivia);
    }

    /// <summary>
    /// Removes trailing whitespace and line breaks from trivia
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trimmed trivia list</returns>
    private static SyntaxTriviaList RemoveTrailingWhitespaceAndLineBreaks(SyntaxTriviaList trivia)
    {
        var trimmedTrivia = trivia.ToList();

        while (trimmedTrivia.Count > 0)
        {
            var lastTrivia = trimmedTrivia[trimmedTrivia.Count - 1];

            if (lastTrivia.IsKind(SyntaxKind.WhitespaceTrivia) == false
                && lastTrivia.IsKind(SyntaxKind.EndOfLineTrivia) == false)
            {
                break;
            }

            trimmedTrivia.RemoveAt(trimmedTrivia.Count - 1);
        }

        return SyntaxFactory.TriviaList(trimmedTrivia);
    }

    /// <summary>
    /// Ensures the trivia starts with a single space
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trivia list with a single leading space</returns>
    private static SyntaxTriviaList EnsureSingleLeadingSpace(SyntaxTriviaList trivia)
    {
        var newTrivia = new List<SyntaxTrivia>
                        {
                            SyntaxFactory.Space
                        };
        newTrivia.AddRange(trivia);

        return SyntaxFactory.TriviaList(newTrivia);
    }

    /// <summary>
    /// Ensures the trivia ends with a single space
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns>Trivia list with a single trailing space</returns>
    private static SyntaxTriviaList EnsureSingleTrailingSpace(SyntaxTriviaList trivia)
    {
        return SyntaxFactory.TriviaList(trivia.Add(SyntaxFactory.Space));
    }

    /// <summary>
    /// Determines whether the trivia contains a line break
    /// </summary>
    /// <param name="trivia">Trivia list</param>
    /// <returns><see langword="true"/> if a line break exists; otherwise, <see langword="false"/></returns>
    private static bool ContainsEndOfLine(SyntaxTriviaList trivia)
    {
        foreach (var currentTrivia in trivia)
        {
            if (currentTrivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    #endregion // Methods

    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0391AssignmentsMustHaveProperLineBreaksAnalyzer.DiagnosticId];

    /// <inheritdoc/>
    public sealed override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc/>
    public sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        foreach (var diagnostic in context.Diagnostics)
        {
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0391Title,
                                                      token => ApplyCodeFixAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0391AssignmentsMustHaveProperLineBreaksCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}