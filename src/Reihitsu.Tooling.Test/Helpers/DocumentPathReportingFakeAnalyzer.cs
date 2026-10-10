using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Reihitsu.Tooling.Test.Helpers;

/// <summary>
/// An analyzer that reports the fake diagnostic when the fixture document's own path satisfies a
/// caller-provided predicate, mirroring how RH4001 decides its diagnostic from the document's file path rather
/// than from the source text
/// </summary>
internal sealed class DocumentPathReportingFakeAnalyzer : DiagnosticAnalyzer
{
    #region Fields

    /// <summary>
    /// Predicate deciding whether the fixture document's path is reported
    /// </summary>
    private readonly Func<string, bool> _shouldReport;

    #endregion // Fields

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentPathReportingFakeAnalyzer"/> class
    /// </summary>
    /// <param name="shouldReport">Predicate deciding whether the fixture document's path is reported</param>
    public DocumentPathReportingFakeAnalyzer(Func<string, bool> shouldReport)
    {
        _shouldReport = shouldReport;
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Reports the fake diagnostic at the start of the tree when its file path matches the predicate
    /// </summary>
    /// <param name="context">Syntax-tree analysis context</param>
    private void AnalyzeSyntaxTree(SyntaxTreeAnalysisContext context)
    {
        if (_shouldReport(context.Tree.FilePath))
        {
            context.ReportDiagnostic(Diagnostic.Create(SupportedDiagnostics[0],
                                                       Location.Create(context.Tree, new TextSpan(0, 0))));
        }
    }

    #endregion // Methods

    #region DiagnosticAnalyzer

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [new DiagnosticDescriptor(FakeDiagnostic.Id, "Title", "Message", "Testing", DiagnosticSeverity.Warning, isEnabledByDefault: true)];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeSyntaxTree);
    }

    #endregion // DiagnosticAnalyzer
}