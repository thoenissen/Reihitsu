using System.Composition;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Reihitsu.Analyzer.CodeFixes.Base;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Core;

namespace Reihitsu.Analyzer.CodeFixes.Rules.Naming;

/// <summary>
/// Providing fixes for <see cref="RH4005InterfaceNameCasingAnalyzer"/>
/// </summary>
[Shared]
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RH4005InterfaceNameCasingCodeFixProvider))]
public class RH4005InterfaceNameCasingCodeFixProvider : CasingCodeFixProviderBase<InterfaceDeclarationSyntax>
{
    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public RH4005InterfaceNameCasingCodeFixProvider()
        : base(new RH4005InterfaceNameCasingAnalyzer(), CodeFixResources.RH4005Title, OnTransformIdentifier)
    {
    }

    #endregion // Constructor

    #region Methods

    /// <summary>
    /// Transform identifier for to IPascalCase
    /// </summary>
    /// <param name="identifier">Identifier</param>
    /// <returns>Transformed identifier; the identifier itself when nothing is left behind its prefix</returns>
    private static string OnTransformIdentifier(string identifier)
    {
        // Only strip the leading character when it is a genuine "I" prefix, that is an 'i'/'I' that is the whole name or
        // is followed by an uppercase letter, a digit, or '_' (for example "iDocumentReader", "i2Factory", or
        // "i_document"); the underscores behind the prefix are dropped with it. Names that merely start with the letter i
        // (for example "index" or "important") keep the whole word so the prefix is prepended (for example "IIndex").
        var rest = identifier;

        if (identifier.Length > 0
            && (identifier[0] == 'i' || identifier[0] == 'I')
            && (identifier.Length == 1
                || char.IsUpper(identifier[1])
                || char.IsDigit(identifier[1])
                || identifier[1] == '_'))
        {
            rest = identifier.Substring(1).TrimStart('_');

            // Nothing is left behind the prefix, so no name can be derived; returning the old name withholds the fix
            if (rest.Length == 0)
            {
                return identifier;
            }
        }

        return $"I{CasingUtilities.ToPascalCase(rest)}";
    }

    #endregion // Methods

    #region CasingCodeFixProviderBase

    /// <inheritdoc/>
    protected override string GetIdentifier(InterfaceDeclarationSyntax node)
    {
        return node.Identifier.ValueText;
    }

    #endregion // CasingCodeFixProviderBase
}