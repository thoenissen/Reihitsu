#pragma warning disable RH2101, S4487, RH7103, RH7110, RH7309, RH7004, RH1003, MSTEST0037
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Reihitsu.Analyzer.CodeFixes.Rules.Clarity;
using Reihitsu.Analyzer.Rules.Clarity;
using Reihitsu.Analyzer.Test.Base;

namespace Reihitsu.Analyzer.Test.Clarity;

/// <summary>
/// Reproduction tests for <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer"/> and <see cref="RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider"/>
/// </summary>
[TestClass]
public class RH3002StatementMustNotUseUnnecessaryParenthesesReproductionTests : BatchCodeFixTestsBase<RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer, RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider>
{
    #region Constants

    /// <summary>
    /// Template of the tested document
    /// </summary>
    private const string Template = """
                                    public class Bag
                                    {
                                        public int Value { get; set; }

                                        public void Add(int value)
                                        {
                                        }
                                    }

                                    public class Test
                                    {
                                        private Test _a;
                                        private int _i;
                                        private System.Func<int> _d;
                                        private string _s;
                                        private int G, A, B;

                                        private void M(bool x, bool y)
                                        {
                                        }

                                        public int Run()
                                        {
                                            //BODY
                                            return 0;
                                        }
                                    }
                                    """;

    #endregion // Constants

    #region Tests

    /// <summary>
    /// Verifying parentheses around a name before an argument list or a null-forgiving operator end in compilable code (iterative single fixes)
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue873SingleFixesProduceCompilableCode(string lineEnding)
    {
        var observation = await ObserveAsync("return ((_d))() + ((_s))!.Length;", lineEnding);

        observation.AssertEveryFixParses();
    }

    /// <summary>
    /// Verifying Fix All on parentheses around a name before an argument list or a null-forgiving operator ends in compilable code
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue873FixAllProducesCompilableCode(string lineEnding)
    {
        var observation = await ObserveAsync("return ((_d))() + ((_s))!.Length;", lineEnding);

        observation.AssertFixAllParses();
    }

    /// <summary>
    /// Verifying parentheses around an invocation target followed by arguments are fixed to the expected text (sibling shapes of the cast-like reparse)
    /// </summary>
    /// <param name="body">Body statement</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("return ((_d))();")]
    [DataRow("return ((_s))!.Length;")]
    [DataRow("return ((_i))!;")]
    [DataRow("return ((_d))() + 1;")]
    public async Task Issue873SiblingShapes(string body)
    {
        var observation = await ObserveAsync(body, "\n");

        observation.AssertEveryFixParses();
        observation.AssertFixAllParses();
    }

    /// <summary>
    /// Verifying the literal reproduction of the with-expression assignment at statement start
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue870WithExpressionAssignmentAtStatementStart(string lineEnding)
    {
        var observation = await ObserveAsync("(_a with { }) = _a;", lineEnding);

        observation.AssertNoFixOffered();
    }

    /// <summary>
    /// Verifying the other shapes listed by the report are not fixed
    /// </summary>
    /// <param name="body">Body statement</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("(A * B) = 5;")]
    [DataRow("(A < B > c) = 5;")]
    [DataRow("(A<B>) = 5;")]
    [DataRow("var l = new System.Collections.Generic.List<int> { (_i) = 1 };")]
    [DataRow("var b = new Bag { (Value) = 1 };")]
    [DataRow("var o = new { (_i) = 1 };")]
    public async Task Issue870OtherShapes(string body)
    {
        var observation = await ObserveAsync(body, "\n");

        observation.AssertNoFixOffered();
    }

    /// <summary>
    /// Verifying Fix All on sibling arguments does not produce a generic invocation
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue874FixAllAcrossSiblingArguments(string lineEnding)
    {
        var observation = await ObserveAsync("M((G < A), (B > (7)));", lineEnding);

        observation.AssertFixAllHasNoGenericName();
    }

    /// <summary>
    /// Verifying an interpolated string is unwrapped without an extra space (single fix)
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue875InterpolatedStringSingleFix(string lineEnding)
    {
        var observation = await ObserveAsync("var s1 = ($\"x{_i}\");", lineEnding);

        observation.AssertSingleFixContains(0, "var s1 = $\"x{_i}\";");
    }

