Role: test implementer
Commit: cdb8646
Verdict: tests written

My prompt carried no intended fix, suspected cause, or hoped-for verdict. Expected values come from the examples and requirements of FMT-STRUCT-028, FMT-STRUCT-029, FMT-BREAK-032, and the change doc's *Visible output changes*; where the change doc says an output stays as on `main` and no spec example gives it, the expected value is `main`'s output, measured with `scripts/format.sh` in the session scratchpad and named so below.

## Tests

New files:

- `src/Reihitsu.Formatter.Test/Unit/StructuralTransforms/AttributeListStructuralTransformTests.cs` (A) — asserts on the output of `StructuralTransformPhase` (and once on `LineBreakPhase`) through `ObservedPipeline`, under LF and CRLF; every positive case also checks no elastic trivia and the FMT-PIPE-021 round trip.
- `src/Reihitsu.Formatter.Test/Regression/Structural/AttributeListStructuralFinalOutputTests.cs` (B) — final output.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| A `ListOfEachDeclarationOwnerIsSplitWithNothingBetweenTheCreatedLists` (20 rows: assembly, module, class, struct, interface, record, enum, delegate, method, constructor, destructor, property, indexer, field, event field, event, operator, conversion, enum member, local function) | FMT-STRUCT-028 | `ObservedPipeline.GetPhaseOutput` | fails: phase output holds `[A, B]`, expected `[A][B]` |
| A `CreatedListsKeepTheSpecifierAndTheWrittenOrder` (`method:`, `return:`, three attributes) | FMT-STRUCT-028 | phase output | fails, same reason |
| A `SeparatorsBetweenTwoAttributesAreDroppedWithTheComma` (`[A,⏎ B]`, `[A⏎ , B]`, `[A,⏎⏎ B]`, `[A⏎⏎ ,⏎ B]`, `[A , B]`, `[A,B]`, `[Obsolete( "x" ) , B( 1 )]`) | FMT-STRUCT-028 | phase output | fails, same reason |
| A `FirstCreatedListKeepsTheLeadingTriviaAndTheLastTheTrailingTrivia` (comment in front and behind) | FMT-STRUCT-028 | phase output | fails: one list instead of two |
| A `AccessorListsAreLeftToTheLineBreakPhase` | FMT-STRUCT-028 (accessor boundary) | phase output | passes (other side of the owner condition) |
| A `ParameterListWithSeveralAttributesIsNotSplit` | FMT-STRUCT-029 | phase output | passes (other side) |
| A `ParameterListsWithoutSpecifierAreMergedWithoutWrittenWhitespace` (`[A,B,C]`, created commas carry nothing, no line break) | FMT-STRUCT-029 | phase output | fails: three lists |
| A `TypeParameterListsAreMerged` | FMT-STRUCT-029 | phase output | fails: two lists |
| A `ListsOfEachTargetAreMergedOnTheirOwn` (`[A,C]`, `[property: B,D]`) | FMT-STRUCT-029 | phase output | fails: sequence differs |
| A `ParamSpecifierAndNoSpecifierAreMergedIntoTheFirstList` | FMT-STRUCT-029 | phase output | fails |
| A `LineBreakInTheStretchIsKeptOnce` (behind first, behind last, behind a removed list, behind every list: exactly one line break between merged `]` and the type) | FMT-STRUCT-029 | phase output | fails: lists not merged |
| A `BlankLineInTheStretchLeavesALineBreak` (at least one) | FMT-STRUCT-029 | phase output | fails |
| A `LineBreakInTheStretchOfATypeParameterIsKept` | FMT-STRUCT-029 | phase output | fails |
| A `NoLineBreakInTheStretchKeepsNone` | FMT-STRUCT-029 | phase output | fails: lists not merged |
| A `LineBreakPhaseDoesNotChangeTheTokensOfDeclarationAttributeLists` (token texts of `LineBreakPhase` input and output equal) | FMT-BREAK-027 | `ObservedPipeline.Run` | fails: 47 of 66 tokens differ |
| B `SplitExampleOfTheRequirement` | FMT-STRUCT-028 | `AssertRuleResult` | passes (output unchanged) |
| B `LineBreaksBetweenTwoAttributesAreDroppedWithTheComma` (second spec example; change-doc changes 1 and 2) | FMT-STRUCT-028 | `AssertRuleResult` | fails: `[     B]`, `[A⏎ ]`, `[⏎ B]` |
| B `LineBreakBehindTheCommaSettlesInOnePassOnEveryOwner` (type, method behind a comment line, local function) | FMT-STRUCT-028 | `AssertRuleResult` | fails: `[ B]`, `[        B]`, `[         B]` |
| B `WhitespaceAroundTheCommaIsDropped` | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `ListWithACommentBetweenItsBracketsIsNotSplit` (`/* */`, `//`) | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `ListWithADirectiveBetweenItsBracketsIsNotSplit` | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `ListWithAnUnknownSpecifierIsNotSplit` | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `ListWithACommentAroundItIsSplit` (`///` in front, `//` in front and behind, `/* */` in front on the line) | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `ListNextToADirectiveOutsideItsBracketsIsSplit` (`#region` in front, active `#if` in front, around, behind) | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `SplitListOfAFieldWithSeveralDeclarators` (with FMT-STRUCT-021) | FMT-STRUCT-028 | `AssertRuleResult` | passes |
| B `LineBreakInFrontOfTheClosingBracketKeepsItsOutput` (`[A, B⏎]`, `main`'s output) | — characterization (change doc: outside the change) | `AssertRuleResult` | passes |
| B `MergeExampleOfTheRequirement` | FMT-STRUCT-029 | `AssertRuleResult` | passes |
| B `LineBreakBetweenMergedListsStillWrapsTheParameterList` (P, Q, R) | FMT-STRUCT-029 | `AssertRuleResult` | passes |
| B `LineBreakBetweenMergedListsWrapsARecordParameterList` (R, R2) | FMT-STRUCT-029 | `AssertRuleResult` | passes (`main`'s output) |
| B `LineBreakBetweenMergedListsWrapsALocalFunctionParameterList` | FMT-STRUCT-029 | `AssertRuleResult` | passes (`main`'s output) |
| B `LineBreakBetweenMergedListsWrapsALambdaParameterList` | FMT-STRUCT-029 | `AssertRuleResult` | passes (`main`'s output) |
| B `LineBreakBetweenMergedListsOfBracketedListsIsJoined` (indexer, type parameter) | FMT-STRUCT-029 | `AssertRuleResult` | passes (`main`'s output) |
| B `MergedListsOfASingleParameterAreJoinedOntoItsLine` (incl. RH5524 shape with a blank line) | FMT-STRUCT-029 | `AssertRuleResult` | passes |
| B `CommentInFrontOfTheFirstListDoesNotBlockTheMerge` | FMT-STRUCT-029 | `AssertSecondPassIsStable` + contains `[A, B] int a` | passes |
| B `CommentLeadingTheTokenBehindTheListsBlocksTheMerge` | FMT-STRUCT-029 | `AssertRuleResult` | passes |
| B `DirectiveLeadingTheFirstListBlocksTheMerge` | FMT-STRUCT-029 | `AssertSecondPassIsStable` + contains `[A] [B] int a` | passes |
| B `DocumentationCommentBetweenTheListsBlocksTheMerge` | FMT-STRUCT-029 | `AssertSecondPassIsStable` + no `[A, B]` | passes |
| B `LambdaAttributeListKeepsItsOutput` (`var f = [A, B] (int x) => x;`, `main`'s output, two passes) | — characterization (change doc: lambda split keeps its output) | one pass + second pass, LF and CRLF, no phase check | passes |
| B `AccessorExampleOfTheRequirement` | FMT-BREAK-032 | `AssertRuleResult` | passes |
| B `AccessorSplitDropsTheLineBreaksBetweenTwoAttributes` (change-doc accessor `G`, and `[C⏎ , D] get => 1;`) | FMT-BREAK-032 | `AssertRuleResult` | fails: `[         D]`, `[C⏎ ]` |
| B `IndexerAccessorListsFollowTheAccessorListLayout` | FMT-BREAK-032 | `AssertRuleResult` | passes |
| B `FormatNodeInDocumentAsyncSplitsAndMergesAttributeLists` | FMT-STRUCT-016 | `FormatNodeInDocumentAsync`, LF and CRLF | passes |

Tags added to existing tests in `src/Reihitsu.Formatter.Test/Regression/LineBreaks/ParameterAttributeTargetLayoutTests.cs` (assertions untouched; they assert shape as well as placement): `FMT-STRUCT-029` on `PropertyTargetedAttributeListWithSeveralAttributesIsNotSplit`, `PropertyTargetedAttributeListsOnOneParameterAreMerged`, `OnlyAttributeListsOfTheSameTargetAreMerged`, `PropertyAndFieldTargetedAttributeListsAreNotMerged`, `ImplicitListsOnSeparateLinesAreMergedInOnePass`; `FMT-STRUCT-028` on `PropertyTargetedAttributeListWithSeveralAttributesOnPropertyIsStillSplit`.

Every failing test was checked to fail on its asserted difference (list text, list count, token sequence, or the change doc's before/after output), not on a compile error or unrelated layout. `scripts/test.sh --project architecture` passes, including `EveryRequirementOfAnEnforcedSpecIsCovered`.

## Not covered, and why

- **Phase-output negatives for the split guards** (comment/directive between brackets, unknown specifier): the structural phase splits nothing today, so a phase-level negative passes on both sides of the change; the final-output tests in B exercise the guard wherever it lives.
- **Exact spacing behind a comment in front of a parameter's first list**: `main` gives `void M(/* c */[A] int a)` with or without a merge, so the missing space is not the merge's; `CommentInFrontOfTheFirstListDoesNotBlockTheMerge` asserts only the merge and stability.
- **Where the kept line break of FMT-STRUCT-029 stands, and its indentation**: left open by the spec (lead decision 9); A asserts only its count between the merged `]` and the type.
- **Lambda split under the phase-level stability check**: `AssertRuleResult` reports that `LineBreakPhase` run alone moves `(int x) => x;` to column 0 on `main`'s output (follow-up 1). The characterization skips that check rather than adding a `PhaseReversalExceptionList` entry for an output the follow-up will change. Existing `ListElementAttributeLayoutSinglePassTests` cover lambdas in argument lists.
- **Code-fix output (SHARED-ATTR-017/018) after the single merge**: the existing RH55xx code-fix tests assert it; the run changes no expected value there.
- **FC-013 (analyzer/formatter parity)**: no final shape changes except the two change-doc rows, which no RH55xx rule reports differently.
- **Follow-ups 2–5 of `00-lead.md`**: outside the run.
- **Failure classes walked**: FC-003 (both sides of owner/target/guard conditions — covered), FC-004/005/019/020/021 (comment, doc comment, directive in/around — covered), FC-006 (specifiers — covered), FC-010/017 (LF/CRLF and second pass — every B test via `AssertRuleResult`, A via both line endings), FC-015/016 (two owners — the accessor split and declaration split share the change-doc rows tested on both), FC-027 (lambdas — covered), FC-028 (node-level entry — covered).

## Observations for the lead

- In my first draft, inputs with `#if DEBUG` around a list produced no split; DEBUG is undefined, so the list was disabled text, not an attribute list. With `#if true` every directive outside the brackets splits, consistent with FMT-STRUCT-028. No spec question remains from it.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~AttributeListStructural"
scripts/test.sh --project formatter --filter "FullyQualifiedName~ParameterAttributeTargetLayoutTests"
scripts/test.sh --project architecture
```
