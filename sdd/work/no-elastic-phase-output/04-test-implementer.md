Role: test implementer
Commit: 637e253
Verdict: tests written

The prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the examples of FMT-PIPE-034 and FMT-BREAK-032 and from the skeptic's matrix (02). The skeptic produced those outputs on today's code, and the change doc keeps them unchanged.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `Helpers/PhaseTreeRoundTrip.ExecuteAndCompare`: the per-phase observer of every formatter fixture now also reports a phase output that carries elastic trivia, checked with `descendIntoTrivia`, whenever the input root carries none. New public `FindElasticTrivia(SyntaxNode)` | FMT-PIPE-034, the issue's goal kept by decision 5 (helper, no tag) | `FormatterTestsBase.AssertRuleResult` → `ApplyRule` | makes 5 existing tests fail (see below) |
| `Unit/Pipeline/NoElasticPhaseOutputTests.EveryPhaseOutputOfTheRequirementExampleCarriesNoElasticTrivia` | FMT-PIPE-034 | `ObservedPipeline.Run`, every phase including both preparation sub-phases, LF + CRLF | fail (LineBreakPhase … CleanupPhase, commas) |
| `…RequirementExampleKeepsItsOutput` | FMT-PIPE-034 | `AssertRuleResult` (idempotency, CRLF, phase stability, observer) | fail (observer) |
| `…MergedListsWithoutSpecifierOnTheDeclarationsLineCarryNoElasticTrivia` (`[A] [B] get`, `[A, B] [C] get`) | FMT-PIPE-034 | `ObservedPipeline.Run` + `AssertRuleResult` | fail |
| `…IndexerWhoseFirstParamListHoldsACommaCarriesNoElasticTrivia` (skeptic matrix row 3) | FMT-PIPE-034 | same | fail |
| `…ThreeParamListsWithAnArgumentListCarryNoElasticTrivia` (row 2, `A(1,2)`) | FMT-PIPE-034 | same | fail |
| `…EventRemoveAccessorParamListsCarryNoElasticTrivia` (row 4) | FMT-PIPE-034 | same | fail |
| `…TypevarListsOfASetterCarryNoElasticTrivia` | FMT-PIPE-034 | `ObservedPipeline.Run` | fail |
| `…EveryPhaseOutputOfAFileEveryPhaseRewritesCarriesNoElasticTrivia`: a fixture with region directives, a documentation comment, usings, a field split, an expression body, an unbraced `if`, switch sections, a column-0 raw string, a tab, and the accessor merge. It also asserts that TabReplacement, StructuralTransform, RegionFormatting, DocumentationCommentFormatting, UsingDirectiveOrdering, LineBreak, SwitchCaseBrace, BlankLine, HorizontalSpacing, and Indentation each change the file | FMT-PIPE-034 | `ObservedPipeline.Run` | fail (LineBreakPhase comma only). The part that checks which phases change the file was run on its own and passes today |
| `…FormatNodeInDocumentAsyncAddsNoElasticTriviaToAMergedAccessorList` (the RH5111 path from 02: `= 1` on the next line) | FMT-PIPE-034 | `FormatNodeInDocumentAsync` on a property parsed from text, LF + CRLF | fail (leading elastic on comma) |
| `…FormatNodeInDocumentAsyncAddsNoElasticTriviaToAMergedAccessorListOverSeveralLines` | FMT-PIPE-034 | same | fail |
| `…FormatNodeAddsNoElasticTriviaToAMergedAccessorList` | FMT-PIPE-034 | `FormatNode` on a parsed property, LF + CRLF | fail |
| `…ElasticTriviaCheckFindsMarkersOnCodeAndDirectiveTokens`: proves the observer check is not vacuous, both on a token of the code and inside a `#define` directive | — (helper test) | `PhaseTreeRoundTrip.FindElasticTrivia` | pass |
| `Unit/LineBreaks/AccessorAttributeMergeCommaTests.CreatedCommaOfParamListsOnTheDeclarationsLineCarriesNoTrivia`: the line-break phase's output holds `[param: A,B] set;`. The comma has empty leading and trailing trivia, and the recorded space behind `]` stays | FMT-BREAK-032 | `ObservedPipeline.GetPhaseOutput("LineBreakPhase")`, LF + CRLF | fail (comma carries trivia) |
| `…CreatedCommaOfParamListsOverSeveralLinesCarriesNoTrivia` | FMT-BREAK-032 | same | fail |
| `…CommaTheFirstListHeldWithASpaceCarriesNoTrivia` (`[param: A, B] [param: C]` → `[param: A,B,C]`) | FMT-BREAK-032, FMT-BREAK-031 | same | fail |
| `…CommasOfListsWithoutSpecifierOnTheDeclarationsLineCarryNoTrivia` (`[A, B] [C] get` → `[A,B,C] get;`) | FMT-BREAK-032 | same | fail |
| `…CreatedCommaOfTypevarListsCarriesNoTrivia` | FMT-BREAK-032 | same | fail |
| `…CommasBetweenAttributesWithAnArgumentListCarryNoTrivia`: checks only the list separators, and leaves the argument comma inside `A(1, 2)` undecided, as 03 states | FMT-BREAK-032 | same | fail |
| `…CommaOfAnAccessorListTheMergeDoesNotRewriteKeepsItsSpace`: the other side of the condition. A single `[C, D]` list on the declaration's line is not merged and stays `[C, D]` | FMT-BREAK-031 | same | pass |
| `Analyzer.Test/Formatting/RH5524…AnalyzerTests.VerifyCodeFixMergesIntoAFirstListThatAlreadyHoldsAComma` (`[A, B] [D]` → `[A, B, D]`) | SHARED-ATTR-018 | `Verify` (BatchCodeFixTestsBase) | pass, must stay |
| `Analyzer.Test/Formatting/RH5531…AnalyzerTests.VerifyCodeFixMergesInlineAccessorListsIntoAFirstListThatAlreadyHoldsAComma` (`{ [A, B] [D] get; set; }` → `[A, B, D]`) | SHARED-ATTR-018 | `Verify` | pass, must stay |
| `Analyzer.Test/Formatting/RH5111…AnalyzerTests.VerifyCodeFixForPropertyWithMergedSetterParamListsKeepsTheCommaSpace` (→ `int Property { get; [param: A, B] set; } = 1;`) | — (change doc: code-fix results unchanged; no requirement decides this combination) | `VerifyUnderBothLineEndings` (LF + CRLF) | pass, must stay |

