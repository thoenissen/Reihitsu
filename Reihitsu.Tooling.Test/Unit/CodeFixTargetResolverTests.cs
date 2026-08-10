using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Tooling.Test.Helpers;

namespace Reihitsu.Tooling.Test.Unit;

/// <summary>
/// Tests for <see cref="CodeFixTargetResolver"/>
/// </summary>
[TestClass]
public sealed class CodeFixTargetResolverTests
{
    #region Methods

    /// <summary>
    /// Verifies that a shipped diagnostic ID resolves to its own analyzer and provider
    /// </summary>
    [TestMethod]
    public void TryResolveResolvesShippedDiagnosticId()
    {
        // Act
        var resolved = CodeFixTargetResolver.TryResolve("RH7101", out var target, out var error);

        // Assert
        Assert.IsTrue(resolved);
        Assert.IsNull(error);
        Assert.AreEqual("RH7101", target.DiagnosticId);
        Assert.AreEqual("RH7101DoNotCombineFieldsCodeFixProvider", target.CodeFixProvider.GetType().Name);
        Assert.HasCount(1, target.Analyzers);
    }

    /// <summary>
    /// Verifies that an ID claimed by two providers is refused instead of one of them being picked, and that the
    /// message names both candidates so the caller can see what the ambiguity is
    /// </summary>
    [TestMethod]
    public void TryResolveRefusesAnIdClaimedBySeveralProviders()
    {
        // Arrange
        var analyzers = new DiagnosticAnalyzer[] { new FakeAnalyzer() };
        var providers = new CodeFixProvider[] { new SecondFakeCodeFix(), new FirstFakeCodeFix() };

        // Act
        var resolved = CodeFixTargetResolver.TryResolve(FakeDiagnostic.Id, analyzers, providers, out var target, out var error);

        // Assert
        Assert.IsFalse(resolved);
        Assert.IsNull(target);
        Assert.Contains("more than one code fix provider", error);
        Assert.Contains("FirstFakeCodeFix, SecondFakeCodeFix", error);
    }

    /// <summary>
    /// Verifies the other side of the ambiguity boundary: exactly one provider resolves
    /// </summary>
    [TestMethod]
    public void TryResolveAcceptsExactlyOneProvider()
    {
        // Arrange
        var analyzers = new DiagnosticAnalyzer[] { new FakeAnalyzer() };
        var providers = new CodeFixProvider[] { new FirstFakeCodeFix() };

        // Act
        var resolved = CodeFixTargetResolver.TryResolve(FakeDiagnostic.Id, analyzers, providers, out var target, out var error);

        // Assert
        Assert.IsTrue(resolved);
        Assert.IsNull(error);
        Assert.IsInstanceOfType<FirstFakeCodeFix>(target.CodeFixProvider);
    }

    /// <summary>
    /// Verifies that an ID no analyzer reports is refused before the provider set is even considered
    /// </summary>
    [TestMethod]
    public void TryResolveRefusesAnIdNoAnalyzerReports()
    {
        // Arrange
        var providers = new CodeFixProvider[] { new FirstFakeCodeFix() };

        // Act
        var resolved = CodeFixTargetResolver.TryResolve(FakeDiagnostic.Id, [], providers, out var target, out var error);

        // Assert
        Assert.IsFalse(resolved);
        Assert.IsNull(target);
        Assert.Contains("no analyzer reports", error);
        Assert.Contains(FakeDiagnostic.Id, error);
    }

    /// <summary>
    /// Verifies that an ID with an analyzer but no provider is refused, so a rule that deliberately ships no fix
    /// cannot be mistaken for a rule whose fix withheld itself on the fixture
    /// </summary>
    [TestMethod]
    public void TryResolveRefusesAnIdWithoutAProvider()
    {
        // Arrange
        var analyzers = new DiagnosticAnalyzer[] { new FakeAnalyzer() };

        // Act
        var resolved = CodeFixTargetResolver.TryResolve(FakeDiagnostic.Id, analyzers, [], out var target, out var error);

        // Assert
        Assert.IsFalse(resolved);
        Assert.IsNull(target);
        Assert.Contains("no code fix provider", error);
    }

    #endregion // Methods
}