using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4107ProtectedFieldCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4107ProtectedFieldCasingCodeFixProvider))]
public class RH4107ProtectedFieldCasingCodeFixProvider : CasingCodeFixProviderBase<VariableDeclaratorSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4107ProtectedFieldCasingCodeFixProvider()
        : base(RH4107ProtectedFieldCasingAnalyzer.DiagnosticId, CodeFixResources.RH4107Title, CasingUtilities.ToUnderLineCamelCase)
    {
    }

    #endregion // Constructor

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(VariableDeclaratorSyntax node)
    {
        return node.Identifier.ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}