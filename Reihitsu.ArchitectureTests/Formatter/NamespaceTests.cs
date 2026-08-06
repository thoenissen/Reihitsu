using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV4;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;
using Reihitsu.Formatter.Pipeline;
using Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules;

namespace Reihitsu.ArchitectureTests.Formatter;

/// <summary>
/// Definition of namespaces to be used by types
/// </summary>
[TestClass]
public class NamespaceTests
{
    #region Tests

    /// <summary>
    /// Tests that all <see cref="CSharpSyntaxRewriter"/> types are in the correct namespace
    /// </summary>
    [TestMethod]
    public void RewriterShouldBeInRewriterNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreAssignableTo(typeof(CSharpSyntaxRewriter))
                          .Should()
                          .ResideInNamespaceMatching(@"^Reihitsu\..*\.Rewriter$")
                          .Check(Assemblies.Formatter);
    }

    /// <summary>
    /// Tests that which are not <see cref="CSharpSyntaxRewriter"/> are not in the rewriter namespace
    /// </summary>
    [TestMethod]
    public void OtherTypesShouldNotBeInRewriterNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreNotAssignableTo(typeof(CSharpSyntaxRewriter))
                          .Should()
                          .NotResideInNamespaceMatching(@"^Reihitsu\..*\.Rewriter$")
                          .Check(Assemblies.Formatter);
    }

    /// <summary>
    /// Tests that all <see cref="ISpacingRule"/> types are in the correct namespace
    /// </summary>
    [TestMethod]
    public void SpacingRulesShouldBeInRewriterNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreAssignableTo(typeof(ISpacingRule))
                          .Should()
                          .ResideInNamespace("Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules")
                          .Check(Assemblies.Formatter);
    }

    /// <summary>
    /// Tests that which are not <see cref="ISpacingRule"/> are not in the spacing rule namespace
    /// </summary>
    [TestMethod]
    public void OtherTypesShouldNotBeInSpacingRuleNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreNotAssignableTo(typeof(ISpacingRule))
                          .Should()
                          .NotResideInNamespace("Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules")
                          .Check(Assemblies.Formatter);
    }

    /// <summary>
    /// Tests that all <see cref="IFormattingPhase"/> types are in the correct namespace
    /// </summary>
    [TestMethod]
    public void FormattingPhasesShouldBeInFormattingPhaseNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreAssignableTo(typeof(IFormattingPhase))
                          .And()
                          .AreNot(typeof(IFormattingPhase))
                          .Should()
                          .ResideInNamespaceMatching(@"^Reihitsu\.Formatter\.Pipeline\.[A-Z][a-zA-Z]+$")
                          .Check(Assemblies.Formatter);
    }

    /// <summary>
    /// Tests that which are not <see cref="IFormattingPhase"/> are not in the formatting phase namespace
    /// </summary>
    [TestMethod]
    public void OtherTypesShouldNotBeInFormattingPhaseNamespace()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .AreNotAssignableTo(typeof(IFormattingPhase))
                          .Should()
                          .NotResideInNamespaceMatching(@"^Reihitsu\.Formatter\.Pipeline\.[A-Z][a-zA-Z]+$")
                          .Check(Assemblies.Formatter);
    }

    #endregion // Tests
}