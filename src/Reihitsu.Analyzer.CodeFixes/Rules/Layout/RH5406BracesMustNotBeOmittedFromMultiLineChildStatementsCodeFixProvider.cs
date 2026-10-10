using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider))]
public class RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsCodeFixProvider()
        : base(RH5406BracesMustNotBeOmittedFromMultiLineChildStatementsAnalyzer.DiagnosticId, CodeFixResources.RH5406Title)
    {
    }

    #endregion // Constructor
}