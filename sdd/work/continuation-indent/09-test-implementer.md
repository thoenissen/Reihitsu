Role: test implementer
Commit: 0c9cbfd
Verdict: tests written

Follow-up round for lead decisions 11 to 14 (`00-lead.md`) and `08-spec-author.md`. The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from RH5419-003/004, RH5401-003/004, FMT-UNSUP-017 (modifier row and overlap paragraph) as amended in 88de891, and from FMT-INDENT-034 / FMT-INDENT-007 for the rewritten inputs of decision 13; none from a patched copy of the code. Every test below was run on this head (`main`'s code). Columns are 0-based.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `RH5419…AnalyzerTests.VerifyFixJoinsALineBreakBetweenAModifierAndTheType` (LF, CRLF) — replaces `VerifyFixIsWithheldForALineBreakBetweenAModifierAndTheType`; expected `public int this[int a, int b] => a;` | RH5419-003, RH5419-004 | `Verify` | fails: fix leaves `public` on its own line |
| `RH5419…AnalyzerTests.VerifySpecificationExampleFixJoinsALineBreakBehindAModifier` (LF, CRLF) | RH5419-003 | `Verify` | fails: same |
| `RH5419…AnalyzerTests.VerifyFixIsWithheldForABlockCommentBehindAModifierOnOneLine` (`public /* note */ int this[nint a,⏎…`) | RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `RH5419…AnalyzerTests.VerifySpecificationExampleOfWithheldFixes` — gains the spec's `public /* note */ int this[nint a,⏎nint b] => 1;` | RH5419-001, RH5419-004 | `VerifyNoCodeFixIsOffered` | fails: fix offered for the new member |
| `RH5419…AnalyzerTests.VerifyFixIsWithheldForALineCommentBehindAModifier` — summary only: withheld because the formatter refuses the indexer | RH5419-004 | `VerifyNoCodeFixIsOffered` | passes (unchanged assertion, new reason) |
| `RH5401…AnalyzerTests.VerifyFixJoinsALineBreakBetweenAModifierAndTheType` (`public⏎int Q⏎=> 2;` → `public int Q => 2;`, LF, CRLF) | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | fails: fix leaves `public` on its own line |
| `RH5401…AnalyzerTests.VerifyFixJoinsALineBreakBetweenTwoModifiers` (`public⏎static int S⏎=> 2;`, LF, CRLF) | RH5401-003, RH5401-004 | `VerifyCodeFixWithEndOfLine` | fails: same |
| `RH5401…AnalyzerTests.VerifyFixIsWithheldForACommentBehindAModifier` (`public /* note */ int Commented⏎=> 1;`, `public // note⏎int Lined⏎=> 1;`, LF, CRLF) | RH5401-004 | `VerifyNoCodeFixIsOffered` | fails: fix offered |
| `RH5401…AnalyzerTests.VerifyFixIsWithheldWhenPropertyStaysOnSeveralLines` — the spec example gains `Commented` | RH5401-004 | `VerifyNoCodeFixIsOffered` | fails: fix offered for `Commented` |
| `ParameterContinuationLineAlignmentTests.LineCommentBehindAFunctionPointerParameterModifierIsUnsupported` — replaces `FunctionPointerParameterTypeAfterModifierAndLineCommentStaysAligned` (property type, not refused by FMT-UNSUP-004) | FMT-UNSUP-017 | `AssertUnsupportedFor(ModifierCommentReason, …, "// keep")` | fails: not refused |
| `UnsupportedJoinedGapCommentTests.EveryRowOfTheTableIsUnsupportedInDeclarations` — 4 new rows: `delegate*<ref // c⏎int, void>` and `delegate*<ref /* c */ int, void>` in a property type, `delegate*<ref /* c */ int, void>` and `delegate*<in // c⏎int, void>` in a method parameter | FMT-UNSUP-017 | `AssertUnsupportedFor(ModifierCommentReason, …)` | fail: not refused |
| `UnsupportedJoinedGapCommentTests.OverlapWithTheVariableDeclarationShapesGivesTheirReason` — 2 new assertions: `delegate*<ref /* c */ int, void> _p;` (field) and `delegate*<ref // c⏎int, void> p = null;` (local) give FMT-UNSUP-004's reason | FMT-UNSUP-004, FMT-UNSUP-010, FMT-UNSUP-017 | `AssertUnsupportedFor(VariableDeclarationCommentReason, …)` | the two new assertions hold on `main` (probed with `scripts/format.sh --check`: FMT-UNSUP-004's reason); the test as a whole still fails on 07's earlier assertions |
| `RH5201…AnalyzerTests`, 8 tests (decision 13): `VerifyNoDiagnosticForChainBehindLineLeadingCommentLaidOutAsTheFormatter`, `…BehindMultiLineCommentTailAndCommentLaidOutAsTheFormatter`, `…DirectlyBehindMultiLineCommentTailLaidOutAsTheFormatter`, `VerifyNoDiagnosticForAlignedChainBehindIndentedLineLeadingComment`, `VerifyMisalignedLinkOfChainBehindLineLeadingCommentIsReportedAndFixed`, `VerifyMisalignedLinkOfChainBehindMultiLineCommentTailAndCommentIsReportedAndFixed`, `VerifyFixOfChainBehindIndentedLineLeadingCommentKeepsTheCommentLine`, `VerifyFixOfChainDirectlyBehindMultiLineCommentTailKeepsTheComment` — `string M` → `void M`, `return` → `_ =`, every line below it +4 (summaries updated) | RH5201-002, RH5201-005, SHARED-CHAIN-009 (tags unchanged) | `AssertFormattingChangesNothing`, `Verify`, `VerifyCodeFixWithEndOfLine`, `Format` | the 5 tests that format the whole document fail: `main` puts the comment line in column 8, the spec's continuation line is column 12 (FMT-INDENT-034); the 3 analyzer-only / fix-only tests pass |
| `RH5112…AnalyzerTests.VerifyFixOfChainBehindLineLeadingCommentMatchesTheFormatter` — `string M` → `void M` (repair of my 07 rewrite) | RH5112-003 | `VerifyCodeFixWithEndOfLine` + `Format` | fails now for the right reason: comment line in column 8, not 12 |
| `RH5301…FormatterTests.VerifyFormatterAlignsObjectCreationToNewBehindCommentThatLeadsTheLine`, `RH5303…FormatterTests.VerifyFormatterAlignsCollectionCreationToNewBehindCommentThatLeadsTheLine` (`return⏎⏎/* c */ new …` → `_ =⏎⏎/* c */ new …`, method `void`, lines +4), `RH5305…FormatterTests.VerifyFormatterAlignsCollectionExpressionToItsBracketBehindCommentThatLeadsTheLine` (same with `values =` on a `List<int> values` parameter, since `_ = [ … ]` has no target type) | tags unchanged (RH530x-001, FMT-INDENT-022/025, FMT-INDENT-030) | `VerifyFormatter` | fail: comment line in column 8, not 12 |

