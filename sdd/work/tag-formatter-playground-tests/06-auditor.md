Role: auditor
Commit: a6bd8e6
Verdict: findings

## Scope

Full audit of head a6bd8e6 (branch `claude/stoic-hawking-udrgwu`) against base `origin/main` d70f487601a974c5e6b8c9f74b37663897a8f6b0. `git ls-remote origin refs/heads/main` returns d70f487, it is an ancestor of HEAD (merged by a6bd8e6), and the working tree is clean. The diff is what the handovers describe: tags and new tests in `Reihitsu.Formatter.Test` and `Reihitsu.Playground.Test`, two `RequirementAttribute` copies, the bUnit reference, `EnforcedSpecPaths` plus the moved report fixture in `RequirementCoverageTests`, `squad/decisions/2026-10-22-939-playground-component-tests.md`, and `sdd/work/`. No production file, spec, or change doc changed. `scripts/lane-check.sh tooling`: `confirmed — claimed tooling, diff needs tooling`. The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Issue thoenissen/Reihitsu#939 "Done when": `RequirementCoverageTests` at a6bd8e6 (rerun because the merge of main came after every role's run, FC-032) passes 13 of 13, and `ReportCoverageOfSpecsNotYetEnforced` prints an empty list, so no active requirement of any spec, including every `shared/` and `cli/` spec, is uncovered. "Contains no `sdd/work/`" is open until the lead deletes the directory before publishing.

Probes ran in a scratch worktree of a6bd8e6 (removed afterwards); nothing in the repository was changed by them.

## Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| 1 | medium | `src/Reihitsu.Formatter.Test/Unit/Pipeline/PipelineInvariantTests.cs`, `CollectUserAuthoredContent` / `AddTriviaContent` (used by `EveryCommentDirectiveDisabledTextAndLiteralValueSurvives`, FMT-PIPE-002) | FC-017, FC-033 | PR-introduced | FMT-PIPE-002 requires the *text* of every literal to survive; the helper records `token.ValueText`, which for a numeric or char literal is its value. Counterexample: a mutant `CleanupPhase` that replaces every `int` `NumericLiteralToken` by `SyntaxFactory.Literal(ValueText, value)` turns `var n=0x1F+1_000;` into `var n = 31 + 1000;` (confirmed with `scripts/format.sh`), and all three `PipelineInvariantTests` still pass. The test's own input contains `0x1F+1_000+1.5e3` and `'\t'`, whose text no tagged test asserts. The helper also rewrites `\r\n` to `\n` inside disabled text before comparing, although FMT-PIPE-007 says disabled text keeps its line endings, so a phase that changed them under CRLF would also pass. Probe: comparing `token.Text` for every literal except `MultiLineRawStringLiteralToken` and dropping the disabled-text normalization passes on the current code. | Compare `token.Text` for every literal token, and fall back to `ValueText` only for the content of multi-line raw literals (plain, `u8`, interpolated), whose indentation FMT-RAW may shift; compare disabled text unnormalized. |
| 2 | medium | `src/Reihitsu.Playground.Test/PlaygroundFormatterTests.cs`: `FormatUsesTheDefaultLanguageVersion`, `FormatEqualsTreeLevelFormattingWithDefaultParseOptions` (PLAY-005) | FC-017 | PR-introduced | PLAY-005 requires Roslyn's default parse options — the default language version (C# 14 through FMT-PIPE-008). Both inputs only separate "C# 6 or later" from older versions (`int P => 1;`). Counterexample: changing `PlaygroundFormatter.Format` to `CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.CSharp10))` passes all 17 `Reihitsu.Playground.Test` tests (mutation run). | Add a construct whose output depends on a recent version, for example an empty class, which becomes `class E;` only from C# 12 (FMT-STRUCT-017), to the language-version test or to the tree-level comparison input. |
| 3 | medium | `src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterEntryPointTests.cs`: `FormatNodeTakesTheIndentationFromTheIndentLevelArgument` (FMT-PIPE-012) | FC-003 | PR-introduced | FMT-PIPE-012 takes the level from `indentLevel` "when that is zero or greater". The tests pass `1` and the default `-1`; the only `indentLevel: 0` call (`FormatNodeRemovesTrailingLineBreaksOfDetachedNode`) is on a detached node, whose position also gives level 0. Counterexample: changing `indentLevel >= 0` to `indentLevel > 0` in `ReihitsuFormatter.FormatNode` (line 122) passes every test tagged FMT-PIPE-012 or FMT-CLEAN-008 (6 of 6, mutation run). | Also format the nested `if` with `indentLevel: 0` and expect it at column 0. |
| 4 | low | `sdd/README.md`, *Tests and requirements*, lines 76 and 79 at a6bd8e6 | — | PR-introduced | Still says the attribute is declared in `Reihitsu.Analyzer.Test`, `Reihitsu.Cli.Test`, and `Reihitsu.Core.Test`, and lists "Enforced today: `analyzer/`, `cli/`, `shared/code-fixes.md`, `shared/diagnostics.md`". The diff adds the attribute to two more projects and enforces `formatter/` and `playground/`. This is spec checker finding 1 in `05-spec-checker.md`, unchanged by the merge of main. Documentation only; it cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. | Name the five declaring projects (or "every test project that tags tests") and add `formatter/` and `playground/` to the enforced list. Routing is the lead's (other squads edit the file). |

## Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which specs must be fully covered (`EnforcedSpecPaths` gains `formatter/`, `playground/`) | Spec paths under `sdd/specs`, plus `shared/` requirements an enforced spec lists under `Uses` (`IsEnforced`) | `sdd/README.md` "Enforced today" list | Code and test agree; the README does not | Finding 4 |
| Requirement discovery in the two new test projects | `RequirementReferenceCatalog`: sources of directories ending in `.Test`/`Tests`, attribute by simple name on a `[TestMethod]` | The five `RequirementAttribute` declarations (namespace irrelevant) | Agree; prelude probe found tags in both projects, and coverage passes at head | ok |
| Report fixture of a not-yet-enforced spec (`CoverageReportCountsUncoveredRequirementsPerSpecNotYetEnforced`) | Fixture repository with `shared/casing.md` and `cli/reihitsu-format.md` | `EnforcedSpecPaths` (real list) | `shared/casing.md` is not on the list and no fixture spec uses it; assertion unchanged | ok (see hints) |
| Literal/disabled-text collection in `PipelineInvariantTests` | `LiteralExpressionSyntax` tokens and `InterpolatedStringTextToken` by `ValueText`; disabled text `\r\n`-normalized | FMT-PIPE-002 "text of every … literal"; FMT-PIPE-007 disabled-text line endings | Span narrower than the requirement | Finding 1 |
| No production predicate, guard, or exemption is added or changed | — | — | — | N/A |

## Policy ownership

| Policy | Owners found by rg | Verdict |
|---|---|---|
| `[Requirement]` attribute declaration | `src/Reihitsu.Analyzer.Test/Base/RequirementAttribute.cs`, `src/Reihitsu.Cli.Test/Helpers/RequirementAttribute.cs`, `src/Reihitsu.Core.Test/RequirementAttribute.cs`, `src/Reihitsu.Formatter.Test/Helpers/RequirementAttribute.cs`, `src/Reihitsu.Playground.Test/RequirementAttribute.cs` — identical except the namespace (`diff`) | One copy per test project is the rule of `sdd/README.md` and the maintainer's instruction; parity proven by `diff`. ok |
| Enforced spec list | `RequirementCoverageTests.EnforcedSpecPaths` (code); `sdd/README.md` line 79 (prose copy) | Prose copy drifted — finding 4 |
| Full-pipeline expectation with LF/CRLF and second pass | `FormatterTestsBase.AssertRuleResult` (pipeline, line ending passed in, re-parsed second pass); `ReihitsuFormatterLineEndingTests.AssertFormatted` (entry point, detected line ending, re-parsed second pass) | Different purposes: the second exists because `AssertRuleResult` bypasses detection and the FMT-PIPE-005/006 guards. Justified. ok |
| Deterministic compilation in tests | `PipelineInvariantTests.Compile`; other `TRUSTED_PLATFORM_ASSEMBLIES` users are in `Reihitsu.Analyzer.Test` and `Reihitsu.Tooling` | Separate projects, no shared helper to reuse. ok |

## Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PipelineInvariantTests.EveryCommentDirectiveDisabledTextAndLiteralValueSurvives` | Every comment, directive, disabled text, literal survives | A changed comment/directive/disabled text or string value is caught; a changed numeric/char literal text is not | `ReihitsuFormatter.FormatSyntaxTree`, `AreSequenceEqual` | Weak — finding 1 |
| `PipelineInvariantTests.RewrittenCodeCompilesToTheSameAssembly` | Formatted code emits identical bytes | Any IL/metadata difference; preconditions assert that the property, field split, and using order were rewritten | Deterministic Release emit, LF and CRLF | ok |
| `PipelineInvariantTests.CommentsDisabledTextAndLiteralsKeepTheirText` | Spec example | Exact output | `AssertRuleResult` | ok |
| `ReihitsuFormatterEntryPointTests` FMT-PIPE-005/006 | Tree returned unchanged / header marker only | `AreSame(tree, result)`; exact output for markers outside the header | Entry point | ok |
| `ReihitsuFormatterEntryPointTests` FMT-PIPE-007 | Majority, tie to `\r\n`, documentation newlines counted, `Environment.NewLine` fallback | Exact bytes; the documentation test is 3:3 only when documentation newlines count, 3:0 for `\n` otherwise | Entry point | ok |
| `ReihitsuFormatterEntryPointTests.FormatDocumentAsyncAndFormatSyntaxTreeProduceTheSameText` | Tree and document text equal for C# 5, 11, latest | Any difference; asserts the input was reformatted | `AdhocWorkspace` with project parse options | ok |
| `ReihitsuFormatterEntryPointTests` FMT-PIPE-010/011 | Only the target is rewritten, trailing trivia kept, accessor shape kept | Exact document text, LF and CRLF | `FormatNodeInDocumentAsync` | ok |
| `ReihitsuFormatterEntryPointTests.FormatNodeTakesTheIndentationFromTheIndentLevelArgument` | `indentLevel >= 0` wins over position | `1` vs position tested; `0` not | `FormatNode` | Weak — finding 3 |
| `ReihitsuFormatterLineEndingTests` (FMT-EOL-001…004) | Existing line breaks normalized; literal line breaks kept | Exact escaped bytes plus a re-parsed second pass; majority independent of how lone `\r`/Unicode terminators count | Own `AssertFormatted` through `FormatSyntaxTree` | ok |
| `RawStringAlignmentPipelineTests`, `WhitespaceCleanupTests`, `ScopeIndentationTests`, `LineStartIndentationTests`, `ListAndHeaderAlignmentTests`, `ExpressionAndPatternAlignmentTests`, `LineBreakRequirementTests`, `SwitchCaseBraceRequirementTests`, `HorizontalSpacingRequirementTests`, `StructuralTransformRequirementTests`, `BlankLineRequirementTests`, `DocumentationCommentRequirementTests`, `RegionFormattingRequirementTests`, `UsingDirectiveRequirementTests` | Spec examples and both sides of named guards | Exact output, LF and CRLF, re-parsed second pass; every "unchanged" test sampled has a transforming counterpart (for example `DeclarationSemicolonsJoinTheDeclarationLine` vs the comment/directive variants, `GotoIntoAnotherSectionLeavesTheSwitchUnbraced` with a multi-line section, `LabelsInsideLambdasDoNotBlockTheBraces` fails if the lambda exclusion in `HasCrossSectionGotoTarget` is dropped) | `AssertRuleResult` | ok; probe: no tagged `AssertRuleResult` input has an error-level parse diagnostic and no output fails to parse (run over the 1152 formatter tests tagged FMT- or SHARED-) |
| `NodeLevelStructuralTransformTests` (FMT-STRUCT-016) | Non-accessor transforms apply through `FormatNode` and `FormatNodeInDocumentAsync` | Exact output on both entry points, LF and CRLF (FC-028) | Entry points | ok |
| `CleanupPhaseTests.RemovesTrailingWhitespaceBeforeLineBreakInFollowingTokenLeadingTrivia` | Second clause of FMT-CLEAN-001 | Exact phase output; the arrange step asserts the trivia shape it builds | `CleanupPhase.Execute` | ok |
| `HomePageTests`, `MainLayoutTests` (PLAY-001…004, 006, 007) | Rendered texts, attributes, order, format-on-click only, message lifecycle, version, link | Exact texts and attributes; strict JS interop with no services; output unchanged after typing | bUnit | ok ("side by side" is CSS and not observable in bUnit) |
| `PlaygroundFormatterTests` PLAY-005 | Default options, no symbols, line ending, idempotency | Symbols, CRLF, and idempotency are falsifiable; the language version is not | `PlaygroundFormatter.Format` | Weak — finding 2 |
| `RequirementCoverageTests.CoverageReportCountsUncoveredRequirementsPerSpecNotYetEnforced` | Report counts uncovered requirements of non-enforced specs, omits enforced ones | Exact report line | Fixture repository | ok |
| Tags on existing tests (1215 tags) | Each tagged test proves its ID | Scan for tagged tests without an exact assertion: the remainder are predicate or contributor unit tests next to exact pipeline tests of the same ID, phase-level `Assert.AreEqual` helpers, or structural asserts (`FormatNodeKeeps*`, FMT-PIPE-011); the skeptics' lists of disputed tests carry no tag | — | ok (see hints for FMT-PIPE-004 and FMT-INDENT-026) |

