Role: test implementer
Commit: 2eb955c
Verdict: tests written (decision 7); blocked (decision 8)

The prompt carried no intended fix, suspected cause, or hoped-for verdict. It named the two lead decisions to carry out, and I took the expected values from the specs, not from the implementer's output.

## Tests

Changed in `src/Reihitsu.Formatter.Test/Unit/RawStringAlignment/RawStringInterpolationHoleTests.cs` (lead decision 7). The inputs are unchanged, and so is each test's subject: the `where` clause sits one level deeper than its declaration's line (FMT-INDENT-017).

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| ConstraintClauseInHoleOfAlignedLiteralIsIndentedFromItsDeclaration | FMT-RAW-007, FMT-INDENT-017, FMT-INDENT-031 | `AssertRuleResult` | pass |
| ConstraintClauseInHoleOfMisalignedLiteralIsIndentedFromItsDeclarationAfterTheShift | FMT-RAW-007, FMT-INDENT-017, FMT-INDENT-031 | `AssertRuleResult` | pass |

**How the expected output was derived.** Both literals end up with content in column 17 (FMT-RAW-007), so `Hello {M(delegate` puts `delegate` in column 26.

- FMT-INDENT-031 puts the opening brace in column 26 and keeps the lines inside the block relative to that brace.
- FMT-INDENT-002 puts the block's statements one level deeper, in column 30. That covers `void L<T>()`, the local function's braces, and `return 1;`.
- FMT-INDENT-017 puts `where T : class` one level deeper than its declaration's line, in column 34.
- The closing `})}` starts in column 26.

Because FMT-INDENT-031 now decides every block column the tests assert, each test is also tagged FMT-INDENT-031. The summaries now say that the block aligns under `delegate`; the old wording was "keeps its column".

The derived output matches the "Output now" block in `07-implementer.md` character for character.

## Decision 8: not applied

I removed `PhaseReversalCauses.LineStartIndentation` together with its now-unused `using Reihitsu.Formatter.Pipeline.LineBreaks;`. After that the formatter test project does not build:

```text
src/Reihitsu.Formatter.Test/Helpers/PhaseReversalCauses.cs(6,21): error S2094: Remove this empty class, write its code or make it an "interface".
```

`scripts/format.sh` turns the empty class into `public static class PhaseReversalCauses;`, and S2094 still reports it. No test needs the cause. The build needs a member, because decision 8 keeps the machinery, and the machinery includes the cause registry that `PhaseStabilityCheckTests.GetKnownCauses` reads by reflection.

I found no `SuppressMessage` and no S2094 suppression anywhere under `src/`. I reverted the helper change, so the file is unchanged on this commit.

Options for the lead:

- **A**: Keep `LineStartIndentation` as a declared cause with no entries. Nothing changes.
- **B**: Delete `PhaseReversalCauses` and make `GetKnownCauses` return an empty list.
  - Cost: `EveryCauseNamesAProductionPhase` and the known-cause assertion become vacuous, and a future reversal has to bring the registry back.
  - This changes the test machinery that decision 8 says to keep.
- **C**: Suppress S2094 on the empty class with a justification. This would be the first suppression in `src/`.

## Not covered, and why

No new tests. The anonymous-method alignment in holes is covered by `06-test-implementer.md`'s matrix. Each changed test keeps its own subject: the constraint clause in a moved and an unmoved literal.

## Commands

```shell
scripts/test.sh --project formatter --filter "FullyQualifiedName~RawStringInterpolationHoleTests.ConstraintClauseInHole"
scripts/test.sh --project formatter      # Passed: 4142, Failed: 0 (Release)
scripts/test.sh --project architecture   # Passed: 92, Failed: 0
```