    /// <summary>
    /// Verifying an interpolated string is unwrapped without an extra space (Fix All)
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue875InterpolatedStringFixAll(string lineEnding)
    {
        var observation = await ObserveAsync("var s1 = ($\"x{_i}\");", lineEnding);

        observation.AssertFixAllContains("var s1 = $\"x{_i}\";");
    }

    /// <summary>
    /// Verifying nested parentheses around an interpolated string before a member access are fixed without an extra space
    /// </summary>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("\n")]
    [DataRow("\r\n")]
    public async Task Issue875InterpolatedStringMemberAccessFixAll(string lineEnding)
    {
        var observation = await ObserveAsync("var n = (($\"x{_i}\")).Length;", lineEnding);

        observation.AssertFixAllContains("var n = $\"x{_i}\".Length;");
    }

    /// <summary>
    /// Verifying nested parentheses around an interpolated string before a member access are fixed without an extra space (single fix)
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    public async Task Issue875InterpolatedStringMemberAccessSingleFix()
    {
        var observation = await ObserveAsync("var n = (($\"x{_i}\")).Length;", "\n");

        observation.AssertEverySingleFixHasNoSpaceBeforeClosingParenthesisOrDot();
    }

    /// <summary>
    /// Verifying the sibling shapes of the interpolated string (verbatim and raw interpolated strings, plain string)
    /// </summary>
    /// <param name="body">Body statement</param>
    /// <param name="expected">Expected text</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation</returns>
    [TestMethod]
    [DataRow("var s1 = ($@\"x{_i}\");", "var s1 = $@\"x{_i}\";")]
    [DataRow("var s1 = (\"x\");", "var s1 = \"x\";")]
    [DataRow("var s1 = ($\"\"\"x{_i}\"\"\");", "var s1 = $\"\"\"x{_i}\"\"\";")]
    public async Task Issue875SiblingShapes(string body, string expected)
    {
        var observation = await ObserveAsync(body, "\n");

        observation.AssertFixAllContains(expected);
    }

    #endregion // Tests

    #region BatchCodeFixTestsBase

    /// <inheritdoc/>
    protected override FixAllScenario GetFixAllScenario()
    {
        const string testCode = """
                                public class Test
                                {
                                    public int Run(int value)
                                    {
                                        return {|#0:({|#1:(value)|})|};
                                    }
                                }
                                """;

        const string fixedCode = """
                                 public class Test
                                 {
                                     public int Run(int value)
                                     {
                                         return value;
                                     }
                                 }
                                 """;

        return new FixAllScenario(testCode,
                                  fixedCode,
                                  Diagnostics(RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId, "Statement must not use unnecessary parentheses", 2),
                                  static config => config.NumberOfFixAllIterations = 1);
    }

    #endregion // BatchCodeFixTestsBase

    #region Helpers