Existing tests whose asserted output the change touches:

- Through the observer, five existing tests fail today: `AttributeListStructuralFinalOutputTests.AccessorExampleOfTheRequirement`, `IndexerAccessorListsFollowTheAccessorListLayout`, `ParamListsOfASetterInAnAccessorListOverSeveralLinesAreMerged`, `ParamListsOfASetterInAnAccessorListOnTheDeclarationsLineAreMerged`, and `ParamAndTypevarListsOfEveryAccessorInAnAccessorListOverSeveralLinesAreMerged`. These are the same five 01 reported. Their assertions are unchanged.
- Full formatter run at this head with the new tests: 22 failed, 5394 passed. The 22 are the 17 new failing tests plus those 5. No other test fails.
- `LineBreakRewriterTests` line 702 asserts `[First,Second] get;` (no space) on the line-break output. That already matches FMT-BREAK-032 and is left alone.
- `AttributeListStructuralTransformTests` asserts `[param: A,B]` on the structural phase, not on the line-break phase. Left alone.
- The SHARED-ATTR-018 tests that assert `, B]` in the about 16 RH55xx analyzer files are left alone and must keep passing.
- The `PhaseTreeRoundTripTests` FMT-PIPE-021 tests are left alone.

## Scenarios (bug fix)

_N/A_: behavior-change lane.

## Not covered, and why

- **Elastic trivia that a node-level input already carries.** Not tested. It is a non-goal of `pipeline.md`, and FMT-PIPE-034 does not decide it. When the root carries elastic trivia, the observer skips its check, so "adds" is never asserted as "carries".
- **A mixed-line-ending input for the LineEndings sub-phase.** The fixtures are normalized to one ending, so this sub-phase changes nothing in them. Its output is still checked in every `ObservedPipeline.Run` test. The skeptic found no adding site there, and 01's probe found none on any fixture.
- **Precheck phase.** `ObservedPipeline.Run` asserts that the precheck changes nothing, so its output is its input, which was parsed from text.
- **CleanupPhase as a phase that changes the fixture.** The broad fixture does not show it changing anything. Its output is still checked for elastic trivia.
- **Failure classes.**
  - FC-003: both sides of the merge condition are covered (merged vs. a single list).
  - FC-013: the code-fix boundary is covered by the RH5524, RH5531, and RH5111 tests.
  - FC-017: the requirement says "no trivia", so the assertions use exact empty trivia lists and the exact line-break-phase text, not "no elastic" alone.
  - Idempotency and CRLF: covered through `AssertRuleResult` and the LF/CRLF loops.
  - Comment or directive in the merged gap: the merge refuses comments (`CanMergeAttributeListGroup`), and existing FMT-STRUCT-029 / RH5531 tests already cover that refusal. FMT-PIPE-034 introduces no new gap behavior there.
  - FC-038 to FC-043: no shape is declared unsupported.
  - Naming and Renamer classes: no naming fix is involved.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~NoElasticPhaseOutputTests|FullyQualifiedName~AccessorAttributeMergeCommaTests|FullyQualifiedName~AttributeListStructuralFinalOutputTests"
scripts/test.sh --project analyzer --filter "FullyQualifiedName~VerifyCodeFixMergesIntoAFirstListThatAlreadyHoldsAComma|FullyQualifiedName~VerifyCodeFixMergesInlineAccessorListsIntoAFirstListThatAlreadyHoldsAComma|FullyQualifiedName~VerifyCodeFixForPropertyWithMergedSetterParamListsKeepsTheCommaSpace"
scripts/test.sh --project formatter
scripts/test.sh --project architecture
```

Architecture run at this head: 180 passed. FMT-PIPE-034 is now covered.

## Follow-up candidates

_None._
