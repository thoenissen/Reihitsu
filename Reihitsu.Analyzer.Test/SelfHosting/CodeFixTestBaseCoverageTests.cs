using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.SelfHosting.Utilities;

namespace Reihitsu.Analyzer.Test.SelfHosting;

/// <summary>
/// Testing that every test class of an analyzer and its code fix declares, through its base class, whether the
/// code fix supports Fix All. <see cref="BatchCodeFixTestsBase{TAnalyzer, TCodeFix}"/> then requires a scenario
/// that corrects more than one diagnostic in one document, and <see cref="SingleCodeFixTestsBase{TAnalyzer, TCodeFix}"/>
/// records that the provider deliberately corrects one occurrence at a time
/// </summary>
[TestClass]
public class CodeFixTestBaseCoverageTests
{
    #region Fields

    /// <summary>
    /// Migration ledger of the test classes that still derive from
    /// <see cref="AnalyzerTestsBase{TAnalyzer, TCodeFix}"/> directly, because the compiler cannot prevent that
    /// inside the same assembly.
    /// <para>
    /// This list is temporary. It is scheduled for deletion together with this field's last entry: every
    /// migration pull request removes the classes it migrates, and the final one removes the list and this
    /// remark with it. Nothing may ever be added here — a new test class picks one of the two code-fix test
    /// bases from the start
    /// </para>
    /// </summary>
    private static readonly IReadOnlySet<string> _notYetMigratedTestClasses = new HashSet<string>(StringComparer.Ordinal)
                                                                              {
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2001PrivateAutoPropertiesShouldNotBeUsedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2004AccessModifierMustBeDeclaredAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH2005FieldsMustBePrivateAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH3106UnnecessaryDelegateParenthesesShouldBeRemovedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Design.RH3107UnnecessaryAttributeConstructorParenthesesShouldBeRemovedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5511ConstructorAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5513MethodAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5515PropertyAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5517FieldAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5519EventAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5527ReturnValueAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH5530AccessorAttributesMustFollowPlacementRulesAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6001KeywordsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6004PreprocessorKeywordsMustNotBePrecededBySpaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6005OperatorKeywordMustBeFollowedBySpaceAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6006OpeningParenthesisMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6007OpeningSquareBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6008ClosingSquareBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6009OpeningBracesMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6010ClosingBracesMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6013OpeningAttributeBracketsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6016MemberAccessSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6017IncrementAndDecrementSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6018NegativeSignsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6019PositiveSignsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6020DereferenceAndAccessOfSymbolsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6021ColonsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6022NoSpaceAfterNewForImplicitlyTypedArraysAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH6023AssignmentOperatorsMustBeSpacedCorrectlyAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7004UsingDeclarationsShouldNotBeUsedAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7101DoNotCombineFieldsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7302RegionsShouldStartWithAUpperCaseLetterAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7303DoNotPlaceRegionsWithinElementsAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7304RegionDirectivesMustUseConsistentIndentationAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7306RegionDescriptionsShouldNotEndWithImplementationAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7309RegionsShouldFollowCategoryOrderAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH7501BreakStatementsShouldNotBeInsideExplicitSwitchCaseBlocksAnalyzerTests",
                                                                                  "Reihitsu.Analyzer.Test.Formatting.RH8303ElementDocumentationHeaderMustBePrecededByBlankLineAnalyzerTests"
                                                                              };

    #endregion // Fields

    #region Methods

    /// <summary>
    /// Gets the repository-relative source path a test class is expected to live in
    /// </summary>
    /// <param name="testClassType">Test class type</param>
    /// <returns>The repository-relative source path</returns>
    private static string GetSourcePath(Type testClassType)
    {
        const string projectNamespace = "Reihitsu.Analyzer.Test";

        var subNamespace = testClassType.Namespace?.Length > projectNamespace.Length
                               ? testClassType.Namespace[projectNamespace.Length..].TrimStart('.').Replace('.', '/')
                               : string.Empty;

        return subNamespace.Length > 0
                   ? $"{projectNamespace}/{subNamespace}/{testClassType.Name}.cs"
                   : $"{projectNamespace}/{testClassType.Name}.cs";
    }

    /// <summary>
    /// Gets the name of the code-fix test base a test class has to derive from
    /// </summary>
    /// <param name="testClass">Test class metadata</param>
    /// <returns>The name of the expected code-fix test base</returns>
    private static string GetExpectedBaseName(DiscoveredCodeFixTestClass testClass)
    {
        var expectedBase = testClass.SupportsFixAll
                               ? typeof(BatchCodeFixTestsBase<,>)
                               : typeof(SingleCodeFixTestsBase<,>);

        return expectedBase.Name[..expectedBase.Name.IndexOf('`', StringComparison.Ordinal)];
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying every code-fix test class derives from one of the two code-fix test bases, so its Fix All
    /// coverage is decided in code instead of being left to whoever writes the next test
    /// </summary>
    [TestMethod]
    public void EveryCodeFixTestClassDerivesFromACodeFixTestsBase()
    {
        var testClasses = AnalyzerMetadataDiscovery.DiscoverCodeFixTestClasses();

        Assert.IsNotEmpty(testClasses);

        var findings = testClasses.Where(testClass => testClass.CodeFixTestsBaseDefinition is null
                                                      && _notYetMigratedTestClasses.Contains(testClass.TestClassType.FullName) is false)
                                  .Select(testClass => $"{testClass.TestClassType.FullName} must derive from {GetExpectedBaseName(testClass)} ({GetSourcePath(testClass.TestClassType)})")
                                  .ToArray();

        if (findings.Length > 0)
        {
            Assert.Fail($"The following changes are required:\n\n{string.Join(Environment.NewLine, findings)}");
        }
    }

    /// <summary>
    /// Verifying the migration ledger only lists test classes that exist and are still unmigrated, so it can
    /// only shrink and never hides a class that has already been migrated
    /// </summary>
    [TestMethod]
    public void MigrationLedgerListsOnlyUnmigratedTestClasses()
    {
        var testClasses = AnalyzerMetadataDiscovery.DiscoverCodeFixTestClasses();
        var unmigratedTestClasses = testClasses.Where(testClass => testClass.CodeFixTestsBaseDefinition is null)
                                               .Select(testClass => testClass.TestClassType.FullName)
                                               .ToHashSet(StringComparer.Ordinal);
        var findings = _notYetMigratedTestClasses.Where(testClass => unmigratedTestClasses.Contains(testClass) is false)
                                                 .OrderBy(testClass => testClass, StringComparer.Ordinal)
                                                 .Select(testClass => $"{testClass} is listed in the migration ledger, but it does not exist or already derives from a code-fix test base")
                                                 .ToArray();

        if (findings.Length > 0)
        {
            Assert.Fail($"The following entries have to be removed from the migration ledger:\n\n{string.Join(Environment.NewLine, findings)}");
        }
    }

    #endregion // Tests
}