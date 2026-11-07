Role: implementer
Commit: see the commit that adds this file (production changes and handover in one commit)
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict.

Of the 52 handed-over tests, 43 now pass, together with the 6 that already passed. 9 fail. None of the 9 can pass under the specs as written. They fall into three groups (D1–D3 below), and each needs a decision by the lead and not a code change from me. Every other formatter, analyzer, core, CLI, architecture, and tooling test passes.

## Changes
| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/GenericConstraintLineBreakRewriter.cs` | Each `TypeParameterConstraintClauseSyntax` is laid out in two steps, for every owner including extension blocks. (1) Unstructured wraps are joined: the gap between the type parameter and the colon, and every gap inside one constraint except gaps inside a type argument list. Those gaps stay with FMT-BREAK-014 and FMT-BREAK-015, which run earlier in the phase. (2) The clause is judged after the joins: a line break behind the colon, or constraints spanning more than one line. A wrapped clause then gets three changes: the first constraint is joined onto the colon, each comma at the start of a line joins the previous constraint's line, and a line break is appended behind each comma whose next constraint shares its line (unless the comma itself is kept at the start of a line). Joins go through `LineBreakTriviaUtilities.CollapseTokenToSameLine`, `CreateJoinedTrailingTrivia`, and `WouldJoinAcrossUnjoinableTrivia`, so none crosses a comment, a directive, or disabled text. The `where`-on-its-own-line step (FMT-BREAK-023) is unchanged. | FMT-BREAK-037, FMT-BREAK-001, FMT-BREAK-031 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/GenericConstraintContributor.cs` | The `where` column is unchanged. In addition, a colon that starts a line is placed at the `where` column + 4. Constraints and commas that start a line go to the first constraint's column; when the first constraint starts a line itself, they go to the colon's column + colon width + 1. Separators use `ListElementAligner.AlignSeparators`. The alignment now runs even when the parent's first line is not placed. | FMT-INDENT-032 |

RH5110's code fix was not changed. It formats through `FormatNodeInDocumentAsync`, so the new layout reaches it (RH5110-003).

## Policy owners
- Constraint-clause layout: `rg -l "TypeParameterConstraintClauseSyntax|ConstraintClauses" src/Reihitsu.Formatter src/Reihitsu.Core` finds `GenericConstraintLineBreakRewriter.cs` and `GenericConstraintContributor.cs` only. Both are extended, and no parallel path was added.
- Join and refusal: `rg -ln "WouldJoinAcrossUnjoinableTrivia" src/Reihitsu.Formatter/Pipeline/LineBreaks` finds the shared `LineBreakTriviaUtilities` and the rewriters that call it. The single joins use `LineBreakTriviaUtilities.CollapseTokenToSameLine`. The batch join of interior wraps (`JoinUnstructuredWraps`) uses the same replacement-map pattern as `ListElementInteriorJoinRewriter.JoinInteriorWraps`, built from the shared `CreateJoinedTrailingTrivia` and `RemoveLeadingEndOfLineAndWhitespace`. That rewriter's `CanJoin` is private and scoped to list elements; a constraint is not a list element, so it could not be reused.
- Separator alignment: `rg -ln "AlignSeparators|SetIfFirstOnLine" src/Reihitsu.Formatter/Pipeline/Indentation/Contributors` finds the shared `ListElementAligner.AlignSeparators`, which is reused.
- Colon spacing: `rg -l "ColonToken" src/Reihitsu.Formatter/Pipeline/HorizontalSpacing` finds `SingleSpaceSpacingRule` (base list and constructor initializer only), `NameColonSpacingRule`, `AttributeTargetColonSpacingRule`, and `ConditionalOperatorSpacingRule`. None covers the constraint-clause colon (see D1).

## Tests run
```text
scripts/test.sh --configuration Debug --project formatter
Failed!  - Failed: 7, Passed: 4561, Total: 4568     (the 7 are D1 x6 and D2)
scripts/test.sh --configuration Debug --project analyzer
Failed!  - Failed: 2, Passed: 7337, Total: 7339     (D1 x1 and D3)
scripts/test.sh --configuration Debug --project core          Passed!  660/660
scripts/test.sh --configuration Debug --project cli           Passed!  251/251
scripts/test.sh --configuration Debug --project architecture  Passed!  115/115
scripts/test.sh --configuration Debug --project tooling       Passed!  126/126
```

