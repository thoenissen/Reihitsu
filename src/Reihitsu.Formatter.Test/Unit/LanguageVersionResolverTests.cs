using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Formatter.Data;
using Reihitsu.Formatter.Utilities;

namespace Reihitsu.Formatter.Test.Unit;

/// <summary>
/// Tests for <see cref="LanguageVersionResolver"/> and the resolved <see cref="FormattingContext.LanguageVersion"/>
/// </summary>
[TestClass]
public class LanguageVersionResolverTests
{
    #region Methods

    /// <summary>
    /// Verifies that the supported maximum is the version Roslyn's <see cref="LanguageVersion.Latest"/> resolves to, so a
    /// Roslyn upgrade that introduces a newer version fails here until the maximum is raised deliberately
    /// </summary>
    [TestMethod]
    public void MaxSupportedLanguageVersionMatchesRoslynLatest()
    {
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersion.Latest.MapSpecifiedToEffectiveVersion());
    }

    /// <summary>
    /// Verifies that a concrete version at or below the maximum resolves to itself
    /// </summary>
    [TestMethod]
    public void ResolveKeepsConcreteVersionsUpToTheMaximum()
    {
        Assert.AreEqual(LanguageVersion.CSharp1, LanguageVersionResolver.Resolve(LanguageVersion.CSharp1));
        Assert.AreEqual(LanguageVersion.CSharp11, LanguageVersionResolver.Resolve(LanguageVersion.CSharp11));
        Assert.AreEqual(LanguageVersion.CSharp13, LanguageVersionResolver.Resolve(LanguageVersion.CSharp13));
        Assert.AreEqual(LanguageVersion.CSharp14, LanguageVersionResolver.Resolve(LanguageVersion.CSharp14));
    }

    /// <summary>
    /// Verifies that every symbolic version resolves to the maximum rather than to a symbolic value
    /// </summary>
    [TestMethod]
    public void ResolveMapsSymbolicVersionsToTheMaximum()
    {
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve(LanguageVersion.Default));
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve(LanguageVersion.Latest));
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve(LanguageVersion.LatestMajor));
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve(LanguageVersion.Preview));
    }

    /// <summary>
    /// Verifies that a concrete version above the maximum is clamped to it
    /// </summary>
    [TestMethod]
    public void ResolveClampsVersionsAboveTheMaximum()
    {
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve((LanguageVersion)1500));
    }

    /// <summary>
    /// Verifies that parse options resolve through their effective language version
    /// </summary>
    [TestMethod]
    public void ResolveReadsTheLanguageVersionOfParseOptions()
    {
        Assert.AreEqual(LanguageVersion.CSharp11, LanguageVersionResolver.Resolve(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp11)));
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve(CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview)));
    }

    /// <summary>
    /// Verifies that missing parse options resolve to the maximum
    /// </summary>
    [TestMethod]
    public void ResolveTreatsMissingParseOptionsAsTheMaximum()
    {
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, LanguageVersionResolver.Resolve((CSharpParseOptions)null));
    }

    /// <summary>
    /// Verifies that only versions newer than the maximum count as exceeding it
    /// </summary>
    [TestMethod]
    public void ExceedsMaximumIsTrueOnlyAboveTheMaximum()
    {
        Assert.IsFalse(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.CSharp14));
        Assert.IsFalse(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.CSharp11));
        Assert.IsFalse(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.Default));
        Assert.IsFalse(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.Latest));
        Assert.IsFalse(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.LatestMajor));
        Assert.IsTrue(LanguageVersionResolver.ExceedsMaximum(LanguageVersion.Preview));
        Assert.IsTrue(LanguageVersionResolver.ExceedsMaximum((LanguageVersion)1500));
    }

    /// <summary>
    /// Verifies that the formatting context stores the resolved version instead of the requested one
    /// </summary>
    [TestMethod]
    public void FormattingContextStoresTheResolvedLanguageVersion()
    {
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, new FormattingContext("\n").LanguageVersion);
        Assert.AreEqual(LanguageVersionResolver.MaxSupportedLanguageVersion, new FormattingContext("\n", languageVersion: LanguageVersion.Preview).LanguageVersion);
        Assert.AreEqual(LanguageVersion.CSharp11, new FormattingContext("\n", languageVersion: LanguageVersion.CSharp11).LanguageVersion);
    }

    /// <summary>
    /// Verifies that the language version of a formatting context cannot change after construction
    /// </summary>
    [TestMethod]
    public void FormattingContextLanguageVersionIsReadOnly()
    {
        var property = typeof(FormattingContext).GetProperty(nameof(FormattingContext.LanguageVersion));

        Assert.IsNotNull(property);
        Assert.IsFalse(property.CanWrite, "FormattingContext.LanguageVersion must not expose a setter or an init accessor.");
    }

    #endregion // Methods
}