using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider))]
public class RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider : CodeFixProvider
{
    #region CodeFixProvider

    /// <inheritdoc/>
    public sealed override ImmutableArray<string> FixableDiagnosticIds => [RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasAnalyzer.DiagnosticId];

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
            context.RegisterCodeFix(CodeAction.Create(CodeFixResources.RH0813Title,
                                                      token => TrailingCommaCodeFixHelper.RemoveTrailingCommaAsync(context.Document, diagnostic.Location.SourceSpan, token),
                                                      nameof(RH0813FinalArrayInitializerItemsMustNotHaveTrailingCommasCodeFixProvider)),
                                    diagnostic);
        }

        return Task.CompletedTask;
    }

    #endregion // CodeFixProvider
}