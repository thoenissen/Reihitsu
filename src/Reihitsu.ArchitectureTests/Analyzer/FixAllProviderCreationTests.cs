using System;
using System.Collections.Generic;
using System.Linq;

using ArchUnitNET.Domain.Dependencies;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;

using RequirementAttribute = Reihitsu.Analyzer.Test.Base.RequirementAttribute;

namespace Reihitsu.ArchitectureTests.Analyzer;

/// <summary>
/// The code-fix provider base owns whether a document lets a fix be offered, and every Fix All path asks it: a Fix All that
/// applies its fixes without registering them is created through the base's <c>CreateFixAllProvider</c>, and the document-order
/// Fix All re-registers each fix through the base. No other code-fix type creates a Fix All provider from a delegate, which
/// would skip the base's decision
/// </summary>
[TestClass]
public class FixAllProviderCreationTests
{
    #region Constants

    /// <summary>
    /// Full name of the Roslyn Fix All provider type
    /// </summary>
    private const string FixAllProviderTypeName = "Microsoft.CodeAnalysis.CodeFixes.FixAllProvider";

    /// <summary>
    /// Full name of the code-fix provider base
    /// </summary>
    private const string ProviderBaseTypeName = "Reihitsu.Analyzer.CodeFixes.Base.ReihitsuCodeFixProviderBase";

    /// <summary>
    /// Full name of the document-order Fix All provider
    /// </summary>
    private const string DocumentOrderFixAllProviderTypeName = "Reihitsu.Analyzer.CodeFixes.Core.DocumentOrderFixAllProvider";

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifies that <c>FixAllProvider.Create</c> is called only by the code-fix provider base and the document-order Fix All
    /// provider, so that every Fix All path asks the base whether a fix may be offered in a document; a call inside a lambda or
    /// an async method counts for the type that declares it
    /// </summary>
    [TestMethod]
    [Requirement("SHARED-FIX-011")]
    [Requirement("SHARED-FIX-012")]
    public void FixAllProviderCreateIsCalledOnlyByTheProviderBaseAndTheDocumentOrderFixAllProvider()
    {
        var callingTypes = GetTypesCallingFixAllProviderCreate();

        CollectionAssert.AreEquivalent(new[] { DocumentOrderFixAllProviderTypeName, ProviderBaseTypeName },
                                       callingTypes,
                                       $"Only the code-fix provider base and the document-order Fix All provider may call FixAllProvider.Create; a provider creates its Fix All through CreateFixAllProvider. Calling types: {string.Join(", ", callingTypes)}");
    }

    #endregion // Tests

    #region Methods

    /// <summary>
    /// Gets the outermost declaring type of every method of the code-fix assembly that calls <c>FixAllProvider.Create</c>, so
    /// that a call in a compiler-generated closure or state machine counts for the type whose source holds it
    /// </summary>
    /// <returns>The full names of the calling types, ordered and without duplicates</returns>
    private static List<string> GetTypesCallingFixAllProviderCreate()
    {
        return Assemblies.Implementation
                         .MethodMembers
                         .Where(method => method.DeclaringType.Assembly.Name.StartsWith("Reihitsu.Analyzer.CodeFixes,", StringComparison.Ordinal)
                                          || method.DeclaringType.Assembly.Name == "Reihitsu.Analyzer.CodeFixes")
                         .Where(method => method.MemberDependencies
                                                .OfType<MethodCallDependency>()
                                                .Any(dependency => dependency.TargetMember.DeclaringType.FullName == FixAllProviderTypeName
                                                                   && dependency.TargetMember.Name.StartsWith("Create(", StringComparison.Ordinal)))
                         .Select(method => GetOutermostTypeName(method.DeclaringType.FullName))
                         .Distinct()
                         .OrderBy(typeName => typeName, StringComparer.Ordinal)
                         .ToList();
    }

    /// <summary>
    /// Gets the full name of the outermost type of a possibly nested type
    /// </summary>
    /// <param name="typeName">Full name of the type</param>
    /// <returns>The full name of the outermost type</returns>
    private static string GetOutermostTypeName(string typeName)
    {
        var nestingIndex = typeName.IndexOfAny(['+', '/']);

        return nestingIndex < 0
                   ? typeName
                   : typeName.Substring(0, nestingIndex);
    }

    #endregion // Methods
}