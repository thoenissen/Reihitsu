using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analysis context extensions: registrations that run only when documentation mode is not
/// <see cref="DocumentationMode.None"/>, and registrations on syntax kinds that occur inside structured trivia, such as
/// documentation comments, their XML elements, preprocessor directives, and the expressions of directive conditions, that
/// run once per node
/// </summary>
internal static class AnalysisContextExtensions
{
    #region Methods

    /// <summary>
    /// Registers a syntax tree action that runs only when documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    internal static void RegisterSyntaxTreeActionWithDocumentationModeCheck(this AnalysisContext context, Action<SyntaxTreeAnalysisContext> action)
    {
        context.RegisterSyntaxTreeAction(WrapWithDocumentationModeCheck(action));
    }

    /// <summary>
    /// Registers a syntax node action that runs only when documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Syntax kinds</param>
    internal static void RegisterSyntaxNodeActionWithDocumentationModeCheck(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
    {
        context.RegisterSyntaxNodeAction(WrapWithDocumentationModeCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Registers a syntax node action that runs only when documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Syntax kinds</param>
    internal static void RegisterSyntaxNodeActionWithDocumentationModeCheck(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, ImmutableArray<SyntaxKind> syntaxKinds)
    {
        context.RegisterSyntaxNodeAction(WrapWithDocumentationModeCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Registers a syntax node action on syntax kinds that occur inside structured trivia, such as preprocessor directives
    /// or the expressions of directive conditions, that runs once per node
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Syntax kinds</param>
    internal static void RegisterStructuredTriviaAction(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
    {
        context.RegisterSyntaxNodeAction(WrapWithRepeatedRunCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Registers a syntax node action on syntax kinds that occur inside structured trivia, such as documentation comments
    /// and their XML elements, that runs once per node, and only when documentation mode is not
    /// <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="context">Analysis context</param>
    /// <param name="action">Action</param>
    /// <param name="syntaxKinds">Syntax kinds</param>
    internal static void RegisterStructuredTriviaActionWithDocumentationModeCheck(this AnalysisContext context, Action<SyntaxNodeAnalysisContext> action, params SyntaxKind[] syntaxKinds)
    {
        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(WrapWithRepeatedRunCheck(action), syntaxKinds);
    }

    /// <summary>
    /// Determines whether a syntax node action runs a second time for the same node inside structured trivia. The compiler
    /// runs the actions for the structured trivia of a type declaration with a parameter list once for the type and once
    /// for its primary constructor. Two groups of trivia are seen by one run only: the trivia inside the parameter list and
    /// inside the argument list of the primary constructor base type by the run for the primary constructor, and the trivia
    /// in front of the base type's name and inside or in front of attribute lists with the <c>method</c> target by the run
    /// for the type. Only a run for the primary constructor is ever reported as repeated, so both groups keep their one run
    /// </summary>
    /// <param name="context">Syntax node analysis context</param>
    /// <returns><see langword="true"/> when the type's own run already saw the node</returns>
    private static bool IsRepeatedRun(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.IsPartOfStructuredTrivia() == false
            || context.ContainingSymbol is not IMethodSymbol { MethodKind: MethodKind.Constructor } constructor)
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
    /// Wraps an action so it does not run a second time for the same node inside structured trivia
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

    /// <summary>
    /// Wraps an action so it only runs when documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="action">Action</param>
    /// <returns>The wrapped action</returns>
    private static Action<SyntaxNodeAnalysisContext> WrapWithDocumentationModeCheck(Action<SyntaxNodeAnalysisContext> action)
    {
        return currentContext =>
               {
                   if (currentContext.Node.SyntaxTree.Options is CSharpParseOptions parseOptions
                       && parseOptions.DocumentationMode == DocumentationMode.None)
                   {
                       return;
                   }

                   action(currentContext);
               };
    }

    /// <summary>
    /// Wraps an action so it only runs when documentation mode is not <see cref="DocumentationMode.None"/>
    /// </summary>
    /// <param name="action">Action</param>
    /// <returns>The wrapped action</returns>
    private static Action<SyntaxTreeAnalysisContext> WrapWithDocumentationModeCheck(Action<SyntaxTreeAnalysisContext> action)
    {
        return currentContext =>
               {
                   if (currentContext.Tree.Options is CSharpParseOptions parseOptions
                       && parseOptions.DocumentationMode == DocumentationMode.None)
                   {
                       return;
                   }

                   action(currentContext);
               };
    }

    #endregion // Methods
}