## Failure classes dismissed

- FC-001, FC-002, FC-005, FC-015 (production), FC-016: no production guard or predicate changed; the test-side guard scope is in the table above.
- FC-004, FC-019, FC-020, FC-021: no edit joins or moves trivia. The new tests include comment, directive, and disabled-text gaps where the requirements name them (FMT-BREAK-001/009, FMT-SWITCH-010 active and inactive, FMT-STRUCT-003/018, FMT-BLANK-005/006/015); inactive directives at line start are avoided because Q-I6 disputes them.
- FC-006, FC-022 to FC-027, FC-029, FC-036: no rewrite is introduced. Literal shapes in new tests (verbatim, interpolated verbatim, raw, `u8` raw, interpolated raw) are covered as inputs; FC-022 for FMT-PIPE-002 is finding 1.
- FC-007, FC-008, FC-009, FC-018, FC-030: no analyzer or code fix in the diff.
- FC-010, FC-011, FC-012: every new pipeline test runs LF and CRLF with a re-parsed second pass; no loop or phase changed.
- FC-013, FC-014: no analyzer/formatter surface changed.
- FC-028: FMT-STRUCT-016 and FMT-PIPE-010/011 tests use both node-level entry points.
- FC-031: the summaries of the changed and new methods describe their assertions; `RequirementCoverageTests` class and field comments name no areas. No `TODO`, no commented-out code, no tracker numbers in added source lines (`git diff -U0` grep).
- FC-032: coverage rerun at a6bd8e6 after the merge of main (13 of 13). The merge touched only analyzer tests and `shared/code-fixes.md`/`pipeline.md` text.
- FC-034: no reproduction; FC-035: expected values are spec examples or characterizations of unchanged code, and every new test's expectation sampled against its requirement text matches it; FC-037: no text-only claim is made.

## Hints

