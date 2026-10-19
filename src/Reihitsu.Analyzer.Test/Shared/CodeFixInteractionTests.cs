using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Design;
using Reihitsu.Analyzer.CodeFixes.Rules.Naming;
using Reihitsu.Analyzer.Rules.Design;
using Reihitsu.Analyzer.Rules.Naming;
using Reihitsu.Analyzer.Test.Base;
using Reihitsu.Analyzer.Test.Verifiers;

namespace Reihitsu.Analyzer.Test.Shared;

/// <summary>
/// Test methods for the shared rule that the code fixes of two rules do not undo each other, exercised through
/// <see cref="RH2005FieldsMustBePrivateCodeFixProvider"/>, whose result <see cref="RH4106PrivateFieldCasingAnalyzer"/>
/// reports, and <see cref="RH4106PrivateFieldCasingCodeFixProvider"/>
/// </summary>
[TestClass]
public class CodeFixInteractionTests
{
    #region Constants

    /// <summary>
    /// The public field the RH2005 fix starts from
    /// </summary>
    private const string PublicField = """
                                       public class C
                                       {
                                           public string {|RH2005:first|};
                                       }
                                       """;

    /// <summary>
    /// The private field the RH2005 fix produces, which RH4106 reports
    /// </summary>
    private const string PrivateField = """
                                        public class C
                                        {
                                            private string {|RH4106:first|};
                                        }
                                        """;

    /// <summary>
    /// The renamed private field the RH4106 fix produces from <see cref="PrivateField"/>
    /// </summary>
    private const string RenamedPrivateField = """
                                               public class C
                                               {
                                                   private string _first;
                                               }
                                               """;

    #endregion // Constants

    #region Methods

    /// <summary>
    /// Removes the diagnostic markup of the provided rule from a source text
    /// </summary>
    /// <param name="source">Source text with markup</param>
    /// <param name="diagnosticId">Diagnostic ID whose markup is removed</param>
    /// <returns>The source text without the markup</returns>
    private static string RemoveMarkup(string source, string diagnosticId)
    {
        return source.Replace($"{{|{diagnosticId}:", string.Empty).Replace("|}", string.Empty);
    }

    #endregion // Methods

    #region Tests

    /// <summary>
    /// Verifying the RH4106 fix, applied to the private field the RH2005 fix produced, renames the field and keeps it
    /// private, so it does not restore the public field the RH2005 fix started from and RH2005 does not report the
    /// result
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [Requirement("SHARED-FIX-004")]
    public async Task RH4106FixDoesNotRestoreTheCodeTheRH2005FixStartedFrom()
    {
        var accessibilityFix = new CSharpCodeFixVerifierTest<RH2005FieldsMustBePrivateAnalyzer, RH2005FieldsMustBePrivateCodeFixProvider>
                               {
                                   TestCode = PublicField,
                                   FixedCode = RemoveMarkup(PrivateField, RH4106PrivateFieldCasingAnalyzer.DiagnosticId),
                                   ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                               };

        await accessibilityFix.RunAsync(CancellationToken.None);

        var namingFix = new CSharpCodeFixVerifierTest<RH4106PrivateFieldCasingAnalyzer, RH4106PrivateFieldCasingCodeFixProvider>
                        {
                            TestCode = PrivateField,
                            FixedCode = RenamedPrivateField,
                            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                        };

        await namingFix.RunAsync(CancellationToken.None);

        var accessibilityCheck = new CSharpAnalyzerVerifierTest<RH2005FieldsMustBePrivateAnalyzer>
                                 {
                                     TestCode = RenamedPrivateField,
                                     ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                                 };

        await accessibilityCheck.RunAsync(CancellationToken.None);
    }

    #endregion // Tests
}