Role: test implementer
Commit: 2bda3fb
Verdict: tests written (disputed test resolved by the recorded reversal; formatter project green)

Inputs:
- `14-implementer.md`, *Disputed tests or scope*;
- `squad/decisions/2026-10-31-phase-tree-round-trip-arrow-comment-reversal.md` (option A: record the reversal);
- `squad/decisions/2026-10-31-phase-tree-round-trip-arrow-comment-layout.md` (the layout stays unspecified).

My prompt carried no intended fix to ignore.

## Changes

`src/Reihitsu.Formatter.Test/Helpers/PhaseReversalExceptionList.cs` has one new entry. It follows the list's conventions: one entry per fixture and phase, the test referenced through `typeof`/`nameof`, a known cause, and the phase-output fingerprint.

```csharp
new(typeof(StructuralTransformSeparatorFinalOutputTests), nameof(StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput), "77770a4201eb2019", PhaseReversalCauses.BlankLineNextToBrace, "921052f14d6bb85f", PhaseReversalLineEndings.Both),
```

- **Fingerprints.** The fixture and phase-output fingerprints are the ones the check reports for the test at d9aa414.
- **Cause.** The existing `BlankLineNextToBrace` cause fits: "the blank-line phase inserts a blank line next to `{` or `}`; the line-break phase removes it again". So I added no new cause.
- **Line endings.** The entry covers both line endings. The check covers LF and CRLF, and the list fails an entry that one line ending does not reach. The test passes, so the reversal happens under both.
- **Removal.** The follow-up on the arrow-comment layout removes this entry. If that change makes the reversal stop happening, the list's own staleness check fails the entry.

`src/Reihitsu.Formatter.Test/Unit/Pipeline/PhaseStabilityCheckTests.cs` has a new test, `CheckCatchesBlankLinePhaseInsertingBlankLineAboveCommentGluedToMemberBodyBrace`. It follows the existing `CheckCatches…` tests: it pins the reversal on the `Get3` member's final output, which `14-implementer.md` and `12-spec-author.md` show as `int Get3()` / `/* c */ {` / `return` / blank line / `/* d */ 42;` / `}`. It asserts:

- that this text is a fixed point of the pipeline;
- that `BlankLinePhase` is the only phase that changes it when run alone;
- that the phase's output is that text with a blank line in front of `/* c */ {`, under LF and CRLF.

It carries no requirement tag, as the other `CheckCatches…` tests carry none.

I changed no expected value of any other test.

## Tests

| Test | Requirement / failure class | Helper | Expected now |
|---|---|---|---|
| `StructuralTransformSeparatorFinalOutputTests.MembersWithACommentAtTheArrowReachAStableOutput` (unchanged, now covered by the entry) | FMT-STRUCT-026 | `AssertSecondPassIsStable` | pass |
| `PhaseStabilityCheckTests.CheckCatchesBlankLinePhaseInsertingBlankLineAboveCommentGluedToMemberBodyBrace` | FC-012 (phase reversal; no requirement) | `AssertOnlyPhaseChanges` | pass |

## Whole run

`scripts/test.sh --project formatter` at 2bda3fb: 3373 passed, 0 failed.

## Not covered, and why

The layout of members with a comment at the arrow stays unspecified by the arrow-comment-layout decision. The follow-up that specifies it removes the entry.

## Commands

```shell
scripts/test.sh --project formatter
scripts/test.sh --project formatter --filter "FullyQualifiedName~MembersWithACommentAtTheArrowReachAStableOutput|FullyQualifiedName~PhaseStabilityCheckTests"
```
