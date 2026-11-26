Role: implementer
Commit: 0f0adb0
Verdict: tests pass

The prompt carried no intended fix, suspected cause, or hoped-for verdict. I worked from `00-lead.md`, the accepted change doc `sdd/changes/2026-11/assignment-and-arm-comment-unsupported.md`, FMT-UNSUP-020/021/022 in `sdd/specs/formatter/unsupported-syntax.md`, and `06-test-implementer.md`. Before the change, the handed-over tests failed as `06` states (181 formatter, 33 analyzer); that count is the test implementer's run on 4f0282c, which I did not repeat separately.

## Changes

| File | What changed | Requirement |
|---|---|---|
| `src/Reihitsu.Formatter/Pipeline/Precheck/PrecheckPhase.cs` | Three reasons (`AssignmentCommentReason`, `SwitchArmExpressionCommentReason`, `NameEqualsCommentReason`) replace `AssignmentInArmCommentReason`. `AddLastCandidates` now holds the one walk of the last shapes: omitted type argument lists (018, unchanged), every `AssignmentExpressionSyntax` and `EqualsValueClauseSyntax` (020), every `SwitchExpressionArmSyntax` (021), and every `AttributeArgumentSyntax` with a name-equals and `UsingDirectiveSyntax` with an alias (022). Candidates appended in the order 017, 018, 020, 021, 022, after every other shape. New private helper `FindTriviaBehindToken(token, behind)`: trailing trivia of the token, then leading trivia of the node's first token. The order comment in `FindUnsupportedSyntax` names the new shapes. | FMT-UNSUP-020, FMT-UNSUP-021, FMT-UNSUP-022, FMT-UNSUP-003, FMT-UNSUP-007 |
| `src/Reihitsu.Formatter/Pipeline/LineBreaks/Rewriter/LineBreakAssignmentRewriter.cs` | The `WouldJoinAcrossUnjoinableTrivia` refusal deleted from the five joins behind an operator or `=`: `NormalizeCollectionInitializerAssignment`, `NormalizeCollectionExpressionAssignment`, `NormalizeAssignmentValuePlacement`, `NormalizeCollectionExpressionEqualsValue`, `NormalizeEqualsValuePlacement`. Their summaries now say the precheck refuses the gap (FMT-UNSUP-020). The refusal in `NormalizeOperatorPlacement` (the gap in front of the operator or `=`) stays. | FMT-BREAK-016, FMT-BREAK-001, FMT-UNSUP-020 |

## Policy owners

- `rg -n "SwitchExpressionArmSyntax|EqualsValueClauseSyntax|NameEquals" src/Reihitsu.Formatter/Pipeline/Precheck src/Reihitsu.Formatter/Pipeline/Core/Utilities/JoinedGapUtilities.cs`: the precheck is the only owner of the three new shapes. `JoinedGapUtilities.GetGap` classifies the gaps in front of a switch arm's `=>` and `when` and the anonymous-member name-equals (FMT-UNSUP-017). The new shapes leave those out: the precheck checks a name-equals only when its parent is an attribute argument or a using directive.
- `rg -ln "WouldJoinAcrossUnjoinableTrivia" src`: no remaining caller joins behind an assignment operator or an equals-value clause's `=`. `LineBreakAssignmentRewriter.NormalizeOperatorPlacement` still uses it for the gap in front (kept, change doc *Checked and kept*). The analyzer callers (RH5101, RH5105–RH5108, RH5113, RH5408, attribute placement) are about other gaps. RH5111-005's own guard is kept, as the change doc says.
- `rg -n "FMT-UNSUP-019|AssignmentInArm" src`: no hits remain.

## Production diff size

`git diff --shortstat origin/main...HEAD -- src`: 2 files changed, 82 insertions(+), 98 deletions(-). No new type, no new public member.

## Deleted special cases

- The refusal in the join behind an assignment operator for a collection initializer (`NormalizeCollectionInitializerAssignment`) — change doc *Deleted*, first entry.
- The refusal in the join behind an assignment operator for a collection expression (`NormalizeCollectionExpressionAssignment`) — first entry.
- The refusal in the join behind an assignment operator for any other value (`NormalizeAssignmentValuePlacement`) — first entry.
- The refusal in the join behind an equals-value clause's `=` for a collection expression (`NormalizeCollectionExpressionEqualsValue`) — first entry.
- The refusal in the join behind an equals-value clause's `=` for any other value (`NormalizeEqualsValuePlacement`) — first entry.
- FMT-UNSUP-019: `AssignmentInArmCommentReason`, `FindTriviaInOmittedTypeArgumentListOrBehindAssignmentInArm`, and `FindTriviaBehindAssignmentInArm`, which walked from an assignment up to the nearest arm or anonymous-object member — change doc *Deleted*, second entry.

I found no further special case. The joins in front of the operator or `=` (`NormalizeOperatorPlacement`) are kept, as the change doc says under *Checked and kept*.

## Tests run

Debug build (`scripts/build.sh --configuration Debug`): 0 warnings, 0 errors.

```text
scripts/test.sh --configuration Debug --project formatter      Passed!  - Failed: 0, Passed:  7374, Skipped: 0, Total:  7374
scripts/test.sh --configuration Debug --project analyzer       Passed!  - Failed: 0, Passed: 11926, Skipped: 0, Total: 11926
scripts/test.sh --configuration Debug --project cli            Passed!  - Failed: 0, Passed:   440, Skipped: 1, Total:   441
scripts/test.sh --configuration Debug --project architecture   Passed!  - Failed: 0, Passed:   198, Skipped: 0, Total:   198
scripts/test.sh --configuration Debug --project core           Passed!  - Failed: 0, Passed:   887, Skipped: 0, Total:   887
scripts/test.sh --configuration Debug --no-build --project selfhosting --filter "Name~RH2008|Name~RH2111"   Passed! - Failed: 0, Passed: 18
```

`06` expected RH2111 to fire on `PrecheckPhase`. It does not: deleting the FMT-UNSUP-019 walk and merging the new checks into the existing last-shape walk keeps the type under the 500-line limit in `reihitsu.json`. `scripts/format.sh` on both changed files changed nothing.

## Disputed tests or scope

_None._

## Architecture findings

_None._

## Follow-up candidates

_None from this stage._ The open worklist that `00-lead.md` records for after the implementer (decision 8) still stands: the FMT-BREAK-030 example with `LineBreakPhaseBlankLineTests.KeepsBlankLinesAboveBlockStatementAndGluedComment`, and FMT-INDENT-030's mention of the nested-initializer anchor.
