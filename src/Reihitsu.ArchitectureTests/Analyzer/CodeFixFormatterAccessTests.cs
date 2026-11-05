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
    /// Verifies that the types the rule bans are the phases, the transforms, the other rewriters the phases run (also the static
    /// ones and those that derive from no syntax rewriter, such as the brace placer, the layout computer, a spacing rule, or a layout
    /// contributor), and the pipeline, so the rule would see a code fix that runs one of them
    /// </summary>
    /// <param name="typeName">Full name of a formatter type a code fix must not use</param>
    [TestMethod]
    [Requirement("FMT-PIPE-009")]
    [DataRow("Reihitsu.Formatter.Pipeline.FormattingPipeline")]
    [DataRow("Reihitsu.Formatter.Pipeline.UsingDirectives.UsingDirectiveOrderingPhase")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.StructuralTransformPhase")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.Rewriter.FieldDeclarationSplitTransform")]
    [DataRow("Reihitsu.Formatter.Pipeline.LineBreaks.Rewriter.MemberLineStartRewriter")]
    [DataRow("Reihitsu.Formatter.Pipeline.BlankLines.Rewriter.BlankLineCollapser")]
    [DataRow("Reihitsu.Formatter.Pipeline.Indentation.Utilities.IndentationRewriter")]
    [DataRow("Reihitsu.Formatter.Pipeline.RegionFormatting.Utilities.RegionNamingRewriter")]
    [DataRow("Reihitsu.Formatter.Pipeline.LineBreaks.Utilities.BracePlacer")]
    [DataRow("Reihitsu.Formatter.Pipeline.LineBreaks.Utilities.TokenGapNormalizer")]
    [DataRow("Reihitsu.Formatter.Pipeline.BlankLines.Utilities.BlankLineEditor")]
    [DataRow("Reihitsu.Formatter.Pipeline.Indentation.Utilities.LayoutComputer")]
    [DataRow("Reihitsu.Formatter.Pipeline.DocumentationComments.Utilities.DocCommentElementNormalizer")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities.ExpressionBodyToBlockConverter")]
    [DataRow("Reihitsu.Formatter.Pipeline.HorizontalSpacing.Rules.CommaSpacingRule")]
    [DataRow("Reihitsu.Formatter.Pipeline.Indentation.Contributors.ArgumentAlignmentContributor")]
    public void BannedTypesIncludeThePhasesTheTransformsAndThePipeline(string typeName)
    {
        Assert.Contains(typeName, GetTypesCodeFixesMustNotUse().Select(type => type.FullName).ToList());
    }

    /// <summary>
    /// Verifies that formatter helpers and utility classes that are no phase, transform, rewriter, or pipeline are not banned, also
    /// when a phase or a transform calls them, and that the data the phases read is not banned
    /// </summary>
    /// <param name="typeName">Full name of a formatter type a code fix may use</param>
    [TestMethod]
    [Requirement("FMT-PIPE-009")]
    [DataRow("Reihitsu.Formatter.ReihitsuFormatter")]
    [DataRow("Reihitsu.Formatter.Utilities.ReihitsuFormatterHelpers")]
    [DataRow("Reihitsu.Formatter.Pipeline.StructuralTransforms.Utilities.ExpressionBodiedTransformUtilities")]
    [DataRow("Reihitsu.Formatter.Pipeline.Core.Utilities.TriviaListUtilities")]
    [DataRow("Reihitsu.Formatter.Pipeline.LineBreaks.Utilities.LineBreakTriviaUtilities")]
    [DataRow("Reihitsu.Formatter.Data.FormattingContext")]
    public void BannedTypesExcludeHelpersAndUtilityClasses(string typeName)
    {
        Assert.IsNotNull(FormatterAssembly.GetType(typeName), $"The type {typeName} must exist in the formatter assembly.");
        Assert.DoesNotContain(typeName, GetTypesCodeFixesMustNotUse().Select(type => type.FullName).ToList());
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
    /// Gets the formatter types a code fix must reach only through <c>ReihitsuFormatter</c>: every phase, the formatting pipeline,
    /// every syntax rewriter of the formatter, and every other class a banned type uses, directly or through another banned type,
    /// which is how a phase runs its transforms and rewriters, the static ones included. A helper, utility, or extension class,
    /// named <c>…Helpers</c>, <c>…Utilities</c>, or <c>…Extensions</c>, and a type of <c>Reihitsu.Formatter.Data</c>, the data the phases read, are not banned
    /// and do not pass the ban on to the types they use
    /// </summary>
    /// <returns>The banned types</returns>
    private static List<Type> GetTypesCodeFixesMustNotUse()
    {
        var pipeline = FormatterAssembly.GetType("Reihitsu.Formatter.Pipeline.FormattingPipeline", throwOnError: true);
        var formatterTypes = Assemblies.Formatter.Classes.Where(type => FormatterAssembly.GetType(type.FullName) != null)
                                                         .ToDictionary(type => type.FullName);
        var banned = new HashSet<string>(formatterTypes.Values
                                                       .Select(type => FormatterAssembly.GetType(type.FullName))
                                                       .Where(type => typeof(IFormattingPhase).IsAssignableFrom(type)
                                                                      || typeof(CSharpSyntaxRewriter).IsAssignableFrom(type)
                                                                      || type == pipeline)
                                                       .Select(type => type.FullName));
        var pending = new Queue<string>(banned);

        while (pending.Count > 0)
        {
            foreach (var dependency in formatterTypes[pending.Dequeue()].Dependencies)
            {
                var targetName = dependency.Target.FullName;

                if (formatterTypes.ContainsKey(targetName)
                    && IsHelperUtilityOrData(targetName) == false
                    && banned.Add(targetName))
                {
                    pending.Enqueue(targetName);
                }
            }
        }

        return banned.Select(typeName => FormatterAssembly.GetType(typeName, throwOnError: true))
                     .ToList();
    }

    /// <summary>
    /// Determines whether a formatter type is a helper, utility, or extension class, or a type of the data the phases read
    /// </summary>
    /// <param name="typeName">Full name of the type</param>
    /// <returns><see langword="true"/> when a code fix may use the type</returns>
    private static bool IsHelperUtilityOrData(string typeName)
    {
        return typeName.EndsWith("Helpers", StringComparison.Ordinal)
               || typeName.EndsWith("Utilities", StringComparison.Ordinal)
               || typeName.EndsWith("Extensions", StringComparison.Ordinal)
               || typeName.StartsWith("Reihitsu.Formatter.Data.", StringComparison.Ordinal);
    }

    #endregion // Methods
}