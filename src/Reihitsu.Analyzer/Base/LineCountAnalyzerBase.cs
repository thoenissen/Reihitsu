using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Reihitsu.Analyzer.Data;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Base;

/// <summary>
/// Base class of the rules that count the lines of code of a declaration's body and report the declaration when the
/// count exceeds its limit. It owns what a line of code is, which code is test code, which limit applies, and how the
/// count is compared with it; a derived rule states which declarations it judges and which span of them it counts
/// </summary>
public abstract class LineCountAnalyzerBase : DiagnosticAnalyzerBase
{
    #region Constants

    /// <summary>
    /// Global analyzer option of the MSBuild property <c>IsTestProject</c>
    /// </summary>
    private const string IsTestProjectOptionName = "build_property.IsTestProject";

    /// <summary>
    /// Global analyzer option of the MSBuild property <c>IsTestingPlatformApplication</c>
    /// </summary>
    private const string IsTestingPlatformApplicationOptionName = "build_property.IsTestingPlatformApplication";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// Syntax kinds of the judged declarations
    /// </summary>
    private readonly ImmutableArray<SyntaxKind> _syntaxKinds;

    /// <summary>
    /// Reads the production setting from the design section
    /// </summary>
    private readonly Func<ConfigurationCategoryDesign, int?> _productionSetting;

    /// <summary>
    /// Reads the test setting from the design section
    /// </summary>
    private readonly Func<ConfigurationCategoryDesign, int?> _testSetting;

    /// <summary>
    /// Default limit of production code
    /// </summary>
    private readonly int _defaultProductionLimit;

