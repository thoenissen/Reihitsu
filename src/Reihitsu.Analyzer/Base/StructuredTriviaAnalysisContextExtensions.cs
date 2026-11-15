using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analysis context extensions for syntax node actions on structured trivia, such as documentation comments, their XML
/// elements, and preprocessor directives
/// </summary>
internal static class StructuredTriviaAnalysisContextExtensions
{
    #region Methods

    /// <summary>
    /// Registers a syntax node action on structured trivia kinds that runs once per trivia node
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Structured trivia kinds</param>
    internal static void RegisterStructuredTriviaAction(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
    {
        context.RegisterSyntaxNodeAction(WrapWithRepeatedRunCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Registers a syntax node action on structured trivia kinds that runs once per trivia node, and only when
    /// documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Structured trivia kinds</param>
    internal static void RegisterStructuredTriviaActionWithDocumentationModeCheck(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
    {
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(WrapWithRepeatedRunCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Determines whether a syntax node action runs a second time for the same trivia node. The compiler runs the actions
    /// for the structured trivia of a type declaration with a parameter list once for the type and once for its primary
    /// constructor, except for the trivia inside the parameter list and inside the argument list of the primary
    /// constructor base type, which only the run for the primary constructor sees
    /// </summary>
    /// <param name="context">Syntax node analysis context</param>
    /// <returns><see langword="true"/> when the type's own run already saw the node</returns>
    private static bool IsRepeatedRun(SyntaxNodeAnalysisContext context)
    {
        if (context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor)
        {
            return false;
        }

        foreach (var reference in constructor.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax(context.CancellationToken) is TypeDeclarationSyntax typeDeclaration
                && typeDeclaration.SyntaxTree == context.Node.SyntaxTree)
            {
                return IsInSpan(typeDeclaration.ParameterList, context.Node) == false
                       && IsInSpan(GetPrimaryConstructorBaseArgumentList(typeDeclaration), context.Node) == false;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the argument list of the primary constructor base type of a type declaration
    /// </summary>
    /// <param name="typeDeclaration">Type declaration</param>
    /// <returns>The argument list, or <see langword="null"/> when the base list holds no primary constructor base type</returns>
    private static ArgumentListSyntax GetPrimaryConstructorBaseArgumentList(TypeDeclarationSyntax typeDeclaration)
    {
        if (typeDeclaration.BaseList == null)
        {
            return null;
        }

        foreach (var baseType in typeDeclaration.BaseList.Types)
        {
            if (baseType is PrimaryConstructorBaseTypeSyntax primaryConstructorBaseType)
            {
                return primaryConstructorBaseType.ArgumentList;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines whether a node lies inside the full span of another node, its trivia included
    /// </summary>
    /// <param name="container">Containing node, or <see langword="null"/></param>
    /// <param name="node">Node</param>
    /// <returns><see langword="true"/> if the container holds the node</returns>
    private static bool IsInSpan(SyntaxNode container, SyntaxNode node)
    {
        return container != null
               && container.FullSpan.Contains(node.Span);
    }

    /// <summary>
    /// Wraps an action so it does not run a second time for the same trivia node
    /// </summary>
    /// <param name="action">Action</param>
    /// <returns>The wrapped action</returns>
    private static Action<SyntaxNodeAnalysisContext> WrapWithRepeatedRunCheck(Action<SyntaxNodeAnalysisContext> action)
    {
        return currentContext =>
               {
                   if (IsRepeatedRun(currentContext))
                   {
                       return;
                   }

                   action(currentContext);
               };
    }

    #endregion // Methods
}