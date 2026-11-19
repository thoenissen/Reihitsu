Role: test implementer
Commit: 61883ae1
Verdict: #1865: tests written (decisions 12, 13, 14, 19 done); #1866: no work in this stage

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Worklist: decisions 12, 13, 14, 18, 19 of `00-lead.md`.

## thoenissen/Reihitsu#1865

### Decision 12 — test code over the limits

**Types over 3000 (11):** each class becomes a `partial` class; the main file keeps its constants, properties, helpers, the `BatchCodeFixTestsBase` region and the first share of the tests, and contiguous runs of test methods move verbatim into `<Class>.Part2.cs` (and `.Part3.cs` for RH3002), in a region of the same name as the one they came from (`Tests`; `Methods` for RH8201, whose tests stand in that region). The split is by size at member boundaries, not by topic: the order of the tests in these files is mostly chronological, and a topic name for a range would claim a grouping that is not there. File names follow RH4001-002 (`Type.Part1.cs`); each part is judged on its own by RH2111-004. Each part repeats the class summary; `[TestClass]` and the base class stay on the main part only.

Lines of code per part (main / part 2 / part 3): RH3002 2289/2325/2336, RH3004 2184/2236, RH2001 2620/2653, RH8201 1936/2000, RH5111 2213/2283, RH5202 1548/1571, RH7101 1528/1529, RH7309 2184/2273, RH4103 1652/1660, RH7109 2696/2743, `HoleAnchorInStatementColumnAlignmentTests` 1759/1767.

**Lead decision needed:** the maintainer rejected partial classes for `AnalyzerResources` (3C). I read that answer as scoped to that class, and chose partial parts for the test classes because a separate class per range would derive from `BatchCodeFixTestsBase` again, so it would need its own `GetFixAllScenario`, would run `FixAllCorrectsEveryDiagnostic` and `CodeFixProvidesFixAllProvider` once more, and would change every moved test's fully qualified name. If the lead reads 3C as a general rule against partial parts, these 11 classes need splitting into separate classes by topic instead; that is an output-free but larger restructuring.

**Methods over 120 (9):** the long local `const string` inputs and expected outputs move to class-level `private const string` constants with a summary (`#region Constants`, created where missing), and the method references them; the fan-out test's fixture dictionary moves to a `private static readonly Dictionary<string, string> _auditFixtures` field (`#region Fields`). Nothing else in the methods changes:

