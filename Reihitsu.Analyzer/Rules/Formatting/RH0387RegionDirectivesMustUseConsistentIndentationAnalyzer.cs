using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Region directives must use consistent indentation
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer : DiagnosticAnalyzerBase<RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0387";

    /// <summary>
    /// Indent size
    /// </summary>
    private const int IndentSize = 4;

    #endregion // Constants

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0387RegionDirectivesMustUseConsistentIndentationAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0387Title), nameof(AnalyzerResources.RH0387MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the expected indentation for a region directive
    /// </summary>
    /// <param name="directiveTrivia">Directive trivia</param>
    /// <param name="sourceText">Source text</param>
    /// <returns>Expected indentation</returns>
    private static int GetExpectedIndentation(SyntaxTrivia directiveTrivia, SourceText sourceText)
    {
        for (var currentNode = directiveTrivia.Token.Parent; currentNode != null; currentNode = currentNode.Parent)
        {
            if (currentNode.Span.Contains(directiveTrivia.SpanStart) == false)
            {
                continue;
            }

            switch (currentNode)
            {
                case TypeDeclarationSyntax typeDeclaration when typeDeclaration.OpenBraceToken.RawKind != 0:
                    return GetLineIndentation(sourceText, typeDeclaration.OpenBraceToken.SpanStart) + IndentSize;

                case NamespaceDeclarationSyntax namespaceDeclaration:
                    return GetLineIndentation(sourceText, namespaceDeclaration.OpenBraceToken.SpanStart) + IndentSize;

                case FileScopedNamespaceDeclarationSyntax fileScopedNamespaceDeclaration:
                    return GetLineIndentation(sourceText, fileScopedNamespaceDeclaration.NamespaceKeyword.SpanStart);

                case CompilationUnitSyntax:
                    return 0;
            }
        }

        return 0;
    }

    /// <summary>
    /// Gets the indentation of the line containing the specified position
    /// </summary>
    /// <param name="sourceText">Source text</param>
    /// <param name="position">Position</param>
    /// <returns>Indentation width</returns>
    private static int GetLineIndentation(SourceText sourceText, int position)
    {
        var line = sourceText.Lines.GetLineFromPosition(position);
        var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
        var indentation = 0;

        while (indentation < lineText.Length
               && lineText[indentation] == ' ')
        {
            indentation++;
        }

        return indentation;
    }

    /// <summary>
    /// Analyzes the syntax tree
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var syntaxRoot = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var directiveTrivia in syntaxRoot.DescendantTrivia(descendIntoTrivia: true))
        {
            if ((directiveTrivia.IsKind(SyntaxKind.RegionDirectiveTrivia) == false
                 && directiveTrivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) == false)
                || RegionDirectiveUtilities.IsWithinElementBody(directiveTrivia))
            {
                continue;
            }

            var line = sourceText.Lines.GetLineFromPosition(directiveTrivia.SpanStart);
            var lineText = FormattingTextAnalysisUtilities.GetLineText(sourceText, line);
            var trimmedLineText = lineText.TrimStart();
            var actualIndentation = lineText.Length - trimmedLineText.Length;
            var expectedIndentation = GetExpectedIndentation(directiveTrivia, sourceText);

            if (actualIndentation != expectedIndentation)
            {
                context.ReportDiagnostic(CreateDiagnostic(directiveTrivia.GetLocation()));
            }
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxTreeAction(OnSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}