    /// <summary>
    /// Default limit of test code
    /// </summary>
    private readonly int _defaultTestLimit;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="diagnosticId">Diagnostic ID</param>
    /// <param name="titleResourceName">Resource name of the title</param>
    /// <param name="messageFormatResourceName">Resource name of the message format, whose arguments are the count and the limit</param>
    /// <param name="productionSetting">Reads the production setting from the design section</param>
    /// <param name="defaultProductionLimit">Default limit of production code</param>
    /// <param name="testSetting">Reads the test setting from the design section</param>
    /// <param name="defaultTestLimit">Default limit of test code</param>
    /// <param name="syntaxKinds">Syntax kinds of the judged declarations</param>
    private protected LineCountAnalyzerBase(string diagnosticId,
                                            string titleResourceName,
                                            string messageFormatResourceName,
                                            Func<ConfigurationCategoryDesign, int?> productionSetting,
                                            int defaultProductionLimit,
                                            Func<ConfigurationCategoryDesign, int?> testSetting,
                                            int defaultTestLimit,
                                            params SyntaxKind[] syntaxKinds)
        : base(diagnosticId, DiagnosticCategory.Design, titleResourceName, messageFormatResourceName)
    {
        _productionSetting = productionSetting;
        _defaultProductionLimit = defaultProductionLimit;
        _testSetting = testSetting;
        _defaultTestLimit = defaultTestLimit;
        _syntaxKinds = [.. syntaxKinds];
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Gets the counted span of a judged declaration and the location of its diagnostic
    /// </summary>
    /// <param name="node">Declaration of one of the registered syntax kinds</param>
    /// <param name="span">Counted span</param>
    /// <param name="location">Location of the diagnostic</param>
    /// <returns><see langword="true"/> when the declaration is judged; otherwise <see langword="false"/></returns>
    private protected abstract bool TryGetCountedSpan(SyntaxNode node, out TextSpan span, out Location location);

    /// <summary>
    /// Gets the span between the braces of a block body, the braces excluded
    /// </summary>
    /// <param name="openBraceToken">Opening brace</param>
    /// <param name="closeBraceToken">Closing brace</param>
    /// <returns>Span between the braces</returns>
    private protected static TextSpan GetSpanBetweenBraces(SyntaxToken openBraceToken, SyntaxToken closeBraceToken)
    {
        return TextSpan.FromBounds(openBraceToken.Span.End, Math.Max(openBraceToken.Span.End, closeBraceToken.SpanStart));
    }

    /// <summary>
    /// Gets the counted span of a body: the tokens between the braces of a block body, or the <c>=&gt;</c> and the
    /// expression of an expression body
    /// </summary>
    /// <param name="body">Block body, or <see langword="null"/></param>
    /// <param name="expressionBody">Expression body, or <see langword="null"/></param>
    /// <param name="span">Counted span</param>
    /// <returns><see langword="true"/> when there is a body; otherwise <see langword="false"/></returns>
    private protected static bool TryGetBodySpan(BlockSyntax body, ArrowExpressionClauseSyntax expressionBody, out TextSpan span)
    {
        if (body != null)
        {
            span = GetSpanBetweenBraces(body.OpenBraceToken, body.CloseBraceToken);

            return true;
        }

        if (expressionBody != null)
        {
            span = expressionBody.Span;

            return true;
        }

        span = default;

        return false;
    }

    /// <summary>
    /// Counts the lines of code of a span: the lines of the text on which at least one character of a token of the
    /// span lies. Each line is counted once, a token that runs over several lines makes each of them a line of code,
    /// and trivia — whitespace, comments, documentation comments, directives, and disabled text — makes no line one
    /// </summary>
    /// <param name="node">Node that holds the span</param>
    /// <param name="span">Counted span</param>
    /// <param name="text">Text of the node's syntax tree</param>
    /// <returns>Lines of code</returns>
    internal static int CountLinesOfCode(SyntaxNode node, TextSpan span, SourceText text)
    {
        var count = 0;
        var lastCountedLine = -1;

        foreach (var token in node.DescendantTokens(span))
        {
            if (token.Span.IsEmpty
                || span.Contains(token.Span) == false)
            {
                continue;
            }

            var firstLine = Math.Max(text.Lines.GetLineFromPosition(token.SpanStart).LineNumber, lastCountedLine + 1);
            var lastLine = text.Lines.GetLineFromPosition(token.Span.End - 1).LineNumber;

            if (lastLine >= firstLine)
            {
                count += lastLine - firstLine + 1;
                lastCountedLine = lastLine;
            }
        }

        return count;
    }

    /// <summary>
    /// Determines whether the compilation is test code: at least one of the MSBuild properties <c>IsTestProject</c>
    /// and <c>IsTestingPlatformApplication</c> is <c>true</c>, compared case-insensitively
    /// </summary>
    /// <param name="globalOptions">Global analyzer options</param>
    /// <returns><see langword="true"/> for test code; otherwise <see langword="false"/></returns>
    internal static bool IsTestCode(AnalyzerConfigOptions globalOptions)
    {
        return IsTrue(globalOptions, IsTestProjectOptionName)
               || IsTrue(globalOptions, IsTestingPlatformApplicationOptionName);
    }

    /// <summary>
    /// Determines whether a global analyzer option has the value <c>true</c>, compared case-insensitively
    /// </summary>
    /// <param name="globalOptions">Global analyzer options</param>
    /// <param name="optionName">Option name</param>
    /// <returns><see langword="true"/> when the option is <c>true</c>; otherwise <see langword="false"/></returns>
    private static bool IsTrue(AnalyzerConfigOptions globalOptions, string optionName)
    {
        return globalOptions.TryGetValue(optionName, out var value)
               && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves the limit of the compilation: the setting for its kind of code when the setting has a valid value,
    /// and that setting's default otherwise
    /// </summary>
    /// <param name="design">Design section, or <see langword="null"/> when there is none</param>
    /// <param name="isTestCode">Whether the compilation is test code</param>
    /// <returns>Limit</returns>
    private int ResolveLimit(ConfigurationCategoryDesign design, bool isTestCode)
    {
        if (isTestCode)
        {
            return (design == null ? null : _testSetting(design)) ?? _defaultTestLimit;
        }

        return (design == null ? null : _productionSetting(design)) ?? _defaultProductionLimit;
    }

    /// <summary>
    /// Start of the compilation
    /// </summary>
    /// <param name="context">Context</param>
    private void OnCompilationStart(CompilationStartAnalysisContext context)
    {
        var design = ConfigurationManager.GetConfiguration(context.Options.AdditionalFiles).Configuration?.Design;
        var isTestCode = IsTestCode(context.Options.AnalyzerConfigOptionsProvider.GlobalOptions);
        var limit = ResolveLimit(design, isTestCode);

        context.RegisterSyntaxNodeAction(analysisContext => OnDeclaration(analysisContext, limit), _syntaxKinds);
    }

    /// <summary>
    /// Analyzing a judged declaration
    /// </summary>
    /// <param name="context">Context</param>
    /// <param name="limit">Limit that applies</param>
    private void OnDeclaration(SyntaxNodeAnalysisContext context, int limit)
    {
        if (TryGetCountedSpan(context.Node, out var span, out var location) == false)
        {
            return;
        }

        var count = CountLinesOfCode(context.Node, span, context.Node.SyntaxTree.GetText(context.CancellationToken));

        if (count > limit)
        {
            context.ReportDiagnostic(CreateDiagnostic(location, count, limit));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        base.Initialize(context);

        context.RegisterCompilationStartAction(OnCompilationStart);
    }

    #endregion // DiagnosticAnalyzer
}