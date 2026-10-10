using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Layout;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Layout;

/// <summary>
/// Code fix provider for <see cref="RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider))]
public class RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider : BlankLineSpanRemovalCodeFixProviderBase
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH5028CodeMustNotContainBlankLinesAtStartOfFileCodeFixProvider()
        : base(RH5028CodeMustNotContainBlankLinesAtStartOfFileAnalyzer.DiagnosticId, CodeFixResources.RH5028Title)
    {
    }

    #endregion // Constructor
}