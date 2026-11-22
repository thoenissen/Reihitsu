using System;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Core;

namespace Reihitsu.Analyzer.Test.Core;

/// <summary>
/// Contains unit tests for <see cref="SpeculativeRebindingHelper.AreEquivalent(SymbolInfo, SymbolInfo)"/>
/// </summary>
[TestClass]
public class SpeculativeRebindingHelperTests
{
    #region Tests

    /// <summary>
    /// Verifies that two symbol infos that neither bound a symbol nor a candidate are never equivalent
    /// </summary>
    [TestMethod]
    public void AreEquivalentReturnsFalseWhenNeitherSymbolInfoBinds()
    {
        Assert.IsFalse(SpeculativeRebindingHelper.AreEquivalent(default, default));
    }

    /// <summary>
    /// Verifies that a bound symbol info is never equivalent to one that failed to bind at all, regardless of side
    /// </summary>
    [TestMethod]
    public void AreEquivalentReturnsFalseWhenOnlyOneSymbolInfoBinds()
    {
        var boundSymbolInfo = GetInvocationSymbolInfo("Unambiguous()");

        Assert.IsFalse(SpeculativeRebindingHelper.AreEquivalent(default, boundSymbolInfo));
        Assert.IsFalse(SpeculativeRebindingHelper.AreEquivalent(boundSymbolInfo, default));
    }

    /// <summary>
    /// Verifies that two symbol infos that bind the same symbol are equivalent
    /// </summary>
    [TestMethod]
    public void AreEquivalentReturnsTrueWhenBothBindTheSameSymbol()
    {
        var boundSymbolInfo = GetInvocationSymbolInfo("Unambiguous()");

        Assert.IsTrue(SpeculativeRebindingHelper.AreEquivalent(boundSymbolInfo, boundSymbolInfo));
    }

    /// <summary>
    /// Verifies that two symbol infos that failed to resolve a single overload, but carry the same candidate
    /// symbols in the same order, are equivalent
    /// </summary>
    [TestMethod]
    public void AreEquivalentReturnsTrueWhenBothHaveTheSameCandidateSymbolsInOrder()
    {
        var ambiguousSymbolInfo = GetInvocationSymbolInfo("Ambiguous(1, 1)");

        Assert.IsNotEmpty(ambiguousSymbolInfo.CandidateSymbols);
        Assert.IsTrue(SpeculativeRebindingHelper.AreEquivalent(ambiguousSymbolInfo, ambiguousSymbolInfo));
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Compiles a fixed source containing an unambiguous and an ambiguous overload call, and returns the
    /// <see cref="SymbolInfo"/> of the invocation expression matching the given text
    /// </summary>
    /// <param name="invocationText">The exact source text of the invocation expression to resolve</param>
    /// <returns>The resolved symbol info</returns>
    private static SymbolInfo GetInvocationSymbolInfo(string invocationText)
    {
        const string source = """
                              class C
                              {
                                  void Unambiguous()
                                  {
                                  }

                                  void Ambiguous(int i, object o)
                                  {
                                  }

                                  void Ambiguous(object o, int i)
                                  {
                                  }

                                  void CallUnambiguous()
                                  {
                                      Unambiguous();
                                  }

                                  void CallAmbiguous()
                                  {
                                      Ambiguous(1, 1);
                                  }
                              }
                              """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest));
        var compilation = CSharpCompilation.Create("TestAssembly")
                                           .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                                           .AddReferences(GetMetadataReferences())
                                           .AddSyntaxTrees(syntaxTree);
        var semanticModel = compilation.GetSemanticModel(syntaxTree);
        var invocation = syntaxTree.GetRoot()
                                   .DescendantNodes()
                                   .OfType<InvocationExpressionSyntax>()
                                   .First(candidate => candidate.ToString() == invocationText);

        return semanticModel.GetSymbolInfo(invocation);
    }

    /// <summary>
    /// Gets the metadata references required to compile the fixed source
    /// </summary>
    /// <returns>Metadata references</returns>
    private static MetadataReference[] GetMetadataReferences()
    {
        var trustedPlatformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        var referencePaths = trustedPlatformAssemblies?.Split(Path.PathSeparator)
                                 ?? [];

        return [.. referencePaths.Select(path => MetadataReference.CreateFromFile(path))];
    }

    #endregion // Methods
}