    /// <summary>
    /// Runs the analyzer, every single fix and Fix All on the template with the provided body
    /// </summary>
    /// <param name="body">Body statement</param>
    /// <param name="lineEnding">Line ending</param>
    /// <returns>The observation</returns>
    private static async Task<Observation> ObserveAsync(string body, string lineEnding)
    {
        var source = Template.Replace("//BODY", body).Replace("\r\n", "\n").Replace("\n", lineEnding);

        using var workspace = new AdhocWorkspace();

        var projectId = ProjectId.CreateNewId();
        var documentId = DocumentId.CreateNewId(projectId);
        var references = ReferenceAssemblies.Net.Net90.ResolveAsync(LanguageNames.CSharp, CancellationToken.None).GetAwaiter().GetResult();
        var solution = workspace.CurrentSolution
                                .AddProject(ProjectInfo.Create(projectId,
                                                               VersionStamp.Create(),
                                                               "TestProject",
                                                               "TestProject",
                                                               LanguageNames.CSharp,
                                                               parseOptions: new CSharpParseOptions(LanguageVersion.Latest),
                                                               compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
                                                               metadataReferences: references))
                                .AddDocument(documentId, "Test.cs", Microsoft.CodeAnalysis.Text.SourceText.From(source));
        var document = solution.GetDocument(documentId);
        var compilation = await document.Project.GetCompilationAsync(CancellationToken.None);
        var diagnostics = (await compilation.WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer()))
                                            .GetAnalyzerDiagnosticsAsync(CancellationToken.None)).Where(static obj => obj.Id == RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzer.DiagnosticId)
                                                                                                 .OrderBy(static obj => obj.Location.SourceSpan.Start)
                                                                                                 .ThenByDescending(static obj => obj.Location.SourceSpan.Length)
                                                                                                 .ToList();
        var provider = new RH3002StatementMustNotUseUnnecessaryParenthesesCodeFixProvider();
        var spans = new List<string>();
        var singleFixes = new List<string>();
        string equivalenceKey = null;

        foreach (var diagnostic in diagnostics)
        {
            spans.Add(source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));

            var actions = new List<CodeAction>();

            await provider.RegisterCodeFixesAsync(new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), CancellationToken.None));

            if (actions.Count == 0)
            {
                singleFixes.Add(null);
            }
            else
            {
                equivalenceKey = actions[0].EquivalenceKey;

                var operations = await actions[0].GetOperationsAsync(CancellationToken.None);
                var changed = operations.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(documentId);

                singleFixes.Add((await changed.GetTextAsync(CancellationToken.None)).ToString());
            }
        }

        string fixAll = null;

        if (diagnostics.Count > 0 && provider.GetFixAllProvider() is { } fixAllProvider)
        {
            var diagnosticProvider = new FixedDiagnosticProvider(diagnostics);
            var context = new FixAllContext(document,
                                            provider,
                                            FixAllScope.Document,
                                            equivalenceKey,
                                            provider.FixableDiagnosticIds,
                                            diagnosticProvider,
                                            CancellationToken.None);
            var action = await fixAllProvider.GetFixAsync(context);

            if (action != null)
            {
                var operations = await action.GetOperationsAsync(CancellationToken.None);
                var changed = operations.OfType<ApplyChangesOperation>().First().ChangedSolution.GetDocument(documentId);

                fixAll = (await changed.GetTextAsync(CancellationToken.None)).ToString();
            }
        }

        return new Observation(source, spans, singleFixes, fixAll);
    }

    #endregion // Helpers

    #region Types

    /// <summary>
    /// Diagnostic provider returning fixed diagnostics
    /// </summary>
    private sealed class FixedDiagnosticProvider : FixAllContext.DiagnosticProvider
    {
        #region Fields

        /// <summary>
        /// Diagnostics
        /// </summary>
        private readonly IReadOnlyList<Diagnostic> _diagnostics;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="diagnostics">Diagnostics</param>
        public FixedDiagnosticProvider(IReadOnlyList<Diagnostic> diagnostics)
        {
            _diagnostics = diagnostics;
        }

        #endregion // Constructor

        #region DiagnosticProvider

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetAllDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetDocumentDiagnosticsAsync(Document document, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>(_diagnostics);
        }

        /// <inheritdoc/>
        public override Task<IEnumerable<Diagnostic>> GetProjectDiagnosticsAsync(Project project, CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Diagnostic>>([]);
        }

        #endregion // DiagnosticProvider
    }

    /// <summary>
    /// Observed analyzer and code fix behavior
    /// </summary>
    private sealed class Observation
    {
        #region Fields

        /// <summary>
        /// Source
        /// </summary>
        private readonly string _source;

        /// <summary>
        /// Reported spans
        /// </summary>
        private readonly List<string> _spans;

        /// <summary>
        /// Result of each single fix, <see langword="null"/> if no action was offered
        /// </summary>
        private readonly List<string> _singleFixes;

        /// <summary>
        /// Result of Fix All
        /// </summary>
        private readonly string _fixAll;

        #endregion // Fields

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="spans">Reported spans</param>
        /// <param name="singleFixes">Single fix results</param>
        /// <param name="fixAll">Fix All result</param>
        public Observation(string source, List<string> spans, List<string> singleFixes, string fixAll)
        {
            _source = source;
            _spans = spans;
            _singleFixes = singleFixes;
            _fixAll = fixAll;
        }

        #endregion // Constructor

        #region Methods

        /// <summary>
        /// Describes the observation
        /// </summary>
        /// <returns>The description</returns>
        private string Describe()
        {
            var lines = new List<string>
                        {
                            $"Reported pairs ({_spans.Count}): " + string.Join(" | ", _spans)
                        };

            for (var index = 0; index < _singleFixes.Count; index++)
            {
                lines.Add($"Single fix {index} ({_spans[index]}): " + (_singleFixes[index] == null ? "<no fix offered>" : ExtractBody(_singleFixes[index])));
            }

            lines.Add("Fix All: " + (_fixAll == null ? "<no fix offered>" : ExtractBody(_fixAll)));

            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// Extracts the body line of the tested method
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>The lines between the braces of Run</returns>
        private static string ExtractBody(string text)
        {
            var normalized = text.Replace("\r\n", "\n");
            var start = normalized.IndexOf("public int Run()", StringComparison.Ordinal);
            var end = normalized.IndexOf("return 0;", start, StringComparison.Ordinal);

            return "[" + normalized.Substring(start, end - start).Replace("\n", "\\n") + "]";
        }

        /// <summary>
        /// Gets the parse errors of a text
        /// </summary>
        /// <param name="text">Text</param>
        /// <returns>The parse errors</returns>
        private static string ParseErrors(string text)
        {
            return string.Join("; ", CSharpSyntaxTree.ParseText(text).GetDiagnostics().Where(static obj => obj.Severity == DiagnosticSeverity.Error).Select(static obj => obj.GetMessage()));
        }

        /// <summary>
        /// Asserts every single fix result parses without errors
        /// </summary>
        public void AssertEveryFixParses()
        {
            Assert.IsGreaterThan(0, _spans.Count, "Nothing reported (alternative expectation)");

            foreach (var fix in _singleFixes.Where(static obj => obj != null))
            {
                Assert.AreEqual(string.Empty, ParseErrors(fix), "Single fix result does not parse." + Environment.NewLine + Describe());
            }
        }

        /// <summary>
        /// Asserts the Fix All result parses without errors
        /// </summary>
        public void AssertFixAllParses()
        {
            if (_fixAll != null)
            {
                Assert.AreEqual(string.Empty, ParseErrors(_fixAll), "Fix All result does not parse." + Environment.NewLine + Describe());
            }
        }

        /// <summary>
        /// Asserts no fix is offered for any reported pair
        /// </summary>
        public void AssertNoFixOffered()
        {
            Assert.IsFalse(_singleFixes.Any(static obj => obj != null) || _fixAll != null, "A fix is offered." + Environment.NewLine + Describe() + Environment.NewLine + "Parse errors of single fixes: " + string.Join(" || ", _singleFixes.Where(static obj => obj != null).Select(ParseErrors)) + " / Fix All: " + (_fixAll == null ? "-" : ParseErrors(_fixAll)));
        }

        /// <summary>
        /// Asserts the Fix All result contains no generic name
        /// </summary>
        public void AssertFixAllHasNoGenericName()
        {
            Assert.IsNotNull(_fixAll, "Fix All offered nothing." + Environment.NewLine + Describe());
            Assert.IsFalse(CSharpSyntaxTree.ParseText(_fixAll).GetRoot().DescendantNodes().OfType<GenericNameSyntax>().Any(), "Fix All result contains a generic name." + Environment.NewLine + Describe());
        }

        /// <summary>
        /// Asserts a single fix result contains a text
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="expected">Expected text</param>
        public void AssertSingleFixContains(int index, string expected)
        {
            Assert.IsGreaterThan(index, _singleFixes.Count, "Not enough diagnostics." + Environment.NewLine + Describe());
            Assert.IsNotNull(_singleFixes[index], Describe());
            Assert.Contains(expected, _singleFixes[index], Describe());
        }

        /// <summary>
        /// Asserts Fix All contains a text
        /// </summary>
        /// <param name="expected">Expected text</param>
        public void AssertFixAllContains(string expected)
        {
            Assert.IsNotNull(_fixAll, Describe());
            Assert.Contains(expected, _fixAll, Describe());
        }

        /// <summary>
        /// Asserts no single fix leaves a space before a closing parenthesis or a dot
        /// </summary>
        public void AssertEverySingleFixHasNoSpaceBeforeClosingParenthesisOrDot()
        {
            Assert.IsGreaterThan(0, _spans.Count, Describe());

            foreach (var fix in _singleFixes.Where(static obj => obj != null))
            {
                Assert.IsFalse(fix.Contains("\" )") || fix.Contains("\" ."), Describe());
            }
        }

        #endregion // Methods
    }

    #endregion // Types
}