- `IdempotencyTests.AssertIdempotentUnderBothEndings` and `SelfHostingTests.FormatterIsIdempotentOnAllSourceFiles`, the only FMT-PIPE-004 tags, format the first pass's tree object instead of its re-parsed text, and `RecursivePatternLayoutIsIdempotent` and `UnbracedEmbeddedStatementIsIdempotent` have inputs the first pass leaves unchanged. Pre-existing helpers; a probe with a re-parsed second pass passes all 20, so nothing is masked today. Every `AssertRuleResult` test re-parses.
- Pre-existing FMT-PIPE-004 violation, now inside an enforced spec: `R P => _r with { A = 1, B = 2 };` formats to `R P => _r with {` with wrapped members, and a second `scripts/format.sh` pass moves `{` onto its own line. Reproduced at a6bd8e6. Skeptic C routed it to the lead; `00-lead.md` records no issue or comment for it, and a search of the tracker found none. Follow-up candidate for the maintainer.
- `RecursivePatternAlignmentTests.ParenthesizedRecursivePatternBreaksParenthesisOntoOwnLine` carries FMT-INDENT-026, but its input `value is ({` depends on the LB-Q7-disputed move of `(` onto its own line; the FMT-INDENT-026 spec example starts with `(` already on its own line. The tag will need to move if LB-Q7 is decided against today's output.
- `CoverageReportCountsUncoveredRequirementsPerSpecNotYetEnforced` uses a real spec path that is not enforced yet (`shared/casing.md`); it will need the same fixture move again when `shared/` is enforced.
- Stale comments in untagged existing tests, already noted by the roles: `ObjectInitializerContributorTests.AlignsCollectionInitializerInsideObjectInitializer` and `SwitchCaseBraceRewriterTests.AddsBracesToTypePatternCaseWithMultipleStatements`.

## Repair round

Role: auditor
Commit: f5719ee
Verdict: pass, text cleanup

### Scope

Repair delta against f88035c: 963611e (test implementer), 022968c (cleaner), 7a89e04 and f5719ee (spec checker). Head f5719ee, working tree clean; `git ls-remote origin refs/heads/main` is still d70f487, which is merged. The repair changes three test files (`PipelineInvariantTests.cs`, `ReihitsuFormatterEntryPointTests.cs`, `PlaygroundFormatterTests.cs`) and three handovers. No production file, spec, project file, or `[Requirement]` line changed (`git diff -U0` shows no added or removed tag). `scripts/lane-check.sh tooling`: confirmed. Scope did not grow, so this is a delta audit. The coordinator's message carried no intended fix or hoped-for verdict.

Each finding was checked against the mutant that falsified it in the first audit. The three mutants ran together in a scratch worktree of f5719ee, which was removed afterwards: the numeric-literal `CleanupPhase` mutant, `indentLevel > 0` in `ReihitsuFormatter.FormatNode`, and `PlaygroundFormatter` parsing with `LanguageVersion.CSharp13`, a version closer than the original C# 10. Each failure message names its own mutant.

### Previous findings

