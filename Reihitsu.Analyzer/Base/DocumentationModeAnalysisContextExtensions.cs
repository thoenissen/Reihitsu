using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Analysis context extensions related to documentation mode
/// </summary>
internal static class DocumentationModeAnalysisContextExtensions
{
    #region Methods

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

    #endregion // Methods
}