Notes:

- **My 07 defect.** The RH5112 rewrite in 3eb0971 kept `string M(string s)` with `_ =`, so it failed on CS0161, not on the spec's difference. Fixed here; no other analyzer test of 3eb0971 uses `_ =`.
- **Decision 13 applied wider than RH5201.** RH5301/RH5303/RH5305 formatter tests held `return⏎⏎/* c */`, which FMT-UNSUP-017 refuses; `VerifyFormatter` would have failed after the change, not passed vacuously, but their purpose (alignment behind a comment-led line) is only kept by the same rewrite. Their blank line behind `_ =` is kept, as 07 kept it in `CommentLedFirstTokenAnchorTests`; `main` keeps it, and no requirement in the spec diff removes a blank line in a gap that stays broken.
- **Multi-line comments (FMT-INDENT-007).** In the RH5201 tests with `/* a⏎ b */`, the whole comment moved +4. Only the first line is the formatter's (continuation column 12); the later line is kept as written, so the shifted input is a fixed point either way. `main` already keeps that later line in place.
- **The deeper-column RH5201 tests** (comment line deeper than the formatter's column) moved from column 12 to 16, so they stay deeper than the new continuation column 12 instead of duplicating the formatter-column tests.

## Scenarios (bug fix)

_Not a bug-fix round._

## Not covered, and why

- RH5419 and RH5401 other sides of the modifier join: pinned by `VerifyFixIsWithheldForALineBreakBetweenTheTypeAndThis` and `VerifyFixIsWithheldForALineBreakBetweenAnExplicitInterfaceNameAndThis` (RH5419-004, unchanged), and by RH5401's existing arrow-gap tests.
- `delegate*<ref⏎int, void>` joined (other side of the function-pointer row): pinned by `ListElementInteriorJoinTests.FunctionPointerParameterWrappedAfterRefModifierIsJoined` (08).
- The formatter's side of RH5419/RH5401 modifier joins: `ContinuationJoinTests.ModifierGapsOfOtherDeclarationsAreJoined` (07).
- Other tests whose input the new shapes refuse: `rg` over `src/Reihitsu.Analyzer.Test`, `src/Reihitsu.Cli.Test`, `src/Reihitsu.Core.Test` for lines ending in `return`/`throw`/`await`/`new`, comments behind those keywords and behind `is`/`not`/`or`/`and`/`in`/`with`, comments behind modifiers, and comments behind `=>`; and over `src/Reihitsu.Formatter.Test` for the same in `\n`-escaped sources. Remaining hits do not reach the formatter or keep their old reason: RH3001/RH3002/RH3003/RH3005/RH3006/RH3101/RH3104/RH3105/RH7105 fixes call no formatter (checked in their providers), and their analyzer-only `Verify` asserts no formatting; RH2005's modifier comments are fields (FMT-UNSUP-004, existing); RH5306 and RH3209 as 07 recorded; RH3202–RH3210 and RH7107 hits are member expression-body arrows (FMT-UNSUP-012, existing); the CLI `[with /* size */ (4)]` is FMT-UNSUP-014 (existing); `new /** Doc */ {` in `DelimitedDocumentationBeforeBlockStatementTests` is an anonymous object, which FMT-UNSUP-017 excepts. Formatter-project helpers (`FormatterPass.Format`) assert that the input is not refused, so no formatter regression test can pass vacuously on a refused input.
- Failure classes: FC-003 both sides (join vs. kept breaks in front of `this`; refused function-pointer modifier vs. FMT-UNSUP-004 overlap) covered above; line endings by the LF/CRLF rows and helpers; FC-017 addressed by decision 13's rewrite; FC-028 not touched by this round.

## Commands

```shell
scripts/test.sh --project analyzer --filter "FullyQualifiedName~RH5419ExpressionStyleGetOnlyIndexersShouldBeSingleLinedAnalyzerTests|FullyQualifiedName~RH5401ExpressionStyleGetOnlyPropertiesShouldBeSingleLinedAnalyzerTests|FullyQualifiedName~RH5201MethodChainsShouldBeAlignedAnalyzerTests|FullyQualifiedName~RH5112WrappedFluentCallsShouldKeepFirstCallOnOriginalLineAnalyzerTests.VerifyFixOfChainBehindLineLeadingComment|FullyQualifiedName~RH5301ObjectInitializerShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5303CollectionInitializerShouldBeFormattedCorrectlyFormatterTests|FullyQualifiedName~RH5305CollectionExpressionsShouldBeFormattedCorrectlyFormatterTests"
scripts/test.sh --project formatter --filter "FullyQualifiedName~UnsupportedJoinedGapCommentTests|FullyQualifiedName~ParameterContinuationLineAlignmentTests"
scripts/test.sh --project architecture
```

On this head: the analyzer filter gives 30 failed / 327 passed, each failure one listed above with the spec's difference; `architecture` 198 passed; `scripts/format.sh` over the changed files formatted nothing.

## Follow-up candidates

- `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheOperandOfAwaitKeepsItsPlace` (`await /** a */(t);`) runs `HorizontalSpacingPhase` alone on an input the precheck now refuses (FMT-UNSUP-017), so no caller reaches it; it still asserts the phase and was left unchanged. The lead may drop or keep it (*Supported inputs*).
