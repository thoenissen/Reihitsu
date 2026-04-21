using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

using Reihitsu.Analyzer.Base;
using Reihitsu.Analyzer.Enumerations;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// RH0334: C# source files should be encoded as UTF-8 with BOM.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class RH0334SourceFilesShouldBeEncodedAsUtf8BomAnalyzer : DiagnosticAnalyzerBase<RH0334SourceFilesShouldBeEncodedAsUtf8BomAnalyzer>
{
    #region Constants

    /// <summary>
    /// Diagnostic ID
    /// </summary>
    public const string DiagnosticId = "RH0334";

    #endregion // Constants

    #region Fields

    /// <summary>
    /// UTF-8 encoding with BOM. The preamble (BOM) is used to identify the encoding of the file.
    /// </summary>
    private static readonly UTF8Encoding _utf8Encoding = new(encoderShouldEmitUTF8Identifier: true);

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0334SourceFilesShouldBeEncodedAsUtf8BomAnalyzer()
        : base(DiagnosticId, DiagnosticCategory.Formatting, nameof(AnalyzerResources.RH0334Title), nameof(AnalyzerResources.RH0334MessageFormat))
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Determines whether the encoding is UTF-8 with BOM.
    /// </summary>
    /// <param name="encoding">Encoding to check</param>
    /// <returns><c>true</c> if the encoding is UTF-8 with BOM; otherwise <c>false</c></returns>
    private static bool IsUtf8BomEncoding(Encoding encoding)
    {
        return encoding is not null
               && string.Equals(encoding.WebName, _utf8Encoding.WebName, StringComparison.OrdinalIgnoreCase)
               && encoding.GetPreamble().AsSpan().SequenceEqual(_utf8Encoding.GetPreamble());
    }

    /// <summary>
    /// Analyzes the syntax tree to ensure .cs files use UTF-8 with BOM.
    /// </summary>
    /// <param name="context">Context</param>
    private void OnSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        var filePath = context.Tree.FilePath;

        if (string.IsNullOrWhiteSpace(filePath)
            || filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) == false)
        {
            return;
        }

        var sourceText = context.Tree.GetText(context.CancellationToken);

        if (IsUtf8BomEncoding(sourceText.Encoding) == false)
        {
            var location = context.Tree.GetRoot(context.CancellationToken).GetFirstToken(includeZeroWidth: true).GetLocation();

            context.ReportDiagnostic(CreateDiagnostic(location));
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