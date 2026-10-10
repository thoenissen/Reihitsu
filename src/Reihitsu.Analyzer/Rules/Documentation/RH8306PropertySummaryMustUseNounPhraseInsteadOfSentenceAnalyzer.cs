using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Core;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Documentation;

/// <summary>
/// RH8306: Property summary must use a noun phrase instead of a sentence
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH8306";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Verbs that introduce a behavior-narrating sentence and therefore must not start a property summary
    /// </summary>
    private static readonly HashSet<string> _sentenceLeadingVerbs = new(StringComparer.OrdinalIgnoreCase)
                                                                    {
                                                                        "Gets",
                                                                        "Sets",
                                                                        "Returns",
                                                                        "Represents",
                                                                        "Indicates",
                                                                        "Specifies",
                                                                        "Determines",
                                                                        "Holds",
                                                                        "Stores"
                                                                    };

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH8306PropertySummaryMustUseNounPhraseInsteadOfSentenceAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Documentation, nameof(AnalyzerResources.RH8306Title), nameof(AnalyzerResources.RH8306MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the visible text content of the XML element
    /// </summary>
    /// <param name="element">XML element</param>
    /// <returns>The concatenated text content with inline elements collapsed to a single separator</returns>
    private static string GetTextContent(XmlElementSyntax element)
    {
        var builder = new StringBuilder();

        foreach (var node in element.Content)
        {
            if (node is XmlTextSyntax textSyntax)
            {
                foreach (var token in textSyntax.TextTokens)
                {
                    builder.Append(token.Text);
                }
            }
            else
            {
                builder.Append(' ');
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Determines whether the specified character terminates a sentence
    /// </summary>
    /// <param name="value">Character</param>
    /// <returns><see langword="true"/> if the character is a sentence terminator</returns>
    private static bool IsSentenceTerminator(char value)
    {
        return value is '.' or '!' or '?';
    }

    /// <summary>
    /// Determines whether the specified type is Boolean or nullable Boolean
    /// </summary>
    /// <param name="type">Type</param>
    /// <returns><see langword="true"/> if the type is Boolean or nullable Boolean</returns>
    private static bool IsBooleanType(ITypeSymbol type)
    {
        if (type?.SpecialType == SpecialType.System_Boolean)
        {
            return true;
        }

        return type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType
               && namedType.TypeArguments.Length == 1
               && namedType.TypeArguments[0].SpecialType == SpecialType.System_Boolean;
    }

    /// <summary>
    /// Gets the first whitespace separated word of the specified text
    /// </summary>
    /// <param name="text">Text</param>
    /// <returns>The first word, or an empty string when the text contains no word</returns>
    private static string GetFirstWord(string text)
    {
        var start = 0;

        while (start < text.Length
               && char.IsWhiteSpace(text[start]))
        {
            start++;
        }

        var end = start;

        while (end < text.Length
               && char.IsWhiteSpace(text[end]) == false)
        {
            end++;
        }

        return text.Substring(start, end - start);
    }

    /// <summary>
    /// Determines whether the summary text reads as a sentence rather than a noun phrase
    /// </summary>
    /// <param name="text">Summary text</param>
    /// <param name="isBooleanProperty"><see langword="true"/> when the summary belongs to a Boolean or nullable Boolean property</param>
    /// <returns><see langword="true"/> if the text is written as a sentence</returns>
    private static bool IsSentence(string text, bool isBooleanProperty)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return false;
        }

        var finalCharacter = trimmed[trimmed.Length - 1];

        if (IsSentenceTerminator(finalCharacter)
            && (finalCharacter != '?'
                || isBooleanProperty == false))
        {
            return true;
        }

        return _sentenceLeadingVerbs.Contains(GetFirstWord(trimmed));
    }

    /// <summary>
    /// Analyzes a property declaration
    /// </summary>
    /// <param name="context">Context</param>
    private void OnPropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        var documentationComment = DirectDocumentationSyntaxChecker.GetDocumentationComment(propertyDeclaration);

        if (documentationComment == null
            || DirectDocumentationSyntaxChecker.GetFirstDirectTag(documentationComment, "summary") is not XmlElementSyntax summaryElement)
        {
            return;
        }

        var propertyType = context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken)?.Type;

        if (IsSentence(GetTextContent(summaryElement), IsBooleanType(propertyType)) == false)
        {
            return;
        }

        context.ReportDiagnostic(CreateDiagnostic(summaryElement.GetLocation()));
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterSyntaxNodeActionWithDocumentationModeCheck(OnPropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    #endregion // DiagnosticAnalyzer
}