| Method | Constants |
|---|---|
| `RH5201…AnalyzerTests.VerifyMisalignedMethodChainsAreDetectedAndFixed` | `MisalignedMethodChainsTestData`, `…ResultData` |
| `RH8032…AnalyzerTests.VerifySpecificationExampleMergeQualifiersOfParameters` | `MergeQualifiersOfParametersSource` |
| `RH8033…AnalyzerTests.VerifySpecificationExampleMergeQualifiersOfTypeParameters` | `MergeQualifiersOfTypeParametersSource` |
| `BlankLineRegionTests.BlankLinesAroundRegionsWithFieldsAndConstructorsAreCorrect` | `RegionsWithFieldsAndConstructorsInput`, `…Expected` |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndEveryNamedBraceOrBracket` | `EveryNamedBraceOrBracketInput`, `…Expected` |
| `CommentBeforeBraceBlankLineTests.RemovesBlankLineBetweenCommentAndEveryBraceAndChainedKeywordOfSpecificationExample` | `SpecificationExampleInput`, `…Expected` |
| `CollectionExpressionAlignmentTests.NestedCollectionExpressionsInObjectInitializerAlignCorrectly` | `NestedCollectionExpressionsInObjectInitializerInput`, `…Expected` |
| `ObjectInitializerAlignmentTests.SwitchExpressionObjectInitializerMisalignedGetsFormatted` | `SwitchExpressionObjectInitializerMisalignedInput`, `…Expected` |
| `ElseKeywordClosingBraceAuditFanOutTests.AuditFanOutNeverGluesElseOntoClosingBrace` | `_auditFixtures` |

**How I proved that assertions, inputs, expected outputs and names are unchanged:**

1. *Type splits:* a Roslyn script (scratch, not committed) listed every member of every top-level type, from the start of its documentation comment to the end of its node, as the pair (type name, exact source text). For each of the 11 classes the sorted multiset over the original file at `HEAD` (73c7ca95) equals the multiset over the main file plus its part files after `scripts/format.sh` — byte-identical, 88 to 258 members per class. So no member's text, attributes, name, input or assertion changed; only the class header gained `partial`.
2. *Method restructures:* the same script collected every string, raw-string, interpolated-text, character and numeric literal value (`ValueText`) of each of the 8 files; the sorted lists before and after are identical (25 to 343 literals per file), so every input and expected output kept its value after re-indentation and formatting. The only code-line changes in the methods are the references: e.g. `await Verify(testData, resultData, …)` → `await Verify(MisalignedMethodChainsTestData, MisalignedMethodChainsResultData, …)`, `AssertRuleResult(input, expected)` → `AssertRuleResult(<Name>Input, <Name>Expected)`, `foreach (var fixture in fixtures)` → `foreach (var fixture in _auditFixtures)`.
3. *Test names and outcomes:* I ran every affected class (13 analyzer, 6 formatter classes) with a TRX logger before the change (Debug, 73c7ca95 tree) and after it (Release); the sorted lists of (class.method, display name incl. data rows, outcome) are identical: 1940 analyzer and 167 formatter results, all passed.

After the change the self-hosting scan's RH2008 and RH2111 rows pass (full run below).

### Decision 13 — build test reworked

`src/Reihitsu.SelfHosting.Test/Repository/BuildConfigurationTests.cs`: `RepositoryBuildPassesRootConfigurationToTheAnalyzers` is replaced by `RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers`, which asserts that no `AdditionalFiles` item of the root build files ends in `reihitsu.json`. The filter is widened from `"/reihitsu.json"` to `reihitsu.json` (any path form, e.g. `$(MSBuildThisFileDirectory)reihitsu.json`), so the absence check cannot be passed by spelling the path differently. When the follow-up rebuilds `binaries/`, this test flips back.

### Decision 14

`src/Reihitsu.Analyzer.Test/Resources/CodeFixResourcesTests.cs:19`: the summary names `CodeFixResources` instead of `AnalyzerResources`.

### Decision 19 — MSTEST0049

`src/Reihitsu.SelfHosting.Test/Analyzers/LineCountSelfHostingTests.cs` (`RepositoryConfigurationIsPassedAndValid`): MSTEST0049 asked for the `WithAnalyzers` overload with a `CancellationToken`, which Roslyn marks obsolete (CS0618). The test now builds `new CompilationWithAnalyzers(compilation, analyzers, new CompilationWithAnalyzersOptions(options, …))`, which has no token overload, and passes `TestContext.CancellationToken` to `GetAnalyzerDiagnosticsAsync` as before. Same analyzer, options and assertions. `scripts/build.sh` (Release): 0 errors, 0 warnings.

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `BuildConfigurationTests.RepositoryBuildDoesNotPassRootConfigurationToTheBinaryAnalyzers` | — (decision 13) | — | passes |
| `LineCountSelfHostingTests.RepositoryConfigurationIsPassedAndValid` (call shape only) | — | — | passes |
| `AnalyzerSelfHostingTests.AnalyzerReportsNoViolationsOnOwnSourceCode` RH2008, RH2111 rows (unchanged; decision 12 makes them pass) | — | `Collect` | passes |
| the 1940 + 167 moved/restructured tests | unchanged tags | unchanged | pass, identical to before |

### Not covered, and why

No production behavior changes in this stage; no failure class beyond FC-017 (not weakening: no assertion changed, proven above) applies.

### Commands

```shell
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~BuildConfigurationTests|FullyQualifiedName~LineCountSelfHostingTests"
scripts/test.sh --project selfhosting --filter "FullyQualifiedName~AnalyzerSelfHostingTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH3002StatementMustNotUseUnnecessaryParenthesesAnalyzerTests|FullyQualifiedName~RH7109ReadonlyElementsMustAppearBeforeNonReadonlyElementsAnalyzerTests"
```

Runs at this tree (Release): `--project selfhosting` Failed 1, Passed 6029 — only `WorkingDocumentsTests.WorkingDocumentsDirectoryContainsNoFiles` (`sdd/work/` exists). `--project architecture` Passed 198.

### Follow-up candidates

_None._

## Decision 18 — lane classification of the root `reihitsu.json` (tooling, for #1865's lane check)

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LaneClassifierTests.ClassifyPathAssignsTheCategoryOfEachKindOfPath("reihitsu.json", Tooling)` | FC-003 (refused side) | — | **fails** on this head: classified `Unknown` |
| `…("samples/reihitsu.json", Unknown)` — a `reihitsu.json` outside the root and outside `src/` stays unclassified | FC-003 (other side) | — | passes |
| `…("src/Reihitsu.Cli.Test/Fixtures/reihitsu.json", Test)` — one inside a test project stays a test file | FC-003 (other side) | — | passes |

`scripts/test.sh --project tooling --filter "FullyQualifiedName~LaneClassifierTests"`: Failed 1 (the root row), Passed 68. For the implementer: `LaneClassifier` and the classification table in `scripts/README.md` (line 145).

## thoenissen/Reihitsu#1866

No work in this stage.
