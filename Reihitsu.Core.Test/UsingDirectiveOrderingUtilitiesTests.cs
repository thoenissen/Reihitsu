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
    #region Fields

    /// <summary>
    /// Expected canonical order for aliases ordered by their target root namespace
    /// </summary>
    private static readonly string[] _aliasCanonicalOrder = ["using Zebra = Alpha.Thing;", "using Apple = Beta.Thing;"];

    /// <summary>
    /// Expected order for aliases ordered by their target root namespace
    /// </summary>
    private static readonly string[] _aliasTargetRootOrder = ["using Z = A.B;", "using A = Z.Q;"];

    /// <summary>
    /// Expected reordered compilation-unit usings
    /// </summary>
    private static readonly string[] _compilationUnitReorderedOrder = ["global using A;", "global using B;", "using System;", "using Zeta;", "using static System.Math;", "using Alias = Example.Tools;"];

    /// <summary>
    /// Expected reordered namespace usings
    /// </summary>
    private static readonly string[] _namespaceReorderedOrder = ["using System;", "using Zeta;"];

    /// <summary>
    /// Expected order when a comment stays attached to its directive
    /// </summary>
    private static readonly string[] _commentDirectiveOrder = ["using Alpha;", "using Beta;", "using Charlie;"];

    #endregion // Fields

    #region Properties

    /// <summary>
    /// Test context for the current test
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
    /// Verifies that a <c>global::</c>-qualified System import is classified into the System namespace group
    /// </summary>
    [TestMethod]
    public void GlobalQualifiedSystemUsingIsClassifiedAsSystemNamespace()
    {
        var usingDirective = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                       using global::System.Text;
                                                                       """)
                                                 .Usings
                                                 .Single();

        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirective));
        Assert.AreEqual(UsingDirectiveOrderingGroup.SystemNamespace, UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective));
        Assert.AreEqual("System", UsingDirectiveOrderingUtilities.GetRootNamespace(usingDirective));
    }

    /// <summary>
    /// Verifies that only the exact System root receives priority and shares a group with its child namespaces
    /// </summary>
    [TestMethod]
    public void ExactSystemRootOrdersFirstAndGroupsOnlyWithItsChildren()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using SYSTEM;
                                                                        using System.Text;
                                                                        using System;
                                                                        """)
                                                  .Usings;

        var canonical = UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usingDirectives);
        var exactSystem = usingDirectives.Single(usingDirective => usingDirective.Name.ToString() == "System");
        var systemChild = usingDirectives.Single(usingDirective => usingDirective.Name.ToString() == "System.Text");
        var caseVariant = usingDirectives.Single(usingDirective => usingDirective.Name.ToString() == "SYSTEM");

        Assert.AreSequenceEqual(["using System;", "using System.Text;", "using SYSTEM;"], canonical.Select(obj => obj.ToString()).ToArray());
        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(exactSystem));
        Assert.AreEqual(UsingDirectiveOrderingGroup.SystemNamespace, UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(exactSystem));
        Assert.IsTrue(UsingDirectiveOrderingUtilities.AreInSameGroup(exactSystem, systemChild));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.AreInSameGroup(exactSystem, caseVariant));
        Assert.AreEqual(string.Empty, UsingDirectiveOrderingUtilities.GetNamespaceGroupOrderKey(exactSystem));
        Assert.AreEqual("SYSTEM", UsingDirectiveOrderingUtilities.GetNamespaceGroupOrderKey(caseVariant));
    }

    /// <summary>
    /// Verifies that every non-exact casing of the System root is classified as an ordinary namespace
    /// </summary>
    [TestMethod]
    public void EveryNonExactSystemCasingIsClassifiedAsAnOrdinaryNamespace()
    {
        const string root = "system";

        for (var casingMask = 0; casingMask < 1 << root.Length; casingMask++)
        {
            var rootNamespace = new string(root.Select((character, index) => (casingMask & (1 << index)) == 0
                                                                                 ? character
                                                                                 : char.ToUpperInvariant(character))
                                               .ToArray());

            if (rootNamespace == "System")
            {
                continue;
            }

            var usingDirective = CoreSyntaxTestHelper.ParseCompilationUnit($"using {rootNamespace};")
                                                     .Usings
                                                     .Single();

            Assert.IsFalse(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirective), rootNamespace);
            Assert.AreEqual(UsingDirectiveOrderingGroup.OtherNamespace,
                            UsingDirectiveOrderingUtilities.GetUsingDirectiveGroup(usingDirective),
                            rootNamespace);
            Assert.AreEqual(rootNamespace, UsingDirectiveOrderingUtilities.GetRootNamespace(usingDirective), rootNamespace);
        }
    }

    /// <summary>
    /// Verifies that child and global-qualified namespaces use the same ordinal System-root boundary
    /// </summary>
    [TestMethod]
    public void ChildAndGlobalQualifiedNamespacesHonorExactSystemRootBoundary()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using System.Text;
                                                                        using SYSTEM.Text;
                                                                        using global::System.Collections;
                                                                        using global::system.Collections;
                                                                        """)
                                                  .Usings;

        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirectives[0]));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirectives[1]));
        Assert.IsTrue(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirectives[2]));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.IsSystemNamespaceUsing(usingDirectives[3]));
        Assert.IsTrue(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[0], usingDirectives[2]));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[0], usingDirectives[1]));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[2], usingDirectives[3]));
        Assert.AreEqual("SYSTEM", UsingDirectiveOrderingUtilities.GetNamespaceGroupOrderKey(usingDirectives[1]));
        Assert.AreEqual("system", UsingDirectiveOrderingUtilities.GetNamespaceGroupOrderKey(usingDirectives[3]));
    }

    /// <summary>
    /// Verifies that global directives form the leading section and that local exact-System priority cannot outrank it
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderPlacesEveryGlobalUsingBeforeEveryLocalUsing()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using System.Text;
                                                                        global using Alias = Beta.Type;
                                                                        global using static Alpha.Helper;
                                                                        global using SYSTEM.Text;
                                                                        global using System.Collections;
                                                                        """)
                                                  .Usings;

        var canonical = UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usingDirectives);

        Assert.AreSequenceEqual([
                                    "global using System.Collections;",
                                    "global using SYSTEM.Text;",
                                    "global using static Alpha.Helper;",
                                    "global using Alias = Beta.Type;",
                                    "using System.Text;"
                                ],
                                canonical.Select(obj => obj.ToString()).ToArray());
        Assert.IsFalse(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[0], usingDirectives[4]));
    }

    /// <summary>
    /// Verifies that ordinal-distinct roots remain contiguous before member names are considered for every using type
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderKeepsOrdinalRootGroupsContiguousForEveryUsingType()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using SYSTEM.Zulu;
                                                                        using system.Bravo;
                                                                        using SYSTEM.Alpha;
                                                                        using static SYSTEM.Zulu;
                                                                        using static system.Bravo;
                                                                        using static SYSTEM.Alpha;
                                                                        using ZuluAlias = SYSTEM.Zulu;
                                                                        using BravoAlias = system.Bravo;
                                                                        using AlphaAlias = SYSTEM.Alpha;
                                                                        """)
                                                  .Usings;

        var canonical = UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usingDirectives);

        Assert.AreSequenceEqual([
                                    "using SYSTEM.Alpha;",
                                    "using SYSTEM.Zulu;",
                                    "using system.Bravo;",
                                    "using static SYSTEM.Alpha;",
                                    "using static SYSTEM.Zulu;",
                                    "using static system.Bravo;",
                                    "using AlphaAlias = SYSTEM.Alpha;",
                                    "using ZuluAlias = SYSTEM.Zulu;",
                                    "using BravoAlias = system.Bravo;"
                                ],
                                canonical.Select(obj => obj.ToString()).ToArray());
    }

    /// <summary>
    /// Verifies that aliases are grouped by the root namespace of their target
    /// </summary>
    [TestMethod]
    public void AliasesAreGroupedByTargetRootNamespace()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using First = Alpha.Thing;
                                                                        using Second = Alpha.Other;
                                                                        using Third = Beta.Thing;
                                                                        """)
                                                  .Usings;

        Assert.IsTrue(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[0], usingDirectives[1]));
        Assert.IsFalse(UsingDirectiveOrderingUtilities.AreInSameGroup(usingDirectives[1], usingDirectives[2]));
    }

    /// <summary>
    /// Verifies that the canonical order orders aliases by the root namespace of their target before the alias name
    /// </summary>
    [TestMethod]
    public void ComputeCanonicalOrderOrdersAliasesByTargetRootNamespace()
    {
        var usingDirectives = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using Zebra = Alpha.Thing;
                                                                        using Apple = Beta.Thing;
                                                                        """)
                                                  .Usings;

        var canonical = UsingDirectiveOrderingUtilities.ComputeCanonicalOrder(usingDirectives);

        Assert.AreSequenceEqual(_aliasCanonicalOrder, canonical.Select(obj => obj.ToString()).ToArray());
    }

    /// <summary>
    /// Verifies that <see cref="UsingDirectiveOrderingUtilities.OrderUsings"/> shares the consolidated canonical policy and
    /// orders aliases by the root namespace of their target rather than by alias name
    /// </summary>
    [TestMethod]
    public void OrderUsingsOrdersAliasesByTargetRootNamespace()
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        using Z = A.B;
                                                                        using A = Z.Q;
                                                                        """);

        var orderedUsings = UsingDirectiveOrderingUtilities.OrderUsings(UsingDirectiveOrderingUtilities.GetUsings(compilationUnit));

        Assert.AreSequenceEqual(_aliasTargetRootOrder, orderedUsings.Select(obj => obj.ToString()).ToArray());
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

        Assert.AreSequenceEqual(_compilationUnitReorderedOrder, orderedCompilationUnitUsings.Select(obj => obj.ToString()).ToArray());
        Assert.AreSequenceEqual(_namespaceReorderedOrder, updatedNamespaceDeclaration.Usings.Select(obj => obj.ToString()).ToArray());
    }

    /// <summary>
    /// Verifies that a comment stays attached to its directive when the group is reordered
    /// </summary>
    [TestMethod]
    public void OrderUsingsKeepsCommentWithItsDirective()
    {
        var compilationUnit = CoreSyntaxTestHelper.ParseCompilationUnit("""
                                                                        // Keep with Charlie
                                                                        using Charlie;
                                                                        using Beta;
                                                                        using Alpha;
                                                                        """);

        var orderedUsings = UsingDirectiveOrderingUtilities.OrderUsings(UsingDirectiveOrderingUtilities.GetUsings(compilationUnit));

        var charlie = orderedUsings.Single(usingDirective => usingDirective.Name.ToString() == "Charlie");
        var alpha = orderedUsings.Single(usingDirective => usingDirective.Name.ToString() == "Alpha");

        Assert.AreSequenceEqual(_commentDirectiveOrder, orderedUsings.Select(obj => obj.ToString()).ToArray());
        Assert.Contains("Keep with Charlie", charlie.GetLeadingTrivia().ToFullString());
        Assert.DoesNotContain("Keep with Charlie", alpha.GetLeadingTrivia().ToFullString());
    }

    #endregion // Tests
}