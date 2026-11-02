Role: implementer
Commit: 4d65391
Verdict: test disputed

The prompt carried no intended fix, suspected cause, or hoped-for verdict. All 28 tests that `06-test-implementer.md` lists as failing now pass, with no test assertion changed. Two tests that existed before the run now fail: `RawStringInterpolationHoleTests.ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration` and `...ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift`. They assert the anonymous-method layout that FMT-INDENT-031 replaces; see *Disputed tests or scope*.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakListRewriter.cs` | `VisitParameterList` skips `CollapseOpenParenToDeclarationLine` when the list belongs to a `ParenthesizedLambdaExpressionSyntax`. The comma and closing-parenthesis collapses still run for lambdas. | FMT-BREAK-011 |
| `src/Reihitsu.Formatter/Pipeline/Indentation/Contributors/LambdaAlignmentContributor.cs` | New `AnonymousMethodExpressionSyntax` case. Its anchor is `async` when present, otherwise `delegate` (`GetAnchorToken`), and it uses the same block shift as a block lambda. It applies only when `IsAlignedInRawStringHole` holds: walking up the ancestors, the first `InterpolationSyntax` belongs to a multi-line raw string (`RawStringLineShifts.IsMultiLineRawStringLiteral`), and no `BlockSyntax` comes before that hole. The class summary was updated. | FMT-INDENT-031 |
| `src/Reihitsu.Formatter.Test/Helpers/PhaseReversalExceptionList.cs` | Removed all five `LineStartIndentation` entries, each reported "Stale phase reversal entry" by its own test. Removed the four `using`s only those entries needed. The list is now empty (lead decision 5). | — |

**Code owner of FMT-INDENT-031.** No new type. The owner is `LambdaAlignmentContributor`, which the FMT-INDENT spec already lists as a code owner. I extended it rather than adding a parallel contributor: the anchor rule and the block shift are the block-lambda ones (maintainer answers 4 and 6).

## Policy owners

- `rg -n "OpenParenToDeclarationLine" src/Reihitsu.Formatter`
  - `CollapseOwnedOpenParenToDeclarationLine` covers the owner visits: anonymous method, class, struct, interface, record, extension, and operator.
  - `CollapseOpenParenToDeclarationLine` is the per-`ParameterList` visit, and it is the only path that reached a lambda.
  - No other copy exists.
- `rg -n "DelegateKeyword|AsyncKeyword" src/Reihitsu.Formatter`
  - The block-anchor policy lives only in `LambdaAlignmentContributor`.
  - `ExpressionBodiedTransformUtilities` checks `async` modifiers for a different decision.
- `rg -n "InterpolatedMultiLineRawStringStartToken" src/Reihitsu.Formatter`
  - Hits are in `RawStringLineShifts.IsMultiLineRawStringLiteral` (reused), `RawStringLineShifts.IsNonRawStringText`, and `LayoutComputer.RegisterRawStringHoleLines`.
  - The two `RawStringLineShifts` and `LayoutComputer` hits are pre-existing inline checks. I added none.
- Hole placement policy: `LayoutModel.AcceptsWrite`/`RecordPlacement` is reused unchanged. The contributor's node is the anonymous method, which lies inside the hole, so its `ShiftRange` places the hole lines (FMT-RAW-007, first condition).

## Tests run

All runs use `--configuration Debug`, on 4d65391.

```text
scripts/test.sh --project formatter --filter "FullyQualifiedName~ParenthesizedLambdaLineStartTests|FullyQualifiedName~AnonymousMethodHoleAlignmentTests|FullyQualifiedName~RawStringInterpolationHoleTests|FullyQualifiedName~PhaseStabilityCheckTests"
  before the change: Failed: 28, Passed: 89, Total: 117
  after:             Failed: 2 (the two disputed tests below), Passed: 115, Total: 117
scripts/test.sh --project formatter      Failed: 2, Passed: 4140, Total: 4142 (the same two)
scripts/test.sh --project analyzer       Passed: 6397, Total: 6397 (RH5417 parity test included)
scripts/test.sh --project core           Passed: 563
scripts/test.sh --project cli            Passed: 251
scripts/test.sh --project architecture   Passed: 92
scripts/test.sh --project tooling        Passed: 125
scripts/build.sh --configuration Debug   0 Warning(s), 0 Error(s)
```

`scripts/format.sh` over the three changed files changed nothing.

## Disputed tests or scope

**Disputed tests.** Both are in `src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/RawStringInterpolationHoleTests.cs`, tagged FMT-RAW-007 and FMT-INDENT-017, and both pass on `main`:

- `ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration` (line 974)
- `ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift` (line 1024)

**What they assert.** An anonymous method in a hole of a multi-line interpolated raw string: `Hello {M(delegate`, then its block on the next line with `{` in column 17 (the literal's content column), and a local function `void L<T>() where T : class` in its body.

**What the spec says.** FMT-INDENT-031 demands that this block, whose opening brace starts a line, move so the brace starts under `delegate`, in column 26. The lines inside keep their indentation relative to the brace. The test's own subject is FMT-INDENT-017: the `where` clause sits one level deeper than its declaration's line. That still holds in the new output.

**Output now** (identical for both tests):

```csharp
        var a = $"""
                 Hello {M(delegate
                          {
                              void L<T>()
                                  where T : class
                              {
                              }

                              return 1;
                          })}
                 """;
```

**Why they disagree.** The expected values predate FMT-INDENT-031 and lock in the layout it replaces. The change doc's *The test expectation changes* section names only `AnonymousMethodBrokenInsideHoleIsStableOnTheSecondPass`, so these two were not listed. The new output follows from requirements already decided: maintainer answer 1 and FMT-INDENT-031. The tests need their expected outputs re-derived; I did not change them.

**Scope.** Two production paths changed, the ones the change targets. No new public member or dependency. The shared helper `RawStringLineShifts.IsMultiLineRawStringLiteral` is reused, not changed.

## Architecture findings

_None._

## Follow-up candidates

- **`PhaseReversalCauses.LineStartIndentation` has no entry left.** The exception list is now empty, and that cause is declared but unused. It is found only through reflection, so no compiler warning reports it. Decision 5 covered removing entries, not causes. Removing the cause, and possibly the empty list machinery, is the lead's call, either as an adjacent cleanup of a few lines or as a follow-up.
