using System.Linq;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Core.Enumerations;

namespace Reihitsu.Core.Test;

/// <summary>
/// Contains unit tests for <see cref="UsingDirectiveOrderingUtilities"/>
/// </summary>
[TestClass]
public class UsingDirectiveOrderingUtilitiesTests
{
    #region Properties

    /// <summary>
    /// Gets or sets the test context for the current test
    /// </summary>
    public TestContext TestContext { get; set; }

    #endregion // Properties

    #region Tests

    /// <summary>
    /// Verifies that using directives are classified into the expected groups
    /// </summary>
    [TestMethod]
    public void ClassificationHelpersReturnExpectedUsingGroups()
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        global using Example.Tools;
                                                                        using System.Collections.Generic;
                                                                        using static System.Math;
                                                                        using Alias = Example.Utilities;
                                                                        using Example.Features;
                                                                        """);
        var usingDirectives = compilationUnit.Usings;

        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsGlobalUsing(usingDirectives[0]));
        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirectives[1]));
        Assert.AreEqual(UsingDirectiveOrderingGroup.Static, UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirectives[2]));
        Assert.AreEqual(UsingDirectiveOrderingGroup.Alias, UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirectives[3]));
        Assert.AreEqual(UsingDirectiveOrderingGroup.OtherNamespace, UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirectives[4]));
    }

    /// <summary>
    /// Verifies that sort helpers use alias names and compare keys case-insensitively
    /// </summary>
    [TestMethod]
    public void SortHelpersReturnExpectedSortInformation()
    {
        var aliasUsingDirective = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                            using Alias = Example.Utilities;
                                                                            """)
                                                      .Usings
                                                      .Single();

        Assert.AreEqual("Alias", UsingDirectiveOrderingUtilities.GetSortKey(aliasUsingDirective));
        Assert.AreEqual(0, UsingDirectiveOrderingUtilities.CompareSortKeys("System", "system"));
    }

    /// <summary>
    /// Verifies that diagnostic lookup resolves both the preferred location and the containing scope
    /// </summary>
    [TestMethod]
    public void DiagnosticHelpersResolveAliasLocationAndContainingScope()
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using Alias = Example.Utilities;
                                                                        """);
        var usingDirective = compilationUnit.Usings.Single();
        var diagnostic = CoreSyntaxTestHelper.CreateDiagnostic(usingDirective.Alias.Name.Identifier.GetLocation());

        var hasScope = UsingDirectiveOrderingUtilities.TryGetUsingDirectiveScope(compilationUnit, diagnostic, out var scope);
        var diagnosticLocation = UsingDirectiveOrderingUtilities.GetDiagnosticLocation(usingDirective);
        var diagnosticLocationText = compilationUnit.SyntaxTree.GetText(TestContext.CancellationToken).ToString(diagnosticLocation.SourceSpan);

        Assert.IsTrue(hasScope);
        Assert.AreSame(compilationUnit, scope);
        Assert.AreEqual("Alias", diagnosticLocationText);
    }

    /// <summary>
    /// Verifies that using directives can be reordered consistently for compilation units and namespaces
    /// </summary>
    [TestMethod]
    public void UsingHelpersReorderCompilationUnitAndNamespaceUsings()
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using Zeta;
                                                                        global using B;
                                                                        using System;
                                                                        global using A;
                                                                        using static System.Math;
                                                                        using Alias = Example.Tools;

                                                                        namespace Sample;
                                                                        """);
        var orderedCompilationUnitUsings = UsingDirectiveOrderingUtilities.OrderUsings(UsingDirectiveOrderingUtilities.GetUsings(compilationUnit));
        var namespaceDeclaration = CoreSyntaxTestHelper.GetSingleNode<FileScopedNamespaceDeclarationSyntax>("""
                                                                                                            namespace Sample;

                                                                                                            using Zeta;
                                                                                                            using System;
                                                                                                            """);
        var orderedNamespaceUsings = UsingDirectiveOrderingUtilities.OrderUsings(UsingDirectiveOrderingUtilities.GetUsings(namespaceDeclaration));
        var updatedNamespaceDeclaration = (FileScopedNamespaceDeclarationSyntax)UsingDirectiveOrderingUtilities.WithUsings(namespaceDeclaration, orderedNamespaceUsings);

        CollectionAssert.AreEqual(new[] { "using System;", "global using A;", "using Zeta;", "global using B;", "using static System.Math;", "using Alias = Example.Tools;" },
                                  orderedCompilationUnitUsings.Select(obj => obj.ToString()).ToArray());
        CollectionAssert.AreEqual(new[] { "using System;", "using Zeta;" },
                                  updatedNamespaceDeclaration.Usings.Select(obj => obj.ToString()).ToArray());
    }

    #endregion // Tests
}