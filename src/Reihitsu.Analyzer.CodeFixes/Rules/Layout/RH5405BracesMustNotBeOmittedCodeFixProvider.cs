using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5405BracesMustNotBeOmittedAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5405BracesMustNotBeOmittedCodeFixProvider))]
public class RH5405BracesMustNotBeOmittedCodeFixProvider : StatementBracesCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5405BracesMustNotBeOmittedCodeFixProvider()
        : base(RH5405BracesMustNotBeOmittedAnalyzer.DiagnosticId, CodeFixResources.RH5405Title)
    {
    }

    #endregion // Constructor
}