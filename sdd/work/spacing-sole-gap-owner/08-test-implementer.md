Role: test implementer
Commit: a933e8a (tests written on it; production code unchanged)
Verdict: tests written

Correction round for rows 16–19 of `00-lead.md` and question 15. Inputs: `00-lead.md`, `04-`/`06-test-implementer.md`, `07-implementer.md` (*Disputed tests* 1–7), `05-spec-author.md` (*Row 19 carried out*), and the spec diff against `origin/main` (FMT-BREAK-026, FMT-BREAK-031, FMT-STRUCT-026, FMT-SPACE-002, -014, -018, -021, -023, -024). The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Expected values come from the requirement text and examples, not from the current output. Where the spec leaves a gap open, the test leaves it open too (item 2 below).

## Disputed tests updated (rows 16, 17)

1. **`LineBreakRewriterTests`, 10 assignment collapse tests.** FMT-BREAK-031 says a join writes no whitespace where the tokens stay two tokens, and the gap already on one line keeps what the author wrote. The expected values are `var value ="test";`, `var value= "test";`, `int x=42;`, `string result =CalculateValue();`, `Value= "test";`, `_name ="default";`, `Name ="test"`, `values[0]= 42;`, `var text ="""`, and `var text =$"""`. Each test is now tagged `FMT-BREAK-031`.
2. **`ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty` and `…IndexerToExpressionBodiedIndexer`.** FMT-STRUCT-026 says the transform writes no whitespace next to the created `=>`. It also says the transform MAY keep or drop the whitespace of the tokens it removes. These inputs (`Value { get => _value; }`) have exactly one author space next to each removed token, so the spec does not decide whether the gap is empty or holds that one space. The tests therefore accept zero or one space on each side of the arrow (`actual.Replace(" =>", "=>").Replace("=> ", "=>")`) and compare everything else exactly. A second written space, or a line break, still fails. The guarantee that the transform writes nothing itself is covered by `StructuralTransformSeparatorTests.GetOnlyMemberTransformWritesNoWhitespaceAround…`, whose inputs carry no author whitespace.
3. **`LineBreakPhaseIntegrationTests.HandlesExpressionBodiedPropertyAcrossLines`.** The expected value is now `public int Value=> 42;`, the form of FMT-BREAK-031's example `public int P=> 1;`. The test is now also tagged `FMT-BREAK-031`.
4. **`ConditionalExpressionFullPipelineTests.SingleLineConditionalWithoutColonSpaceIsNotRewritten`.** Renamed to `SingleLineConditionalWithoutColonSpaceGetsOneSpaceBehindTheColon`. It now expects `var x = cond ? a : b;` (FMT-SPACE-018, answer 4B) and is tagged `FMT-SPACE-018`. The old summary called this test a control for a policy gap, which answer 4B closes.
5. **`SpacingPolicyTests.NoRuleAppliesBetweenTypeAndIdentifier`.** Renamed to `DeclaredNameRuleAppliesBetweenTypeAndIdentifier`. It now asserts 1 (FMT-SPACE-021, answer 7C) and is tagged `FMT-SPACE-021`. To keep a unit test for the policy's "no rule" result, `NoRuleAppliesBetweenCastAndOperand` is new: the gap `(string)` / `o` is FMT-SPACE-002's own example of a gap no rule covers. It is tagged `FMT-SPACE-002`.
6. **`IndentationTests.ImplicitStackAllocInitializerAlignsToStackAllocKeyword`.** The expected value is now `stackalloc []` (FMT-SPACE-024, answer 10B). The braces and members stay at the `stackalloc` column (FMT-INDENT-022), which is unchanged. The test is now also tagged `FMT-SPACE-024`.

## Row 18

The full-pipeline test `RelationalPatternSpacingTests.DocumentationCommentInFrontOfTheOperandKeepsItsPlace` is **removed**, and the case is now the phase-level test `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheRelationalOperandKeepsItsPlace`. Its input is `return x is > /** a */5;` and its expected output is `return x is > /** a */ 5;`; it is tagged FMT-SPACE-023 and FMT-SPACE-014.

The pipeline's real output was not used as the expectation, for two reasons:

- In that output the documentation phase moves the comment to its own line, a blank line appears in front of it, and the result reads `/** a */5`. FMT-SPACE-014 covers only a comment that shares its line with both tokens, so the spec does not decide the gap that remains.
- The implementer's claim that phase-level coverage already existed was wrong: no phase test covered the relational operand. Moving the case was therefore needed, not only removing it.

## Tests

