using System;
using System.Collections.Generic;
using System.Linq;

using ArchUnitNET.Fluent;
using ArchUnitNET.MSTestV4;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.ArchitectureTests.Properties;
using Reihitsu.Formatter.Pipeline;

using Assembly = System.Reflection.Assembly;

using RequirementAttribute = Reihitsu.Analyzer.Test.Base.RequirementAttribute;

namespace Reihitsu.ArchitectureTests.Analyzer;

/// <summary>
/// Code-fix code reaches a formatter phase or structural transform only through <c>ReihitsuFormatter</c>: it runs no phase, no
/// transform, and no other rewriter a phase runs, calls no member of one, and does not use <c>FormattingPipeline</c>. The rule is
/// keyed on those types, so formatter helpers and utility classes that are none of them stay callable
/// </summary>
[TestClass]
public class CodeFixFormatterAccessTests
{
    #region Tests

    /// <summary>
    /// Verifies that no type of the code-fix assembly depends on a formatter phase, a structural transform, another rewriter of the
    /// formatter, or the formatting pipeline
    /// </summary>
    [TestMethod]
    [Requirement("FMT-PIPE-009")]
    public void CodeFixesDoNotDependOnFormatterPhasesTransformsOrThePipeline()
    {
        ArchRuleDefinition.Types()
                          .That()
                          .ResideInAssembly(Assembly.Load("Reihitsu.Analyzer.CodeFixes"))
                          .Should()
                          .NotDependOnAny(GetTypesCodeFixesMustNotUse())
                          .Check(Assemblies.Implementation);
    }

    /// <summary>
    /// Verifies that the types the rule bans are the phases, the transforms and other rewriters, and the pipeline, so the rule
    /// would see a code fix that runs one of them
    /// </summary>
    /// <param name="typeName">Full name of a formatter type a code fix must not use</param>
    [TestMethod]
    [Requirement("FMT-PIPE-009")]
    [DataRow("Reihitsu.Formatter.Pipeline.FormattingPipeline")]
    [DataRow("Reihitsu.Formatter.Pipeline.UsingDirectives.UsingDirectiveOrderingPhase")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.StructuralTransformPhase")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter.FieldDeclarationSplitTransform")]
    public void BannedTypesIncludeThePhasesTheTransformsAndThePipeline(string typeName)
    {
        CollectionAssert.Contains(GetTypesCodeFixesMustNotUse().Select(type => type.FullName).ToList(), typeName);
    }

    /// <summary>
    /// Verifies that formatter helpers and utility classes that are no phase, transform, rewriter, or pipeline are not banned, also
    /// when a transform calls them
    /// </summary>
    /// <param name="typeName">Full name of a formatter type a code fix may use</param>
    [TestMethod]
    [Requirement("FMT-PIPE-009")]
    [DataRow("Reihitsu.Formatter.ReihitsuFormatter")]
    [DataRow("Reihitsu.Formatter.Utilities.ReihitsuFormatterHelpers")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities.ExpressionBodiedTransformUtilities")]
    public void BannedTypesExcludeHelpersAndUtilityClasses(string typeName)
    {
        Assert.IsNotNull(FormatterAssembly.GetType(typeName), $"The type {typeName} must exist in the formatter assembly.");
        CollectionAssert.DoesNotContain(GetTypesCodeFixesMustNotUse().Select(type => type.FullName).ToList(), typeName);
    }

    #endregion // Tests

    #region Properties

    /// <summary>
    /// The formatter assembly
    /// </summary>
    private static Assembly FormatterAssembly => typeof(IFormattingPhase).Assembly;

    #endregion // Properties

    #region Methods

    /// <summary>
    /// Gets the formatter types a code fix must reach only through <c>ReihitsuFormatter</c>: every phase, every syntax rewriter of
    /// the formatter (the structural transforms and the rewriters the phases run), and the formatting pipeline
    /// </summary>
    /// <returns>The banned types</returns>
    private static List<Type> GetTypesCodeFixesMustNotUse()
    {
        var pipeline = FormatterAssembly.GetType("Reihitsu.Formatter.Pipeline.FormattingPipeline", throwOnError: true);

        return FormatterAssembly.GetTypes()
                                .Where(type => type.IsInterface == false
                                               && (typeof(IFormattingPhase).IsAssignableFrom(type)
                                                   || typeof(CSharpSyntaxRewriter).IsAssignableFrom(type)
                                                   || type == pipeline))
                                .ToList();
    }

    #endregion // Methods
}