| # | Status | Evidence |
|---|---|---|
| 1 | closed | `CollectUserAuthoredContent` now records `token.Text` for every literal token and `ValueText` only for multi-line raw content (`IsMultiLineRawStringContent`: `MultiLineRawStringLiteralToken`, `Utf8MultiLineRawStringLiteralToken`, and text tokens of the nearest `InterpolatedStringExpressionSyntax` whose start token is `InterpolatedMultiLineRawStringStartToken`, so a regular interpolated string nested in a raw hole still compares text). Disabled text is compared as written. The input gains an interpolated and a `u8` multi-line raw literal. Against the numeric-literal mutant, `EveryCommentDirectiveDisabledTextAndLiteralTextSurvives` fails: 2 of 23 elements differ, starting at `literal:0x1F`. The raw-content fallback is necessary, because the interpolated raw string's leading indentation token records as `raw:` (empty value) while its text moves with FMT-RAW. |
| 2 | closed | `FormatUsesTheDefaultLanguageVersion` asserts `Formatted` and the exact text for an extension block (C# 14), an empty class (`class Empty;`, C# 12), and a get-only property (C# 6). `FormatEqualsTreeLevelFormattingWithDefaultParseOptions` gains an empty class and a single-line extension block. With the C# 13 mutant both fail: "C# 14 input must not be reported as a syntax error" and `Formatted` false. The exact-text test writes the extension block in Allman layout, so it does not pin LB-Q1, which disputes brace placement only when `{` shares the header line. The equality test contains the LB-Q1 shape but compares against the formatter's own output, so it holds however LB-Q1 is decided. |
| 3 | closed | `FormatNodeTakesTheIndentationFromTheIndentLevelArgument` adds `indentLevel: 0` on the nested `if` and expects column 0. With the `> 0` mutant it fails: the expected text is at column 0, the actual at column 8. |
| 4 | open | `sdd/README.md` lines 76 and 79 are unchanged. The lead routes this to the squad that owns the file, as a follow-up. It is documentation only: it cannot reach compiled behavior, diagnostics, public API documentation, or rule metadata. It is the only open finding, hence `pass, text cleanup`. |

### Findings

| # | Severity | Location | Failure class | Scope relation | What goes wrong | Suggested change |
|---|---|---|---|---|---|---|
| _None new._ | | | | | | |

### Guard scope

| Decision | Inspected span | Counterpart predicate | Agreement | Verdict |
|---|---|---|---|---|
| Which literal tokens may change text (`IsMultiLineRawStringContent`) | Multi-line raw tokens (plain, `u8`) and the text tokens of the nearest enclosing interpolated string with a multi-line raw start | FMT-RAW-001/002/004: only multi-line raw literals are re-aligned. Single-line raw (`"""  single  """u8`) keeps text comparison | Agree; single-line and nested regular interpolated strings stay on text comparison | ok |

### Policy ownership

_N/A — the repair adds no policy; `IsMultiLineRawStringContent` is a private test helper with no counterpart in test code. Production decides raw-string alignment by token kind in `RawStringAlignmentPhase`, and this helper's kind list matches it._

### Assertion adequacy

| Test | Invariant | Falsifying observation | Helper | Verdict |
|---|---|---|---|---|
| `PipelineInvariantTests.EveryCommentDirectiveDisabledTextAndLiteralTextSurvives` | Text of every comment, directive, disabled text, and non-raw literal; value of multi-line raw content | Numeric-literal mutant fails; the test implementer reports a CRLF→LF disabled-text mutant failing too | `FormatSyntaxTree`, `AreSequenceEqual`, LF and CRLF | ok |
| `PlaygroundFormatterTests.FormatUsesTheDefaultLanguageVersion` | Default language version (C# 14) | C# 13 mutant fails; C# 10 too per the test implementer | `PlaygroundFormatter.Format`, exact text | ok; no formatter rule separates C# 14 from a preview version, so this is the tightest pin available |
| `PlaygroundFormatterTests.FormatEqualsTreeLevelFormattingWithDefaultParseOptions` | Playground output equals tree-level output with default options | C# 13 mutant fails | Formatter-computed oracle, as PLAY-005 defines it | ok |
| `ReihitsuFormatterEntryPointTests.FormatNodeTakesTheIndentationFromTheIndentLevelArgument` | `indentLevel` 0 and 1 win over position 2 | `> 0` mutant fails | `FormatNode` | ok |

### Comments and naming

The renamed test and both changed summaries describe the new assertions (FC-031). The new helper's summary states its kind list and why. No `TODO` or tracker number appears in the added lines. The old name `EveryCommentDirectiveDisabledTextAndLiteralValueSurvives` survives only in `sdd/work/` handovers.

### Failure classes dismissed

All classes dismissed in the first audit stay dismissed for the same reasons. The repair touches no production code and no guard, and it moves no tag. FC-003, FC-017, and FC-033 were checked above by mutation.

### Hints

- Correction to the first audit's second hint: the `with` initializer non-idempotency was already filed before that audit, as Q-P2, a comment dated 2026-10-21 on thoenissen/Reihitsu#1366. `00-lead.md` records it as "Q-P2 as comment". The claim that it was not recorded was wrong.
- The other first-audit hints are unchanged and remain non-blocking.