As a probe only, I added `TypeParameterConstraintClauseSyntax` to `SingleSpaceSpacingRule.IsSpacedColonToken` and then reverted it. It is not in the commit. With the probe, the formatter suite failed only D2 (4567/4568), and the analyzer suite failed only D3 (7338/7339).

## Disputed tests or scope

### D1 — a joined constraint colon gets no space (spec gap; 7 tests)
Tests:
- `ConstraintClauseLineBreakTests.SingleConstraintWrappedBehindColonIsJoined`
- `ConstraintClauseLineBreakTests.FirstOfSeveralConstraintsWrappedBehindColonIsJoined`
- `ConstraintClauseLineBreakTests.ReportedWrappedConstraintClausesAreLaidOutOneConstraintPerLine`
- `ConstraintClauseLineBreakTests.ColonWrappedBelowTypeParameterJoinsItsLine`
- `ConstraintClauseLineBreakTests.ColonWrappedBelowTypeParameterJoinsAndWrappedClauseIsLaidOut`
- `ConstraintClauseLineBreakTests.BlankLineBehindColonGoesWithTheJoin`
- `RH5110…AnalyzerTests.VerifyFixLaysOutWrappedConstraints`

Each expects `where T : IDisposable` and `where T : class` after a join at the colon. The specs produce `where T :IDisposable` and `where T: class`:
- FMT-BREAK-031 says a join writes no whitespace unless the tokens would merge.
- FMT-SPACE-002 says a joined pair that no rule covers keeps the separator the join wrote.
- No FMT-SPACE rule covers the constraint colon. FMT-SPACE-009 names only base-list and constructor-initializer colons, and RH6021's non-goals name the constraint colon explicitly.

FMT-BREAK-037's own examples show `where T : IDisposable`, so the spec contradicts itself. Possible resolutions:
- **(a)** Extend FMT-SPACE-009 to the constraint-clause colon. This is a one-line change in `SingleSpaceSpacingRule` and makes all 7 tests pass. It also changes visible output: a one-line `where T:class` written tight would become `where T : class`. That is an output question for the maintainer, and RH6021 would then lag the formatter (P11 allows that).
- **(b)** Record a FMT-BREAK-031 exception for the constraint colon, so the join writes one space, like the recorded exception for attribute-list merges.

I did not ship either one.

### D2 — `ConstraintClauseAlignmentTests.LocalFunctionInLambdaClauseAligns` expects a lambda block layout the formatter does not produce
The test expects the block of `System.Action a = () =>⏎{` at column 8. `LambdaAlignmentContributor` aligns a lambda block to the lambda's `(`, column 26, and it already does so on the unchanged code. I confirmed this with `scripts/trace.sh` on the same input with the clause on one line, with this change stashed. My output aligns the clause correctly relative to its `where` (`new()` under `class`), but the whole block stands at column 26. The test's expected output needs correcting by the test implementer: the block, local function, and clause all move 18 columns right. Nothing in this run's specs asks for a change in lambda layout.

### D3 — `RH5110…AnalyzerTests.VerifyFixIsOfferedWhereCommentKeepsColonOnItsOwnLine` asserts a split the spec does not state
Input: `where T // c⏎ : class, new()`. The test expects `: class,⏎  new()`, one constraint per line. Under FMT-BREAK-037 the clause is wrapped only when a line break stands behind the colon or the constraints span more than one line. Neither holds here: the comment keeps the colon on its own line, and `class, new()` stays on one line. So the formatter keeps `: class, new()`, which is what my code does. The colon line is placed one level deeper than `where`, as FMT-INDENT-032 states. Every spec and change-doc example of this shape (FMT-INDENT-032 `P`, change doc entry 9) starts already split, so none decides it.

Either the test changes to `: class, new()`, or the spec adds "a colon kept on its own line wraps the clause" (an output question). The formatter suite has no counterpart test for this exact shape.

No scope growth: two production files are changed, both of them named by the handovers' owners. No new public member or dependency.

## Architecture findings
_None._

## Follow-up candidates
- The column of a line inside a constraint that a comment keeps wrapped outside a type argument list, such as `System. // c⏎ IDisposable`, is still not placed: it keeps the block column. This is lead follow-up 7 and was left out deliberately (Q5).
