using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider))]
public class RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider()
        : base(RH0373BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, CodeFixResources.RH0373Title)
    {
    }

    #endregion // Constructor
}