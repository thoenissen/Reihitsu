using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.Base;

namespace Reihitsu.Analyzer.Rules.Formatting;

/// <summary>
/// Code fix provider for <see cref="RH0362BracesMustNotBeOmittedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH0362BracesMustNotBeOmittedCodeFixProvider))]
public class RH0362BracesMustNotBeOmittedCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH0362BracesMustNotBeOmittedCodeFixProvider()
        : base(RH0362BracesMustNotBeOmittedAnalyzer.DiagnosticId, CodeFixResources.RH0362Title)
    {
    }

    #endregion // Constructor
}