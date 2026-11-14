Role: test implementer
Commit: 53f6660
Verdict: #1600: tests written; #1150: tests written; #1274: reproduced; #1143: reproduced; #1673: reproduced; #1678: no scenario (no behavior change, Q5 A)

My prompt carried no intended fix, suspected cause, or hoped-for verdict; it named the requirements to cover, which I took from the accepted specs and change doc. Expected values come from the spec examples; where a spec gives an input but no output, I used the observed output of unchanged code that the run does not change (noted per test).

"Fails today" means: run on 7a6ec68 plus these tests, the test fails on the assertion it is about (fix offered where it must not be, document formatted where it must be left alone, Fix All needs a second pass). Every failing test was checked to fail for that reason, not on a compile error or unrelated layout.

## thoenissen/Reihitsu#1600

### Tests

`src/Reihitsu.Formatter.Test/Unit/ReihitsuFormatterHiddenCodeTests.cs` (new). Helper `AssertLeftAlone` checks `FormatSyntaxTree` and `FormatDocumentAsync`, LF and CRLF, output unchanged and `Unsupported` null. The "formatted" side goes through `ReihitsuFormatter.FormatSyntaxTree`, not `AssertRuleResult`, because `AssertRuleResult` calls `FormattingPipeline.Execute` and bypasses the entry-point checks.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleCommentThatIsCodeInAnotherBuildLeavesTheDocumentUnchanged | FMT-PIPE-033 | AssertLeftAlone | fails |
| SpecExampleCommentOutsideEveryGroupAndMarkdownHeadingAreFormatted | FMT-PIPE-033 | FormatSyntaxTree + AssertRuleResult | passes |
| SpecExampleStringOpenedOnlyByAnExcludedBranchIsADecidedGap | FMT-PIPE-033 | FormatSyntaxTree | passes |
| EveryKindOfCarrierLeavesTheDocumentUnchanged (8 rows: verbatim, raw, interpolated verbatim with `#else` behind a hole, interpolated raw behind a hole, `@""u8`, `""""""u8`, `/* */`, `/** */`) | FMT-PIPE-033 | AssertLeftAlone | fails (all rows) |
| EveryLineKindThatEndsTheGroupCounts (`#elif Y`, `#endif`, `#  else`, indented `#else`) | FMT-PIPE-033 | AssertLeftAlone | fails (all rows) |
| CarrierWithoutALineThatEndsTheGroupIsFormatted (`# Title`, `#if`, `#region`, `#nullable`, ` * #else`, `#else` on the first line, `///`, `//`) | FMT-PIPE-033 | FormatSyntaxTree | passes |
| CarrierOutsideEveryGroupIsFormatted (no group; behind an ended group) | FMT-PIPE-033 | FormatSyntaxTree | passes |
| CarrierBehindAnElseDirectiveLeavesTheDocumentUnchanged | FMT-PIPE-033 | AssertLeftAlone | fails |
| CarrierInABranchThatADefinedSymbolActivatesLeavesTheDocumentUnchanged (`#if A`, `A` defined) | FMT-PIPE-033 | AssertLeftAlone | fails |
| CarrierWhoseDirectiveLinesCloseOnlyItsOwnGroupsCounts (04-skeptic's two balanced inputs) | FMT-PIPE-033 | AssertLeftAlone | fails |
| CarrierBesideAnUnsupportedShapeIsNotReportedAsUnsupported (FMT-UNSUP-004 shape; FMT-UNSUP-008 shape) | FMT-PIPE-033 | AssertLeftAlone | fails |
| CarrierBesideASyntaxErrorIsReturnedUnchanged | FMT-PIPE-033 | FormatSyntaxTree | passes |
| NodeLevelEntryPointsDoNotCheckForACarrier (`FormatNodeInDocumentAsync`, `FormatNode` format the method) | FMT-PIPE-033 | direct | passes |

Analyzer side (SHARED-FIX-012):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3004…SpecExampleNoFixIsOfferedInADocumentWithAHidingCarrier (LF, CRLF, `A` defined) | SHARED-FIX-012 | VerifyNoCodeFixIsOffered | fails |
| RH6024…SpecExampleNoFixIsOfferedInADocumentWithAHidingCarrier (LF, CRLF, `A` defined) | SHARED-FIX-012 | VerifyNoCodeFixIsOffered | fails |
| RH6024…NoFixIsOfferedForAnEditInFrontOfTheHidingCarrier | SHARED-FIX-012 | VerifyNoCodeFixIsOffered | fails |
| RH6024…NoFixIsOfferedForEveryKindOfHidingCarrier (verbatim/`#else`, raw/`#endif`, `/** */`/`#elif`) | SHARED-FIX-012 | VerifyNoCodeFixIsOffered | fails (all rows) |
| RH6024…SpecExampleFixIsOfferedForACommentOutsideEveryGroup | SHARED-FIX-012 | Verify | passes |
| RH6024…FixIsOfferedForACarrierWithoutALineThatEndsTheGroup | SHARED-FIX-012 | Verify | passes |
| RH6024…SpecExampleFixIsOfferedForAStringThatOnlyAnExcludedBranchOpens | SHARED-FIX-012 | Verify | passes |
| RH6024…FixAllLeavesADocumentWithAHidingCarrierUnchanged (two documents, BatchFixer in document/project/solution) | SHARED-FIX-012 | Verify, multi-document | fails |
| RH4103…NoRenameIsOfferedWhenAnotherDocumentItChangesHoldsAHidingCarrier (casing base; reference behind the carrier in a second document) | SHARED-FIX-012 | Verify (fixed = test state) | fails |
| RH4103…FixAllLeavesADocumentWithAHidingCarrierUnchanged (casing `CasingFixAllProvider`, which bypasses `RegisterCodeFixesAsync`; 04-skeptic item 4) | SHARED-FIX-012 | Verify, multi-document | fails |
| RH2001…NoFixIsOfferedWhenAPartialPartInAnotherDocumentHoldsAHidingCarrier (Renamer into a partial part; 04-skeptic item 5) | SHARED-FIX-012 | Verify (fixed = test state) | fails |

CLI (`src/Reihitsu.Cli.Test/EndToEnd/CliHiddenCodeEndToEndTests.cs`, new, real formatter through `Program.Main`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleCheckVerboseNamesTheFileAsSkippedAndSucceeds | CLI-021 | Program.Main | fails |
| CheckWithoutVerbosePrintsOnlyTheSummary | CLI-021 | Program.Main | fails |
| SpecExampleWriteModeLeavesTheFileUnchanged | CLI-021 | Program.Main | fails |
| Utf8BomDoesNotRewriteTheSkippedFile (content already formatted, so only the BOM could change) | CLI-021 | Program.Main | fails |
| DryRunDoesNotCountTheSkippedFileAsWouldBeFormatted | CLI-021 | Program.Main | fails |
| CheckExitCodeFollowsTheOtherFiles | CLI-021 | Program.Main | fails |
| CarrierBesideAnUnsupportedShapeIsSkippedWithoutAnError | CLI-021 | Program.Main | fails |
| CarrierBesideASyntaxErrorIsSkippedForTheSyntaxError | CLI-021 | Program.Main | passes |

Playground (`src/Reihitsu.Playground.Test/HomePageTests.cs`, bUnit through the page):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleInputWithHiddenCodeIsShownUnchangedWithMessage | PLAY-009 | bUnit | fails |
| HiddenCodeMessageDisappearsOnNextSuccessfulFormat | PLAY-009 | bUnit | fails |
| HiddenCodeBesideAnUnsupportedShapeShowsTheHiddenCodeMessage | PLAY-009 | bUnit | fails |
| HiddenCodeBesideASyntaxErrorShowsTheSyntaxErrorMessage | PLAY-009 | bUnit | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| Issue input, `A` defined, RH3004 | LF, CRLF | code fix | reproduced | fix offered vs not offered |
| Mirrored `#if !A` input | LF, CRLF | whole-document formatter | reproduced | block re-indented vs input unchanged |
| Same input | — | CLI, Playground | reproduced | formatted vs skipped with message |
| RH6024 edit in front of / behind the carrier | LF (front), LF+CRLF (behind) | code fix | reproduced | fix offered vs not offered |

### Not covered, and why

- A Fix All of a `DocumentOrderFixAllProvider` rule over a carrier document: that provider re-registers through `RegisterCodeFixesAsync`, the same path the BatchFixer test covers (04-skeptic, #1143 section). No separate test.
- The CLI unit tests with a mocked `ISourceFormatter`: CLI-021 requires the CLI to decide by FMT-PIPE-033's definition, and the mock cannot say how the implementer exposes it, so I used end-to-end tests only.
- `PlaygroundFormatterTests`: `PlaygroundFormatResult`'s shape for this outcome is the implementer's choice; the page tests check the visible behavior.
- Per-tree cost cache (04-skeptic item 6): implementation note, nothing to assert.

## thoenissen/Reihitsu#1150

### Tests

New helper `VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded(source)` in `AnalyzerTestsBase<TAnalyzer, TCodeFix>`. As a control, it checks that the fix is offered on the valid source. Then it inserts `    void N(` in front of the source's last `}` and checks that the analyzer still reports and that no action is registered for any diagnostic. The source of each test is the class's own Fix All scenario, with its markup removed.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `NoFixIsOfferedInAFileWithASyntaxError` in RH5412 (base of RH5412–RH5416), RH3205 (base of RH3205–RH3207), RH3203, RH3208, RH3202, RH5417, RH3004, RH2001, RH5101, RH5102, RH5110, RH5111, RH5113, RH5205, RH5206, RH5301, RH5302, RH5303, RH5304, RH5305, RH5306, RH5307, RH5401, RH5403, RH5404, RH5408, RH5112, RH5201 (28 classes) | SHARED-FIX-011 | VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded | fails (all 28: fix offered in the syntax-error file; the analyzer still reports in each) |
| RH5308…SpecExampleNoFixIsOfferedInAFileWithASyntaxError | SHARED-FIX-011 | VerifyNoCodeFixIsOffered, `CompilerDiagnostics.None` | fails |
| RH3004…SpecExampleEditThenFormatFixIsNotOfferedInAFileWithASyntaxError (`Action a = delegate () { M(); };`) | SHARED-FIX-011 | VerifyNoCodeFixIsOfferedOnceASyntaxErrorIsAdded | fails |
| RH6023…FixIsStillOfferedInAFileWithASyntaxError (control: text-edit fix writes `a = 1;`) | — (untagged, per 02-skeptic) | TryApplyCodeFixAsync | passes |

The "fix offered once the syntax error is gone" half of the SHARED-FIX-011 example for RH5308 is covered by the existing RH5308-004 tests and by the helper's control step.

### Not covered, and why

- RH5401-004 against SHARED-FIX-011's third sentence: a follow-up in `00-lead.md`, outside the run.

## thoenissen/Reihitsu#1274

### Tests

New: `src/Reihitsu.Formatter.Test/Unit/Precheck/UnsupportedConditionalOperatorCommentTests.cs`. `FormatterTestsBase` gains `ConditionalOperatorCommentReason` and `AssertUnsupportedFor(reason, …)`; `AssertUnsupported` now delegates to it with the FMT-UNSUP-004 reason, so its behavior is unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleCommentsBehindEitherOperatorAreUnsupported | FMT-UNSUP-008 | AssertUnsupportedFor | fails |
| EachCommentBetweenAnOperatorAndItsBranchIsUnsupported (7 rows: `? //`, `: //`, `? /* */ 1`, `: /* */ 2`, comment line before the true branch, comment line before the false branch, `?/* c7 */1`) | FMT-UNSUP-008 | AssertUnsupportedFor | fails (all rows) |
| InlineCommentsWithTheBranchOnTheOperatorsLineAreUnsupported (Q9 A input, location 5,21) | FMT-UNSUP-008 | AssertUnsupportedFor | fails |
| SpecExampleDirectiveAndDisabledTextBehindTheQuestionMarkAreUnsupported | FMT-UNSUP-008 | AssertUnsupportedFor | fails |
| DirectiveBehindTheColonIsUnsupported (`#pragma`) | FMT-UNSUP-008 | AssertUnsupportedFor | fails |
| DocumentationCommentBehindAnOperatorIsUnsupportedInEveryDocumentationMode (`/** */` and `///`, Parse/None/Diagnose) | FMT-UNSUP-008 | AssertUnsupportedFor | fails (all 6) |
| FirstOfTwoCommentsIsReported | FMT-UNSUP-008 | AssertUnsupportedFor | fails |
| NestedConditionalIsCheckedWithItsOwnOperators (nested true/false branch, lambda in a branch, interpolation hole) | FMT-UNSUP-008 | AssertUnsupportedFor | fails (all rows) |
| SpecExampleCommentsInFrontOfTheOperatorsAreSupported | FMT-UNSUP-008 | AssertSupported | passes |
| CommentOutsideTheShapeIsSupported (behind condition, behind true branch, `// c` behind condition, comment line in front of `:`, inside a branch, inside parentheses, nullable `int?`, `?.`, `?[]`, `??`) | FMT-UNSUP-008 | AssertSupported | passes |
| CommentBehindTheRefOfARefBranchIsSupported | FMT-UNSUP-008 | AssertSupported | passes |
| FormatDocumentAsyncRefusesTheShape | FMT-UNSUP-008 | direct | fails |
| NodeLevelEntryPointsRefuseATargetThatHoldsTheShape (`FormatNode`, `FormatNodeInDocumentAsync`, context entry point refused; the method beside the shape formatted) | FMT-UNSUP-008 | direct | fails |
| FindUnsupportedSyntaxAnswersForTheConditionalAndNotForPartOfIt (whole conditional and root refused; `WhenTrue` with the comment in its leading trivia, and `Condition`, not refused) | FMT-UNSUP-008, FMT-UNSUP-003 | direct | fails |
| FindUnsupportedSyntaxGivesTheAnswerOfThePrecheck | FMT-UNSUP-008 | direct | passes (parity; both null today) |
| OutputOfAFormattedConditionalIsNotUnsupported (8 rows: comments in front of `?`/`:` that the line-break phase moves; `/** */` in front of either operator) | FMT-UNSUP-005, FMT-UNSUP-008 | FormatSyntaxTree twice | passes |
| CliHiddenCodeEndToEndTests.CommentBehindAConditionalOperatorIsReportedAsUnsupported (error line at (5,21), exit code 2) | FMT-UNSUP-008 | Program.Main | fails |

RH5308 (`RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests.cs`):

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| SpecExampleNoFixIsOfferedForACommentBehindAnOperator (LF, CRLF) | RH5308-005 | VerifyNoCodeFixIsOffered | fails |
| SpecExampleConditionalTheFormatterRefusesIsReported | SHARED-DIAG-010, RH5308-005 | VerifyNoCodeFixIsOffered | fails (fix offered; the diagnostic is already reported) |
| NoFixIsOfferedForAnInlineCommentBehindTheQuestionMark | RH5308-005 | VerifyNoCodeFixIsOffered | fails |
| NoFixIsOfferedForADirectiveBehindTheQuestionMark | RH5308-005 | VerifyNoCodeFixIsOffered | fails |
| NoFixIsOfferedWhenTheStatementHoldsAnotherConditionalWithACommentBehindItsOperator | RH5308-005 | VerifyNoCodeFixIsOffered | fails |
| SpecExampleCommentAboveTheOperatorsLineDoesNotWithholdTheFix (LF, CRLF) | RH5308-005 | Verify, VerifyCodeFixWithEndOfLine | passes. The expected output is the observed one: FMT-BLANK-011 puts a blank line in front of `// c` (05-spec-author) |
| RH7101…NoFixIsOfferedInATypeThatHoldsACommentBehindAConditionalOperator (change doc #4) | SHARED-FIX-010 | VerifyNoCodeFixIsOffered | fails |

Existing tests changed because their input now holds the FMT-UNSUP-008 shape. With the shape refused, the specs demand either the refusal or a withheld fix:

| Test (before → after) | Change | Why |
|---|---|---|
| `DelimitedDocumentationInExpressionTests`: the 18 `MovesDelimitedDocumentationBehind{Colon,QuestionMark,BlockComment…}…` tests → `DelimitedDocumentationBehind…IsUnsupported` | FMT-BREAK-018 → FMT-UNSUP-008; `AssertRuleResult` → `AssertUnsupportedFor` (first comment behind the operator) | Their inputs hold a `/** */` (or `/* */`) behind `?`/`:`. Their old expected output also held the shape again (`:` ⏎ ⏎ `/** d */ 2`), which FMT-UNSUP-005 forbids. The 6 tests for other operators are unchanged. |
| `RH3202…VerifyFixMovesDelimitedDocumentationBehind…` (9) → `VerifyNoFixForDelimitedDocumentationBehind…` | FMT-BREAK-018 → SHARED-FIX-010; `VerifyNoCodeFixIsOffered` under LF and CRLF | RH3202's fix formats the method, which the formatter refuses. Markup changed to `{|RH3202:…|}`. |
| `ConditionalExpressionFullPipelineTests.DanglingColonBefore{Comment,Directive}IsNotJoined` → `…IsUnsupported` | untagged → FMT-UNSUP-008 | A comment line or `#if` between `:` and the false branch. |
| `ConditionalOperatorSpacingTests.CommentBehindTheColonKeepsItsPlace` → `CommentBehindTheColonIsUnsupported` | FMT-SPACE-018/013 → FMT-UNSUP-008 | `:/* z */2` |
| `CommentJoinTests.TernaryQuestion/ColonIsNotCollapsedIntoTrailingComment`, `DirectiveJoinTests.TernaryQuestion/ColonIsNotCollapsedAcrossDirective` | deleted (phase-level, FMT-BREAK-001) | They test the two line-break-phase refusals that the change doc deletes, on an input the precheck now refuses. Their refusal is covered by `EachCommentBetweenAnOperatorAndItsBranchIsUnsupported` and the directive tests above. |
| `JoinSeparatorPhaseTests.CommentInTheGapStillRefusesEveryJoin` / `DirectiveInTheGapStillRefusesEveryJoin` | removed only the `return b ? // r` / `#pragma … CS0005` conditional part; summaries updated | Same deleted guards; the other joins stay covered. |
| `SameLineGapRulePhaseTests.DocumentationCommentsAroundTheQuestionMarkKeepTheirPlace` | **not changed** | It runs `HorizontalSpacingPhase.Execute` directly, no entry point; no deleted special case is involved, and it passes after the change. Its `b ?/** y */1` part is now unreachable; whether to drop it is the implementer's or cleaner's call. |

I searched every test project, both for inline `? //`, `? /*`, and `: /*`, and with a multi-line scan for a comment or `#` line behind a `?` or `:` that ends its line. The search found nothing else. It also found nothing in production source, so self-hosting is unaffected.

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `a ? // c` (issue) | LF, CRLF | RH5308 fix | reproduced | fix offered, diagnostic stays vs no fix |
| `? 1 : // c` | LF, CRLF | RH5308 fix | reproduced | same |
| Same inputs | LF, CRLF | formatter | reproduced | formatted half-wrapped vs refused as unsupported |
| `a ? /* c */ 1⏎ : 2` | LF | RH5308 fix, formatter | reproduced | fixed and formatted vs refused, no fix |
| `#if DEBUG` branch behind `?` | LF, CRLF | RH5308 fix, formatter | reproduced | fix offered / formatted vs refused |

### Not covered, and why

- A throw site of `SyntaxNotSupportedException` under FMT-UNSUP-006: the specs name no phase that refuses this shape, so there is no throw site to test.
- FC-038 to FC-043 walk: the boundary on each side is covered (in front of the operator, behind the branch's first token, `?.`/`?[]`/`??`/nullable, `ref`). Every documentation mode is covered. Every entry point and `FindUnsupportedSyntax` are covered, with a node holding part of the shape. Closure is covered.

## thoenissen/Reihitsu#1143

### Tests

Settings changed to `= 1`: these now reproduce SHARED-FIX-002. All fail with "Expected '1' iterations but found '2'".

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH3001 FixAllCorrectsEveryDiagnostic; FixOfANestedNegationInAWholeArgumentFixesBoth (overlap comments removed) | SHARED-FIX-002 (base tag; second untagged as before) | Verify | fails |
| RH3102 FixAllCorrectsEveryDiagnostic | SHARED-FIX-002 | Verify | fails |
| RH5110 FixAllCorrectsEveryDiagnostic | SHARED-FIX-002 | Verify | fails |
| RH7004 FixAllCorrectsEveryDiagnostic (overlap comment removed) | SHARED-FIX-002 | Verify | fails |
| RH7102 FixAllCorrectsEveryDiagnostic (overlap comment removed) | SHARED-FIX-002 | Verify | fails |
| RH7103 FixAllCorrectsEveryDiagnostic (two static methods, no initializer: not the issue's guard-dependent case) | SHARED-FIX-002, RH7103-008 | Verify | fails |
| RH7109 FixAllCorrectsEveryDiagnostic (overlap sentence removed) | SHARED-FIX-002 | Verify | fails |
| RH3208 `-2` → `1` | (existing tags) | Verify | passes |
| 14 attribute-shape scenarios RH5502–RH5531, `-2` → `1`; the "one pass or two is not what this scenario asserts" sentence removed | SHARED-FIX-002 | Verify | passes (already resolved, 02-skeptic) |
| New: RH6016 FixAllCorrectsAConditionalAccessWithSpacesOnBothSidesOfTheQuestionMarkInOnePass (`s ? . Length`) | SHARED-FIX-002 | Verify | fails |
| New: RH8204 FixAllRemovesNestedPlaceholdersInOnePass | SHARED-FIX-002 | Verify | fails |
| New: RH6024 FixAllCorrectsAChainOfOperatorsInOnePass (`a+b+c`, `a==b&&b>0`) | SHARED-FIX-002 | Verify | passes (regression) |
| New: RH6023 FixAllCorrectsAnAssignmentInsideTheTargetOfAnotherInOnePass (`d[k=1]=2`) | SHARED-FIX-002 | Verify | passes (regression) |

Second passes kept, each with a comment that names its reason:

| Test | Reason |
|---|---|
| RH5104 (two tests) | Guard `HasCommentBeforeOnLine` withholds the second comment's fix on the starting code (probe: the fix converges in 2 iterations, 1 action). Comment rewritten from the overlap reason (FC-031) to the guard. |
| RH5405 FixAllWraps…InTwoIterations (two tests) | The guard in `StatementBracesCodeFixProviderBase.TryCreateChanges` for other code in front of the owner (SHARED-BRACE-003). |
| RH5202 VerifyFixAllFixesRawStringNestedInHoleFirstAndThenOuterLiteral… (LF, CRLF) | RH5202-006 withholds the outer literal's fix while the nested literal is misaligned. I tried 1, saw it fail, and restored 2 with the comment. |
| RH7310 VerifyNestedEmptyRegionsAreRemoved | Only the inner region is reported on the starting code; the outer one is reported after the first fix, so SHARED-FIX-002 does not cover it. |
| RH3002 OuterParenthesesAroundInvokedReceiverChainAreReportedAndFixed | Same: one diagnostic on the starting code; the inner parentheses are reported after the first fix. |

Left as they are (legitimate per 02-skeptic): RH4106, RH4107, RH4109 (`-2`, withheld fix keeps the harness running), RH5028, RH8402 (`-2`, document scope fixes one document per iteration).

### Not covered, and why

- RH5204: its scenario already runs at one pass (`= 1`), so it is covered.

## thoenissen/Reihitsu#1673

### Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| RH5308…SpecExampleNoFixIsOfferedForAConditionalInAnAssemblyAttribute | RH5308-006, SHARED-FIX-005 | VerifyNoCodeFixIsOffered | fails |
| RH5308…NoFixIsOfferedForAConditionalInAModuleAttribute | RH5308-006, SHARED-FIX-005 | VerifyNoCodeFixIsOffered | fails |
| RH5308…FixIsOfferedForAConditionalInAnAttributeOnAType (other side: a member declaration exists) | RH5308-006 | TryApplyCodeFixAsync | passes |

### Scenarios (bug fix)

| Scenario | Line ending | Surface | Result | Observed vs expected |
|---|---|---|---|---|
| `[assembly: CLSCompliant(true ?⏎ true :⏎ false)]` | LF | RH5308 fix | reproduced | one action offered vs none |

## thoenissen/Reihitsu#1678

No tests. The maintainer's answer Q5 A changes no behavior; SHARED-ORDER-008's wording changed only.

## Whole run

- Failure classes walked: FC-007, FC-008 (#1143, #1274), FC-009 (#1673, #1150), FC-013 (SHARED-DIAG-010 test), FC-015 (the tests use only public entry points, so any second statement of the carrier definition shows up only as divergent behavior between the formatter, fix, CLI, and Playground tests on the same inputs), FC-018 (renames: RH4103, RH2001 through the solution), FC-020/FC-022/FC-024 (directive and disabled-text inputs throughout), FC-031 (RH5104 comment), FC-033 (CRLF and symbol variants), FC-035 (expected values from specs; the observed outputs used are from code the run does not change), FC-038–FC-043 (#1274 above). FC-010/011/012/014/030 are not reachable by a precheck refusal, a whole-file refusal, or a registration check; the closure test covers the formatter's output.
- `scripts/format.sh --check` over the 72 changed test files: 0 need formatting.
- `scripts/test.sh` per project on this commit:
  - analyzer: 67 failed, exactly the "fails" rows above: 30 SHARED-FIX-011 (28 classes, RH5308, RH3004), 10 SHARED-FIX-012, 6 RH5308-005/SHARED-DIAG-010/RH7101, 9 RH3202, 2 RH5308-006, 10 SHARED-FIX-002 (7 `FixAllCorrectsEveryDiagnostic`, RH3001 nested, RH6016, RH8204).
  - formatter: 65 failed, all FMT-PIPE-033 and FMT-UNSUP-008 rows above, including the rewritten existing tests.
  - cli: 8 failed, as listed.
  - playground: 3 failed, as listed.
  - architecture: 117 passed, including `EveryRequirementOfAnEnforcedSpecIsCovered`, so every new requirement has a tagged test.
  - core and tooling: passed.
- Not run: the self-hosting scan (CI runs it). No production source holds the FMT-UNSUP-008 shape (scanned).

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~ReihitsuFormatterHiddenCodeTests|FullyQualifiedName~UnsupportedConditionalOperatorCommentTests|FullyQualifiedName~DelimitedDocumentationInExpressionTests|FullyQualifiedName~ConditionalExpressionFullPipelineTests|FullyQualifiedName~ConditionalOperatorSpacingTests|FullyQualifiedName~JoinSeparatorPhaseTests|FullyQualifiedName~CommentJoinTests|FullyQualifiedName~DirectiveJoinTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~NoFixIsOfferedInAFileWithASyntaxError|FullyQualifiedName~FixIsStillOfferedInAFileWithASyntaxError|FullyQualifiedName~EditThenFormatFixIsNotOffered|FullyQualifiedName~HidingCarrier|FullyQualifiedName~ExcludedBranchOpens|FullyQualifiedName~OutsideEveryGroup|FullyQualifiedName~WithoutALineThatEndsTheGroup"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5308ConditionalExpressionsShouldBeFormattedCorrectlyAnalyzerTests|FullyQualifiedName~RH3202ExpressionStyleMethodsShouldNotBeUsedAnalyzerTests|FullyQualifiedName~RH7101DoNotCombineFieldsAnalyzerTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~FixAllCorrectsEveryDiagnostic|FullyQualifiedName~InOnePass|FullyQualifiedName~RH3001|FullyQualifiedName~RH3002|FullyQualifiedName~RH5104|FullyQualifiedName~RH5202|FullyQualifiedName~RH5405|FullyQualifiedName~RH7310|FullyQualifiedName~RH3208"
scripts/test.sh --project cli --filter "FullyQualifiedName~CliHiddenCodeEndToEndTests"
scripts/test.sh --project playground --filter "FullyQualifiedName~HiddenCode"
scripts/test.sh --project architecture
```