`AssertRuleResult` runs the full pipeline under LF and CRLF, plus a second pass, phase stability, and the per-phase round trip. "Expected now" is the result on this tree. "Red on main" was checked by running `scripts/format.sh` from an `origin/main` worktree on the same inputs. There, `b /** sum */+b` gave `b /** sum */+ b`; `b /** sum */   + b`, `b /* x */   /** y */+b`, and `c ? b /** yes */   : b` stayed unchanged; and `c ? b /** yes */:b` also stayed unchanged.

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `LineBreakRewriterTests.CollapsesVariableValueToEqualsLine`, `…VariableEqualsToTargetLine`, `…VariableEqualsAndValueToTargetLine`, `…AssignmentValueToEqualsLine`, `…PropertyAssignmentEqualsToTargetLine`, `…FieldValueToEqualsLine`, `…MemberInitializationValueToEqualsLine`, `…IndexAssignmentEqualsToTargetLine`, `…RawMultilineStringAssignmentToEqualsLine`, `…InterpolatedRawMultilineStringAssignmentToEqualsLine` (updated) | FMT-BREAK-031 | line-break phase alone | green |
| `ExpressionBodiedTransformTests.ConvertsGetOnlyPropertyToExpressionBodiedProperty`, `…IndexerToExpressionBodiedIndexer` (updated) | FMT-STRUCT-026 | structural phase alone | green |
| `LineBreakPhaseIntegrationTests.HandlesExpressionBodiedPropertyAcrossLines` (updated) | FMT-BREAK-026, FMT-BREAK-031 | line-break phase alone | green |
| `ConditionalExpressionFullPipelineTests.SingleLineConditionalWithoutColonSpaceGetsOneSpaceBehindTheColon` (renamed, updated) | FMT-SPACE-018 | AssertRuleResult | green |
| `SpacingPolicyTests.DeclaredNameRuleAppliesBetweenTypeAndIdentifier` (renamed, updated) | FMT-SPACE-021 | `SpacingPolicy.GetDesiredSpacesAfter` | green |
| `SpacingPolicyTests.NoRuleAppliesBetweenCastAndOperand` (new) | FMT-SPACE-002 | `SpacingPolicy.GetDesiredSpacesAfter` | green |
| `IndentationTests.ImplicitStackAllocInitializerAlignsToStackAllocKeyword` (updated) | FMT-INDENT-022, FMT-SPACE-024 | AssertRuleResult | green |
| `SameLineGapRulePhaseTests.DocumentationCommentInFrontOfTheRelationalOperandKeepsItsPlace` (moved from `RelationalPatternSpacingTests`) | FMT-SPACE-023, FMT-SPACE-014 | spacing phase alone | green |
| `HorizontalSpacingRequirementTests.CommentOfTheRightTokenKeepsItsPlace` (new) — FMT-SPACE-014's widened example | FMT-SPACE-014 | AssertRuleResult | green (red on main) |
| `RightCarriedDocumentationCommentSpacingTests.SpaceInFrontOfABinaryOperatorIsInsertedBehindTheComment` (new) — `+`, `*`, `-` | FMT-SPACE-014, FMT-SPACE-010 | AssertRuleResult | green (red on main) |
| `RightCarriedDocumentationCommentSpacingTests.RunOfSpacesBehindTheCommentBecomesTheDemandedSpace` (new) | FMT-SPACE-014, FMT-SPACE-010 | AssertRuleResult | green (red on main) |
| `RightCarriedDocumentationCommentSpacingTests.DoubleSpaceInFrontOfTheCommentIsKept` (new) — `b  /** sum */+ b`, `+b`, and the stable `d  /** both */ + e` | FMT-SPACE-014, FMT-SPACE-010 | AssertRuleResult | green (red on main) |
| `RightCarriedDocumentationCommentSpacingTests.SpaceInFrontOfTheConditionalColonIsInsertedBehindTheComment` (new) — `/** yes */:b` and `/** yes */   : b` | FMT-SPACE-014, FMT-SPACE-018 | AssertRuleResult | green (red on main) |
| `RightCarriedDocumentationCommentSpacingTests.WhitespaceBetweenTheCommentsOfBothTokensIsKept` (new) — `b /* x */   /** y */+b` | FMT-SPACE-014, FMT-SPACE-013 | AssertRuleResult | green (red on main) |

The other side of the condition, where the rule allows no space and the whitespace behind the comment is removed, is covered by the `;` and `]` lines of the requirement example and by the existing `HorizontalSpacingRewriterTests.RemovesGapBetweenDelimitedDocumentationCommentAndFollowingToken`, the name-colon test, and the attribute-target test.

## Results

On this tree, with `scripts/test.sh --project <p>`:

- formatter: `Passed: 3948, Failed: 0`
- analyzer: `Passed: 6394, Failed: 0`
- architecture: `Passed: 92, Failed: 0`. This includes `EveryRequirementOfAnEnforcedSpecIsCovered`.

The self-hosting project was not run in this round. `WorkingDocumentsDirectoryContainsNoFiles` fails there while `sdd/work/` exists.

## Not covered, and why

- **A right-carried comment in front of a token that starts a node** (relational operand, `when`, a declared name, the expression behind `=>`, a cast operand). Whole-document formatting moves the comment to its own line (`05-spec-author.md`), so these cases are tested at phase level only (`SameLineGapRulePhaseTests`, lead decision 11).
- **A gap behind a right-carried comment that no rule decides** (`new[] { /** z */   1 }`). FMT-SPACE-014 does not decide it, and lead decision 21 dropped it.
- **Directive in the gap.** A directive needs a line of its own, so no comment shares a line with both tokens and FMT-SPACE-014 does not apply.
- **CRLF and the second pass.** Both are inside `AssertRuleResult` for every full-pipeline test above.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RightCarriedDocumentationCommentSpacingTests|FullyQualifiedName~CommentOfTheRightTokenKeepsItsPlace|FullyQualifiedName~DocumentationCommentInFrontOfTheRelationalOperandKeepsItsPlace|FullyQualifiedName~SpacingPolicyTests|FullyQualifiedName~SingleLineConditionalWithoutColonSpaceGetsOneSpaceBehindTheColon|FullyQualifiedName~ImplicitStackAllocInitializerAlignsToStackAllocKeyword|FullyQualifiedName~HandlesExpressionBodiedPropertyAcrossLines|FullyQualifiedName~ExpressionBodiedTransformTests|FullyQualifiedName~LineBreakRewriterTests"
scripts/test.sh --project architecture --filter "FullyQualifiedName~Sdd"
```
